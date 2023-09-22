namespace Fic.XTB.FlowExecutionHistory
{
    partial class FlowExecutionHistory
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlowExecutionHistory));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsbExport = new System.Windows.Forms.ToolStripSplitButton();
            this.tsbExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbCsv = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbConnectFlowApi = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cbBrowser = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cbProfile = new System.Windows.Forms.ToolStripComboBox();
            this.tsbSettings = new System.Windows.Forms.ToolStripButton();
            this.dgvFlowRuns = new System.Windows.Forms.DataGridView();
            this.FlowRunId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowRunCorrelationId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowRunFlow = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowRunStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowRunStartDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowRunDurationInSeconds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowRunUrl = new System.Windows.Forms.DataGridViewImageColumn();
            this.bsFlowRuns = new System.Windows.Forms.BindingSource(this.components);
            this.cbxStatus = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tbSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.gbFlow = new System.Windows.Forms.GroupBox();
            this.clbFlows = new System.Windows.Forms.CheckedListBox();
            this.gbFlowRuns = new System.Windows.Forms.GroupBox();
            this.tbDurationThreshold = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpDateTo = new System.Windows.Forms.DateTimePicker();
            this.lblDateTo = new System.Windows.Forms.Label();
            this.dtpDateFrom = new System.Windows.Forms.DateTimePicker();
            this.lblDateFrom = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.toolStripMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFlowRuns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsFlowRuns)).BeginInit();
            this.gbFlow.SuspendLayout();
            this.gbFlowRuns.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbRefresh,
            this.tsbExport,
            this.tsbConnectFlowApi,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.cbBrowser,
            this.toolStripLabel2,
            this.cbProfile,
            this.tsbSettings});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1694, 38);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbClose.Image = ((System.Drawing.Image)(resources.GetObject("tsbClose.Image")));
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(34, 33);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tsbRefresh.Image")));
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Size = new System.Drawing.Size(98, 33);
            this.tsbRefresh.Text = "Refresh";
            this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
            // 
            // tsbExport
            // 
            this.tsbExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbExcel,
            this.tsbCsv});
            this.tsbExport.Image = ((System.Drawing.Image)(resources.GetObject("tsbExport.Image")));
            this.tsbExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExport.Name = "tsbExport";
            this.tsbExport.Size = new System.Drawing.Size(108, 33);
            this.tsbExport.Text = "Export";
            this.tsbExport.ButtonClick += new System.EventHandler(this.tsbExport_ButtonClick);
            // 
            // tsbExcel
            // 
            this.tsbExcel.Image = ((System.Drawing.Image)(resources.GetObject("tsbExcel.Image")));
            this.tsbExcel.Name = "tsbExcel";
            this.tsbExcel.Size = new System.Drawing.Size(152, 34);
            this.tsbExcel.Text = "Excel";
            this.tsbExcel.Click += new System.EventHandler(this.tsbExcel_Click);
            // 
            // tsbCsv
            // 
            this.tsbCsv.Image = ((System.Drawing.Image)(resources.GetObject("tsbCsv.Image")));
            this.tsbCsv.Name = "tsbCsv";
            this.tsbCsv.Size = new System.Drawing.Size(152, 34);
            this.tsbCsv.Text = "CSV";
            this.tsbCsv.Click += new System.EventHandler(this.tsbCsv_Click);
            // 
            // tsbConnectFlowApi
            // 
            this.tsbConnectFlowApi.Image = ((System.Drawing.Image)(resources.GetObject("tsbConnectFlowApi.Image")));
            this.tsbConnectFlowApi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbConnectFlowApi.Name = "tsbConnectFlowApi";
            this.tsbConnectFlowApi.Size = new System.Drawing.Size(296, 33);
            this.tsbConnectFlowApi.Text = "Connect to Power Automate API";
            this.tsbConnectFlowApi.Click += new System.EventHandler(this.tsbConnectPowerAutomateApi_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(75, 33);
            this.toolStripLabel1.Text = "Browser";
            this.toolStripLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbBrowser
            // 
            this.cbBrowser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBrowser.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbBrowser.Name = "cbBrowser";
            this.cbBrowser.Size = new System.Drawing.Size(121, 38);
            this.cbBrowser.SelectedIndexChanged += new System.EventHandler(this.cbBrowser_SelectedIndexChanged);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(130, 33);
            this.toolStripLabel2.Text = "Browser Profile";
            // 
            // cbProfile
            // 
            this.cbProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProfile.DropDownWidth = 150;
            this.cbProfile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbProfile.Name = "cbProfile";
            this.cbProfile.Size = new System.Drawing.Size(200, 38);
            this.cbProfile.SelectedIndexChanged += new System.EventHandler(this.cbProfile_SelectedIndexChanged);
            // 
            // tsbSettings
            // 
            this.tsbSettings.Image = ((System.Drawing.Image)(resources.GetObject("tsbSettings.Image")));
            this.tsbSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSettings.Name = "tsbSettings";
            this.tsbSettings.Size = new System.Drawing.Size(104, 33);
            this.tsbSettings.Text = "Settings";
            this.tsbSettings.Click += new System.EventHandler(this.tsbSettings_Click);
            // 
            // dgvFlowRuns
            // 
            this.dgvFlowRuns.AllowUserToAddRows = false;
            this.dgvFlowRuns.AllowUserToDeleteRows = false;
            this.dgvFlowRuns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFlowRuns.AutoGenerateColumns = false;
            this.dgvFlowRuns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvFlowRuns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFlowRuns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FlowRunId,
            this.FlowRunCorrelationId,
            this.FlowRunFlow,
            this.FlowRunStatus,
            this.FlowRunStartDate,
            this.FlowRunDurationInSeconds,
            this.FlowRunUrl});
            this.dgvFlowRuns.DataSource = this.bsFlowRuns;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(5);
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFlowRuns.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvFlowRuns.Location = new System.Drawing.Point(6, 73);
            this.dgvFlowRuns.Name = "dgvFlowRuns";
            this.dgvFlowRuns.ReadOnly = true;
            this.dgvFlowRuns.RowHeadersWidth = 62;
            this.dgvFlowRuns.RowTemplate.Height = 28;
            this.dgvFlowRuns.Size = new System.Drawing.Size(1108, 688);
            this.dgvFlowRuns.TabIndex = 6;
            this.dgvFlowRuns.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFlowRuns_CellClick);
            this.dgvFlowRuns.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvFlowRuns_CellFormatting);
            this.dgvFlowRuns.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFlowRuns_CellMouseEnter);
            // 
            // FlowRunId
            // 
            this.FlowRunId.DataPropertyName = "Id";
            this.FlowRunId.HeaderText = "Id";
            this.FlowRunId.MinimumWidth = 8;
            this.FlowRunId.Name = "FlowRunId";
            this.FlowRunId.ReadOnly = true;
            this.FlowRunId.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.FlowRunId.Visible = false;
            // 
            // FlowRunCorrelationId
            // 
            this.FlowRunCorrelationId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FlowRunCorrelationId.DataPropertyName = "CorrelationId";
            this.FlowRunCorrelationId.HeaderText = "Correlation Id";
            this.FlowRunCorrelationId.MinimumWidth = 8;
            this.FlowRunCorrelationId.Name = "FlowRunCorrelationId";
            this.FlowRunCorrelationId.ReadOnly = true;
            // 
            // FlowRunFlow
            // 
            this.FlowRunFlow.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FlowRunFlow.DataPropertyName = "Flow";
            this.FlowRunFlow.HeaderText = "Flow";
            this.FlowRunFlow.MinimumWidth = 8;
            this.FlowRunFlow.Name = "FlowRunFlow";
            this.FlowRunFlow.ReadOnly = true;
            this.FlowRunFlow.Visible = false;
            // 
            // FlowRunStatus
            // 
            this.FlowRunStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.FlowRunStatus.DataPropertyName = "Status";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.FlowRunStatus.DefaultCellStyle = dataGridViewCellStyle1;
            this.FlowRunStatus.HeaderText = "Status";
            this.FlowRunStatus.MinimumWidth = 8;
            this.FlowRunStatus.Name = "FlowRunStatus";
            this.FlowRunStatus.ReadOnly = true;
            this.FlowRunStatus.Width = 102;
            // 
            // FlowRunStartDate
            // 
            this.FlowRunStartDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.FlowRunStartDate.DataPropertyName = "StartDate";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "dd.MM.yyyy HH:mm:ss";
            dataGridViewCellStyle2.NullValue = null;
            this.FlowRunStartDate.DefaultCellStyle = dataGridViewCellStyle2;
            this.FlowRunStartDate.HeaderText = "Start Date";
            this.FlowRunStartDate.MinimumWidth = 8;
            this.FlowRunStartDate.Name = "FlowRunStartDate";
            this.FlowRunStartDate.ReadOnly = true;
            this.FlowRunStartDate.Width = 129;
            // 
            // FlowRunDurationInSeconds
            // 
            this.FlowRunDurationInSeconds.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.FlowRunDurationInSeconds.DataPropertyName = "DurationInSeconds";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.FlowRunDurationInSeconds.DefaultCellStyle = dataGridViewCellStyle3;
            this.FlowRunDurationInSeconds.HeaderText = "Duration (s)";
            this.FlowRunDurationInSeconds.MinimumWidth = 8;
            this.FlowRunDurationInSeconds.Name = "FlowRunDurationInSeconds";
            this.FlowRunDurationInSeconds.ReadOnly = true;
            this.FlowRunDurationInSeconds.Width = 138;
            // 
            // FlowRunUrl
            // 
            this.FlowRunUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.FlowRunUrl.HeaderText = "Details";
            this.FlowRunUrl.Image = ((System.Drawing.Image)(resources.GetObject("FlowRunUrl.Image")));
            this.FlowRunUrl.MinimumWidth = 24;
            this.FlowRunUrl.Name = "FlowRunUrl";
            this.FlowRunUrl.ReadOnly = true;
            this.FlowRunUrl.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.FlowRunUrl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.FlowRunUrl.Width = 104;
            // 
            // cbxStatus
            // 
            this.cbxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxStatus.FormattingEnabled = true;
            this.cbxStatus.Items.AddRange(new object[] {
            "All",
            "Succeeded",
            "Failed"});
            this.cbxStatus.Location = new System.Drawing.Point(68, 35);
            this.cbxStatus.Name = "cbxStatus";
            this.cbxStatus.Size = new System.Drawing.Size(194, 28);
            this.cbxStatus.TabIndex = 7;
            this.cbxStatus.SelectedIndexChanged += new System.EventHandler(this.cbxStatus_SelectedIndexChanged);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(6, 37);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(56, 20);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Status";
            // 
            // tbSearch
            // 
            this.tbSearch.Location = new System.Drawing.Point(72, 39);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(486, 26);
            this.tbSearch.TabIndex = 10;
            this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(6, 42);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(60, 20);
            this.lblSearch.TabIndex = 11;
            this.lblSearch.Text = "Search";
            // 
            // gbFlow
            // 
            this.gbFlow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbFlow.Controls.Add(this.clbFlows);
            this.gbFlow.Controls.Add(this.lblSearch);
            this.gbFlow.Controls.Add(this.tbSearch);
            this.gbFlow.Enabled = false;
            this.gbFlow.Location = new System.Drawing.Point(7, 49);
            this.gbFlow.Name = "gbFlow";
            this.gbFlow.Size = new System.Drawing.Size(564, 767);
            this.gbFlow.TabIndex = 12;
            this.gbFlow.TabStop = false;
            this.gbFlow.Text = "Flows";
            // 
            // clbFlows
            // 
            this.clbFlows.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbFlows.CheckOnClick = true;
            this.clbFlows.FormattingEnabled = true;
            this.clbFlows.Location = new System.Drawing.Point(6, 73);
            this.clbFlows.Name = "clbFlows";
            this.clbFlows.Size = new System.Drawing.Size(552, 671);
            this.clbFlows.TabIndex = 12;
            this.clbFlows.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbFlows_ItemCheck);
            // 
            // gbFlowRuns
            // 
            this.gbFlowRuns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFlowRuns.Controls.Add(this.tbDurationThreshold);
            this.gbFlowRuns.Controls.Add(this.label1);
            this.gbFlowRuns.Controls.Add(this.dtpDateTo);
            this.gbFlowRuns.Controls.Add(this.lblDateTo);
            this.gbFlowRuns.Controls.Add(this.dtpDateFrom);
            this.gbFlowRuns.Controls.Add(this.lblDateFrom);
            this.gbFlowRuns.Controls.Add(this.dgvFlowRuns);
            this.gbFlowRuns.Controls.Add(this.cbxStatus);
            this.gbFlowRuns.Controls.Add(this.lblStatus);
            this.gbFlowRuns.Enabled = false;
            this.gbFlowRuns.Location = new System.Drawing.Point(577, 49);
            this.gbFlowRuns.Name = "gbFlowRuns";
            this.gbFlowRuns.Size = new System.Drawing.Size(1114, 767);
            this.gbFlowRuns.TabIndex = 13;
            this.gbFlowRuns.TabStop = false;
            this.gbFlowRuns.Text = "Flow Runs";
            // 
            // tbDurationThreshold
            // 
            this.tbDurationThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDurationThreshold.Location = new System.Drawing.Point(414, 35);
            this.tbDurationThreshold.Name = "tbDurationThreshold";
            this.tbDurationThreshold.Size = new System.Drawing.Size(100, 26);
            this.tbDurationThreshold.TabIndex = 14;
            this.tbDurationThreshold.Text = "0";
            this.tbDurationThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tbDurationThreshold.Leave += new System.EventHandler(this.tbDurationThreshold_Leave);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(268, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 20);
            this.label1.TabIndex = 13;
            this.label1.Text = "Duration threshold";
            // 
            // dtpDateTo
            // 
            this.dtpDateTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpDateTo.CustomFormat = "dd.MM.yyyy HH:mm tt";
            this.dtpDateTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDateTo.Location = new System.Drawing.Point(908, 36);
            this.dtpDateTo.Name = "dtpDateTo";
            this.dtpDateTo.Size = new System.Drawing.Size(200, 26);
            this.dtpDateTo.TabIndex = 12;
            this.dtpDateTo.ValueChanged += new System.EventHandler(this.dtpDateTo_ValueChanged);
            // 
            // lblDateTo
            // 
            this.lblDateTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDateTo.AutoSize = true;
            this.lblDateTo.Location = new System.Drawing.Point(817, 41);
            this.lblDateTo.Name = "lblDateTo";
            this.lblDateTo.Size = new System.Drawing.Size(66, 20);
            this.lblDateTo.TabIndex = 11;
            this.lblDateTo.Text = "Date To";
            // 
            // dtpDateFrom
            // 
            this.dtpDateFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpDateFrom.CustomFormat = "dd.MM.yyyy HH:mm tt";
            this.dtpDateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDateFrom.Location = new System.Drawing.Point(611, 37);
            this.dtpDateFrom.Name = "dtpDateFrom";
            this.dtpDateFrom.Size = new System.Drawing.Size(200, 26);
            this.dtpDateFrom.TabIndex = 10;
            this.dtpDateFrom.ValueChanged += new System.EventHandler(this.dtpDate_ValueChanged);
            // 
            // lblDateFrom
            // 
            this.lblDateFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDateFrom.AutoSize = true;
            this.lblDateFrom.Location = new System.Drawing.Point(520, 39);
            this.lblDateFrom.Name = "lblDateFrom";
            this.lblDateFrom.Size = new System.Drawing.Size(85, 20);
            this.lblDateFrom.TabIndex = 9;
            this.lblDateFrom.Text = "Date From";
            // 
            // FlowExecutionHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbFlowRuns);
            this.Controls.Add(this.gbFlow);
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FlowExecutionHistory";
            this.Size = new System.Drawing.Size(1694, 819);
            this.OnCloseTool += new System.EventHandler(this.FlowExecutionHistory_OnCloseTool);
            this.ConnectionUpdated += new XrmToolBox.Extensibility.PluginControlBase.ConnectionUpdatedHandler(this.FlowExecutionHistory_ConnectionUpdated);
            this.Load += new System.EventHandler(this.FlowExecutionHistory_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFlowRuns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsFlowRuns)).EndInit();
            this.gbFlow.ResumeLayout(false);
            this.gbFlow.PerformLayout();
            this.gbFlowRuns.ResumeLayout(false);
            this.gbFlowRuns.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.DataGridView dgvFlowRuns;
        private System.Windows.Forms.BindingSource bsFlowRuns;
        private System.Windows.Forms.ComboBox cbxStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox tbSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.GroupBox gbFlow;
        private System.Windows.Forms.GroupBox gbFlowRuns;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.DateTimePicker dtpDateFrom;
        private System.Windows.Forms.Label lblDateFrom;
        private System.Windows.Forms.DateTimePicker dtpDateTo;
        private System.Windows.Forms.Label lblDateTo;
        private System.Windows.Forms.CheckedListBox clbFlows;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private System.Windows.Forms.ToolStripComboBox cbBrowser;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cbProfile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TextBox tbDurationThreshold;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton tsbConnectFlowApi;
        private System.Windows.Forms.ToolStripSplitButton tsbExport;
        private System.Windows.Forms.ToolStripMenuItem tsbExcel;
        private System.Windows.Forms.ToolStripMenuItem tsbCsv;
        private System.Windows.Forms.ToolStripButton tsbSettings;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowRunId;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowRunCorrelationId;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowRunFlow;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowRunStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowRunStartDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowRunDurationInSeconds;
        private System.Windows.Forms.DataGridViewImageColumn FlowRunUrl;
    }
}
