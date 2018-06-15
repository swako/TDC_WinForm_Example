// file:	FTDI.cs
//
// summary:	Implements the ftdi class
using System;
using System.Runtime.InteropServices;

namespace ThorLabs.MotionControl.FTD2xx_Net
{
	/// <summary> FTDI Port controller. </summary>
	public class FTDI
	{
		#region exceptions
		/// <summary> Exception thrown by errors within the FTDI drivers. </summary>
		/// <seealso cref="T:System.Exception"/>
		internal class GeneralException : Exception
		{
			/// <summary> Constructor. </summary>
			/// <param name="message"> The message. </param>
			internal GeneralException(string message)
				: base(message)
			{
			}
		}

		/// <summary> Exception thrown by errors within the FTDI drivers which cause an IO error. </summary>
		/// <seealso cref="T:System.Exception"/>
		internal class IOErrorException : Exception
		{
			/// <summary> Constructor. </summary>
			/// <param name="message"> The message. </param>
			internal IOErrorException(string message)
				: base(message)
			{
			}

			/// <summary> Constructor. </summary>
			/// <param name="message"> The message. </param>
			/// <param name="ex">	   The ex. </param>
			internal IOErrorException(string message, Exception ex)
				: base(message, ex)
			{
			}
		}

		/// <summary> Exception thrown when a device handle is invalid. </summary>
		/// <seealso cref="T:System.Exception"/>
		internal class InvalidHandleException : Exception
		{
			/// <summary> Constructor. </summary>
			/// <param name="message"> The message. </param>
			internal InvalidHandleException(string message)
				: base(message)
			{
			}
		}

		/// <summary> Exception thrown if device not found. </summary>
		/// <seealso cref="T:System.Exception"/>
		internal class DeviceNotFoundException : Exception
		{
			/// <summary> Constructor. </summary>
			/// <param name="message"> The message. </param>
			internal DeviceNotFoundException(string message)
				: base(message)
			{
			}
		}
		#endregion

		#region enumerations

		/// <summary> Permitted parity values for FTDI devices. </summary>
		public enum Parity
		{
			/// <summary> No Parity. </summary>
			None = 0,
			/// <summary> Odd Parity. </summary>
			Odd = 1,
			/// <summary> Even Parity. </summary>
			Even = 2,
			/// <summary> Mark Parity. </summary>
			Mark = 3,
			/// <summary> Space parity. </summary>
			Space = 4
		}

		/// <summary> Status values for FTDI devices. </summary>
		private enum Status
		{
			/// <summary> Status OK. </summary>
			Ok = 0,
			/// <summary> The device handle is invalid. </summary>
			InvalidHandle = 1,
			/// <summary> Device not found. </summary>
			DeviceNotFound = 2,
			/// <summary> Device is not open. </summary>
			DeviceNotOpened = 3,
			/// <summary> IO error. </summary>
			IOError = 4,
			/// <summary> Insufficient resources. </summary>
			InsufficientResources = 5,
			/// <summary> A parameter was invalid. </summary>
			InvalidParameter = 6,
			/// <summary> The requested baud rate is invalid. </summary>
			InvalidBaudRate = 7,
			/// <summary> Device not opened for erase. </summary>
			DeviceNotOpenedForErase = 8,
			/// <summary> Device not opened for write. </summary>
			DeviceNotOpenedForWrite = 9,
			/// <summary> Failed to write to device. </summary>
			FailedToWriteDevice = 10,
			/// <summary> Failed to read the device EEPROM. </summary>
			EEReadFailed = 11,
			/// <summary> Failed to write the device EEPROM. </summary>
			EEWriteFailed = 12,
			/// <summary> Failed to erase the device EEPROM. </summary>
			EEEraseFailed = 13,
			/// <summary> An EEPROM is not fitted to the device. </summary>
			EENotPresent = 14,
			/// <summary> Device EEPROM is blank. </summary>
			EENoteProgrammed = 15,
			/// <summary> Invalid arguments. </summary>
			InvalidArguments = 16,
			/// <summary> An unsupported action. </summary>
			NotSupported = 17,
			/// <summary> Another error has occurred. </summary>
			OtherError = 18
		}

