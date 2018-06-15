// file:	ThorlabsDevice.cs
//
// summary:	Implements the thorlabs device class
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using ThorLabs.MotionControl.FTD2xx_Net;

namespace TDC_WinForm_Net
{
	/// <summary> A Gemeric Thorlabs device. </summary>
	public class ThorlabsDevice
	{
		/// <summary> The device port. </summary>
		private FTDI _device;

		/// <summary> Information describing the device. </summary>
		private readonly Dictionary<string, DeviceInformation> _deviceInfo = new Dictionary<string, DeviceInformation>();

		/// <summary> Default constructor. </summary>
		public ThorlabsDevice()
		{
			DeviceType = 0;
		}

		/// <summary> Gets the collection of available devices. </summary>
		/// <returns> The found devices. </returns>
		public int FindDevices()
		{
			// clear device list
			_deviceInfo.Clear();

			// get the number of devices
			int count = FTDI.NumberOfDevices();
			for(int i = 0; i < count; i++)
			{
				// retrieve the FTDI device information
				DeviceInformation di = FTDI.DeviceInformation(i);
				if(di.ThorlabsDevice)
				{
					_deviceInfo.Add(di.SerialNumber, di);
				}

			}
			return count;
		}

		/// <summary> Gets or sets the type of the device. </summary>
		/// <value> The type of the device. </value>
		public uint DeviceType { get; private set; }

		/// <summary> Gets the devices. </summary>
		/// <value> The devices. </value>
		public List<string> Devices
		{
			get { return _deviceInfo.Keys.ToList(); }
		}

		/// <summary> Gets a device information. </summary>
		/// <param name="serialNo"> The serial no. </param>
		/// <returns> The device information. </returns>
		public DeviceInformation GetDeviceInfo( string serialNo )
		{
			if(_deviceInfo.ContainsKey(serialNo))
			{
				return _deviceInfo[serialNo];
			}
			return null;
		}

		/// <summary> Connects and initializes the device. </summary>
		/// <param name="serialNo"> The serial no of the device. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool Connect( string serialNo )
		{
			// disconnect existing device if any
			if (_device != null)
			{
				_device.Close();
				_device = null;
			}

			// check serial number is a real value
			if (string.IsNullOrEmpty(serialNo))
			{
				return false;
			}

			// get the device information
			DeviceType = _deviceInfo[serialNo].DeviceType();

			// connect new device
			_device = new FTDI(serialNo) {DeviceInfo = _deviceInfo[serialNo]};
			if(!IsConnected)
			{
				return false;
			}

			// initialize device communications
			bool success = _device.SetBaudRate(115200);
			success &= _device.SetDataCharacteristics(FTDI.DataBits.EightBits, FTDI.StopBits.FT_STOP_BITS_1, FTDI.Parity.None);
			success &=  _device.SetFlowControl(FTDI.FlowControl.RtsCts, 0x11, 0x13);
			success &=  _device.ResetDevice();
			success &= _device.Purge(FTDI.PurgeFlags.PurgeRx | FTDI.PurgeFlags.PurgeTx);
			success &= _device.ResetDevice();
			success &=  _device.SetTimeOuts(300, 300); //extend timeout while board finishes reset

			return success;
		}

		/// <summary> Gets a value indicating whether this object is connected. </summary>
		/// <value> true if this object is connected, false if not. </value>
		public bool IsConnected
		{
			get { return (_device != null) && _device.IsConnected; }
		}

		/// <summary> Gets a device information. </summary>
		/// <returns> The device information. </returns>
		public string GetDeviceInfo()
		{
			try
			{
				// check if device is connected
				if(!_device.IsConnected)
				{
					return "Not Connected";
				}

				// get a simple Thorlabs Message Structure and convert to a byte[] array
				MessageStruct message = SimpleMessageStruct(DeviceMessages.MGMSG_HW_REQ_INFO);
				byte[] byteArray = Utilities.SerializeMessage(message);

				// send the message
				if(!SendCommand(byteArray))
				{
					return "Error Writing Command";
				}

				// get expected size of response
				int structSize = Marshal.SizeOf(typeof(GetHardwareInfo));
				// wait for response from device (timeout 2 secs)
				byteArray = WaitForReply(structSize, 2000);
				if(byteArray == null)
				{
					return "Error waiting for response";
				}

				// convert response to a GetHardwareInfo structure
				GetHardwareInfo hardwareInfo = Utilities.DeserializeMsg<GetHardwareInfo>(byteArray);
				return hardwareInfo.ToString();
			}
			catch(Exception ex)
			{
				return ex.Message;
			}
		}

