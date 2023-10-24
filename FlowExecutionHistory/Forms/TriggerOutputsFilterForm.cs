using Fic.XTB.FlowExecutionHistory.Enums;
using Fic.XTB.FlowExecutionHistory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Fic.XTB.FlowExecutionHistory.Forms
{
    public partial class TriggerOutputsFilterForm : Form
    {
        private FlowExecutionHistory _frc;

        private List<string> _attributes;
        private List<OutputTriggerFilter> _operators;

        public ConditionGroup ConditionGroup { get; set; }

        public TriggerOutputsFilterForm(FlowExecutionHistory frc)
        {
            InitializeComponent();

            _frc = frc;

            var allAttributes = new List<string>();

            foreach (var flow in frc.Flows.Where(f => f.IsSelected))
            {
                var triggerOutput = flow.FlowRuns.FirstOrDefault()?.TriggerOutputs ?? flow.FlowRuns.FirstOrDefault()?.GetTriggerOutputs();

                if (triggerOutput == null) { continue; }

                var attributes = triggerOutput.Body.Keys.ToList();

                allAttributes.AddRange(attributes);
            }

            _attributes = allAttributes.Distinct().OrderBy(k => k).ToList();
            _operators = Enum.GetValues(typeof(OutputTriggerFilter)).Cast<OutputTriggerFilter>().ToList();

            cbGroupOperator.DataSource = Enum.GetValues(typeof(GroupOperator)).Cast<GroupOperator>().ToList();

            tableLayoutPanel2.RowStyles.Clear();
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            AddFilterConditionControl();
        }

        private void btnFIlter_Click(object sender, EventArgs e)
        {
            var filterConditions = GetAllFilterConditions();
            var groupOperator = (GroupOperator)cbGroupOperator.SelectedItem;

            var conditionGroup = new ConditionGroup
            {
                GroupOperator = groupOperator,
                FilterConditions = filterConditions
            };

            ConditionGroup = conditionGroup;

            _frc.ApplyTriggerOutputsFilters();

            Close();
        }

        private List<FilterCondition> GetAllFilterConditions()
        {
            var filterConditions = new List<FilterCondition>();

            foreach (var control in tableLayoutPanel2.Controls)
            {
                if (control is FilterConditionControl filterConditionControl)
                {
                    filterConditions.Add(filterConditionControl.FilterCondition);
                }
            }

            return filterConditions;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AddFilterConditionControl()
        {
            var customControl = new FilterConditionControl(_attributes, _operators, Properties.Resources.delete);
            customControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            customControl.RowIndex = 0;
            customControl.RemoveButtonClicked += (sender, args) => OnRemoveButtonClicked((FilterConditionControl)sender);

            tableLayoutPanel2.Controls.Add(customControl, 0, tableLayoutPanel2.RowCount);
            tableLayoutPanel2.RowCount++;

            UpdateControlRowIndices();
        }

        private void UpdateControlRowIndices()
        {
            for (int i = 0; i < tableLayoutPanel2.Controls.Count; i++)
            {
                (tableLayoutPanel2.Controls[i] as FilterConditionControl).RowIndex = i;
            }
        }

        private void OnRemoveButtonClicked(FilterConditionControl control)
        {
            int row = control.RowIndex;

            tableLayoutPanel2.Controls.RemoveAt(row);
            tableLayoutPanel2.RowCount = tableLayoutPanel2.RowCount - 1;

            tableLayoutPanel2.PerformLayout();


            UpdateControlRowIndices();
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            AddFilterConditionControl();
        }

        private void tableLayoutPanel2_Layout(object sender, LayoutEventArgs e)
        {
            if (tableLayoutPanel2.HorizontalScroll.Visible)
            {
                // Add padding to the right side equal to the width of the scrollbar
                tableLayoutPanel2.Padding = new Padding(0, 0, SystemInformation.VerticalScrollBarWidth, 0);
            }
            else
            {
                // Reset padding if the scrollbar is not visible
                tableLayoutPanel2.Padding = new Padding(0);
            }
        }
    }
}
