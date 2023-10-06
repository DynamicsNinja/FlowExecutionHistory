using Fic.XTB.FlowExecutionHistory.Enums;
using Fic.XTB.FlowExecutionHistory.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class TriggerOutputsFilterForm : Form
    {
        private FlowExecutionHistory _frc;
        public TriggerOutputsFilterForm(FlowExecutionHistory frc)
        {
            InitializeComponent();

            _frc = frc;

            var triggerOutput = frc.FlowRuns.FirstOrDefault()?.GetTriggerOutputs();

            foreach (var key in triggerOutput.Body.Keys.OrderBy(k => k))
            {
                cbBodyKeys.Items.Add(key);
            }

            cbOperator.DataSource = Enum.GetValues(typeof(OutputTriggerFilter));

            cbBodyKeys.SelectedIndex = 0;
            cbOperator.SelectedIndex = 0;
        }

        private void btnFIlter_Click(object sender, EventArgs e)
        {
            var filterAttribute = (string)cbBodyKeys.SelectedItem;
            var filterOperator = (OutputTriggerFilter)cbOperator.SelectedItem;
            var filterValue = tbValue.Text.ToLower();

            var filterCondition = new FilterCondition
            {
                Attribute = filterAttribute,
                Operator = filterOperator,
                Value = filterValue
            };

            _frc.FilterRunsByTriggerOutputs(filterCondition);

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
