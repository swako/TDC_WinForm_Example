using System.Collections.Generic;

namespace TDC_WinForm_Net
{
	/// <summary> Thorlabs motor. </summary>
	public abstract class ThorlabsMotor
	{
		/// <summary> The thorlabs device to be controlled. </summary>
		protected readonly ThorlabsDevice _device;

		/// <summary> collection of motor device IDs. </summary>
		private readonly List<uint> _motorDevices = new List<uint> {40, 67, 70, 73, 80, 83}; 

		/// <summary> Constructor. </summary>
		/// <param name="device"> The device. </param>
		protected ThorlabsMotor( ThorlabsDevice device )
		{
			_device = device;
		}

		/// <summary> Gets a value indicating whether this object is motor. </summary>
		/// <value> true if this object is motor, false if not. </value>
		public bool IsMotor
		{ get { return _motorDevices.Contains(_device.DeviceType); } }

		/// <summary> Gets the correct message structure for the supplied message byte[] array. </summary>
		/// <param name="data"> The source byte[] array. </param>
		/// <returns> The correct message structure of NULL if not known. </returns>
		public object ObjectFromData(byte[] data)
		{
			if ((data == null) || (data.Length < 6))
			{
				return null;
			}
			// select depending upon the Message Id
			switch (ThorlabsDevice.MessageType(data))
			{
				case 0:
					return null;
				case DeviceMessages.MGMSG_MOT_GET_DCSTATUSUPDATE:	// DC Servo Motor Status response message
				case DeviceMessages.MGMSG_MOT_GET_STATUSUPDATE:		// Stepper Motor Status response message
					return Utilities.DeserializeMsg<MotorStatus>(data);
				case DeviceMessages.MGMSG_MOT_MOVE_HOMED:			// Motor Homed Message
				case DeviceMessages.MGMSG_MOT_MOVE_COMPLETE:		// Motor Moved Message
					return Utilities.DeserializeMsg<MessageStruct>(data);
			}
			return null;
		}
	}
}
