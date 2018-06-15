namespace TDC_WinForm_Net
{
	partial class Form1
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
			this._btnGetDevices = new System.Windows.Forms.Button();
			this._richOutput = new System.Windows.Forms.RichTextBox();
			this._btnIdentifyDevice = new System.Windows.Forms.Button();
			this._btnTestDevice = new System.Windows.Forms.Button();
			this._cmbDeviceList = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// _btnGetDevices
			// 
			this._btnGetDevices.Location = new System.Drawing.Point(13, 13);
			this._btnGetDevices.Name = "_btnGetDevices";
			this._btnGetDevices.Size = new System.Drawing.Size(100, 33);
			this._btnGetDevices.TabIndex = 0;
			this._btnGetDevices.Text = "Get Devices";
			this._btnGetDevices.UseVisualStyleBackColor = true;
			this._btnGetDevices.Click += new System.EventHandler(this._btnGetDevices_Click);
			// 
			// _richOutput
			// 
			this._richOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._richOutput.Location = new System.Drawing.Point(12, 52);
			this._richOutput.Name = "_richOutput";
			this._richOutput.Size = new System.Drawing.Size(460, 198);
			this._richOutput.TabIndex = 1;
			this._richOutput.Text = "";
			// 
			// _btnIdentifyDevice
			// 
			this._btnIdentifyDevice.Location = new System.Drawing.Point(215, 13);
			this._btnIdentifyDevice.Name = "_btnIdentifyDevice";
			this._btnIdentifyDevice.Size = new System.Drawing.Size(100, 33);
			this._btnIdentifyDevice.TabIndex = 2;
			this._btnIdentifyDevice.Text = "Initialize Device";
			this._btnIdentifyDevice.UseVisualStyleBackColor = true;
			this._btnIdentifyDevice.Click += new System.EventHandler(this._btnIdentifyDevice_Click);
			// 
			// _btnTestDevice
			// 
			this._btnTestDevice.Location = new System.Drawing.Point(321, 13);
			this._btnTestDevice.Name = "_btnTestDevice";
			this._btnTestDevice.Size = new System.Drawing.Size(100, 33);
			this._btnTestDevice.TabIndex = 3;
			this._btnTestDevice.Text = "Test Device";
			this._btnTestDevice.UseVisualStyleBackColor = true;
			this._btnTestDevice.Click += new System.EventHandler(this._btnTestDevice_Click);
			// 
			// _cmbDeviceList
			// 
			this._cmbDeviceList.FormattingEnabled = true;
			this._cmbDeviceList.Location = new System.Drawing.Point(119, 20);
			this._cmbDeviceList.Name = "_cmbDeviceList";
			this._cmbDeviceList.Size = new System.Drawing.Size(90, 21);
			this._cmbDeviceList.TabIndex = 4;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 262);
			this.Controls.Add(this._cmbDeviceList);
			this.Controls.Add(this._btnTestDevice);
			this.Controls.Add(this._btnIdentifyDevice);
			this.Controls.Add(this._richOutput);
			this.Controls.Add(this._btnGetDevices);
			this.MinimumSize = new System.Drawing.Size(500, 300);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _btnGetDevices;
		private System.Windows.Forms.RichTextBox _richOutput;
		private System.Windows.Forms.Button _btnIdentifyDevice;
		private System.Windows.Forms.Button _btnTestDevice;
		private System.Windows.Forms.ComboBox _cmbDeviceList;
	}
}

