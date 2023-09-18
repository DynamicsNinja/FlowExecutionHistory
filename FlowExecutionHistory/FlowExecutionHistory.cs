using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fic.XTB.FlowExecutionHistory.Extensions;
using Fic.XTB.FlowExecutionHistory.Forms;
using Fic.XTB.FlowExecutionHistory.Helpers;
using Fic.XTB.FlowExecutionHistory.Models;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;
using Fic.XTB.FlowExecutionHistory.Services;
using McTools.Xrm.Connection;
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

        private Settings _settings;

        private AccessTokenResponse _flowAccessToken;

        private List<FlowRun> _flowRuns = new List<FlowRun>();
        private List<Flow> _flows = new List<Flow>();

        public FlowExecutionHistory()
        {
            InitializeComponent();
        }

        private void FlowExecutionHistory_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out _settings))
            {
                _settings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");

                cbBrowser.SelectedIndex = 0;
            }
            else
            {
                LogInfo("Settings found and loaded");

                if (_settings.Browser != null && _settings.BrowserProfile != null)
                {
                    for (var i = 0; i < cbBrowser.Items.Count; i++)
                    {
                        var browser = (Browser)cbBrowser.Items[i];

                        if (browser.Type != _settings.Browser.Type) { continue; }

                        cbBrowser.SelectedIndex = i;

                        break;
                    }
                }
                else
                {
                    cbBrowser.SelectedIndex = 0;
                }
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
                        Name = (string)f["name"]
                    }).OrderBy(f => f.Name).ToList();
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (!(args.Result is List<Flow> flows))
                    {
                        return;
                    }

                    _flows = flows;

                    clbFlows.Items.AddRange(_flows.ToArray());

                    gbFlow.Text = $"Flows ({_flows.Count})";
                }
            });
        }

        private void GetFlowRuns()
        {
            var selectedFlows = _flows.Where(f => f.IsSelected).ToList();

            var dateFrom = dtpDateFrom.Value;
            var dateTo = dtpDateTo.Value;
            var status = cbxStatus.Text;

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

                    var flowRuns = new List<FlowRun>();

                    var options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 8
                    };

                    Parallel.ForEach(selectedFlows, options, f =>
                    {
                        var fr = flowClient.GetFlowRuns(f, status, dateFrom, dateTo);
                        flowRuns.AddRange(fr);
                    });

                    args.Result = flowRuns;
                },
                PostWorkCallBack = (args) =>
                {
                    gbFlow.Enabled = true;
                    gbFlowRuns.Enabled = true;

                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (!(args.Result is List<FlowRun> runs))
                    {
                        return;
                    }


                    _flowRuns = runs;

                    dgvFlowRuns.Columns["FlowRunFLow"].Visible = selectedFlows.Count > 1;
                    dgvFlowRuns.DataSource = new SortableBindingList<FlowRun>(_flowRuns);

                    gbFlowRuns.Text = $@"Flow Runs ({_flowRuns.Count})";
                }
            });
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail,
            string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            ExecuteMethod(GetFlows);


            if (_settings == null || detail == null)
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
            if (e.RowIndex < 0) { return; }

            var flowRun = (FlowRun)dgvFlowRuns.Rows[e.RowIndex].DataBoundItem;

            switch (dgvFlowRuns.Columns[e.ColumnIndex].Name)
            {
                case "FlowRunUrl":
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo(_settings.Browser.Executable)
                    {
                        Arguments = flowRun.Url
                    };

                    switch (_settings.Browser.Type)
                    {
                        case BrowserEnum.Chrome:
                        case BrowserEnum.Edge:
                            process.StartInfo.Arguments += $" --profile-directory=\"{_settings.BrowserProfile.Path}\"";
                            break;
                        case BrowserEnum.Firefox:
                            process.StartInfo.Arguments += $" -P \"{_settings.BrowserProfile.Path}\"";
                            break;
                    }

                    process.Start();
                    break;
                case "FlowRunStatus":
                    GetFlowRunErrorDetails(flowRun);
                    break;
            }
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

                    args.Result = errorDetails;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (!(args.Result is FlowRunRemediationResponse errorDetails))
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

                    dgvFlowRuns.Cursor = flowRun.Status == "Failed" ? Cursors.Hand : Cursors.Default;
                    break;
                default:
                    dgvFlowRuns.Cursor = Cursors.Default;
                    break;
            }
        }

        private void clbFlows_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var selectedFlow = (Flow)clbFlows.SelectedItem;
            var checkedValue = e.NewValue == CheckState.Checked ? true : false;

            selectedFlow.IsSelected = checkedValue;

            GetFlowRuns();
        }

        private void dgvFlowRuns_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            switch (dgvFlowRuns.Columns[e.ColumnIndex].Name)
            {
                case "FlowRunStatus":
                    {
                        var flowRun = (FlowRun)dgvFlowRuns.Rows[e.RowIndex].DataBoundItem;

                        if (flowRun == null)
                        {
                            return;
                        }

                        e.CellStyle.BackColor = flowRun.Status == "Succeeded" ? Color.Green : Color.Red;
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    }
                case "FlowRunFlow":
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

            _settings.Browser = selectedBrowser;

            cbProfile.Items.Clear();
            cbProfile.Items.AddRange(selectedBrowser.Profiles.ToArray());

            var profileIndex = selectedBrowser.Profiles.FindIndex(bp => bp.Path.Equals(_settings.BrowserProfile?.Path));

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

            _settings.BrowserProfile = selectedBrowserProfile;

            SaveSettings();
        }

        private void FlowExecutionHistory_OnCloseTool(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            SettingsManager.Instance.Save(GetType(), _settings);
        }

        private void FlowExecutionHistory_ConnectionUpdated(object sender, ConnectionUpdatedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ConnectionDetail.S2SClientSecret))
            {
                MessageBox.Show(
                    "This tool is using server to server connection to get Power Automate flow runs. Please use connection that is authenticated with client ID and client secret.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            gbFlowRuns.Enabled = true;
            gbFlow.Enabled = true;

            cbxStatus.SelectedIndex = 0;

            var today = DateTime.Now;
            var fromDateTime = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            var toDateTime = fromDateTime.AddDays(1);

            dtpDateFrom.Value = fromDateTime;
            dtpDateTo.Value = toDateTime;

            var browsers = BrowserLoader.GetBrowsers();
            cbBrowser.Items.AddRange(browsers.ToArray());
        }
    }
}