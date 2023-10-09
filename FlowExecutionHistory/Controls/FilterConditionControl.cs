using Fic.XTB.FlowExecutionHistory.Enums;
using Fic.XTB.FlowExecutionHistory.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class FilterConditionControl : UserControl
{
    private TableLayoutPanel tableLayoutPanel;
    private ComboBox attributeComboBox;
    private ComboBox operatorComboBox;
    private TextBox valueTextBox;
    private PictureBox removePictureBox;
    public int RowIndex { get; set; }

    public event EventHandler<EventArgs> RemoveButtonClicked;

    public FilterCondition FilterCondition
    {
        get
        {
            return new FilterCondition(
                (string)attributeComboBox.SelectedItem,
                (OutputTriggerFilter)operatorComboBox.SelectedItem,
                valueTextBox.Text
            );
        }
    }

    public FilterConditionControl(List<string> attributes, List<OutputTriggerFilter> operators, Image img)
    {
        Padding = new Padding(0, 0, 0, 0);
        Margin = new Padding(0, 0, 0, 0);
        // Initialize controls
        tableLayoutPanel = new TableLayoutPanel();
        attributeComboBox = new ComboBox();
        operatorComboBox = new ComboBox();
        valueTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
        };

        removePictureBox = new PictureBox
        {
            Image = img,
            Dock = DockStyle.Fill,
            Size = new Size(26, 26),
        };

        removePictureBox.Click += (sender, e) => removeButton_Click(sender, e);

        AutoSize = true;

        // Set up attributeComboBox
        attributeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        attributeComboBox.DataSource = new List<string>(attributes);

        // Set up operatorComboBox
        operatorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        operatorComboBox.DataSource = new List<OutputTriggerFilter>(operators);

        // Set up TableLayoutPanel
        tableLayoutPanel.AutoSize = true;
        tableLayoutPanel.Dock = DockStyle.Fill;
        tableLayoutPanel.ColumnCount = 4;


        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));

        // Add controls to the TableLayoutPanel
        tableLayoutPanel.Controls.Add(attributeComboBox, 0, 0);
        tableLayoutPanel.Controls.Add(operatorComboBox, 1, 0);
        tableLayoutPanel.Controls.Add(valueTextBox, 2, 0);
        tableLayoutPanel.Controls.Add(removePictureBox, 3, 0);

        // Add TableLayoutPanel to this custom control
        this.Controls.Add(tableLayoutPanel);
    }

    private void removeButton_Click(object sender, EventArgs e)
    {
        OnRemoveButtonClicked(EventArgs.Empty);
    }

    protected virtual void OnRemoveButtonClicked(EventArgs e)
    {
        RemoveButtonClicked?.Invoke(this, e);
        Dispose();
    }
}
