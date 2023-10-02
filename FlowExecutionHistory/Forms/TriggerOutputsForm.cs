using System.Collections.Generic;
using System.Windows.Forms;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class TriggerOutputsForm : Form
    {
        private readonly TriggerOutputsResponseDto _triggerOutputs;
        public TriggerOutputsForm(TriggerOutputsResponseDto triggerOutputs)
        {
            InitializeComponent();

            _triggerOutputs = triggerOutputs;

            if (triggerOutputs.Body != null)
            {
                foreach (var kvp in triggerOutputs.Body)
                {
                    dgvTriggerOutputsBody.Rows.Add(kvp.Key, kvp.Value);
                }
            }


            if (triggerOutputs.Headers != null)
            {
                foreach (var kvp in triggerOutputs.Headers)
                {
                    dgvTriggerOutputsHeaders.Rows.Add(kvp.Key, kvp.Value);
                }
            }

        }

        private void tbSearchHeaders_TextChanged(object sender, System.EventArgs e)
        {
            var searchTerm = tbSearchHeaders.Text;
            FilterGridValues(searchTerm, _triggerOutputs.Headers, dgvTriggerOutputsHeaders);
        }

        private void tbSearchBody_TextChanged(object sender, System.EventArgs e)
        {
            var searchTerm = tbSearchBody.Text.ToLower();
            FilterGridValues(searchTerm, _triggerOutputs.Body, dgvTriggerOutputsBody);
        }

        private void FilterGridValues(string searchTerm, Dictionary<string, object> dict, DataGridView dgv)
        {

            dgv.Rows.Clear();

            foreach (var kvp in dict)
            {
                if (string.IsNullOrWhiteSpace(searchTerm)
                    || kvp.Key.ToLower().Contains(searchTerm)
                        || kvp.Value.ToString().ToLower().Contains(searchTerm))
                {
                    dgv.Rows.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
