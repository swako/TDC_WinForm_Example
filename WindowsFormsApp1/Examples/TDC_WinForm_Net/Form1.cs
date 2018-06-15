// file:	Form1.cs
//
// summary:	Implements the form 1 class
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ThorLabs.MotionControl.FTD2xx_Net;

namespace TDC_WinForm_Net
{
	/// <summary> Form 1. </summary>
	/// <seealso cref="T:System.Windows.Forms.Form"/>
	public partial class Form1 : Form
	{
		/// <summary> The thorlabs device. </summary>
		private readonly ThorlabsDevice _thorlabsDevice = new ThorlabsDevice();
		/// <summary> Default constructor. </summary>
		public Form1()
		{
			InitializeComponent();
		}

		/// <summary> Event handler. Called by _btnGetDevices for click events. </summary>
		/// <param name="sender"> Source of the event. </param>
		/// <param name="e">	  Event information. </param>
		private void _btnGetDevices_Click(object sender, EventArgs e)
		{
			_cmbDeviceList.Items.Clear();
			_richOutput.Clear();

			try
			{
				// get number of devices
				int count = _thorlabsDevice.FindDevices();
				AppendOutput(string.Format("Number of devices detected = {0}", count), Color.Black, true);

				// get device info for each device
				// and populate ComboBox
				List<string> serialNos = _thorlabsDevice.Devices;
				foreach(string serialNo in serialNos)
				{
					DeviceInformation di = _thorlabsDevice.GetDeviceInfo(serialNo);
					if((di != null) && di.ThorlabsDevice)
					{
						AppendOutput(string.Format("\tSerial Number = {0}", di.SerialNumber), Color.DarkBlue);
						AppendOutput(string.Format("\tDescription = {0}", di.Description), Color.DarkBlue);
						AppendOutput(string.Format("\tDevice Type = {0}", di.DeviceType()), Color.DarkBlue);

						_cmbDeviceList.Items.Add(di.SerialNumber);
					}
					else
					{
						AppendOutput(string.Format("Device = {0} Not One of Ours", di.ID), Color.Red);
					}
				}
				_cmbDeviceList.SelectedIndex = 0;

			}
			catch(Exception ex)
			{
				// catch any errors thrown by FTDI controller
				AppendOutput(ex.ToString(), Color.Red);
			}
		}

		/// <summary> Event handler. Called by _btnIdentifyDevice for click events. </summary>
		/// <param name="sender"> Source of the event. </param>
		/// <param name="e">	  Event information. </param>
		private void _btnIdentifyDevice_Click(object sender, EventArgs e)
		{
			try
			{
				// check that a valid device is selected
				if(_cmbDeviceList.Items.Count == 0)
				{
					AppendOutput("No Items Available", Color.Red);
					return;
				}
				if(_cmbDeviceList.SelectedItem == null)
				{
					AppendOutput("No Item Selected", Color.Red);
					return;
				}

				// connect device
				string serialNo = _cmbDeviceList.SelectedItem as string;
				if(_thorlabsDevice.Connect(serialNo))
				{
					// get Hardware Info from actual device
					AppendOutput(string.Format("Device {0} Connected", serialNo), Color.Black);
					string info = _thorlabsDevice.GetDeviceInfo();
					AppendOutput(info, Color.Black);

				}
				else
				{
					AppendOutput(string.Format("Device {0} Not Connected", serialNo), Color.Red);
				}
			}
			catch (Exception ex)
			{
				AppendOutput(ex.ToString(), Color.Red);
			}
		}

		/// <summary> Event handler. Called by _btnTestDevice for click events. </summary>
		/// <param name="sender"> Source of the event. </param>
		/// <param name="e">	  Event information. </param>
		private void _btnTestDevice_Click(object sender, EventArgs e)
		{
			try
			{
				// check a device is availabl;e and connected
				if(!_thorlabsDevice.IsConnected)
				{
					AppendOutput("No Device Initialized", Color.Red);
					return;
				}

				// get the device as a motor for homing
				ThorlabsMotorHome motorHome = new ThorlabsMotorHome(_thorlabsDevice);
				if(motorHome.IsMotor)
				{
					// home device
					string info = motorHome.HomeDevice();
					AppendOutput(info, Color.Black);

					// get the motor as a motor for moving
					ThorlabsMotorMove motorMove = new ThorlabsMotorMove(_thorlabsDevice);
					List<int> steps = new List<int> { 1000, 2000, -500, 2000, -1000, 1000, 2000, -500, 2000, -1000 };
					foreach (int step in steps)
					{
						// move the motor by the number of steps
						// Note - these steps are specific to a TCube.DCServo device
						info = motorMove.MoveRelative(step * 100);
						AppendOutput(info, Color.Black);
					}
				}
				else
				{
					AppendOutput("Not a motor", Color.Red);
				}
			}
			catch (Exception ex)
			{
				AppendOutput(ex.ToString(), Color.Red);
			}
		}

		/// <summary> Appends text to the output window. </summary>
		/// <param name="text">    The text. </param>
		/// <param name="color">   The color. </param>
		/// <param name="newLine"> (Optional) the new line. </param>
		private void AppendOutput( string text, Color color, bool newLine = true )
		{
			_richOutput.SelectionStart = _richOutput.TextLength;
			_richOutput.SelectionLength = 0;
			_richOutput.SelectionColor = color;
			_richOutput.AppendText(text);
			if(newLine)
			{
				_richOutput.AppendText(Environment.NewLine);
			}
		}
	}
}
