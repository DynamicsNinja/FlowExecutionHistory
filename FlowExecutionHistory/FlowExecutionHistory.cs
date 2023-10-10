using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fic.XTB.FlowExecutionHistory.Enums;
using Fic.XTB.FlowExecutionHistory.Extensions;
using Fic.XTB.FlowExecutionHistory.Forms;
using Fic.XTB.FlowExecutionHistory.Helpers;
using Fic.XTB.FlowExecutionHistory.Models;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;
using Fic.XTB.FlowExecutionHistory.Services;
using McTools.Xrm.Connection;
using Microsoft.Identity.Client;
using Microsoft.Office.Interop.Excel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using BrowserProfile = Fic.XTB.FlowExecutionHistory.Models.BrowserProfile;

namespace Fic.XTB.FlowExecutionHistory
{
    public partial class FlowExecutionHistory : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        public string RepositoryName => "FlowExecutionHistory";
        public string UserName => "DynamicsNinja";
        public string DonationDescription => "Thanks for supporting Flow Execution History tool";
        public string EmailAccount => "ivan.ficko@outlook.com";

        public Settings Settings;

        private AccessTokenResponse _flowAccessToken;

        public List<FlowRun> FlowRuns = new List<FlowRun>();
        private List<Flow> _flows = new List<Flow>();
        private List<Color> _colors = new List<Color>();

        public DataGridView FlowRunsGrid;

        private TriggerOutputsFilterForm _triggerOutputsFilterForm;

        public FlowExecutionHistory()
        {
            InitializeComponent();

            FlowRunsGrid = dgvFlowRuns;
        }

