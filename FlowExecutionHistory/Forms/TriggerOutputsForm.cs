using System.Windows.Forms;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class TriggerOutputsForm : Form
    {
        public TriggerOutputsForm(TriggerOutputsResponseDto triggerOutputs)
        {
            InitializeComponent();

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
    }
}