		/// <summary> List devices flag. </summary>
		private enum ListDevicesFlag : uint
		{
			/// <summary> Lists number of devices. </summary>
			NumberOnly = 0x80000000,
			/// <summary> Lists devices by index. </summary>
			ByIndex = 0x40000000,
			/// <summary> Lists all devices. </summary>
			All = 0x20000000
		}

		/// <summary> Open devices flag. </summary>
		/// <remarks> Used to open a FTDI device. </remarks>
		[Flags]
		enum OpenFlag
		{
			/// <summary> Opens device by serial number. </summary>
			BySerialNumber = 1,
			/// <summary> Opens device by description. </summary>
			ByDescription = 2,
			/// <summary> Opens device by location. </summary>
			ByLocation = 4
		}

		/// <summary> Permitted data bits for FTDI devies. </summary>
		public enum DataBits
		{
			/// <summary> 8 Data Bits. </summary>
			EightBits = 8,
			/// <summary> 7 Data bits. </summary>
			SevenBits = 7
		}

		/// <summary> Permitted stop bits for FTDI devices. </summary>
		public enum StopBits
		{
			/// <summary> 1 Stop Bit. </summary>
			FT_STOP_BITS_1 = 0,
			/// <summary> 2 Stop bits. </summary>
			FT_STOP_BITS_2 = 2
		}

		/// <summary> Permitted flow control values for FTDI devices. </summary>
		public enum FlowControl
		{
			/// <summary> No flow control. </summary>
			None = 0x0,
			/// <summary> RTS/CTS flow control. </summary>
			RtsCts = 0x100,
			/// <summary> DTR/DSR flow control. </summary>
			DtrDsr = 0x200,
			/// <summary> Xon/Xoff flow control. </summary>
			XonXoff = 0x400
		}

		/// <summary> Flags that provide information on the FTDI device status. </summary>
		internal enum StatusFlags : uint
		{
			/// <summary> Indicates that the device is open. </summary>
			Opened = 0x00000001,
			/// <summary> Indicates that the device is enumerated as a hi-speed USB device. </summary>
			HighSpeed = 0x00000002
		}

		/// <summary> Purge buffer constant definitions. </summary>
		[Flags]
		public enum PurgeFlags
		{
			/// <summary> Purge Rx buffer. </summary>
			PurgeRx = 1,

			/// <summary> Purge Tx buffer. </summary>
			PurgeTx = 2
		}
		#endregion

		#region Imported functions
		/// <summary> Port internals. </summary>
		private static class PortInternals
		{
			/// <summary> FDTI device driver functions.
			/// 
			/// See FTDI2XX Programmers guide for more details. The pdf file can be found on
			/// 
			/// www.ftdichip.com
			/// 
			/// Note that the version used here is FTDI device driver 2.01. </summary>
			/// <param name="lpBuffer1">  The first pointer to a buffer. </param>
			/// <param name="lpszBuffer"> The buffer. </param>
			/// <param name="lngFlags">   The flags. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_ListDevices( int lpBuffer1, string lpszBuffer, int lngFlags );

			/// <summary> Queries if a given ft open. </summary>
			/// <param name="intDeviceNumber"> The int device number. </param>
			/// <param name="lngHandle">	   [in,out] The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_Open(short intDeviceNumber, ref int lngHandle);

			/// <summary> Ft close. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_Close(IntPtr lngHandle);

			/// <summary> Ft read. </summary>
			/// <param name="lngHandle">	    The handle. </param>
			/// <param name="lpszBuffer">	    The buffer. </param>
			/// <param name="lngBufferSize">    Size of the buffer. </param>
			/// <param name="lngBytesReturned"> [in,out] The bytes returned. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_Read(IntPtr lngHandle, string lpszBuffer, int lngBufferSize, ref int lngBytesReturned);

