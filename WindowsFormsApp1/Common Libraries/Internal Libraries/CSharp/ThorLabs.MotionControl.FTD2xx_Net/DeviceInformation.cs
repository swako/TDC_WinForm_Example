using System;
using System.Collections.Generic;

namespace ThorLabs.MotionControl.FTD2xx_Net
{
	/// <summary> List of FTDI device types. </summary>
	public enum DeviceType : uint
	{
		/// <summary>
		/// FT232B or FT245B device
		/// </summary>
		FT_DEVICE_232BM = 0,
		/// <summary>
		/// FT8U232AM or FT8U245AM device
		/// </summary>
		FT_DEVICE_232AM = 1,
		/// <summary>
		/// FT8U100AX device
		/// </summary>
		FT_DEVICE_100AX = 2,
		/// <summary>
		/// Unknown device
		/// </summary>
		FT_DEVICE_UNKNOWN = 3,
		/// <summary>
		/// FT2232 device
		/// </summary>
		FT_DEVICE_2232C = 4,
		/// <summary>
		/// FT232R or FT245R device
		/// </summary>
		FT_DEVICE_232R = 5,
		/// <summary>
		/// FT2232H device
		/// </summary>
		FT_DEVICE_2232H = 6,
		/// <summary>
		/// FT4232H device
		/// </summary>
		FT_DEVICE_4232H = 7,
		/// <summary>
		/// FT232H device
		/// </summary>
		FT_DEVICE_232H = 8
	}

	/// <summary> Holds device information. </summary>
	public class DeviceInformation
	{
		/// <summary> Indicates if device is opened. </summary>
		public bool Opened;
		/// <summary> Indicates if device is highspeed. </summary>
		public bool HighSpeed;
		/// <summary> Indicates the device type. </summary>
		public DeviceType Type;
		/// <summary> The Vendor ID and Product ID of the device. </summary>
		public UInt32 ID;
		/// <summary> The device serial number. </summary>
		public string SerialNumber;
		/// <summary> The device description. </summary>
		public string Description;

		/// <summary> Gets the vid. </summary>
		/// <value> The vid. </value>
		public uint VID
		{ get { return (ID >> 16) & 0xFFFF; } }

		/// <summary> Gets the PID. </summary>
		/// <value> The PID. </value>
		public uint PID
		{ get { return ID & 0xFFFF; } }

		/// <summary> Gets a value indicating whether the thorlabs device. </summary>
		/// <value> true if thorlabs device, false if not. </value>
		public bool ThorlabsDevice
		{ get { return VID == 0x0403; }}

		/// <summary> Gets the device type. </summary>
		/// <returns> . </returns>
		public uint DeviceType()
		{
			if(VID != 0x0403)
			{
				return 0;
			}
			if(PID != 0xFAF0)
			{
				return PID;
			}
			return uint.Parse(SerialNumber.Substring(0, 2));
		}
	}

	/// <summary> Collection of device informations. </summary>
	/// <seealso>
	///     <cref>T:System.Collections.Generic.List{ThorLabs.MotionControl.FTD2xx_Net.DeviceInformation}</cref>
	/// </seealso>
	public class DeviceInformationCollection : List<DeviceInformation>
	{
	}
}