        private void FlowExecutionHistory_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out Settings, ConnectionDetail.ConnectionId.ToString()))
            {
                Settings = new Settings();

                if (ConnectionDetail.BrowserName != BrowserEnum.None)
                {
                    for (var i = 0; i < cbBrowser.Items.Count; i++)
                    {
                        var browser = (Browser)cbBrowser.Items[i];

                        if (browser.Type != ConnectionDetail.BrowserName) { continue; }

                        cbBrowser.SelectedIndex = i;

                        break;
                    }
                }
                else
                {
                    cbBrowser.SelectedIndex = 0;
                }

                if (ConnectionDetail.BrowserProfile != null)
                {
                    for (var i = 0; i < cbProfile.Items.Count; i++)
                    {
                        var browserProfile = (BrowserProfile)cbProfile.Items[i];

                        if (browserProfile.Name != ConnectionDetail.BrowserProfile) { continue; }

                        cbProfile.SelectedIndex = i;

                        break;
                    }
                }
                else
                {
                    cbProfile.SelectedIndex = 0;
                }
            }
            else
            {
                LogInfo("Settings found and loaded");

                if (Settings.Browser != null && Settings.BrowserProfile != null)
                {
                    for (var i = 0; i < cbBrowser.Items.Count; i++)
                    {
                        var browser = (Browser)cbBrowser.Items[i];

                        if (browser.Type != Settings.Browser.Type) { continue; }

                        cbBrowser.SelectedIndex = i;

                        break;
                    }
                }
                else
                {
                    cbBrowser.SelectedIndex = 0;
                }
            }

            if (string.IsNullOrWhiteSpace(ConnectionDetail.S2SClientSecret))
            {
                ShowInfoNotification(
                    "Enhance your experience by switching to client and secret authorization, eliminating the connect to Power Automate API step",
                    new Uri("https://learn.microsoft.com/en-us/power-platform/admin/manage-application-users#create-an-application-user")
                    );
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void GetFlows()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting flows",
                Work = (worker, args) =>
                {
                    var fetch = $@"
                    <fetch>
	                    <entity name='workflow'>
		                     <attribute name='workflowid' />
		                     <attribute name='workflowidunique' />
                             <attribute name='clientdata' />
		                     <attribute name='name' />
                             <filter type='and'>
			                    <condition attribute='category' operator='eq' value='5' />
		                    </filter>
	                    </entity>
                    </fetch>";

                    var entities = Service.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToList();

                    args.Result = entities.Select(f => new Flow
                    {
                        Id = ((Guid)f["workflowidunique"]).ToString("D"),
                        Name = (string)f["name"],
                        ClientDataJson = (string)f["clientdata"]
                    }).OrderBy(f => f.Name).ToList();
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error);
                    }
                    else
                    {

                        if (!(args.Result is List<Flow> flows)) { return; }

                        _flows = flows;

                        clbFlows.Items.AddRange(_flows.ToArray());

                        gbFlow.Text = $"Flows ({_flows.Count})";
                    }

                }
            });
        }

        private void GetFlowRuns()
        {
            var selectedFlows = _flows.Where(f => f.IsSelected).ToList();

            var dateFrom = (DateTimeOffset)dtpDateFrom.Value;
            var dateTo = (DateTimeOffset)dtpDateTo.Value;

            var status = cbxStatus.Text;
            var durationThreshold = string.IsNullOrWhiteSpace(tbDurationThreshold.Text)
                ? 0
                : int.Parse(tbDurationThreshold.Text);

            if (!selectedFlows.Any())
            {
                dgvFlowRuns.DataSource = null;
                return;
            }

            gbFlow.Enabled = false;
            gbFlowRuns.Enabled = false;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting flow runs",
                Work = (worker, args) =>
                {
                    _flowAccessToken = _flowAccessToken ?? ConnectionDetail.GetPowerAutomateAccessToken();

                    var flowClient = new FlowClient(ConnectionDetail.EnvironmentId, _flowAccessToken.access_token);

                    var options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount * 4
                    };

                    Parallel.ForEach(selectedFlows, options, f =>
                    {
                        var fr = flowClient.GetFlowRuns(f, status, dateFrom, dateTo);
                        fr = fr.Where(r => r.DurationInSeconds >= durationThreshold).ToList();
                        f.FlowRuns = fr;
                    });

                    var flowRuns = selectedFlows.SelectMany(f => f.FlowRuns).ToList();

                    args.Result = flowRuns;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error.InnerException);
                    }
                    else
                    {
                        if (!(args.Result is List<FlowRun> runs)) { return; }

                        FlowRuns = runs;

                        dgvFlowRuns.Columns["FlowRunFLow"].Visible = selectedFlows.Count > 1;
                        dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(FlowRuns);
                        dgvFlowRuns.Sort(dgvFlowRuns.Columns["FlowRunStartDate"], ListSortDirection.Descending);

                        ShowHideTriggerOutputFilterButtons(selectedFlows.Count == 1 && runs.Count > 0);

                        gbFlowRuns.Text = $@"Flow Runs ({FlowRuns.Count})";
                    }


                    gbFlow.Enabled = true;
                    gbFlowRuns.Enabled = true;
                }
            });
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail,
            string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            _colors = ColorHelper.GetAllColors(1000);
            ExecuteMethod(GetFlows);

            if (Settings == null || detail == null)
            {
                return;
            }

            LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
        }

        private void cbxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetFlowRuns();
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            var searchText = tbSearch.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                clbFlows.DataSource = _flows;
            }
            else
            {
                var filteredFlows = _flows.Where(f => f.Name.ToLower().Contains(searchText)).ToList();
                clbFlows.DataSource = filteredFlows;
            }

            clbFlows.ItemCheck -= clbFlows_ItemCheck;

            for (var i = 0; i < clbFlows.Items.Count; i++)
            {
                var flow = (Flow)clbFlows.Items[i];

                clbFlows.SetItemChecked(i, flow.IsSelected);
            }

            clbFlows.ItemCheck += clbFlows_ItemCheck;
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
        {
            GetFlowRuns();
        }

        private void dtpDateTo_ValueChanged(object sender, EventArgs e)
        {
            GetFlowRuns();
        }

        private void dgvFlowRuns_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) { return; }

            var flowRun = (FlowRun)dgvFlowRuns.Rows[e.RowIndex].DataBoundItem;

            switch (dgvFlowRuns.Columns[e.ColumnIndex].Name)
            {
                case "FlowRunUrl":
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo(Settings.Browser.Executable)
                    {
                        Arguments = flowRun.Url
                    };

                    switch (Settings.Browser.Type)
                    {
                        case BrowserEnum.Chrome:
                        case BrowserEnum.Edge:
                            process.StartInfo.Arguments += $" --profile-directory=\"{Settings.BrowserProfile.Path}\"";
                            break;
                        case BrowserEnum.Firefox:
                            process.StartInfo.Arguments += $" -P \"{Settings.BrowserProfile.Path}\"";
                            break;
                    }

                    process.Start();
                    break;
                case "FlowRunTriggerOutputs":
                    if (flowRun.TriggerOutputsUrl == null)
                    {
                        MessageBox.Show(
                            "There are no trigger outputs to show.",
                            "Information",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                            );

                        return;
                    }

                    if (flowRun.TriggerOutputs == null)
                    {
                        GetTriggerOutputs(flowRun);
                    }
                    else
                    {
                        var triggerOutputsForm = new TriggerOutputsForm(flowRun.TriggerOutputs);
                        triggerOutputsForm.ShowDialog();
                    }

                    break;
                case "FlowRunStatus":
                    if (flowRun.Status != Enums.FlowRunStatus.Failed) { return; }

                    if (flowRun.Error == null)
                    {
                        GetFlowRunErrorDetails(flowRun);
                    }
                    else
                    {
                        var errorForm = new FlowRunErrorForm(flowRun.Error);
                        errorForm.ShowDialog();
                    }

                    break;
                case "FlowRunCorrelationId":
                    foreach (DataGridViewRow row in dgvFlowRuns.Rows)
                    {
                        var cell = row.Cells[e.ColumnIndex];

                        if (flowRun.CorrelationId == cell.Value.ToString())
                        {
                            cell.Style.BackColor = Color.Red;
                            cell.Style.ForeColor = Color.White;
                        }
                        else
                        {
                            cell.Style.BackColor = Color.White; // Reset cell color if condition is not met
                            cell.Style.ForeColor = Color.Black; // Reset cell text color if condition is not met
                        }
                    }
                    break;
            }
        }

        private void GetTriggerOutputs(FlowRun flowRun)
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting trigger outputs",
                Work = (worker, args) =>
                {

                    var triggerOutputs = flowRun.GetTriggerOutputs();

                    args.Result = triggerOutputs;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error);
                    }

                    if (!(args.Result is TriggerOutputsResponseDto triggerOutputs))
                    {
                        return;
                    }

                    var triggerOutputsForm = new TriggerOutputsForm(triggerOutputs);
                    triggerOutputsForm.ShowDialog();
                }
            });
        }

        private void GetFlowRunErrorDetails(FlowRun flowRun)
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting error details",
                Work = (worker, args) =>
                {
                    _flowAccessToken = _flowAccessToken ?? ConnectionDetail.GetPowerAutomateAccessToken();

                    var flowClient = new FlowClient(ConnectionDetail.EnvironmentId, _flowAccessToken.access_token);
                    var errorDetails = flowClient.GetFlowRunErrorDetails(flowRun);

                    flowRun.Error = new FlowRunError
                    {
                        Message = errorDetails.errorSubject,
                        Details = errorDetails.errorDescription
                                  ?? errorDetails?.operationOutputs?.body?.ToString()
                    };

                    args.Result = flowRun.Error;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error);
                    }

                    if (!(args.Result is FlowRunError errorDetails))
                    {
                        return;
                    }

                    var errorForm = new FlowRunErrorForm(errorDetails);
                    errorForm.ShowDialog();
                }
            });
        }

        private void dgvFlowRuns_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                dgvFlowRuns.Cursor = Cursors.Default;
                return;
            }

            var columnName = dgvFlowRuns.Columns[e.ColumnIndex].Name;

            switch (columnName)
            {
                case "FlowRunUrl":
                    dgvFlowRuns.Cursor = Cursors.Hand;
                    break;
                case "FlowRunStatus":
                    var flowRun = (FlowRun)dgvFlowRuns.Rows[e.RowIndex].DataBoundItem;

                    dgvFlowRuns.Cursor = flowRun.Status == Enums.FlowRunStatus.Failed ? Cursors.Hand : Cursors.Default;
                    break;
                default:
                    dgvFlowRuns.Cursor = Cursors.Default;
                    break;
            }
        }

        private void clbFlows_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var selectedFlow = (Flow)clbFlows.Items[e.Index];
            var checkedValue = e.NewValue == CheckState.Checked;

            SetCheckedFlow(selectedFlow, checkedValue);

            GetFlowRuns();
        }

        private void SetCheckedFlow(Flow flow, bool checkedValue)
        {
            Color? poppedColor = null;
            if (checkedValue)
            {
                poppedColor = _colors[_colors.Count - 1];
                _colors.RemoveAt(_colors.Count - 1);
            }
            else
            {
                if (flow.Color != null) { _colors.Add((Color)flow.Color); }
            }

            flow.Color = poppedColor;
            flow.IsSelected = checkedValue;
        }

        private void dgvFlowRuns_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var flowRun = (FlowRun)dgvFlowRuns.Rows[e.RowIndex].DataBoundItem;

            switch (dgvFlowRuns.Columns[e.ColumnIndex].Name)
            {
                case "FlowRunStatus":
                    {

                        if (flowRun == null)
                        {
                            return;
                        }

                        e.CellStyle.BackColor = flowRun.Status == Enums.FlowRunStatus.Succeeded ? Color.Green : Color.Red;
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    }
                case "FlowRunFlow":
                    if (!Settings.UseFlowColors) { return; }
                    if (flowRun.Flow.Color == null) { return; }

                    e.CellStyle.BackColor = (Color)flowRun.Flow.Color;
                    e.CellStyle.ForeColor = Color.Black;
                    break;
            }
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            GetFlowRuns();
        }

        private void cbBrowser_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedBrowser = (Browser)cbBrowser.SelectedItem;

            if (selectedBrowser == null)
            {
                return;
            }

            Settings.Browser = selectedBrowser;

            cbProfile.Items.Clear();
            cbProfile.Items.AddRange(selectedBrowser.Profiles.ToArray());

            var profileIndex = selectedBrowser.Profiles.FindIndex(bp => bp.Path.Equals(Settings.BrowserProfile?.Path));

            cbProfile.SelectedIndex = profileIndex == -1 ? 0 : profileIndex;

            SaveSettings();
        }

        private void cbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedBrowserProfile = (BrowserProfile)cbProfile.SelectedItem;

            if (selectedBrowserProfile == null)
            {
                return;
            }

            Settings.BrowserProfile = selectedBrowserProfile;

            SaveSettings();
        }

        private void FlowExecutionHistory_OnCloseTool(object sender, EventArgs e)
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            SettingsManager.Instance.Save(GetType(), Settings, ConnectionDetail.ConnectionId.ToString());
        }

        private void FlowExecutionHistory_ConnectionUpdated(object sender, ConnectionUpdatedEventArgs e)
        {
            gbFlowRuns.Enabled = !string.IsNullOrWhiteSpace(ConnectionDetail.S2SClientSecret);
            gbFlow.Enabled = !string.IsNullOrWhiteSpace(ConnectionDetail.S2SClientSecret);
            tsbConnectFlowApi.Visible = string.IsNullOrWhiteSpace(ConnectionDetail.S2SClientSecret);

            cbxStatus.SelectedIndex = 0;

            var today = DateTime.Now;
            var fromDateTime = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            var toDateTime = fromDateTime.AddDays(1);

            dtpDateFrom.Value = fromDateTime;
            dtpDateTo.Value = toDateTime;

            var browsers = BrowserLoader.GetBrowsers();
            cbBrowser.Items.AddRange(browsers.ToArray());
        }

        private void tbDurationThreshold_Leave(object sender, EventArgs e)
        {
            GetFlowRuns();
        }

        private async void tsbConnectPowerAutomateApi_Click(object sender, EventArgs e)
        {
            var clientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
            var redirectUri = "app://58145B91-0C36-4500-8554-080854F2AC97";
            var scopes = new[] { "https://service.flow.microsoft.com/.default" };

            if (ConnectionDetail.TenantId == Guid.Empty)
            {
                MessageBox.Show(
                    "You are using the deprecated connection method. Please use OAuth/MFA or Client ID/Secret method.",
                    "Deprecated Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var tenantId = ConnectionDetail.TenantId.ToString("D");

            var app = PublicClientApplicationBuilder.Create(clientId).WithRedirectUri(redirectUri).Build();

            AuthenticationResult authResult;

            try
            {
                var accounts = await app.GetAccountsAsync();
                var account = accounts.FirstOrDefault();

                authResult = await app
                    .AcquireTokenSilent(scopes, account)
                    .WithTenantId(tenantId)
                    .ExecuteAsync();

                _flowAccessToken = new AccessTokenResponse
                {
                    access_token = authResult.AccessToken,
                    token_type = authResult.TokenType,
                };

                gbFlowRuns.Enabled = true;
                gbFlow.Enabled = true;
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    authResult = await app
                        .AcquireTokenInteractive(scopes)
                        .WithTenantId(tenantId)
                        .ExecuteAsync();

                    _flowAccessToken = new AccessTokenResponse
                    {
                        access_token = authResult.AccessToken,
                        token_type = authResult.TokenType,
                    };

                    gbFlowRuns.Enabled = true;
                    gbFlow.Enabled = true;

                }
                catch (MsalClientException ex)
                {
                    if (ex.ErrorCode == "authentication_canceled")
                    {
                        return;
                    }
                }
            }
        }

        private void tsbCsv_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.Title = "Export to CSV";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == "") { return; }

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Exporting CSV",
                Work = (worker, args) =>
                {
                    var sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);

                    sw.WriteLine("Id;Flow Name;Status;Start Date;Duration In Seconds;Url");

                    foreach (var flowRun in FlowRuns)
                    {
                        sw.WriteLine($"{flowRun.Id};{flowRun.Flow.Name};{flowRun.Status};{flowRun.StartDate};{flowRun.DurationInSeconds};{flowRun.Url}");
                    }

                    sw.Close();
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error);
                    }

                    var result = MessageBox.Show(
                        "Do you you want to open exported CSV file?",
                        "Export Completed",
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.No) { return; }

                    Process.Start(saveFileDialog.FileName);
                }
            });
        }

        private void tsbExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        public void ExportToExcel()
        {
            var saveFileDialog = new SaveFileDialog();
            var filter = "Excel file (*.xlsx)|*.xlsx| All Files (*.*)|*.*";
            saveFileDialog.Filter = filter;
            saveFileDialog.Title = @"Export as Excel file";

            if (saveFileDialog.ShowDialog() != DialogResult.OK) { return; }

            WorkAsync(new WorkAsyncInfo("Creating Excel file...",
               (eventargs) =>
               {
                   var excel = new Microsoft.Office.Interop.Excel.Application();
                   var wb = excel.Workbooks.Add();
                   var sh = (Worksheet)wb.Sheets.Add();
                   sh.Name = "Flow Runs";

                   sh.Cells[1, 1] = "Id";
                   sh.Cells[1, 2] = "Flow Name";
                   sh.Cells[1, 3] = "Status";
                   sh.Cells[1, 4] = "Start Date";
                   sh.Cells[1, 5] = "Duration in seconds";
                   sh.Cells[1, 6] = "Url";

                   for (var index = 0; index < FlowRuns.Count; index++)
                   {
                       var row = (FlowRun)FlowRuns[index];

                       sh.Cells[index + 2, "A"] = row.Id;
                       sh.Cells[index + 2, "B"] = row.Flow.Name;
                       sh.Cells[index + 2, "C"] = row.Status;
                       sh.Cells[index + 2, "D"] = row.StartDate;
                       sh.Cells[index + 2, "E"] = (int)row.DurationInSeconds;

                       // Add hyperlink to the cell containing the URL
                       var cell = (Range)sh.Cells[index + 2, "F"];
                       sh.Hyperlinks.Add(cell, row.Url);
                   }

                   var statusColumn = sh.Range["C2:C" + (FlowRuns.Count + 1)];
                   var successCondition = (FormatCondition)statusColumn.FormatConditions.Add(
                       XlFormatConditionType.xlExpression,
                       XlFormatConditionOperator.xlEqual,
                       $"=$C2=\"{Enums.FlowRunStatus.Succeeded}\""
                   );

                   successCondition.Interior.Color = ColorTranslator.ToOle(Color.Green);
                   successCondition.Font.Bold = true;

                   var failureCondition = (FormatCondition)statusColumn.FormatConditions.Add(
                       XlFormatConditionType.xlExpression,
                       XlFormatConditionOperator.xlEqual,
                       $"=$C2=\"{Enums.FlowRunStatus.Failed}\""
                   );

                   failureCondition.Interior.Color = ColorTranslator.ToOle(Color.Red);
                   failureCondition.Font.Bold = true;

                   var range = sh.Range["A1", $"F{FlowRuns.Count + 1}"];
                   FormatAsTable(range, "Table1", "TableStyleMedium15");

                   range.Columns.AutoFit();

                   excel.DisplayAlerts = false;
                   wb.SaveAs(saveFileDialog.FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                   wb.Close(true);
                   excel.Quit();
               })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Error != null)
                    {
                        MessageBox.Show(completedargs.Error.Message);
                    }

                    var result = MessageBox.Show(
                        "Do you you want to open exported Excel file?",
                        "Export Completed",
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.No) { return; }

                    Process.Start(saveFileDialog.FileName);
                }
            });
        }
        public void FormatAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
                    sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }

        private void tsbExport_ButtonClick(object sender, EventArgs e)
        {
            tsbExport.ShowDropDown();
        }

        private void tsbSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(this);
            settingsForm.ShowDialog();
        }

        private void cbSelectAllFlows_CheckedChanged(object sender, EventArgs e)
        {
            var isChecked = cbSelectAllFlows.Checked;

            clbFlows.ItemCheck -= clbFlows_ItemCheck;

            for (var i = 0; i < clbFlows.Items.Count; i++)
            {
                var flow = (Flow)clbFlows.Items[i];

                clbFlows.SetItemChecked(i, isChecked);
                SetCheckedFlow(flow, isChecked);
            }

            clbFlows.ItemCheck += clbFlows_ItemCheck;

            GetFlowRuns();
        }

        public void FilterRunsByTriggerOutputs(ConditionGroup conditionGroup)
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting trigger outputs",
                Work = (worker, args) =>
                {
                    var options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount * 4
                    };

                    var list = new List<FlowRun>();

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Parallel.ForEach(FlowRuns, options, fr =>
                    {
                        if (fr.TriggerOutputs != null) { return; }

                        fr.GetTriggerOutputs();
                    });

                    var attributes = conditionGroup.FilterConditions.Select(fc => fc.Attribute).Distinct().ToList();

                    foreach (var fr in FlowRuns)
                    {
                        var outputs = fr.TriggerOutputs;

                        if (!attributes.All(a => outputs.Body.ContainsKey(a))) { continue; }

                        var isMatch = conditionGroup.Evaluate(outputs.Body);

                        if (isMatch)
                        {
                            list.Add(fr);
                        }
                    }

                    stopwatch.Stop();

                    args.Result = list;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error.InnerException);
                    }
                    else
                    {
                        var filteredFlowRuns = (List<FlowRun>)args.Result;
                        dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(filteredFlowRuns);

                        gbFlowRuns.Text = $@"Flow Runs ({filteredFlowRuns.Count})";
                    }
                }
            });

        }

        private void tsbGetTriggerOutputs_Click(object sender, EventArgs e)
        {
            if (FlowRuns.FirstOrDefault().TriggerOutputsUrl == null)
            {
                MessageBox.Show("There are no trigger outputs to filter runs.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _triggerOutputsFilterForm = _triggerOutputsFilterForm ?? new TriggerOutputsFilterForm(this);
                _triggerOutputsFilterForm.ShowDialog();
            }
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(FlowRuns);
            gbFlowRuns.Text = $@"Flow Runs ({FlowRuns.Count})";
        }

        private void ShowHideTriggerOutputFilterButtons(bool show)
        {
            tslTriggerOutputs.Visible = show;
            tsbGetTriggerOutputs.Visible = show;
            btnResetFilters.Visible = show;
            tssTriggerOutputs.Visible = show;

            if (FlowRuns.FirstOrDefault()?.TriggerOutputsUrl == null || !show) { return; }

            _triggerOutputsFilterForm = new TriggerOutputsFilterForm(this);
        }
    }
}