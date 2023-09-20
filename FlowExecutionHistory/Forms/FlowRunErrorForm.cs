using System.Windows.Forms;
using Fic.XTB.FlowExecutionHistory.Models;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class FlowRunErrorForm : Form
    {
        public FlowRunErrorForm(FlowRunError flowRunError)
        {
            InitializeComponent();

            tbError.Text = flowRunError?.Message;
            tbDetails.Text = flowRunError?.Details;
        }
    }
}
