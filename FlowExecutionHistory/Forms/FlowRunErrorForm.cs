using System.Windows.Forms;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class FlowRunErrorForm : Form
    {
        public FlowRunErrorForm(FlowRunRemediationResponse remediation)
        {
            InitializeComponent();

            tbDetails.Text = remediation.errorDescription;
            tbError.Text = remediation.errorSubject;
        }
    }
}