			/// <summary> Ft read. </summary>
			/// <param name="lngHandle">	    The handle. </param>
			/// <param name="lpszBuffer">	    [in,out] The buffer. </param>
			/// <param name="lngBufferSize">    Size of the buffer. </param>
			/// <param name="lngBytesReturned"> [in,out] The bytes returned. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_Read(IntPtr lngHandle, ref byte lpszBuffer, int lngBufferSize, ref int lngBytesReturned);

			/// <summary> Ft write. </summary>
			/// <param name="lngHandle">	   The handle. </param>
			/// <param name="lpszBuffer">	   [in,out] The buffer. </param>
			/// <param name="lngBufferSize">   Size of the buffer. </param>
			/// <param name="lngBytesWritten"> [in,out] The bytes written. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_Write(IntPtr lngHandle, ref byte lpszBuffer, int lngBufferSize, ref int lngBytesWritten);

			/// <summary> Ft set baud rate. </summary>
			/// <param name="lngHandle">   The handle. </param>
			/// <param name="lngBaudRate"> The baud rate. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetBaudRate(IntPtr lngHandle, int lngBaudRate);

			/// <summary> Ft set data characteristics. </summary>
			/// <param name="lngHandle">    The handle. </param>
			/// <param name="byWordLength"> Length of the by word. </param>
			/// <param name="byStopBits">   The by stop bits. </param>
			/// <param name="byParity">	    The by parity. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetDataCharacteristics(IntPtr lngHandle, byte byWordLength, byte byStopBits, byte byParity);

			/// <summary> Ft set flow control. </summary>
			/// <param name="lngHandle">	  The handle. </param>
			/// <param name="intFlowControl"> The int flow control. </param>
			/// <param name="byXonChar">	  The by XON character. </param>
			/// <param name="byXoffChar">	  The by XOFF character. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetFlowControl(IntPtr lngHandle, short intFlowControl, byte byXonChar, byte byXoffChar);

			/// <summary> Ft reset device. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_ResetDevice(IntPtr lngHandle);

			/// <summary> Ft set dtr. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetDtr(IntPtr lngHandle);

			/// <summary> Ft colour dtr. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_ClrDtr(IntPtr lngHandle);

			/// <summary> Ft set RTS. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetRts(IntPtr lngHandle);

			/// <summary> Ft colour RTS. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_ClrRts(IntPtr lngHandle);

			/// <summary> Ft get modem status. </summary>
			/// <param name="lngHandle">	  The handle. </param>
			/// <param name="lngModemStatus"> [in,out] The modem status. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_GetModemStatus(IntPtr lngHandle, ref int lngModemStatus);

			/// <summary> Ft purge. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <param name="lngMask">   The mask. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_Purge(IntPtr lngHandle, int lngMask);

			/// <summary> Ft get status. </summary>
			/// <param name="lngHandle">	  The handle. </param>
			/// <param name="lngRXBytes">	  [in,out] The receive in bytes. </param>
			/// <param name="lngTXBytes">	  [in,out] The transmit in bytes. </param>
			/// <param name="lngEventsDWord"> [in,out] The events d word. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_GetStatus(IntPtr lngHandle, ref int lngRXBytes, ref int lngTXBytes, ref int lngEventsDWord);

			/// <summary> Ft get queue status. </summary>
			/// <param name="lngHandle">  The handle. </param>
			/// <param name="lngRXBytes"> [in,out] The receive in bytes. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_GetQueueStatus(IntPtr lngHandle, ref int lngRXBytes);

			/// <summary> Ft get event status. </summary>
			/// <param name="lngHandle">	  The handle. </param>
			/// <param name="lngEventsDWord"> [in,out] The events d word. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_GetEventStatus(IntPtr lngHandle, ref int lngEventsDWord);

			/// <summary> Ft set characters. </summary>
			/// <param name="lngHandle">		  The handle. </param>
			/// <param name="byEventChar">		  The by event character. </param>
			/// <param name="byEventCharEnabled"> The by event character enabled. </param>
			/// <param name="byErrorChar">		  The by error character. </param>
			/// <param name="byErrorCharEnabled"> The by error character enabled. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetChars(int lngHandle, byte byEventChar, byte byEventCharEnabled, byte byErrorChar, byte byErrorCharEnabled);

