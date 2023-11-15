using System;
using System.Windows.Forms;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly FlowExecutionHistory _fec;
        public SettingsForm(FlowExecutionHistory fec)
        {
            _fec = fec;

            InitializeComponent();

            cbFlowColors.Checked = _fec.Settings.UseFlowColors;
            cbShowFreindlyCorrIds.Checked = _fec.Settings.ShowFriendlyCorrelationIds;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _fec.Settings.UseFlowColors = cbFlowColors.Checked;
            _fec.Settings.ShowFriendlyCorrelationIds = cbShowFreindlyCorrIds.Checked;

            _fec.SaveSettings();

            _fec.FlowRunsGrid.Invalidate();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