		/// <summary> Wait for reply from the device. </summary>
		/// <param name="structSize"> Size of the structure. </param>
		/// <param name="timeout">    The timeout. </param>
		/// <returns> returns received byte[] array. </returns>
		public byte[] WaitForReply(int structSize, int timeout)
		{
			// set reference time for timeout
			DateTime refTime = DateTime.Now + TimeSpan.FromMilliseconds(timeout);
			// loop, waiting for the response to be available
			while (_device.NumberOfBytesInRxBuffer() < structSize)
			{
				// check for timeout
				Thread.Sleep(100);
				if(DateTime.Now > refTime)
				{
					return null;
				}
			}
			// read the bytes into a new byte array
			int bytesRead;
			byte[] byteArray = new byte[structSize];
			_device.Read(ref byteArray, structSize, out bytesRead);
			return byteArray;
		}

		/// <summary> Wait for any response from the device. </summary>
		/// <param name="timeout"> The timeout. </param>
		/// <returns> returns the received byte[] array. </returns>
		public byte[] WaitForUnknownReply(int timeout)
		{
			// wait for the 6 byte header to be received
			byte[] header = WaitForReply(6, timeout);
			if(header == null)
			{
				return null;
			}

			// extrat the header into a Thorlabs message header structure
			MessageStruct headerStruct = Utilities.DeserializeMsg<MessageStruct>(header);
			// if the message is complete (destination < 0x80) then return header bytes
			if((headerStruct._destination & 0x80) == 0)
			{
				return header;
			}

			// wait for the remainder of the message (length defined by header packet length field)
			byte[] data = WaitForReply(headerStruct._packetLength, 200);
			if(data == null)
			{
				return null;
			}

			// return both byte arrays as a single message byte array
			byte[] complete = new byte[headerStruct._packetLength + 6];
			header.CopyTo(complete, 0);
			data.CopyTo(complete, header.Length);
			return complete;
		}

		/// <summary> Flushes any messages unread. </summary>
		public void FlushMessages()
		{
			while(WaitForUnknownReply(0) != null)
			{
			}
		}

		/// <summary> Gets the message type from the byte[] array. </summary>
		/// <param name="data"> source byte[] array. </param>
		/// <returns> returns the message id number. </returns>
		public static UInt16 MessageType( byte[] data )
		{
			if((data == null) || (data.Length < 6))
			{
				return 0;
			}
			return (UInt16)(data[0] + (data[1]<<8));
		}

		/// <summary> Sends the command to the FTDI device. </summary>
		/// <param name="byteArray"> Array of bytes. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool SendCommand(byte[] byteArray)
		{
			int bytesWritten;
			// write byte[] array
			if (!_device.Write(byteArray, byteArray.Length, out bytesWritten))
			{
				return false;
			}
			return true;
		}

		/// <summary> Gets the device. </summary>
		/// <value> The device. </value>
		public FTDI Device
		{ get { return _device; } }

		/// <summary> Get a Simple message structure (single channel device). </summary>
		/// <param name="command"> The command. </param>
		/// <param name="param1">  (Optional) the first parameter. </param>
		/// <param name="param2">  (Optional) the second parameter. </param>
		/// <returns> . </returns>
		public static MessageStruct SimpleMessageStruct( UInt16 command, byte param1 = 1, byte param2 = 0 )
		{
			return new MessageStruct { _destination = 0x21, _param1 = param1, _param2 = param2, _source = 1, _messageId = command };
		}

		/// <summary> Get a Simple message structure (including channel number). </summary>
		/// <param name="command"> The command. </param>
		/// <param name="channel"> The channel, zero indexed. </param>
		/// <param name="param1">  (Optional) the first parameter. </param>
		/// <param name="param2">  (Optional) the second parameter. </param>
		/// <returns> . </returns>
		public static MessageStruct SimpleMessageStruct(UInt16 command, int channel, byte param1 = 0, byte param2 = 0)
		{
			return new MessageStruct { _destination = (byte)(0x21 + channel), _param1 = param1, _param2 = param2, _source = 1, _messageId = command };
		}
	}
}
