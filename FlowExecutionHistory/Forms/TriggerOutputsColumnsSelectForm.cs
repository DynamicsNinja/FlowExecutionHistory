using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class TriggerOutputsColumnsSelectForm : Form
    {
        private readonly FlowExecutionHistory _frc;

        public List<string> SelectedColumns
        {
            get
            {
                var selectedColumns = new List<string>();

                foreach (var item in clbColumns.CheckedItems)
                {
                    selectedColumns.Add(item.ToString());
                }

                return selectedColumns;
            }
            set
            {
                foreach (var item in clbColumns.Items)
                {
                    clbColumns.SetItemChecked(clbColumns.Items.IndexOf(item), value.Contains(item));
                }
            }
        }

        public TriggerOutputsColumnsSelectForm(FlowExecutionHistory fec, List<string> columns)
        {
            InitializeComponent();

            _frc = fec;


            foreach (var column in columns.OrderBy(c => c))
            {
                clbColumns.Items.Add(column);
            }
        }

        public void UpdateAttributes(List<string> columns)
        {
            var checkedAttributes = clbColumns.CheckedItems
                .OfType<string>().ToList()
                .Where(columns.Contains).ToList();

            clbColumns.Items.Clear();

            foreach (var column in columns.OrderBy(c => c))
            {
                clbColumns.Items.Add(column);

                if (!checkedAttributes.Contains(column)) { continue; }

                clbColumns.SetItemChecked(clbColumns.Items.Count - 1, true);
            }
        }

        private void btnSelect_Click(object sender, System.EventArgs e)
        {
            var selectedColumns = new List<string>();

            foreach (var item in clbColumns.CheckedItems)
            {
                selectedColumns.Add(item.ToString());
            }

            SelectedColumns = selectedColumns;

            _frc.ApplyTriggerOutputsFilters();

            Close();
        }

        private void cbSelectAll_CheckedChanged(object sender, System.EventArgs e)
        {
            var selected = cbSelectAll.Checked;

            for (var i = 0; i < clbColumns.Items.Count; i++)
            {
                clbColumns.SetItemChecked(i, selected);
            }
        }
    }
}
