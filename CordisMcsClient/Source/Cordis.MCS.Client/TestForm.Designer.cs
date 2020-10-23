namespace Cordis.EnSoTestGUI
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtBoxServerMessages = new System.Windows.Forms.TextBox();
            this.txtBoxControllerMessages = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBoxMpTree = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.controllersComboBox = new System.Windows.Forms.ComboBox();
            this.serverConnectedLabel = new System.Windows.Forms.Label();
            this.connectionTextBox = new System.Windows.Forms.TextBox();
            this.testGetMpTree = new System.Windows.Forms.CheckBox();
            this.clearScreenButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnGetMpProperties = new System.Windows.Forms.CheckBox();
            this.status = new System.Windows.Forms.Label();
            this.btnExeCommand = new System.Windows.Forms.CheckBox();
            this.txtBoxMpProp = new System.Windows.Forms.TextBox();
            this.txtBoxMachinePart = new System.Windows.Forms.TextBox();
            this.txtBoxCommand = new System.Windows.Forms.TextBox();
            this.txtBoxObservers = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxSettings = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBoxSetting = new System.Windows.Forms.TextBox();
            this.txtBoxSettingValue = new System.Windows.Forms.TextBox();
            this.btnReadSetting = new System.Windows.Forms.CheckBox();
            this.btnWriteSetting = new System.Windows.Forms.CheckBox();
            this.txtBoxStatemachineHistory = new System.Windows.Forms.TextBox();
            this.btnStatemachineHistory = new System.Windows.Forms.CheckBox();
            this.txtBoxStatemachineName = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.txtBoxPlugInCmdArguments = new System.Windows.Forms.TextBox();
            this.comBoxPluginCmd = new System.Windows.Forms.ComboBox();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtBoxServerMessages
            // 
            this.txtBoxServerMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtBoxServerMessages.Location = new System.Drawing.Point(16, 737);
            this.txtBoxServerMessages.Margin = new System.Windows.Forms.Padding(4);
            this.txtBoxServerMessages.Multiline = true;
            this.txtBoxServerMessages.Name = "txtBoxServerMessages";
            this.txtBoxServerMessages.ReadOnly = true;
            this.txtBoxServerMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxServerMessages.Size = new System.Drawing.Size(1320, 82);
            this.txtBoxServerMessages.TabIndex = 6;
            // 
            // txtBoxControllerMessages
            // 
            this.txtBoxControllerMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtBoxControllerMessages.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtBoxControllerMessages.Location = new System.Drawing.Point(16, 601);
            this.txtBoxControllerMessages.Margin = new System.Windows.Forms.Padding(4);
            this.txtBoxControllerMessages.Multiline = true;
            this.txtBoxControllerMessages.Name = "txtBoxControllerMessages";
            this.txtBoxControllerMessages.ReadOnly = true;
            this.txtBoxControllerMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxControllerMessages.Size = new System.Drawing.Size(1320, 106);
            this.txtBoxControllerMessages.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 717);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Server messages:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 581);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Controller messages:";
            // 
            // txtBoxMpTree
            // 
            this.txtBoxMpTree.Location = new System.Drawing.Point(16, 79);
            this.txtBoxMpTree.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxMpTree.Multiline = true;
            this.txtBoxMpTree.Name = "txtBoxMpTree";
            this.txtBoxMpTree.ReadOnly = true;
            this.txtBoxMpTree.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxMpTree.Size = new System.Drawing.Size(646, 152);
            this.txtBoxMpTree.TabIndex = 0;
            this.txtBoxMpTree.WordWrap = false;
            this.txtBoxMpTree.TextChanged += new System.EventHandler(this.resultsTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "Controller:";
            // 
            // controllersComboBox
            // 
            this.controllersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.controllersComboBox.FormattingEnabled = true;
            this.controllersComboBox.Location = new System.Drawing.Point(190, 42);
            this.controllersComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.controllersComboBox.Name = "controllersComboBox";
            this.controllersComboBox.Size = new System.Drawing.Size(223, 24);
            this.controllersComboBox.TabIndex = 1;
            this.controllersComboBox.SelectedIndexChanged += new System.EventHandler(this.ControllersComboBox_SelectedIndexChanged);
            // 
            // serverConnectedLabel
            // 
            this.serverConnectedLabel.AutoSize = true;
            this.serverConnectedLabel.Location = new System.Drawing.Point(11, 12);
            this.serverConnectedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.serverConnectedLabel.Name = "serverConnectedLabel";
            this.serverConnectedLabel.Size = new System.Drawing.Size(127, 17);
            this.serverConnectedLabel.TabIndex = 1;
            this.serverConnectedLabel.Text = "Server connection:";
            // 
            // connectionTextBox
            // 
            this.connectionTextBox.Location = new System.Drawing.Point(190, 10);
            this.connectionTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.connectionTextBox.Name = "connectionTextBox";
            this.connectionTextBox.ReadOnly = true;
            this.connectionTextBox.Size = new System.Drawing.Size(223, 22);
            this.connectionTextBox.TabIndex = 0;
            // 
            // testGetMpTree
            // 
            this.testGetMpTree.Appearance = System.Windows.Forms.Appearance.Button;
            this.testGetMpTree.BackColor = System.Drawing.SystemColors.Control;
            this.testGetMpTree.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.testGetMpTree.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.testGetMpTree.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.testGetMpTree.Location = new System.Drawing.Point(439, 38);
            this.testGetMpTree.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.testGetMpTree.Name = "testGetMpTree";
            this.testGetMpTree.Size = new System.Drawing.Size(222, 28);
            this.testGetMpTree.TabIndex = 20;
            this.testGetMpTree.Text = "Get MachinePart tree";
            this.testGetMpTree.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.testGetMpTree.UseVisualStyleBackColor = false;
            this.testGetMpTree.CheckedChanged += new System.EventHandler(this.btnGetMachinePartTree);
            // 
            // clearScreenButton
            // 
            this.clearScreenButton.Location = new System.Drawing.Point(0, 0);
            this.clearScreenButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.clearScreenButton.Name = "clearScreenButton";
            this.clearScreenButton.Size = new System.Drawing.Size(67, 18);
            this.clearScreenButton.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.clearScreenButton);
            this.groupBox5.Location = new System.Drawing.Point(2073, 12);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox5.Size = new System.Drawing.Size(98, 69);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            // 
            // btnGetMpProperties
            // 
            this.btnGetMpProperties.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnGetMpProperties.BackColor = System.Drawing.SystemColors.Control;
            this.btnGetMpProperties.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnGetMpProperties.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnGetMpProperties.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGetMpProperties.Location = new System.Drawing.Point(439, 250);
            this.btnGetMpProperties.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGetMpProperties.Name = "btnGetMpProperties";
            this.btnGetMpProperties.Size = new System.Drawing.Size(222, 28);
            this.btnGetMpProperties.TabIndex = 21;
            this.btnGetMpProperties.Text = "Get MachinePart properties";
            this.btnGetMpProperties.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnGetMpProperties.UseVisualStyleBackColor = false;
            this.btnGetMpProperties.CheckedChanged += new System.EventHandler(this.btnGetMpPropertiesClick);
            // 
            // status
            // 
            this.status.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(13, 823);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(60, 17);
            this.status.TabIndex = 22;
            this.status.Text = "Status...";
            // 
            // btnExeCommand
            // 
            this.btnExeCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExeCommand.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnExeCommand.BackColor = System.Drawing.SystemColors.Control;
            this.btnExeCommand.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnExeCommand.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnExeCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExeCommand.Location = new System.Drawing.Point(1114, 291);
            this.btnExeCommand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnExeCommand.Name = "btnExeCommand";
            this.btnExeCommand.Size = new System.Drawing.Size(222, 28);
            this.btnExeCommand.TabIndex = 23;
            this.btnExeCommand.Text = "Execute command";
            this.btnExeCommand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnExeCommand.UseVisualStyleBackColor = false;
            this.btnExeCommand.CheckedChanged += new System.EventHandler(this.btnExeCommandClick);
            // 
            // txtBoxMpProp
            // 
            this.txtBoxMpProp.Location = new System.Drawing.Point(16, 294);
            this.txtBoxMpProp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxMpProp.Multiline = true;
            this.txtBoxMpProp.Name = "txtBoxMpProp";
            this.txtBoxMpProp.ReadOnly = true;
            this.txtBoxMpProp.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxMpProp.Size = new System.Drawing.Size(646, 285);
            this.txtBoxMpProp.TabIndex = 24;
            this.txtBoxMpProp.WordWrap = false;
            // 
            // txtBoxMachinePart
            // 
            this.txtBoxMachinePart.Location = new System.Drawing.Point(16, 253);
            this.txtBoxMachinePart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxMachinePart.Name = "txtBoxMachinePart";
            this.txtBoxMachinePart.Size = new System.Drawing.Size(397, 22);
            this.txtBoxMachinePart.TabIndex = 25;
            this.txtBoxMachinePart.TextChanged += new System.EventHandler(this.txtBoxMachinePart_TextChanged);
            // 
            // txtBoxCommand
            // 
            this.txtBoxCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxCommand.Location = new System.Drawing.Point(691, 294);
            this.txtBoxCommand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxCommand.Name = "txtBoxCommand";
            this.txtBoxCommand.Size = new System.Drawing.Size(397, 22);
            this.txtBoxCommand.TabIndex = 26;
            this.txtBoxCommand.TextChanged += new System.EventHandler(this.txtBoxProperty_TextChanged);
            // 
            // txtBoxObservers
            // 
            this.txtBoxObservers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxObservers.Location = new System.Drawing.Point(691, 79);
            this.txtBoxObservers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxObservers.Multiline = true;
            this.txtBoxObservers.Name = "txtBoxObservers";
            this.txtBoxObservers.ReadOnly = true;
            this.txtBoxObservers.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxObservers.Size = new System.Drawing.Size(312, 152);
            this.txtBoxObservers.TabIndex = 27;
            this.txtBoxObservers.WordWrap = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(687, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 17);
            this.label1.TabIndex = 28;
            this.label1.Text = "Observers:";
            // 
            // txtBoxSettings
            // 
            this.txtBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxSettings.Location = new System.Drawing.Point(1021, 79);
            this.txtBoxSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxSettings.Multiline = true;
            this.txtBoxSettings.Name = "txtBoxSettings";
            this.txtBoxSettings.ReadOnly = true;
            this.txtBoxSettings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxSettings.Size = new System.Drawing.Size(315, 152);
            this.txtBoxSettings.TabIndex = 29;
            this.txtBoxSettings.WordWrap = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1018, 46);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 17);
            this.label4.TabIndex = 30;
            this.label4.Text = "Settings:";
            // 
            // txtBoxSetting
            // 
            this.txtBoxSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxSetting.Location = new System.Drawing.Point(691, 253);
            this.txtBoxSetting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxSetting.Name = "txtBoxSetting";
            this.txtBoxSetting.Size = new System.Drawing.Size(200, 22);
            this.txtBoxSetting.TabIndex = 31;
            this.txtBoxSetting.TextChanged += new System.EventHandler(this.txtBoxSetting_TextChanged);
            // 
            // txtBoxSettingValue
            // 
            this.txtBoxSettingValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxSettingValue.Location = new System.Drawing.Point(911, 253);
            this.txtBoxSettingValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxSettingValue.Name = "txtBoxSettingValue";
            this.txtBoxSettingValue.Size = new System.Drawing.Size(177, 22);
            this.txtBoxSettingValue.TabIndex = 32;
            this.txtBoxSettingValue.TextChanged += new System.EventHandler(this.txtBoxSettingValue_TextChanged);
            // 
            // btnReadSetting
            // 
            this.btnReadSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReadSetting.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnReadSetting.BackColor = System.Drawing.SystemColors.Control;
            this.btnReadSetting.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnReadSetting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnReadSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadSetting.Location = new System.Drawing.Point(1115, 250);
            this.btnReadSetting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnReadSetting.Name = "btnReadSetting";
            this.btnReadSetting.Size = new System.Drawing.Size(102, 28);
            this.btnReadSetting.TabIndex = 33;
            this.btnReadSetting.Text = "Read setting";
            this.btnReadSetting.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnReadSetting.UseVisualStyleBackColor = false;
            this.btnReadSetting.CheckedChanged += new System.EventHandler(this.btnReadSingleSettingClick);
            // 
            // btnWriteSetting
            // 
            this.btnWriteSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWriteSetting.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnWriteSetting.BackColor = System.Drawing.SystemColors.Control;
            this.btnWriteSetting.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnWriteSetting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnWriteSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWriteSetting.Location = new System.Drawing.Point(1234, 250);
            this.btnWriteSetting.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnWriteSetting.Name = "btnWriteSetting";
            this.btnWriteSetting.Size = new System.Drawing.Size(102, 28);
            this.btnWriteSetting.TabIndex = 34;
            this.btnWriteSetting.Text = "Write setting";
            this.btnWriteSetting.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnWriteSetting.UseVisualStyleBackColor = false;
            this.btnWriteSetting.CheckedChanged += new System.EventHandler(this.btnWriteSingleSettingClick);
            // 
            // txtBoxStatemachineHistory
            // 
            this.txtBoxStatemachineHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxStatemachineHistory.BackColor = System.Drawing.SystemColors.Control;
            this.txtBoxStatemachineHistory.Location = new System.Drawing.Point(690, 369);
            this.txtBoxStatemachineHistory.Multiline = true;
            this.txtBoxStatemachineHistory.Name = "txtBoxStatemachineHistory";
            this.txtBoxStatemachineHistory.ReadOnly = true;
            this.txtBoxStatemachineHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxStatemachineHistory.Size = new System.Drawing.Size(646, 167);
            this.txtBoxStatemachineHistory.TabIndex = 35;
            this.txtBoxStatemachineHistory.WordWrap = false;
            // 
            // btnStatemachineHistory
            // 
            this.btnStatemachineHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStatemachineHistory.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnStatemachineHistory.BackColor = System.Drawing.SystemColors.Control;
            this.btnStatemachineHistory.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnStatemachineHistory.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnStatemachineHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStatemachineHistory.Location = new System.Drawing.Point(1114, 335);
            this.btnStatemachineHistory.Name = "btnStatemachineHistory";
            this.btnStatemachineHistory.Size = new System.Drawing.Size(222, 28);
            this.btnStatemachineHistory.TabIndex = 37;
            this.btnStatemachineHistory.Text = "Read Statemachine history";
            this.btnStatemachineHistory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnStatemachineHistory.UseVisualStyleBackColor = false;
            this.btnStatemachineHistory.CheckedChanged += new System.EventHandler(this.btnStatemachineHistoryClick);
            // 
            // txtBoxStatemachineName
            // 
            this.txtBoxStatemachineName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxStatemachineName.Location = new System.Drawing.Point(691, 338);
            this.txtBoxStatemachineName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxStatemachineName.Name = "txtBoxStatemachineName";
            this.txtBoxStatemachineName.Size = new System.Drawing.Size(397, 22);
            this.txtBoxStatemachineName.TabIndex = 38;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox1.BackColor = System.Drawing.SystemColors.Control;
            this.checkBox1.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.checkBox1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox1.Location = new System.Drawing.Point(983, 554);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(105, 28);
            this.checkBox1.TabIndex = 35;
            this.checkBox1.Text = "Execute";
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.btnPlugInCommand);
            // 
            // txtBoxPlugInCmdArguments
            // 
            this.txtBoxPlugInCmdArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxPlugInCmdArguments.Location = new System.Drawing.Point(1114, 557);
            this.txtBoxPlugInCmdArguments.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBoxPlugInCmdArguments.Name = "txtBoxPlugInCmdArguments";
            this.txtBoxPlugInCmdArguments.Size = new System.Drawing.Size(222, 22);
            this.txtBoxPlugInCmdArguments.TabIndex = 39;
            this.txtBoxPlugInCmdArguments.Text = "arg1, arg2, arg3";
            // 
            // comBoxPluginCmd
            // 
            this.comBoxPluginCmd.FormattingEnabled = true;
            this.comBoxPluginCmd.Location = new System.Drawing.Point(691, 557);
            this.comBoxPluginCmd.Name = "comBoxPluginCmd";
            this.comBoxPluginCmd.Size = new System.Drawing.Size(277, 24);
            this.comBoxPluginCmd.TabIndex = 41;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1348, 853);
            this.Controls.Add(this.comBoxPluginCmd);
            this.Controls.Add(this.txtBoxPlugInCmdArguments);
            this.Controls.Add(this.txtBoxStatemachineName);
            this.Controls.Add(this.btnStatemachineHistory);
            this.Controls.Add(this.txtBoxStatemachineHistory);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnWriteSetting);
            this.Controls.Add(this.btnReadSetting);
            this.Controls.Add(this.txtBoxSettingValue);
            this.Controls.Add(this.txtBoxSetting);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtBoxSettings);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBoxObservers);
            this.Controls.Add(this.txtBoxCommand);
            this.Controls.Add(this.txtBoxMachinePart);
            this.Controls.Add(this.txtBoxMpProp);
            this.Controls.Add(this.btnExeCommand);
            this.Controls.Add(this.status);
            this.Controls.Add(this.btnGetMpProperties);
            this.Controls.Add(this.txtBoxMpTree);
            this.Controls.Add(this.testGetMpTree);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.connectionTextBox);
            this.Controls.Add(this.serverConnectedLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.controllersComboBox);
            this.Controls.Add(this.txtBoxControllerMessages);
            this.Controls.Add(this.txtBoxServerMessages);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(526, 477);
            this.Name = "TestForm";
            this.Text = "Cordis MCS Client example";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtBoxServerMessages;
        private System.Windows.Forms.TextBox txtBoxControllerMessages;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBoxMpTree;
        private System.Windows.Forms.ComboBox controllersComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label serverConnectedLabel;
        private System.Windows.Forms.TextBox connectionTextBox;
        private System.Windows.Forms.Button clearScreenButton;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox testGetMpTree;
        private System.Windows.Forms.CheckBox btnGetMpProperties;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.CheckBox btnExeCommand;
        private System.Windows.Forms.TextBox txtBoxMpProp;
        private System.Windows.Forms.TextBox txtBoxMachinePart;
        private System.Windows.Forms.TextBox txtBoxCommand;
        private System.Windows.Forms.TextBox txtBoxObservers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxSettings;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBoxSetting;
        private System.Windows.Forms.TextBox txtBoxSettingValue;
        private System.Windows.Forms.CheckBox btnReadSetting;
        private System.Windows.Forms.CheckBox btnWriteSetting;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox txtBoxStatemachineHistory;
        private System.Windows.Forms.CheckBox btnStatemachineHistory;
        private System.Windows.Forms.TextBox txtBoxStatemachineName;
        private System.Windows.Forms.TextBox txtBoxPlugInCmdArguments;
        private System.Windows.Forms.ComboBox comBoxPluginCmd;
    }
}

