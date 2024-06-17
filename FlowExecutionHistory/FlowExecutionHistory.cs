using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
using Microsoft.Xrm.Sdk;
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
        public List<Solution> Solutions = new List<Solution>();

        private List<Color> _colors = new List<Color>();

        public DataGridView FlowRunsGrid;

        private TriggerOutputsFilterForm _triggerOutputsFilterForm;
        private TriggerOutputsColumnsSelectForm _triggerOutputsColumnsSelectForm;

        private readonly List<DataGridViewColumn> _initialColumns = new List<DataGridViewColumn>();

        private FlowClient flowClient;
        private DataverseClient dataverseClient;

        private OrganizationGeo Geo;

        public ApplicationInsights ApplicationInsights;

        public FlowExecutionHistory()
        {
            ApplicationInsights = new ApplicationInsights();

            InitializeComponent();

            FlowRunsGrid = dgvFlowRuns;

            foreach (DataGridViewColumn column in dgvFlowRuns.Columns)
            {
                _initialColumns.Add(column.Clone() as DataGridViewColumn);
            }
        }

        private void FlowExecutionHistory_Load(object sender, EventArgs e)
        {
            ApplicationInsights.LogEvent("Load");

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
                    cbBrowser.SelectedIndex = cbBrowser.Items.Count > 0 ? 0 : -1;
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
                    cbProfile.SelectedIndex = cbProfile.Items.Count > 0 ? 0 : -1;
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
                    cbBrowser.SelectedIndex = cbBrowser.Items.Count > 0 ? 0 : -1;
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
                     var entities = dataverseClient.GetFlows();

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
                             IsManaged = isManaged,
                             WorkflowId = f.Id
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

        public void GetFlowRuns()
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

            ApplicationInsights.LogEvent("GetFlowRuns");

            var includeTriggerOutputs = _triggerOutputsColumnsSelectForm?.SelectedColumns?.Count > 0;

            gbFlow.Enabled = false;
            gbFlowRuns.Enabled = false;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting flow runs",
                Work = (worker, args) =>
                {
                    _flowAccessToken = _flowAccessToken ?? ConnectionDetail.GetPowerAutomateAccessToken(Geo);

                    flowClient = flowClient ?? new FlowClient(ConnectionDetail.EnvironmentId, _flowAccessToken.access_token, Geo);

                    var options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount * 4
                    };

                    foreach (var f in selectedFlows)
                    {
                        var fr = flowClient.GetFlowRuns(f, status, dateFrom, dateTo, durationThreshold, includeTriggerOutputs);

                        f.FlowRuns = fr;
                    }

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

                        ShowHideTriggerOutputFilterButtons(selectedFlows.Count >= 1 && runs.Count > 0);

                        _triggerOutputsFilterForm = _triggerOutputsFilterForm ?? new TriggerOutputsFilterForm(this);
                        _triggerOutputsFilterForm.UpdateAttributes();

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

            Geo = detail.GetGeo();
            dataverseClient = new DataverseClient(newService);

            _colors = ColorHelper.GetAllColors(1000);
            ExecuteMethod(GetFlows);
            ExecuteMethod(GetSolutions);

            if (Settings == null || detail == null)
            {
                return;
            }

            LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
        }

        private void GetSolutions()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting solutions",
                Work = (worker, args) =>
                {
                    var solutions = dataverseClient.GetSolutions();

                    args.Result = solutions;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error.InnerException);
                    }
                    else
                    {
                        if (!(args.Result is List<Solution> solutions)) { return; }

                        var allSolutions = new Solution
                        {
                            Id = Guid.Empty,
                            Name = "<All Solutions>"
                        };

                        // add all solutions to the list
                        solutions.Insert(0, allSolutions);

                        cbSolutions.Items.Clear();
                        cbSolutions.DataSource = solutions;
                    }
                }
            });
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
            var draft = cbxFlowStatusDraft.Checked;

            var showAutomated = cbAutomated.Checked;
            var showScheduled = cbScheduled.Checked;
            var showInstant = cbInstant.Checked;

            var showManaged = cbManaged.Checked;
            var showUmmanaged = cbUnmanaged.Checked;

            var selectedSolution = (Solution)cbSolutions.SelectedItem;

            var filteredFlows = Flows
                .Where(f =>
                    f.Status == FlowStatus.Activated && activated
                    || f.Status == FlowStatus.Draft && draft)
                .Where(f =>
                    f.TriggerType.StartsWith(FlowTriggerType.Automated) && showAutomated
                    || f.TriggerType == FlowTriggerType.Scheduled && showScheduled
                    || f.TriggerType == FlowTriggerType.Instant && showInstant)
                .Where(f =>
                    f.IsManaged && showManaged
                    || !f.IsManaged && showUmmanaged)
                .Where(f =>
                    string.IsNullOrWhiteSpace(searchText) ||
                    f.Name.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
                .ToList();

            if (selectedSolution != null && selectedSolution.Id != Guid.Empty)
            {
                filteredFlows = filteredFlows
                    .Where(f => selectedSolution.FlowIds.Contains(f.WorkflowId))
                    .ToList();
            }

            gbFlow.Text = $"Flows ({filteredFlows.Count})";

            clbFlows.DataSource = filteredFlows;

            clbFlows.ItemCheck -= clbFlows_ItemCheck;

            for (var i = 0; i < clbFlows.Items.Count; i++)
            {
                var flow = (Flow)clbFlows.Items[i];

                clbFlows.SetItemChecked(i, flow.IsSelected);
            }

            clbFlows.ItemCheck += clbFlows_ItemCheck;

            gbFlow.Enabled = true;
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
                        var cellValue = cell.Value.ToString();

                        if (flowRun.CorrelationId == cellValue || flowRun.FriendlyCorrelationId == cellValue)
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
                    _flowAccessToken = _flowAccessToken ?? ConnectionDetail.GetPowerAutomateAccessToken(Geo);

                    flowClient = flowClient ?? new FlowClient(ConnectionDetail.EnvironmentId, _flowAccessToken.access_token, Geo);
                    var errorDetails = flowClient.GetFlowRunErrorDetails(flowRun);

                    flowRun.Error = new FlowRunError
                    {
                        Message = errorDetails.errorSubject,
                        Details = errorDetails.errorDescription
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
                case "FlowRunCorrelationId":
                    e.Value = Settings.ShowFriendlyCorrelationIds ? flowRun.FriendlyCorrelationId : flowRun.CorrelationId;
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
            var audienceUrl = FlowEndpointHelper.GetAudienceUrl(Geo);
            var scopes = new[] { $"{audienceUrl}/.default" };

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

            var filenName = $"runs-history-{DateTime.Now:yyyyMMddHHmmss}.csv";
            saveFileDialog.FileName = filenName;

            if (saveFileDialog.ShowDialog() != DialogResult.OK) { return; }

            ApplicationInsights.LogEvent("ExportToCsv");

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Exporting CSV",
                Work = (worker, args) =>
                {
                    CsvService.ExportToCsv(FilteredFlowRuns, saveFileDialog.FileName);
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

            var filenName = $"runs-history-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            saveFileDialog.FileName = filenName;

            if (saveFileDialog.ShowDialog() != DialogResult.OK) { return; }

            ApplicationInsights.LogEvent("ExportToExcel");

            WorkAsync(new WorkAsyncInfo("Creating Excel file...",
               (eventargs) =>
               {
                   ExcelService.ExportToExcelNew(FilteredFlowRuns, saveFileDialog.FileName);
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

                        ShowHideErrorColumn(Settings.ShowErrorColumn);

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
                _triggerOutputsFilterForm.UpdateAttributes();

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
            _triggerOutputsFilterForm.UpdateAttributes();
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

                        flowRun.TriggerOutputs = flowRun.TriggerOutputs ?? flowClient.GetTriggerOutputsForFlowRun(flowRun);
                    });

                    var allAttributes = FlowRuns
                        .Where(fr => fr.TriggerOutputs != null)
                        .Where(fr => fr.TriggerOutputs.Body != null)
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

        private void cbSolutions_SelectedIndexChanged(object sender, EventArgs e)
        {
            gbFlow.Enabled = false;

            if (cbSolutions.SelectedIndex == -1) { return; }

            var selectedSolution = (Solution)cbSolutions.SelectedItem;

            if (selectedSolution.FlowIds == null)
            {
                var flowIds = dataverseClient.GetSolutionComponents(selectedSolution.Id);
                selectedSolution.FlowIds = flowIds;
            }

            FilterFlows();
        }

        public void ShowHideErrorColumn(bool show)
        {
            dgvFlowRuns.Invoke(new Action(() =>
            {
                var errorColumn = dgvFlowRuns.Columns["FlowRunError"];
                var status = cbxStatus.Text;

                errorColumn.Visible = show && status != Enums.FlowRunStatus.Succeeded;
            }));
        }

        private void tsbDonate_Click(object sender, EventArgs e)
        {
            ApplicationInsights.LogEvent("Donate");

            var url = "https://www.buymeacoffee.com/dynamicsninja";

            Process.Start(url);
        }
    }
}