			/// <summary> Ft set timeouts. </summary>
			/// <param name="lngHandle">	   The handle. </param>
			/// <param name="lngReadTimeout">  The read timeout. </param>
			/// <param name="lngWriteTimeout"> The write timeout. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetTimeouts(IntPtr lngHandle, int lngReadTimeout, int lngWriteTimeout);

			/// <summary> Ft set break on. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetBreakOn(IntPtr lngHandle);

			/// <summary> Ft set break off. </summary>
			/// <param name="lngHandle"> The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_SetBreakOff(IntPtr lngHandle);

			/// <summary> Ft open ex. </summary>
			/// <param name="arg1">		 The first argument. </param>
			/// <param name="arg2">		 The second argument. </param>
			/// <param name="lngHandle"> [in,out] The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_OpenEx(string arg1, int arg2, ref IntPtr lngHandle);

			/// <summary> Ft create device information list. </summary>
			/// <param name="lngNumDevs"> [in,out] Number of devs. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_CreateDeviceInfoList(ref int lngNumDevs);

			/// <summary> [DllImport("FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling =
			/// true)] static private extern int FT_GetDeviceInfoDetail(int lngIndex, int lngFlags, int
			/// lngType, uint lngID, int lngLocId, string strSerialNumber, string strDescription, ref int
			/// lngHandle); </summary>
			/// <param name="lngIndex">		   The index. </param>
			/// <param name="lngFlags">		   [in,out] The flags. </param>
			/// <param name="lngType">		   [in,out] The type. </param>
			/// <param name="lngID">		   [in,out] The identifier. </param>
			/// <param name="lngLocId">		   [in,out] Identifier for the location. </param>
			/// <param name="strSerialNumber"> The serial number. </param>
			/// <param name="strDescription">  The description. </param>
			/// <param name="lngHandle">	   [in,out] The handle. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_GetDeviceInfoDetail(UInt32 lngIndex, ref UInt32 lngFlags, ref UInt32 lngType, ref UInt32 lngID, ref UInt32 lngLocId, byte[] strSerialNumber,
				                                                byte[] strDescription, ref IntPtr lngHandle );

			/// <summary> Ft get number devices. </summary>
			/// <param name="lpBuffer1">  [in,out] The first pointer to a buffer. </param>
			/// <param name="lpszBuffer"> The buffer. </param>
			/// <param name="lngFlags">   The flags. </param>
			/// <returns> . </returns>
			[DllImport( "FTD2XX.DLL", EntryPoint = "FT_ListDevices", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
			public static extern int FT_GetNumDevices(ref int lpBuffer1, string lpszBuffer, uint lngFlags);
		};

		#endregion

		#region private data
		private IntPtr _handle;
		#endregion

		#region static methods

		/// <summary> To return the number of devices. </summary>
		/// <exception cref="IOErrorException"> Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="GeneralException"> Thrown when a General error condition occurs. </exception>
		/// <returns> The number of devices connected. </returns>
		static public Int32 NumberOfDevices()
		{

			int numberOfDevices = 0;

			// Discover number of devices by creating the device list.
			Status returnCode = (Status)PortInternals.FT_CreateDeviceInfoList(ref numberOfDevices);

			// Test for error.
			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error occurred whilst trying to determine the numebr of devices.");
			}
			if (returnCode != Status.Ok)
			{
				// An unexpected error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to determine the number of devices.");
			}

			// Return number of USB nodes.
			return numberOfDevices;
		}

