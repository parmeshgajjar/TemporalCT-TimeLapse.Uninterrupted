namespace ShearBoxController_InspectXContinuousScan
{
	partial class TimeLapse_FlyScanForm
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
            this.btn_Start = new System.Windows.Forms.Button();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.panel_CTProfile = new System.Windows.Forms.Panel();
            this.button_BrowseProfile = new System.Windows.Forms.Button();
            this.label_CTProfile = new System.Windows.Forms.Label();
            this.textBox_Profile = new System.Windows.Forms.TextBox();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.comboBoxProfile = new System.Windows.Forms.ComboBox();
            this.panel_InspectXConnection = new System.Windows.Forms.Panel();
            this.btn_CTDisconnect = new System.Windows.Forms.Button();
            this.btn_CTConnect = new System.Windows.Forms.Button();
            this.label_InspectXConnectionPanel = new System.Windows.Forms.Label();
            this.panel_SystemStatus = new System.Windows.Forms.Panel();
            this.panel_NoCompletedScans = new System.Windows.Forms.Panel();
            this.box_CompletedScans = new System.Windows.Forms.Label();
            this.label_CompletedCTScans = new System.Windows.Forms.Label();
            this.panel_CurrentInspectXStatus = new System.Windows.Forms.Panel();
            this.box_CurrentInspectXState = new System.Windows.Forms.Label();
            this.label_CurrentInspectXState = new System.Windows.Forms.Label();
            this.panel_CurrentRunningStatus = new System.Windows.Forms.Panel();
            this.box_CurrentRunningState = new System.Windows.Forms.Label();
            this.label_CurrentRunningState = new System.Windows.Forms.Label();
            this.panel_TimerStatus = new System.Windows.Forms.Panel();
            this.box_TimerState = new System.Windows.Forms.Label();
            this.label_TimerValue = new System.Windows.Forms.Label();
            this.panel_InspectXConnectionStatus = new System.Windows.Forms.Panel();
            this.box_InspectXConnection = new System.Windows.Forms.Label();
            this.label_InspectXConnection = new System.Windows.Forms.Label();
            this.panel_SystemState = new System.Windows.Forms.Panel();
            this.box_ApplicationState = new System.Windows.Forms.Label();
            this.label_SystemState = new System.Windows.Forms.Label();
            this.label_SystemStatus = new System.Windows.Forms.Label();
            this.panel_OutputLog = new System.Windows.Forms.Panel();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.label_OutputLog = new System.Windows.Forms.Label();
            this.panel_TimerSettings = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_minutes = new System.Windows.Forms.NumericUpDown();
            this.label_hours = new System.Windows.Forms.Label();
            this.numericUpDown_hours = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label_TimerPanel = new System.Windows.Forms.Label();
            this.panel_ProjectSettings = new System.Windows.Forms.Panel();
            this.checkBox_ManualReturnManipulatorToZero = new System.Windows.Forms.CheckBox();
            this.label_ManualReturnManipulatorToZero = new System.Windows.Forms.Label();
            this.numericUpDown_NumberOfScans = new System.Windows.Forms.NumericUpDown();
            this.label_NoScans = new System.Windows.Forms.Label();
            this.button_BrowseDirectory = new System.Windows.Forms.Button();
            this.textBox_RootDirectory = new System.Windows.Forms.TextBox();
            this.label_ProjectFolder = new System.Windows.Forms.Label();
            this.textBox_ProjectName = new System.Windows.Forms.TextBox();
            this.label_ProjectName = new System.Windows.Forms.Label();
            this.label_ProjectSettings = new System.Windows.Forms.Label();
            this.openFileDialogCTProfile = new System.Windows.Forms.OpenFileDialog();
            this.backgroundWorker_MainRoutine = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker_Abort = new System.ComponentModel.BackgroundWorker();
            this.timer_CTScanInterval = new System.Windows.Forms.Timer(this.components);
            this.timer_General = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker_XCTScan = new System.ComponentModel.BackgroundWorker();
            this.timer_StopAutoCondition = new System.Windows.Forms.Timer(this.components);
            this.panel_CTProfile.SuspendLayout();
            this.panel_InspectXConnection.SuspendLayout();
            this.panel_SystemStatus.SuspendLayout();
            this.panel_NoCompletedScans.SuspendLayout();
            this.panel_CurrentInspectXStatus.SuspendLayout();
            this.panel_CurrentRunningStatus.SuspendLayout();
            this.panel_TimerStatus.SuspendLayout();
            this.panel_InspectXConnectionStatus.SuspendLayout();
            this.panel_SystemState.SuspendLayout();
            this.panel_OutputLog.SuspendLayout();
            this.panel_TimerSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_minutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_hours)).BeginInit();
            this.panel_ProjectSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_NumberOfScans)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_Start
            // 
            this.btn_Start.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Start.Location = new System.Drawing.Point(26, 344);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(457, 67);
            this.btn_Start.TabIndex = 0;
            this.btn_Start.Text = "START";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // btn_Stop
            // 
            this.btn_Stop.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Stop.ForeColor = System.Drawing.Color.Red;
            this.btn_Stop.Location = new System.Drawing.Point(26, 344);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(457, 67);
            this.btn_Stop.TabIndex = 1;
            this.btn_Stop.Text = "STOP";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Visible = false;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // panel_CTProfile
            // 
            this.panel_CTProfile.Controls.Add(this.button_BrowseProfile);
            this.panel_CTProfile.Controls.Add(this.label_CTProfile);
            this.panel_CTProfile.Controls.Add(this.textBox_Profile);
            this.panel_CTProfile.Controls.Add(this.btn_Refresh);
            this.panel_CTProfile.Controls.Add(this.comboBoxProfile);
            this.panel_CTProfile.Location = new System.Drawing.Point(12, 186);
            this.panel_CTProfile.Name = "panel_CTProfile";
            this.panel_CTProfile.Size = new System.Drawing.Size(471, 91);
            this.panel_CTProfile.TabIndex = 2;
            // 
            // button_BrowseProfile
            // 
            this.button_BrowseProfile.Location = new System.Drawing.Point(363, 57);
            this.button_BrowseProfile.Name = "button_BrowseProfile";
            this.button_BrowseProfile.Size = new System.Drawing.Size(105, 25);
            this.button_BrowseProfile.TabIndex = 18;
            this.button_BrowseProfile.Text = "Browse Profile";
            this.button_BrowseProfile.UseVisualStyleBackColor = true;
            this.button_BrowseProfile.Click += new System.EventHandler(this.button_BrowseProfile_Click);
            // 
            // label_CTProfile
            // 
            this.label_CTProfile.AutoSize = true;
            this.label_CTProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_CTProfile.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label_CTProfile.Location = new System.Drawing.Point(8, 4);
            this.label_CTProfile.Name = "label_CTProfile";
            this.label_CTProfile.Size = new System.Drawing.Size(84, 18);
            this.label_CTProfile.TabIndex = 17;
            this.label_CTProfile.Text = "CT Profile";
            // 
            // textBox_Profile
            // 
            this.textBox_Profile.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_Profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Profile.Location = new System.Drawing.Point(14, 61);
            this.textBox_Profile.Name = "textBox_Profile";
            this.textBox_Profile.ReadOnly = true;
            this.textBox_Profile.Size = new System.Drawing.Size(343, 20);
            this.textBox_Profile.TabIndex = 16;
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Location = new System.Drawing.Point(363, 25);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(105, 25);
            this.btn_Refresh.TabIndex = 13;
            this.btn_Refresh.Text = "Refresh";
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // comboBoxProfile
            // 
            this.comboBoxProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProfile.FormattingEnabled = true;
            this.comboBoxProfile.Location = new System.Drawing.Point(14, 28);
            this.comboBoxProfile.Name = "comboBoxProfile";
            this.comboBoxProfile.Size = new System.Drawing.Size(343, 21);
            this.comboBoxProfile.TabIndex = 14;
            this.comboBoxProfile.SelectedIndexChanged += new System.EventHandler(this.comboBoxProfile_SelectedIndexChanged);
            // 
            // panel_InspectXConnection
            // 
            this.panel_InspectXConnection.Controls.Add(this.btn_CTDisconnect);
            this.panel_InspectXConnection.Controls.Add(this.btn_CTConnect);
            this.panel_InspectXConnection.Controls.Add(this.label_InspectXConnectionPanel);
            this.panel_InspectXConnection.Location = new System.Drawing.Point(12, 13);
            this.panel_InspectXConnection.Name = "panel_InspectXConnection";
            this.panel_InspectXConnection.Size = new System.Drawing.Size(471, 54);
            this.panel_InspectXConnection.TabIndex = 3;
            // 
            // btn_CTDisconnect
            // 
            this.btn_CTDisconnect.Location = new System.Drawing.Point(251, 25);
            this.btn_CTDisconnect.Name = "btn_CTDisconnect";
            this.btn_CTDisconnect.Size = new System.Drawing.Size(217, 25);
            this.btn_CTDisconnect.TabIndex = 2;
            this.btn_CTDisconnect.Text = "Disconnect";
            this.btn_CTDisconnect.UseVisualStyleBackColor = true;
            this.btn_CTDisconnect.Click += new System.EventHandler(this.btn_CTDisconnect_Click);
            // 
            // btn_CTConnect
            // 
            this.btn_CTConnect.Location = new System.Drawing.Point(14, 25);
            this.btn_CTConnect.Name = "btn_CTConnect";
            this.btn_CTConnect.Size = new System.Drawing.Size(217, 25);
            this.btn_CTConnect.TabIndex = 1;
            this.btn_CTConnect.Text = "Connect";
            this.btn_CTConnect.UseVisualStyleBackColor = true;
            this.btn_CTConnect.Click += new System.EventHandler(this.btn_CTConnect_Click);
            // 
            // label_InspectXConnectionPanel
            // 
            this.label_InspectXConnectionPanel.AutoSize = true;
            this.label_InspectXConnectionPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_InspectXConnectionPanel.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label_InspectXConnectionPanel.Location = new System.Drawing.Point(11, 4);
            this.label_InspectXConnectionPanel.Name = "label_InspectXConnectionPanel";
            this.label_InspectXConnectionPanel.Size = new System.Drawing.Size(170, 18);
            this.label_InspectXConnectionPanel.TabIndex = 0;
            this.label_InspectXConnectionPanel.Text = "Inspect-X Connection";
            // 
            // panel_SystemStatus
            // 
            this.panel_SystemStatus.Controls.Add(this.panel_NoCompletedScans);
            this.panel_SystemStatus.Controls.Add(this.panel_CurrentInspectXStatus);
            this.panel_SystemStatus.Controls.Add(this.panel_CurrentRunningStatus);
            this.panel_SystemStatus.Controls.Add(this.panel_TimerStatus);
            this.panel_SystemStatus.Controls.Add(this.panel_InspectXConnectionStatus);
            this.panel_SystemStatus.Controls.Add(this.panel_SystemState);
            this.panel_SystemStatus.Controls.Add(this.label_SystemStatus);
            this.panel_SystemStatus.Location = new System.Drawing.Point(489, 13);
            this.panel_SystemStatus.Name = "panel_SystemStatus";
            this.panel_SystemStatus.Size = new System.Drawing.Size(471, 132);
            this.panel_SystemStatus.TabIndex = 4;
            // 
            // panel_NoCompletedScans
            // 
            this.panel_NoCompletedScans.Controls.Add(this.box_CompletedScans);
            this.panel_NoCompletedScans.Controls.Add(this.label_CompletedCTScans);
            this.panel_NoCompletedScans.Location = new System.Drawing.Point(326, 78);
            this.panel_NoCompletedScans.Name = "panel_NoCompletedScans";
            this.panel_NoCompletedScans.Size = new System.Drawing.Size(142, 47);
            this.panel_NoCompletedScans.TabIndex = 3;
            // 
            // box_CompletedScans
            // 
            this.box_CompletedScans.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.box_CompletedScans.ForeColor = System.Drawing.Color.Black;
            this.box_CompletedScans.Location = new System.Drawing.Point(3, 22);
            this.box_CompletedScans.Name = "box_CompletedScans";
            this.box_CompletedScans.Size = new System.Drawing.Size(135, 19);
            this.box_CompletedScans.TabIndex = 1;
            this.box_CompletedScans.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CompletedCTScans
            // 
            this.label_CompletedCTScans.Location = new System.Drawing.Point(3, 5);
            this.label_CompletedCTScans.Name = "label_CompletedCTScans";
            this.label_CompletedCTScans.Size = new System.Drawing.Size(135, 23);
            this.label_CompletedCTScans.TabIndex = 0;
            this.label_CompletedCTScans.Text = "Completed CT scans";
            this.label_CompletedCTScans.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel_CurrentInspectXStatus
            // 
            this.panel_CurrentInspectXStatus.Controls.Add(this.box_CurrentInspectXState);
            this.panel_CurrentInspectXStatus.Controls.Add(this.label_CurrentInspectXState);
            this.panel_CurrentInspectXStatus.Location = new System.Drawing.Point(170, 78);
            this.panel_CurrentInspectXStatus.Name = "panel_CurrentInspectXStatus";
            this.panel_CurrentInspectXStatus.Size = new System.Drawing.Size(142, 47);
            this.panel_CurrentInspectXStatus.TabIndex = 3;
            // 
            // box_CurrentInspectXState
            // 
            this.box_CurrentInspectXState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.box_CurrentInspectXState.ForeColor = System.Drawing.Color.White;
            this.box_CurrentInspectXState.Location = new System.Drawing.Point(3, 22);
            this.box_CurrentInspectXState.Name = "box_CurrentInspectXState";
            this.box_CurrentInspectXState.Size = new System.Drawing.Size(135, 19);
            this.box_CurrentInspectXState.TabIndex = 1;
            this.box_CurrentInspectXState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CurrentInspectXState
            // 
            this.label_CurrentInspectXState.Location = new System.Drawing.Point(3, 5);
            this.label_CurrentInspectXState.Name = "label_CurrentInspectXState";
            this.label_CurrentInspectXState.Size = new System.Drawing.Size(135, 23);
            this.label_CurrentInspectXState.TabIndex = 0;
            this.label_CurrentInspectXState.Text = "Current Inspect-X State";
            this.label_CurrentInspectXState.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel_CurrentRunningStatus
            // 
            this.panel_CurrentRunningStatus.Controls.Add(this.box_CurrentRunningState);
            this.panel_CurrentRunningStatus.Controls.Add(this.label_CurrentRunningState);
            this.panel_CurrentRunningStatus.Location = new System.Drawing.Point(14, 78);
            this.panel_CurrentRunningStatus.Name = "panel_CurrentRunningStatus";
            this.panel_CurrentRunningStatus.Size = new System.Drawing.Size(142, 47);
            this.panel_CurrentRunningStatus.TabIndex = 2;
            // 
            // box_CurrentRunningState
            // 
            this.box_CurrentRunningState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.box_CurrentRunningState.ForeColor = System.Drawing.Color.White;
            this.box_CurrentRunningState.Location = new System.Drawing.Point(3, 22);
            this.box_CurrentRunningState.Name = "box_CurrentRunningState";
            this.box_CurrentRunningState.Size = new System.Drawing.Size(135, 19);
            this.box_CurrentRunningState.TabIndex = 1;
            this.box_CurrentRunningState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CurrentRunningState
            // 
            this.label_CurrentRunningState.Location = new System.Drawing.Point(3, 5);
            this.label_CurrentRunningState.Name = "label_CurrentRunningState";
            this.label_CurrentRunningState.Size = new System.Drawing.Size(135, 23);
            this.label_CurrentRunningState.TabIndex = 0;
            this.label_CurrentRunningState.Text = "Current Running State";
            this.label_CurrentRunningState.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel_TimerStatus
            // 
            this.panel_TimerStatus.Controls.Add(this.box_TimerState);
            this.panel_TimerStatus.Controls.Add(this.label_TimerValue);
            this.panel_TimerStatus.Location = new System.Drawing.Point(326, 25);
            this.panel_TimerStatus.Name = "panel_TimerStatus";
            this.panel_TimerStatus.Size = new System.Drawing.Size(142, 47);
            this.panel_TimerStatus.TabIndex = 2;
            // 
            // box_TimerState
            // 
            this.box_TimerState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.box_TimerState.ForeColor = System.Drawing.Color.White;
            this.box_TimerState.Location = new System.Drawing.Point(3, 22);
            this.box_TimerState.Name = "box_TimerState";
            this.box_TimerState.Size = new System.Drawing.Size(135, 19);
            this.box_TimerState.TabIndex = 3;
            this.box_TimerState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_TimerValue
            // 
            this.label_TimerValue.Location = new System.Drawing.Point(3, 5);
            this.label_TimerValue.Name = "label_TimerValue";
            this.label_TimerValue.Size = new System.Drawing.Size(135, 23);
            this.label_TimerValue.TabIndex = 0;
            this.label_TimerValue.Text = "Timer State";
            this.label_TimerValue.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel_InspectXConnectionStatus
            // 
            this.panel_InspectXConnectionStatus.Controls.Add(this.box_InspectXConnection);
            this.panel_InspectXConnectionStatus.Controls.Add(this.label_InspectXConnection);
            this.panel_InspectXConnectionStatus.Location = new System.Drawing.Point(170, 25);
            this.panel_InspectXConnectionStatus.Name = "panel_InspectXConnectionStatus";
            this.panel_InspectXConnectionStatus.Size = new System.Drawing.Size(142, 47);
            this.panel_InspectXConnectionStatus.TabIndex = 2;
            // 
            // box_InspectXConnection
            // 
            this.box_InspectXConnection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.box_InspectXConnection.ForeColor = System.Drawing.Color.White;
            this.box_InspectXConnection.Location = new System.Drawing.Point(3, 22);
            this.box_InspectXConnection.Name = "box_InspectXConnection";
            this.box_InspectXConnection.Size = new System.Drawing.Size(135, 19);
            this.box_InspectXConnection.TabIndex = 2;
            this.box_InspectXConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_InspectXConnection
            // 
            this.label_InspectXConnection.Location = new System.Drawing.Point(3, 5);
            this.label_InspectXConnection.Name = "label_InspectXConnection";
            this.label_InspectXConnection.Size = new System.Drawing.Size(135, 23);
            this.label_InspectXConnection.TabIndex = 0;
            this.label_InspectXConnection.Text = "Inspect-X Connection";
            this.label_InspectXConnection.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel_SystemState
            // 
            this.panel_SystemState.Controls.Add(this.box_ApplicationState);
            this.panel_SystemState.Controls.Add(this.label_SystemState);
            this.panel_SystemState.Location = new System.Drawing.Point(14, 25);
            this.panel_SystemState.Name = "panel_SystemState";
            this.panel_SystemState.Size = new System.Drawing.Size(142, 47);
            this.panel_SystemState.TabIndex = 1;
            // 
            // box_ApplicationState
            // 
            this.box_ApplicationState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.box_ApplicationState.ForeColor = System.Drawing.Color.White;
            this.box_ApplicationState.Location = new System.Drawing.Point(3, 22);
            this.box_ApplicationState.Name = "box_ApplicationState";
            this.box_ApplicationState.Size = new System.Drawing.Size(135, 19);
            this.box_ApplicationState.TabIndex = 1;
            this.box_ApplicationState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_SystemState
            // 
            this.label_SystemState.Location = new System.Drawing.Point(3, 5);
            this.label_SystemState.Name = "label_SystemState";
            this.label_SystemState.Size = new System.Drawing.Size(135, 23);
            this.label_SystemState.TabIndex = 0;
            this.label_SystemState.Text = "System State";
            this.label_SystemState.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label_SystemStatus
            // 
            this.label_SystemStatus.AutoSize = true;
            this.label_SystemStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_SystemStatus.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label_SystemStatus.Location = new System.Drawing.Point(11, 4);
            this.label_SystemStatus.Name = "label_SystemStatus";
            this.label_SystemStatus.Size = new System.Drawing.Size(117, 18);
            this.label_SystemStatus.TabIndex = 0;
            this.label_SystemStatus.Text = "System Status";
            // 
            // panel_OutputLog
            // 
            this.panel_OutputLog.Controls.Add(this.textBoxLog);
            this.panel_OutputLog.Controls.Add(this.label_OutputLog);
            this.panel_OutputLog.Location = new System.Drawing.Point(489, 151);
            this.panel_OutputLog.Name = "panel_OutputLog";
            this.panel_OutputLog.Size = new System.Drawing.Size(471, 268);
            this.panel_OutputLog.TabIndex = 5;
            // 
            // textBoxLog
            // 
            this.textBoxLog.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBoxLog.Location = new System.Drawing.Point(14, 25);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(454, 235);
            this.textBoxLog.TabIndex = 6;
            // 
            // label_OutputLog
            // 
            this.label_OutputLog.AutoSize = true;
            this.label_OutputLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_OutputLog.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label_OutputLog.Location = new System.Drawing.Point(14, 4);
            this.label_OutputLog.Name = "label_OutputLog";
            this.label_OutputLog.Size = new System.Drawing.Size(91, 18);
            this.label_OutputLog.TabIndex = 0;
            this.label_OutputLog.Text = "Output Log";
            // 
            // panel_TimerSettings
            // 
            this.panel_TimerSettings.Controls.Add(this.label2);
            this.panel_TimerSettings.Controls.Add(this.numericUpDown_minutes);
            this.panel_TimerSettings.Controls.Add(this.label_hours);
            this.panel_TimerSettings.Controls.Add(this.numericUpDown_hours);
            this.panel_TimerSettings.Controls.Add(this.label1);
            this.panel_TimerSettings.Controls.Add(this.label_TimerPanel);
            this.panel_TimerSettings.Location = new System.Drawing.Point(12, 283);
            this.panel_TimerSettings.Name = "panel_TimerSettings";
            this.panel_TimerSettings.Size = new System.Drawing.Size(471, 55);
            this.panel_TimerSettings.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(345, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "minutes";
            // 
            // numericUpDown_minutes
            // 
            this.numericUpDown_minutes.Location = new System.Drawing.Point(276, 29);
            this.numericUpDown_minutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDown_minutes.Name = "numericUpDown_minutes";
            this.numericUpDown_minutes.Size = new System.Drawing.Size(63, 20);
            this.numericUpDown_minutes.TabIndex = 4;
            this.numericUpDown_minutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_minutes.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label_hours
            // 
            this.label_hours.AutoSize = true;
            this.label_hours.Location = new System.Drawing.Point(237, 31);
            this.label_hours.Name = "label_hours";
            this.label_hours.Size = new System.Drawing.Size(33, 13);
            this.label_hours.TabIndex = 3;
            this.label_hours.Text = "hours";
            // 
            // numericUpDown_hours
            // 
            this.numericUpDown_hours.Location = new System.Drawing.Point(168, 29);
            this.numericUpDown_hours.Maximum = new decimal(new int[] {
            36,
            0,
            0,
            0});
            this.numericUpDown_hours.Name = "numericUpDown_hours";
            this.numericUpDown_hours.Size = new System.Drawing.Size(63, 20);
            this.numericUpDown_hours.TabIndex = 2;
            this.numericUpDown_hours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Time interval for starting scan:";
            // 
            // label_TimerPanel
            // 
            this.label_TimerPanel.AutoSize = true;
            this.label_TimerPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_TimerPanel.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label_TimerPanel.Location = new System.Drawing.Point(11, 4);
            this.label_TimerPanel.Name = "label_TimerPanel";
            this.label_TimerPanel.Size = new System.Drawing.Size(117, 18);
            this.label_TimerPanel.TabIndex = 0;
            this.label_TimerPanel.Text = "Timer Settings";
            // 
            // panel_ProjectSettings
            // 
            this.panel_ProjectSettings.Controls.Add(this.checkBox_ManualReturnManipulatorToZero);
            this.panel_ProjectSettings.Controls.Add(this.label_ManualReturnManipulatorToZero);
            this.panel_ProjectSettings.Controls.Add(this.numericUpDown_NumberOfScans);
            this.panel_ProjectSettings.Controls.Add(this.label_NoScans);
            this.panel_ProjectSettings.Controls.Add(this.button_BrowseDirectory);
            this.panel_ProjectSettings.Controls.Add(this.textBox_RootDirectory);
            this.panel_ProjectSettings.Controls.Add(this.label_ProjectFolder);
            this.panel_ProjectSettings.Controls.Add(this.textBox_ProjectName);
            this.panel_ProjectSettings.Controls.Add(this.label_ProjectName);
            this.panel_ProjectSettings.Controls.Add(this.label_ProjectSettings);
            this.panel_ProjectSettings.Location = new System.Drawing.Point(12, 73);
            this.panel_ProjectSettings.Name = "panel_ProjectSettings";
            this.panel_ProjectSettings.Size = new System.Drawing.Size(471, 107);
            this.panel_ProjectSettings.TabIndex = 7;
            // 
            // checkBox_ManualReturnManipulatorToZero
            // 
            this.checkBox_ManualReturnManipulatorToZero.AutoSize = true;
            this.checkBox_ManualReturnManipulatorToZero.Location = new System.Drawing.Point(435, 85);
            this.checkBox_ManualReturnManipulatorToZero.Name = "checkBox_ManualReturnManipulatorToZero";
            this.checkBox_ManualReturnManipulatorToZero.Size = new System.Drawing.Size(15, 14);
            this.checkBox_ManualReturnManipulatorToZero.TabIndex = 23;
            this.checkBox_ManualReturnManipulatorToZero.UseVisualStyleBackColor = true;
            this.checkBox_ManualReturnManipulatorToZero.CheckedChanged += new System.EventHandler(this.checkBox_ManualReturnManipulatorToZero_CheckedChanged);
            // 
            // label_ManualReturnManipulatorToZero
            // 
            this.label_ManualReturnManipulatorToZero.AutoSize = true;
            this.label_ManualReturnManipulatorToZero.Location = new System.Drawing.Point(262, 85);
            this.label_ManualReturnManipulatorToZero.Name = "label_ManualReturnManipulatorToZero";
            this.label_ManualReturnManipulatorToZero.Size = new System.Drawing.Size(174, 13);
            this.label_ManualReturnManipulatorToZero.TabIndex = 22;
            this.label_ManualReturnManipulatorToZero.Text = "Manually return manipulator to zero:";
            // 
            // numericUpDown_NumberOfScans
            // 
            this.numericUpDown_NumberOfScans.Location = new System.Drawing.Point(94, 83);
            this.numericUpDown_NumberOfScans.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_NumberOfScans.Name = "numericUpDown_NumberOfScans";
            this.numericUpDown_NumberOfScans.Size = new System.Drawing.Size(87, 20);
            this.numericUpDown_NumberOfScans.TabIndex = 21;
            this.numericUpDown_NumberOfScans.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_NumberOfScans.ValueChanged += new System.EventHandler(this.numericUpDown_NumberOfShearCycles_ValueChanged);
            // 
            // label_NoScans
            // 
            this.label_NoScans.AutoSize = true;
            this.label_NoScans.Location = new System.Drawing.Point(14, 85);
            this.label_NoScans.Name = "label_NoScans";
            this.label_NoScans.Size = new System.Drawing.Size(69, 13);
            this.label_NoScans.TabIndex = 20;
            this.label_NoScans.Text = "No of Scans:";
            // 
            // button_BrowseDirectory
            // 
            this.button_BrowseDirectory.Location = new System.Drawing.Point(363, 50);
            this.button_BrowseDirectory.Name = "button_BrowseDirectory";
            this.button_BrowseDirectory.Size = new System.Drawing.Size(105, 25);
            this.button_BrowseDirectory.TabIndex = 19;
            this.button_BrowseDirectory.Text = "Browse Directory";
            this.button_BrowseDirectory.UseVisualStyleBackColor = true;
            this.button_BrowseDirectory.Click += new System.EventHandler(this.btn_Browse_Click);
            // 
            // textBox_RootDirectory
            // 
            this.textBox_RootDirectory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_RootDirectory.Location = new System.Drawing.Point(94, 54);
            this.textBox_RootDirectory.Name = "textBox_RootDirectory";
            this.textBox_RootDirectory.ReadOnly = true;
            this.textBox_RootDirectory.Size = new System.Drawing.Size(263, 20);
            this.textBox_RootDirectory.TabIndex = 4;
            this.textBox_RootDirectory.TextChanged += new System.EventHandler(this.textBox_RootDirectory_TextChanged);
            // 
            // label_ProjectFolder
            // 
            this.label_ProjectFolder.AutoSize = true;
            this.label_ProjectFolder.Location = new System.Drawing.Point(14, 56);
            this.label_ProjectFolder.Name = "label_ProjectFolder";
            this.label_ProjectFolder.Size = new System.Drawing.Size(78, 13);
            this.label_ProjectFolder.TabIndex = 3;
            this.label_ProjectFolder.Text = "Root Directory:";
            // 
            // textBox_ProjectName
            // 
            this.textBox_ProjectName.Location = new System.Drawing.Point(94, 22);
            this.textBox_ProjectName.Name = "textBox_ProjectName";
            this.textBox_ProjectName.Size = new System.Drawing.Size(374, 20);
            this.textBox_ProjectName.TabIndex = 2;
            this.textBox_ProjectName.TextChanged += new System.EventHandler(this.textBox_ProjectName_TextChanged);
            // 
            // label_ProjectName
            // 
            this.label_ProjectName.AutoSize = true;
            this.label_ProjectName.Location = new System.Drawing.Point(14, 26);
            this.label_ProjectName.Name = "label_ProjectName";
            this.label_ProjectName.Size = new System.Drawing.Size(74, 13);
            this.label_ProjectName.TabIndex = 1;
            this.label_ProjectName.Text = "Project Name:";
            // 
            // label_ProjectSettings
            // 
            this.label_ProjectSettings.AutoSize = true;
            this.label_ProjectSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ProjectSettings.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label_ProjectSettings.Location = new System.Drawing.Point(11, 4);
            this.label_ProjectSettings.Name = "label_ProjectSettings";
            this.label_ProjectSettings.Size = new System.Drawing.Size(128, 18);
            this.label_ProjectSettings.TabIndex = 0;
            this.label_ProjectSettings.Text = "Project Settings";
            // 
            // openFileDialogCTProfile
            // 
            this.openFileDialogCTProfile.DefaultExt = "ctprofile";
            this.openFileDialogCTProfile.Filter = "CT Profile (*.ctprofile) | *.ctprofile.xml";
            this.openFileDialogCTProfile.Title = "Select CT Profile";
            // 
            // backgroundWorker_MainRoutine
            // 
            this.backgroundWorker_MainRoutine.WorkerSupportsCancellation = true;
            this.backgroundWorker_MainRoutine.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_MainRoutine_DoWork);
            this.backgroundWorker_MainRoutine.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_MainRoutine_RunWorkerCompleted);
            // 
            // backgroundWorker_Abort
            // 
            this.backgroundWorker_Abort.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Abort_DoWork);
            this.backgroundWorker_Abort.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_Abort_RunWorkerCompleted);
            // 
            // timer_CTScanInterval
            // 
            this.timer_CTScanInterval.Tick += new System.EventHandler(this.timer_CTScanInterval_Tick);
            // 
            // timer_General
            // 
            this.timer_General.Tick += new System.EventHandler(this.timer_General_Tick);
            // 
            // backgroundWorker_XCTScan
            // 
            this.backgroundWorker_XCTScan.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_XCTScan_DoWork);
            this.backgroundWorker_XCTScan.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_XCTScan_RunWorkerCompleted);
            // 
            // timer_StopAutoCondition
            // 
            this.timer_StopAutoCondition.Tick += new System.EventHandler(this.timer_StopAutoCondition_Tick);
            // 
            // TimeLapse_FlyScanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(969, 426);
            this.Controls.Add(this.panel_ProjectSettings);
            this.Controls.Add(this.panel_TimerSettings);
            this.Controls.Add(this.panel_OutputLog);
            this.Controls.Add(this.panel_SystemStatus);
            this.Controls.Add(this.panel_InspectXConnection);
            this.Controls.Add(this.panel_CTProfile);
            this.Controls.Add(this.btn_Start);
            this.Controls.Add(this.btn_Stop);
            this.Name = "TimeLapse_FlyScanForm";
            this.Text = "Automated Time-lapse CT";
            this.panel_CTProfile.ResumeLayout(false);
            this.panel_CTProfile.PerformLayout();
            this.panel_InspectXConnection.ResumeLayout(false);
            this.panel_InspectXConnection.PerformLayout();
            this.panel_SystemStatus.ResumeLayout(false);
            this.panel_SystemStatus.PerformLayout();
            this.panel_NoCompletedScans.ResumeLayout(false);
            this.panel_CurrentInspectXStatus.ResumeLayout(false);
            this.panel_CurrentRunningStatus.ResumeLayout(false);
            this.panel_TimerStatus.ResumeLayout(false);
            this.panel_InspectXConnectionStatus.ResumeLayout(false);
            this.panel_SystemState.ResumeLayout(false);
            this.panel_OutputLog.ResumeLayout(false);
            this.panel_OutputLog.PerformLayout();
            this.panel_TimerSettings.ResumeLayout(false);
            this.panel_TimerSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_minutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_hours)).EndInit();
            this.panel_ProjectSettings.ResumeLayout(false);
            this.panel_ProjectSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_NumberOfScans)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.Panel panel_CTProfile;
        private System.Windows.Forms.Label label_CTProfile;
        private System.Windows.Forms.TextBox textBox_Profile;
        private System.Windows.Forms.ComboBox comboBoxProfile;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Panel panel_InspectXConnection;
        private System.Windows.Forms.Button btn_CTDisconnect;
        private System.Windows.Forms.Button btn_CTConnect;
        private System.Windows.Forms.Label label_InspectXConnectionPanel;
        private System.Windows.Forms.Panel panel_SystemStatus;
        private System.Windows.Forms.Label label_SystemStatus;
        private System.Windows.Forms.Panel panel_OutputLog;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label label_OutputLog;
        private System.Windows.Forms.Button button_BrowseProfile;
        private System.Windows.Forms.Panel panel_SystemState;
        private System.Windows.Forms.Label label_SystemState;
        private System.Windows.Forms.Panel panel_TimerSettings;
        private System.Windows.Forms.Label label_TimerPanel;
        private System.Windows.Forms.Panel panel_ProjectSettings;
        private System.Windows.Forms.Button button_BrowseDirectory;
        private System.Windows.Forms.TextBox textBox_RootDirectory;
        private System.Windows.Forms.Label label_ProjectFolder;
        private System.Windows.Forms.TextBox textBox_ProjectName;
        private System.Windows.Forms.Label label_ProjectName;
        private System.Windows.Forms.Label label_ProjectSettings;
        private System.Windows.Forms.Panel panel_TimerStatus;
        private System.Windows.Forms.Label label_TimerValue;
        private System.Windows.Forms.Panel panel_InspectXConnectionStatus;
        private System.Windows.Forms.Label label_InspectXConnection;
        private System.Windows.Forms.Panel panel_NoCompletedScans;
        private System.Windows.Forms.Label box_CompletedScans;
        private System.Windows.Forms.Label label_CompletedCTScans;
        private System.Windows.Forms.Panel panel_CurrentInspectXStatus;
        private System.Windows.Forms.Label box_CurrentInspectXState;
        private System.Windows.Forms.Label label_CurrentInspectXState;
        private System.Windows.Forms.Panel panel_CurrentRunningStatus;
        private System.Windows.Forms.Label box_CurrentRunningState;
        private System.Windows.Forms.Label label_CurrentRunningState;
        private System.Windows.Forms.Label box_TimerState;
        private System.Windows.Forms.Label box_InspectXConnection;
        private System.Windows.Forms.Label box_ApplicationState;
        public System.Windows.Forms.OpenFileDialog openFileDialogCTProfile;
        private System.ComponentModel.BackgroundWorker backgroundWorker_MainRoutine;
        private System.Windows.Forms.CheckBox checkBox_ManualReturnManipulatorToZero;
        private System.Windows.Forms.Label label_ManualReturnManipulatorToZero;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Abort;
        private System.Windows.Forms.NumericUpDown numericUpDown_NumberOfScans;
        private System.Windows.Forms.Label label_NoScans;
        private System.Windows.Forms.NumericUpDown numericUpDown_hours;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_minutes;
        private System.Windows.Forms.Label label_hours;
        private System.Windows.Forms.Timer timer_CTScanInterval;
        private System.Windows.Forms.Timer timer_General;
        private System.ComponentModel.BackgroundWorker backgroundWorker_XCTScan;
        private System.Windows.Forms.Timer timer_StopAutoCondition;
	}
}

