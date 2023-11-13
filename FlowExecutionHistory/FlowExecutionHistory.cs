﻿using System;
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
using Newtonsoft.Json;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Action = System.Action;
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
        public List<FlowRun> FilteredFlowRuns = null;

        public List<Flow> Flows = new List<Flow>();
        public Dictionary<string, TriggerOutputsResponseDto> CachedTriggerOutputs = new Dictionary<string, TriggerOutputsResponseDto>();

        private List<Color> _colors = new List<Color>();

        public DataGridView FlowRunsGrid;

        private TriggerOutputsFilterForm _triggerOutputsFilterForm;
        private TriggerOutputsColumnsSelectForm _triggerOutputsColumnsSelectForm;

        private readonly List<DataGridViewColumn> _initialColumns = new List<DataGridViewColumn>();

        public FlowExecutionHistory()
        {
            InitializeComponent();

            FlowRunsGrid = dgvFlowRuns;

            foreach (DataGridViewColumn column in dgvFlowRuns.Columns)
            {
                _initialColumns.Add(column.Clone() as DataGridViewColumn);
            }
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
                        <attribute name='statecode' />
                        <attribute name='statuscode' />
                        <attribute name='ismanaged' />
                        <filter type='and'>
                          <condition attribute='category' operator='eq' value='5' />
                        </filter>
                      </entity>
                    </fetch>";

                    var entities = Service.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToList();

                    var fetchedFlows = new List<Flow>();

                    foreach (var f in entities)
                    {
                        var flowId = ((Guid)f["workflowidunique"]).ToString("D");
                        var flowName = (string)f["name"];
                        var flowStatus = (FlowStatus)((OptionSetValue)f["statecode"]).Value;
                        var clentDataJson = (string)f["clientdata"];
                        var isManaged = (bool)f["ismanaged"];

                        var clientData = JsonConvert.DeserializeObject<FlowClientData>(clentDataJson);

                        var triggerType = clientData?.properties?.definition?.triggers?.FirstOrDefault().Value?.type;

                        var flow = new Flow
                        {
                            Id = flowId,
                            Name = flowName,
                            ClientData = clientData,
                            Status = flowStatus,
                            TriggerType = triggerType,
                            IsManaged = isManaged
                        };

                        fetchedFlows.Add(flow);
                    }

                    args.Result = fetchedFlows.OrderBy(f => f.Name).ToList();
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

                        Flows = flows;

                        FilterFlows();
                    }

                }
            });
        }

        private void GetFlowRuns()
        {
            var selectedFlows = Flows.Where(f => f.IsSelected).ToList();

            var dateFrom = (DateTimeOffset)dtpDateFrom.Value;
            var dateTo = (DateTimeOffset)dtpDateTo.Value;

            var status = cbxStatus.Text;
            var durationThreshold = string.IsNullOrWhiteSpace(tbDurationThreshold.Text)
                ? 0
                : int.Parse(tbDurationThreshold.Text);

            if (!selectedFlows.Any())
            {
                ShowHideTriggerOutputFilterButtons(false);
                dgvFlowRuns.DataSource = null;
                return;
            }

            var includeTriggerOutputs = _triggerOutputsColumnsSelectForm?.SelectedColumns?.Count > 0;

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
                        fr = fr.Where(r => r.DurationInMilliseconds / 1000 >= durationThreshold).ToList();
                        f.FlowRuns = fr;
                    });

                    var flowRuns = selectedFlows.SelectMany(f => f.FlowRuns).ToList();

                    if (includeTriggerOutputs)
                    {
                        Parallel.ForEach(flowRuns, options,
                            fr =>
                            {
                                fr.TriggerOutputs = fr.TriggerOutputs ?? GetTriggerOutputsForFlowRun(fr);

                            });

                        foreach (var fr in flowRuns)
                        {
                            CachedTriggerOutputs[fr.Flow.Id] = fr.TriggerOutputs;
                        }
                    }

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

                        ShowHideTriggerOutputFilterButtons(selectedFlows.Count >= 1 && runs.Count > 0);

                        _triggerOutputsFilterForm = _triggerOutputsFilterForm ?? new TriggerOutputsFilterForm(this);

                        ApplyTriggerOutputsFilters();
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
            FilterFlows();
        }

        private void FilterFlows()
        {
            var searchText = tbSearch.Text.ToLower();
            var activated = cbxFlowStatusActivated.Checked;
            var dreft = cbxFlowStatusDraft.Checked;

            var showAutomated = cbAutomated.Checked;
            var showScheduled = cbScheduled.Checked;
            var showInstant = cbInstant.Checked;

            var showManaged = cbManaged.Checked;
            var showUmmanaged = cbUnmanaged.Checked;

            var filteredFlows = Flows
                .Where(f =>
                    f.Status == FlowStatus.Activated && activated
                    || f.Status == FlowStatus.Draft && dreft)
                .Where(f =>
                    f.TriggerType == FlowTriggerType.Automated && showAutomated
                    || f.TriggerType == FlowTriggerType.Scheduled && showScheduled
                    || f.TriggerType == FlowTriggerType.Instant && showInstant)
                .Where(f =>
                    f.IsManaged && showManaged
                    || !f.IsManaged && showUmmanaged)
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredFlows = filteredFlows.Where(f => f.Name.ToLower().Contains(searchText)).ToList();
            };

            gbFlow.Text = $"Flows ({filteredFlows.Count})";

            clbFlows.DataSource = filteredFlows;

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

            var column = dgvFlowRuns.Columns[e.ColumnIndex];
            var columnName = column.Name;

            switch (columnName)
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

                case "FlowRunDurationInSeconds":
                    e.Value = TimeFormatter.MillisecondsTimeString(flowRun.DurationInMilliseconds);
                    break;
            }

            if (columnName.StartsWith("to_"))
            {
                if (flowRun.TriggerOutputs == null) { return; }

                var triggerOutputName = columnName.Replace("to_", "");
                var triggerOutputs = flowRun.TriggerOutputs.Body;
                var found = triggerOutputs.TryGetValue(triggerOutputName, out var triggerOutput) ? triggerOutput : null;

                if (found == null) { return; }

                e.Value = triggerOutput;
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

                HideNotification();
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

                    HideNotification();
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
                        sw.WriteLine($"{flowRun.Id};{flowRun.Flow.Name};{flowRun.Status};{flowRun.StartDate};{flowRun.DurationInMilliseconds / 1000};{flowRun.Url}");
                    }

                    sw.Close();
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error);
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            "Do you you want to open exported CSV file?",
                            "Export Completed",
                            MessageBoxButtons.YesNo);

                        if (result == DialogResult.No) { return; }

                        Process.Start(saveFileDialog.FileName);
                    }
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
                       sh.Cells[index + 2, "E"] = row.DurationInMilliseconds / 1000;

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
                    else
                    {
                        var result = MessageBox.Show(
                            "Do you you want to open exported Excel file?",
                            "Export Completed",
                            MessageBoxButtons.YesNo);

                        if (result == DialogResult.No) { return; }

                        Process.Start(saveFileDialog.FileName);
                    }
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

        public void ApplyTriggerOutputsFilters()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading flow runs",
                Work = (worker, args) =>
                {
                    var conditionGroup = _triggerOutputsFilterForm?.ConditionGroup;
                    var allAttributes = _triggerOutputsColumnsSelectForm?.SelectedColumns ?? new List<string>();

                    if (conditionGroup != null || allAttributes.Count != 0)
                    {
                        var options = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount * 4
                        };

                        Parallel.ForEach(FlowRuns, options, fr =>
                        {
                            fr.TriggerOutputs = fr.TriggerOutputs ?? fr.GetTriggerOutputs();
                        });
                    }

                    var filteredFlowRuns = new List<FlowRun>();

                    if (conditionGroup != null)
                    {
                        foreach (var fr in FlowRuns)
                        {
                            var outputs = fr.TriggerOutputs;

                            if (outputs == null) { continue; }

                            var isMatch = conditionGroup.Evaluate(outputs.Body);
                            if (!isMatch) { continue; }

                            filteredFlowRuns.Add(fr);
                        }
                    }

                    FilteredFlowRuns = conditionGroup == null ? FlowRuns : filteredFlowRuns;

                    dgvFlowRuns.Invoke(new Action(() =>
                    {
                        dgvFlowRuns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        ResetFlowRunsGridColumns();

                        foreach (var field in allAttributes)
                        {
                            var newColumn = new DataGridViewTextBoxColumn();
                            newColumn.Name = $"to_{field}";
                            newColumn.HeaderText = field;

                            dgvFlowRuns.Columns.Add(newColumn);
                        }

                        dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(FilteredFlowRuns ?? FlowRuns);
                        dgvFlowRuns.Sort(dgvFlowRuns.Columns["FlowRunStartDate"], ListSortDirection.Descending);

                        var showFlowNamesIfMultipleSelected = Flows.Count(f => f.IsSelected) > 1;

                        dgvFlowRuns.Columns["FlowRunFLow"].Visible = showFlowNamesIfMultipleSelected;

                        var flowRunsCount = (FilteredFlowRuns ?? FlowRuns).Count;

                        gbFlowRuns.Text = $@"Flow Runs ({flowRunsCount})";
                    }));
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error.InnerException);
                    }
                }
            });
        }

        //public void FilterRunsByTriggerOutputs(ConditionGroup conditionGroup)
        //{
        //    ResetFlowRunsGridColumns();

        //    WorkAsync(new WorkAsyncInfo
        //    {
        //        Message = "Getting trigger outputs",
        //        Work = (worker, args) =>
        //        {
        //            var options = new ParallelOptions
        //            {
        //                MaxDegreeOfParallelism = Environment.ProcessorCount * 4
        //            };

        //            var list = new List<FlowRun>();

        //            var stopwatch = new Stopwatch();
        //            stopwatch.Start();

        //            Parallel.ForEach(FlowRuns, options, fr =>
        //            {
        //                fr.TriggerOutputs = fr.TriggerOutputs ?? fr.GetTriggerOutputs();
        //            });

        //            foreach (var fr in FlowRuns)
        //            {
        //                var outputs = fr.TriggerOutputs;

        //                if (outputs == null) { continue; }

        //                var isMatch = conditionGroup.Evaluate(outputs.Body);

        //                if (!isMatch) { continue; }

        //                list.Add(fr);
        //            }

        //            stopwatch.Stop();

        //            args.Result = list;
        //        },
        //        PostWorkCallBack = (args) =>
        //        {
        //            if (args.Error != null)
        //            {
        //                ShowErrorDialog(args.Error.InnerException);
        //            }
        //            else
        //            {
        //                FilteredFlowRuns = (List<FlowRun>)args.Result;
        //                //dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(FilteredFlowRuns);

        //                gbFlowRuns.Text = $@"Flow Runs ({FilteredFlowRuns.Count})";

        //                PopulateTriggerOutputsFields();
        //            }
        //        }
        //    });

        //}

        private void tsbGetTriggerOutputs_Click(object sender, EventArgs e)
        {
            var anyOutputs = FlowRuns.Any(fr => !string.IsNullOrWhiteSpace(fr.TriggerOutputsUrl));

            if (!anyOutputs)
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
            _triggerOutputsFilterForm = new TriggerOutputsFilterForm(this);

            FilteredFlowRuns = null;
            dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(FlowRuns);
            dgvFlowRuns.Sort(dgvFlowRuns.Columns["FlowRunStartDate"], ListSortDirection.Descending);
            gbFlowRuns.Text = $@"Flow Runs ({FlowRuns.Count})";
        }

        private void ShowHideTriggerOutputFilterButtons(bool show)
        {
            tslTriggerOutputs.Visible = show;
            tsbGetTriggerOutputs.Visible = show;
            btnResetFilters.Visible = show;
            tssTriggerOutputs.Visible = show;
            btnShowColumns.Visible = show;

            if (FlowRuns.FirstOrDefault()?.TriggerOutputsUrl == null || !show) { return; }

            _triggerOutputsFilterForm = _triggerOutputsFilterForm ?? new TriggerOutputsFilterForm(this);
        }

        private void cbxFlowStatusDraft_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();
        }

        private void cbxFlowStatusActivated_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();
        }

        private void clbFlows_MouseMove(object sender, MouseEventArgs e)
        {
            var index = clbFlows.IndexFromPoint(e.Location);

            if (index < 0 || index >= clbFlows.Items.Count) { return; }

            var itemText = clbFlows.Items[index].ToString();

            var checkBoxWidth = 18;
            var textWidth = TextRenderer.MeasureText(itemText, clbFlows.Font).Width + checkBoxWidth;

            var isTextOverflowing = textWidth > clbFlows.ClientSize.Width;

            toolTip1.SetToolTip(clbFlows, isTextOverflowing ? itemText : "");
        }

        private void btnShowColumns_Click(object sender, EventArgs e)
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Showing trigger output columns",
                Work = (worker, args) =>
                {
                    var options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount * 4
                    };

                    Parallel.ForEach(Flows, options, f =>
                    {
                        var flowRun = f.FlowRuns?.FirstOrDefault();

                        if (flowRun == null) { return; }

                        flowRun.TriggerOutputs = flowRun.TriggerOutputs ?? GetTriggerOutputsForFlowRun(flowRun);
                    });

                    var allAttributes = FlowRuns
                        .Where(fr => fr.TriggerOutputs != null)
                        .SelectMany(fr => fr.TriggerOutputs.Body.Keys)
                        .Distinct()
                        .ToList();

                    args.Result = allAttributes;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error.InnerException);
                    }
                    else
                    {
                        var attributes = args.Result as List<string>;
                        _triggerOutputsColumnsSelectForm = _triggerOutputsColumnsSelectForm ?? new TriggerOutputsColumnsSelectForm(this, attributes);
                        _triggerOutputsColumnsSelectForm.UpdateAttributes(attributes);
                        _triggerOutputsColumnsSelectForm.ShowDialog();
                    }
                }
            });
        }

        //public void PopulateTriggerOutputsFields()
        //{
        //    var allAttributes = _triggerOutputsColumnsSelectForm.SelectedColumns;

        //    WorkAsync(new WorkAsyncInfo
        //    {
        //        Message = "Showing trigger output columns",
        //        Work = (worker, args) =>
        //        {
        //            var options = new ParallelOptions
        //            {
        //                MaxDegreeOfParallelism = Environment.ProcessorCount * 4
        //            };

        //            Parallel.ForEach(FlowRuns, options, fr =>
        //            {
        //                fr.TriggerOutputs = fr.TriggerOutputs ?? GetTriggerOutputsForFlowRun(fr);
        //            });

        //            dgvFlowRuns.Invoke(new Action(() =>
        //            {
        //                dgvFlowRuns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

        //                ResetFlowRunsGridColumns();

        //                if (allAttributes.Count == 0) { return; }

        //                foreach (var field in allAttributes)
        //                {
        //                    var newColumn = new DataGridViewTextBoxColumn();
        //                    newColumn.Name = $"to_{field}";
        //                    newColumn.HeaderText = field;

        //                    dgvFlowRuns.Columns.Add(newColumn);
        //                }

        //                dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(FilteredFlowRuns ?? FlowRuns);
        //            }));
        //        },
        //        PostWorkCallBack = (args) =>
        //        {
        //            if (args.Error != null)
        //            {
        //                ShowErrorDialog(args.Error.InnerException);
        //            }
        //        }
        //    });
        //}

        private TriggerOutputsResponseDto GetTriggerOutputsForFlowRun(FlowRun flowRun)
        {
            var triggerOutputs = CachedTriggerOutputs.TryGetValue(flowRun.Id, out var to) ? to : flowRun.GetTriggerOutputs();

            return triggerOutputs;
        }

        private void ResetFlowRunsGridColumns()
        {
            dgvFlowRuns.Columns.Clear();
            dgvFlowRuns.Columns.AddRange(_initialColumns.ToArray());
        }

        private void dgvFlowRuns_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //ApplyTriggerOutputsFilters();
        }

        private void cbScheduled_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();

        }

        private void cbAutomated_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();
        }

        private void cbInstant_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();
        }

        private void cbManaged_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();
        }

        private void cbUnmanaged_CheckedChanged(object sender, EventArgs e)
        {
            FilterFlows();
        }
    }
}