		/// <summary> Returns the serial number of the device at the specified index. </summary>
		/// <exception cref="IOErrorException"> Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="GeneralException"> Thrown when a General error condition occurs. </exception>
		/// <param name="deviceIndex"> Index of the connected device. </param>
		/// <returns> Device serial number. </returns>
		static public string DeviceSerialNumber(Int32 deviceIndex)
		{
			string buffer = string.Empty;
			buffer = buffer.PadRight(64);

			// Create device list to retreive device serial number.
			Status returnCode = (Status)PortInternals.FT_ListDevices(deviceIndex, buffer, (Int32)ListDevicesFlag.ByIndex + (Int32)OpenFlag.BySerialNumber);

			if (returnCode == Status.IOError)
			{
				throw new IOErrorException("An IO error occurred whilst trying to determine the device serial number.");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				throw new GeneralException("An unknown error occurred whilst trying to get the device serial number.");
			}

			return buffer;
		}

		/// <summary> Gets information on all of the FTDI devices available. </summary>
		/// <returns> Device information. </returns>
		static public DeviceInformationCollection AllDeviceInformation()
		{
			// Determine number of devices.
			int numberOfDevices = NumberOfDevices();

			DeviceInformationCollection allDeviceInformation = new DeviceInformationCollection();

			for (Int32 deviceIndex = 0; deviceIndex < numberOfDevices; deviceIndex++)
			{
				// Determine device information.
				allDeviceInformation.Add(DeviceInformation(deviceIndex));
			}

			return allDeviceInformation;
		}

		/// <summary> Returns true if a device with the given serial number is available. </summary>
		/// <exception cref="IOErrorException"> Thrown when an I/O Error error condition occurs. </exception>
		/// <param name="serialNumber"> Serial number of device to checked. </param>
		/// <returns> True if device is available. </returns>
		static public bool DeviceAvailable(string serialNumber)
		{

			DeviceInformationCollection allDeviceInformation;

			try
			{
				// Determine currently available devices.
				allDeviceInformation = AllDeviceInformation();
			}
			catch (IOErrorException ex)
			{
				// An IO error occurred whilst determining device information.
				throw new IOErrorException(string.Format(
					"Failed to determine if device with serial number {0} is present.", serialNumber), ex);
			}

			// Find a device with matching serial number.
			DeviceInformation deviceInformation = allDeviceInformation.Find(device => (device.SerialNumber == serialNumber));

			// Return true if found.
			return (deviceInformation != null);
		}

		/// <summary> Gets the description of the device. </summary>
		/// <exception cref="IOErrorException"> Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="GeneralException"> Thrown when a General error condition occurs. </exception>
		/// <param name="deviceIndex"> Index of the connected device. </param>
		/// <param name="description"> [out] Description of the device. </param>
		static public void DeviceDescription(Int32 deviceIndex, out string description)
		{
			string buffer = string.Empty;
			buffer = buffer.PadRight(64);

			// Determine device description.
			Status returnCode = (Status)PortInternals.FT_ListDevices(deviceIndex, buffer, (Int32)ListDevicesFlag.ByIndex + (Int32)OpenFlag.ByDescription);

			description = buffer;
			description = description.Substring(0, description.IndexOf("\0", StringComparison.InvariantCulture));

			if (returnCode == Status.IOError)
			{
				throw new IOErrorException("An IO error occurred whilst trying to determine the device description.");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to get the device description.");
			}
		}

