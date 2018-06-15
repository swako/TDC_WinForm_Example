// file:	ThorlabsStructures.cs
//
// summary:	Implements the thorlabs structures class
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TDC_WinForm_Net
{
	/// <summary> Device messages. </summary>
	/// <remarks> Refer to APT Documentation</remarks>
	public static class DeviceMessages
	{
		public const UInt16 MGMSG_HW_REQ_INFO = 0x0005;
		public const UInt16 MGMSG_HW_GET_INFO = 0x0006;
		public const UInt16 MGMSG_MOT_MOVE_HOME = 0x0443;
		public const UInt16 MGMSG_MOT_MOVE_HOMED = 0x0444;
		public const UInt16 MGMSG_MOT_MOVE_RELATIVE = 0x0448;
		public const UInt16 MGMSG_MOT_MOVE_COMPLETE = 0x0464;
		public const UInt16 MGMSG_MOT_REQ_STATUSUPDATE = 0x0480;
		public const UInt16 MGMSG_MOT_GET_STATUSUPDATE = 0x0481;
		public const UInt16 MGMSG_MOT_REQ_DCSTATUSUPDATE = 0x0490;
		public const UInt16 MGMSG_MOT_GET_DCSTATUSUPDATE = 0x0491;
	};

	/// <summary> Message structure. </summary>
	[StructLayout( LayoutKind.Explicit, Pack = 1 )]
	public struct MessageStruct
	{
		/// <summary> Message ID. </summary>
		[FieldOffset(0)]
		public UInt16 _messageId;

		/// <summary> Parameter1. </summary>
		[FieldOffset(2)]
		public byte _param1;

		/// <summary> Parameter2. </summary>
		[FieldOffset(3)]
		public byte _param2;

		/// <summary> Destination Address. </summary>
		[FieldOffset(4)]
		public byte _destination;

		/// <summary> Source address, normally 0x01. </summary>
		[FieldOffset(5)]
		public byte _source;

		/// <summary> Length of the packet. </summary>
		[FieldOffset(2)]
		public UInt16 _packetLength;
	};

	/// <summary> Message structure. </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GetHardwareInfo
	{
		/// <summary> Identifier for the message. </summary>
		public MessageStruct _header;
		/// <summary> The serial no. </summary>
		public UInt32 _serialNo;
		/// <summary> The model. </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] _model;
		/// <summary> The type. </summary>
		public UInt16 _type;
		/// <summary> The version. </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] _version;
		/// <summary> The notes. </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
		public byte[] _notes;
		/// <summary> The empty. </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
		public byte[] _empty;
		/// <summary> The hardware version. </summary>
		public UInt16 _hardwareVersion;
		/// <summary> State of the modifier. </summary>
		public UInt16 _modState;
		/// <summary> Number of channels. </summary>
		public UInt16 _numberOfChannels;

		/// <summary> Returns the fully qualified type name of this instance. </summary>
		/// <returns> A <see cref="T:System.String"/> containing a fully qualified type name. </returns>
		/// <seealso>
		///     <cref>M:System.ValueType.ToString()</cref>
		/// </seealso>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Serial Number = {0}{1}", _serialNo, Environment.NewLine);
			sb.AppendFormat("Model = {0}{1}", Encoding.ASCII.GetString(_model).TrimEnd('\0'), Environment.NewLine);
			sb.AppendFormat("Type = {0}{1} ", _type, Environment.NewLine);
			sb.AppendFormat("Version = {0}.{1}.{2}.{3}{4}", _version[0], _version[1], _version[2], _version[3], Environment.NewLine);
			sb.AppendFormat("Hardware Version = {0}.{1}{2}", (byte)(_hardwareVersion >> 8), (byte)(_hardwareVersion & 0xFF), Environment.NewLine);
			sb.AppendFormat("Mod State = {0}{1}", _modState, Environment.NewLine);
			sb.AppendFormat("Notes = {0}{1}", Encoding.ASCII.GetString(_notes).TrimEnd('\0'), Environment.NewLine);
			sb.AppendFormat("Number of Channels = {0}{1}", _numberOfChannels, Environment.NewLine);
			return sb.ToString();
		}
	};

	/// <summary> Message structure. </summary>
	/// <remarks> This structure is used for both DCServo and Stepper controller messages which have slightly different formats.
	/// 		  DCServo uses a 16 bit velocity field whereas Stepper controllers use 32bit encoder field.</remarks>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct MotorStatus
	{
		/// <summary> Identifier for the message. </summary>
		[FieldOffset(0)]
		public MessageStruct _header;
		/// <summary> The channel. </summary>
		[FieldOffset(6)]
		public UInt16 _channel;
		/// <summary> The position. </summary>
		[FieldOffset(8)]
		public Int32 _position;
		/// <summary> The velocity. </summary>
		/// <remarks> DC Servo only</remarks>
		[FieldOffset(12)]
		public UInt16 _velocity;
		/// <summary> The unused. </summary>
		/// <remarks> DC Servo only</remarks>
		[FieldOffset(14)]
		public UInt16 _unused;
		/// <summary> The status. </summary>
		[FieldOffset(16)]
		public UInt32 _status;
		/// <summary> The encoder. </summary>
		/// <remarks> Stepper Controller only</remarks>
		[FieldOffset(12)]
		public UInt16 _encoder;

		/// <summary> Returns the fully qualified type name of this instance. </summary>
		/// <returns> A <see cref="T:System.String"/> containing a fully qualified type name. </returns>
		/// <seealso>
		///     <cref>M:System.ValueType.ToString()</cref>
		/// </seealso>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Position = {0}{1}", _position, Environment.NewLine);
			sb.AppendFormat("Velocity = {0}{1}", _velocity, Environment.NewLine);
			sb.AppendFormat("Status = {0:x}{1} ", _status, Environment.NewLine);
			return sb.ToString();
		}
	};

	/// <summary> Message structure. </summary>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct MotorMoveRelative
	{
		/// <summary> Constructor. </summary>
		/// <param name="distance"> The distance. </param>
		public MotorMoveRelative(Int32 distance)
		{
			_channel = 1;
			_header = ThorlabsDevice.SimpleMessageStruct(DeviceMessages.MGMSG_MOT_MOVE_RELATIVE);
			_header._packetLength = 6;
			_header._destination |= 0x80;
			_distance = distance;
		}

		/// <summary> Identifier for the message. </summary>
		[FieldOffset(0)]
		public MessageStruct _header;
		/// <summary> The channel. </summary>
		[FieldOffset(6)]
		public UInt16 _channel;
		/// <summary> The distance. </summary>
		[FieldOffset(8)]
		public Int32 _distance;

		/// <summary> Returns the fully qualified type name of this instance. </summary>
		/// <returns> A <see cref="T:System.String"/> containing a fully qualified type name. </returns>
		/// <seealso>
		///     <cref>M:System.ValueType.ToString()</cref>
		/// </seealso>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Distance = {0}{1}", _distance, Environment.NewLine);
			return sb.ToString();
		}
	};
}