		/// <summary> Returns information for the specified device. </summary>
		/// <exception cref="IOErrorException"> Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="GeneralException"> Thrown when a General error condition occurs. </exception>
		/// <param name="deviceIndex"> Index of the connected device. </param>
		/// <returns> Device information. </returns>
		static public DeviceInformation DeviceInformation(Int32 deviceIndex)
		{
			UInt32 flags = 0;
			UInt32 devicetype = 0;
			UInt32 deviceID = 0;
			UInt32 locationID = 0;
			byte[] serialNumber = new byte[16];
			byte[] description = new byte[64];
			IntPtr handle = IntPtr.Zero;

			// This function is used instead of ListDevices
			// as it is less prone to fail.
			Status returnCode = (Status)PortInternals.FT_GetDeviceInfoDetail((UInt32)deviceIndex, ref flags, ref devicetype, ref deviceID, ref locationID, serialNumber, description, ref handle);

			if (returnCode == Status.IOError)
			{
				throw new IOErrorException("An IO error occurred whilst trying to determine the device information.");
			}
			if (returnCode != Status.Ok)
			{
				throw new GeneralException("An unknown error occurred whilst trying to determine the device information.");
			}

			DeviceInformation information = new DeviceInformation
			{
				HighSpeed = (flags & (uint)StatusFlags.HighSpeed) != 0,
				ID = deviceID,
				Type = (DeviceType)devicetype,
				Opened = (flags & (uint)StatusFlags.Opened) != 0,
				SerialNumber = System.Text.Encoding.ASCII.GetString(serialNumber)
			};
			information.SerialNumber = information.SerialNumber.Substring(0, information.SerialNumber.IndexOf("\0", StringComparison.InvariantCulture));
			information.Description = System.Text.Encoding.ASCII.GetString(description);
			information.Description = information.Description.Substring(0, information.Description.IndexOf("\0", StringComparison.InvariantCulture));

			return information;
		}

		#endregion

		/// <summary> Default constructor. </summary>
		public FTDI()
		{
			_handle = IntPtr.Zero;
		}

		/// <summary> Constructor. </summary>
		/// <param name="serialNo"> The serial number of the associated device. </param>
		/// <remarks> This constructor will attempt to open the device</remarks>
		public FTDI(string serialNo)
		{
			Open(serialNo);
		}

		/// <summary> Gets a value indicating whether this object is connected. </summary>
		/// <value> true if this object is connected, false if not. </value>
		public bool IsConnected
		{
			get { return _handle != IntPtr.Zero; }
		}

		/// <summary> Gets or sets the information describing the device. </summary>
		/// <value> Information describing the device. </value>
		public DeviceInformation DeviceInfo { get; set; }

		/// <summary> Sends a reset command to the device. </summary>
		/// <exception cref="IOErrorException"> Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="GeneralException"> Thrown when a General error condition occurs. </exception>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool ResetDevice()
		{
			if(!IsConnected)
			{
				return false;
			}

			// Reset the device.
			Status returnCode = (Status)PortInternals.FT_ResetDevice(_handle);

			// Check for internal error.
			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error occurred whilst trying to reset the device.");
			}
			if (returnCode != Status.Ok)
			{
				// An unexpected error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to rest the device.");
			}
			return true;
		}

		/// <summary> Write data to an open FTDI device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="buffer">	    Array of data to be written to the device. </param>
		/// <param name="bytesToWrite"> Number of bytes from the array to be written. </param>
		/// <param name="bytesWritten"> [out] Bytes actually written to the device. </param>
		public bool Write(byte[] buffer, Int32 bytesToWrite, out Int32 bytesWritten)
		{
			bytesWritten = 0;

			if(!IsConnected)
			{
				return false;
			}


			// Write the data.
			Status returnCode = (Status)PortInternals.FT_Write(_handle, ref buffer[0], bytesToWrite, ref bytesWritten);

			if (returnCode == Status.IOError)
			{
				// An IO error occurred during the attempted write. 
				throw new IOErrorException(string.Format(
					"An IO error occurred whilst trying to write data to the device. {0} bytes written.",
					bytesWritten));
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to write to a device. 
				throw new InvalidHandleException(string.Format(
				                                                    "An invalid device handle was used to write to a device. {0} bytes written.",
				                                                    bytesWritten));
			}
			if (returnCode != Status.Ok)
			{
				// An unknown error occurred, that is not documented by FTDI.
				throw new GeneralException("An unknown error occurred during the wrtie attempt.");
			}

			return bytesToWrite == bytesWritten;
		}

		/// <summary> Asserts the Request To Send (RTS) line. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool SetRts()
		{
			if (!IsConnected)
			{
				return false;
			}

			// Set thr RTS line.
			Status returnCode = (Status)PortInternals.FT_SetRts(_handle);

			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error whilst trying to assert the RTS line.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to assert the RTS line. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to assert the RTS line.");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to assert the RTS line.");
			}
			return true;
		}

		/// <summary> Sets the data characteristics of the device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="dataBits"> The number of data bits. </param>
		/// <param name="stopBits"> The number of stop bits. </param>
		/// <param name="parity">   The parity of the data. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool SetDataCharacteristics(DataBits dataBits, StopBits stopBits, Parity parity)
		{
			if (!IsConnected)
			{
				return false;
			}

			// Set the data characteristics.
			Status returnCode = (Status)PortInternals.FT_SetDataCharacteristics(_handle, (byte)dataBits, (byte)stopBits, (byte)parity);

			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error occurred whilst trying to set the data characteristics.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to set the data characteristics. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to set the data charcateristics.");
			}
			if (returnCode != Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to set the data characteristics.");
			}
			return true;
		}

		/// <summary> Sets the read and write timeouts for the device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="readTimeout">  Read time out period in milli seconds. </param>
		/// <param name="writeTimeOut"> Write time out period in milliseconds. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool SetTimeOuts(Int32 readTimeout, Int32 writeTimeOut)
		{
			if (!IsConnected)
			{
				return false;
			}

			// Set the time out times.
			Status returnCode = (Status)PortInternals.FT_SetTimeouts(_handle, readTimeout, writeTimeOut);

			// Check return code.
			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error occurred whilst trying to set the time periods.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to set the time out periods. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to set the time out periods.");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to set the time out periods.");
			}
			return true;
		}

		/// <summary> Stes the data flow control for the device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="flow"> The type of flow control. </param>
		/// <param name="xOn">  The Xon character for Xon/Xoff flow control. </param>
		/// <param name="xOff"> The Xoff character for Xon/Xoff flow control. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public bool SetFlowControl(FlowControl flow, byte xOn, byte xOff)
		{
			if (!IsConnected)
			{
				return false;
			}

			// Set the flow control.
			Status returnCode = (Status)PortInternals.FT_SetFlowControl(_handle, (Int16)flow, xOn, xOff);

			// Check return code.
			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error occurred whilst trying to set the flow control.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to set the time out periods. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to set the flow control.");
			}
			if (returnCode != Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to set the flow control.");
			}
			return true;
		}

		/// <summary> Sets the baud rate of the device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="baudRate"> The desired Baud rate for the device. </param>
		public bool SetBaudRate(Int32 baudRate)
		{
			if (!IsConnected)
			{
				return false;
			}

			// Set baud rate.
			Status returnCode = (Status)PortInternals.FT_SetBaudRate(_handle, baudRate);

			// Check return code.
			if (returnCode ==Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException("An IO error occurred whilst trying to set the baud rate.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to set the baud rate. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to set the baud rate.");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException("An unknown error occurred whilst trying to set the baud rate.");
			}
			return true;
		}

		/// <summary> Opens the FTDI device with the specified serial number. </summary>
		/// <exception cref="IOErrorException">		   Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="DeviceNotFoundException"> Thrown when a Device Not Found error condition
		/// 										   occurs. </exception>
		/// <exception cref="GeneralException">		   Thrown when a General error condition occurs. </exception>
		/// <param name="serialNumber"> Serial number of the device to open. </param>
		public bool Open(string serialNumber)
		{
			if(IsConnected)
			{
				return true;
			}

			_handle = IntPtr.Zero;

			// Open comms channel.
			Status returnCode = (Status)PortInternals.FT_OpenEx(serialNumber, (Int32)OpenFlag.BySerialNumber, ref _handle);

			// Check return code.
			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException(string.Format(
					"An IO error occurred whilst trying to open device with serial number '{0}'.", serialNumber));
			}
			if (returnCode == Status.DeviceNotFound)
			{
				// Device not found.
				throw new DeviceNotFoundException(string.Format(
					"Device with serial number {0} not found.", serialNumber));
			}
			if (returnCode != Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException(string.Format(
					"An unknown error occurred whilst trying to open device with serial number '{0}'.", serialNumber));
			}
			return IsConnected;
		}

		/// <summary> Gets the number of bytes available in the receive buffer. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <returns> true if it succeeds, false if it fails. </returns>
		public Int32 NumberOfBytesInRxBuffer()
		{

			if (!IsConnected)
			{
				return 0;
			}

			Int32 numberOfBytesInQueue = 0;

			// Determine number of bytes in receive buffer.
			Status returnCode = (Status)PortInternals.FT_GetQueueStatus(_handle, ref numberOfBytesInQueue);

			// Check return code.
			if (returnCode == Status.IOError)
			{
				// An IO error occurred.
				throw new IOErrorException(
					"An IO error occurred whilst trying to determine the number of bytes in the receive buffer.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to set the baud rate. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to determine the number of bytes in the receive buffer.");
			}
			if (returnCode != Status.Ok)
			{
				// An undocumented error occurred.
				throw new GeneralException(
					"An unknown error occurred whilst trying to determine the number of bytes in the receive buffer.");
			}
			return numberOfBytesInQueue;
		}

		/// <summary> Purge data from the devices transmit and/or receive buffers. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="flags">  Flags for purge operation. </param>
		public bool Purge(PurgeFlags flags)
		{
			if (!IsConnected)
			{
				return false;
			}

			// Purge the buffers.
			Status returnCode = (Status)PortInternals.FT_Purge(_handle, (Int32)flags);

			// Test return code for errors.
			if (returnCode == Status.IOError)
			{
				throw new IOErrorException("An IO error occurred whilst trying to purge the device.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to purge the device. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to purge the device buffer(s).");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				throw new GeneralException("An unknown error occurred whilst trying to purge the device.");
			}
			return true;
		}

		/// <summary> Read data from an open FTDI device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		/// <param name="buffer">			   [in,out] Destination for data read. </param>
		/// <param name="numberOfBytesToRead"> Number of bytes to be read. </param>
		/// <param name="numberOfBytesRead">   [out] Actual number of bytes read from buffer. </param>
		public bool Read(ref byte[] buffer, Int32 numberOfBytesToRead, out Int32 numberOfBytesRead)
		{
			Int32 actualNumberOfBytesRead = 0;
			numberOfBytesRead = 0;

			if (!IsConnected)
			{
				return false;
			}

			// Read data from buffer.
			Status returnCode = (Status)PortInternals.FT_Read(_handle, ref buffer[0], numberOfBytesToRead, ref actualNumberOfBytesRead);

			numberOfBytesRead = actualNumberOfBytesRead;

			// Test return code for errors.
			if (returnCode == Status.IOError)
			{
				throw new IOErrorException("An IO error occurred whilst trying to read the device receive buffer.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to purge the device. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to readthe device receive buffer.");
			}
			if (returnCode != Status.Ok)
			{
				throw new GeneralException(
					"An unknown error occurred whilst trying to read the device receive buffer.");
			}
			return true;
		}

		/// <summary> Closes the handle to an open FTDI device. </summary>
		/// <exception cref="IOErrorException">		  Thrown when an I/O Error error condition occurs. </exception>
		/// <exception cref="InvalidHandleException"> Thrown when an Invalid Handle error condition occurs. </exception>
		/// <exception cref="GeneralException">		  Thrown when a General error condition occurs. </exception>
		public bool Close()
		{
			if (!IsConnected)
			{
				return false;
			}

			// Close the FTDI device.
			Status returnCode = (Status)PortInternals.FT_Close(_handle);

			// Test return code for errors.
			if (returnCode == Status.IOError)
			{
				throw new IOErrorException("An IO error occurred whilst trying to close the device.");
			}
			if (returnCode == Status.InvalidHandle)
			{
				// A device handle became invalid whilst trying to purge the device. 
				throw new InvalidHandleException(
					"The device hanlde was invalid when trying to close the device.");
			}
			if (returnCode != (Int32)Status.Ok)
			{
				throw new GeneralException(
					"An unknown error occurred whilst trying to close the device.");
			}
			return true;
		}
	}
}
