using System.Threading;

namespace TDC_WinForm_Net
{
	/// <summary> Thorlabs motor home. </summary>
	/// <seealso cref="T:TDC_WinForm_Net.ThorlabsMotor"/>
	public class ThorlabsMotorHome : ThorlabsMotor
	{
		/// <summary> Constructor. </summary>
		/// <param name="device"> The device. </param>
		public ThorlabsMotorHome( ThorlabsDevice device )
			: base(device)
		{
		}

		/// <summary> Gets the home device. </summary>
		/// <returns> . </returns>
		public string HomeDevice()
		{
			bool complete;
			// start the homing operation
			if(!StartHoming(out complete))
			{
				return "Error Homing device";
			}
			if(complete)
			{
				return "Homed";
			}
			Thread.Sleep(50);

			// wait for the homing operation to complete
			WaitForHomimgComplete();

			return "Homed";
		}

		/// <summary> Starts a homing operation. </summary>
		/// <param name="complete"> [out] The complete. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		private bool StartHoming(out bool complete)
		{
			complete = false;
			// gets the MotorHome command structure as a byte[] array
			MessageStruct message = ThorlabsDevice.SimpleMessageStruct(DeviceMessages.MGMSG_MOT_MOVE_HOME);
			byte[] byteArray = Utilities.SerializeMessage(message);

			// flush any existing messages and send move message
			_device.FlushMessages();
			if (!_device.SendCommand(byteArray))
			{
				return false;
			}

			// get a Thorlabs simple message to request a status as a byte[] array
			MessageStruct statusMessage = ThorlabsDevice.SimpleMessageStruct(DeviceMessages.MGMSG_MOT_REQ_DCSTATUSUPDATE);
			byteArray = Utilities.SerializeMessage(statusMessage);

			// loop until device is homing
			bool started = false;
			while(!started && !complete)
			{
				// request status
				_device.SendCommand(byteArray);

				// get any response (maybe a status response or a homed response)
				byte[] response = _device.WaitForUnknownReply(250);
				// get response as a message structure
				object returnObject = ObjectFromData(response);
				if(returnObject != null)
				{
					// if a motor status then check the homing bit
					if (returnObject is MotorStatus)
					{
						started = ((((MotorStatus)returnObject)._status & 0x0200) != 0);
					}
					// if a simple message structure then check for homed message
					if (returnObject is MessageStruct)
					{
						complete = ( (MessageStruct)returnObject )._messageId == DeviceMessages.MGMSG_MOT_MOVE_HOMED;
					}
				}
			}

			return true;
		}

		/// <summary> Wait for homing to complete. </summary>
		/// <returns> true if it succeeds, false if it fails. </returns>
		private bool WaitForHomimgComplete()
		{
			// get a Thorlabs simple message to request a status as a byte[] array
			MessageStruct statusMessage = ThorlabsDevice.SimpleMessageStruct(DeviceMessages.MGMSG_MOT_REQ_DCSTATUSUPDATE);
			byte[] byteArray = Utilities.SerializeMessage(statusMessage);

			// loop until homed complete
			bool homed = false;
			while (!homed)
			{
				// send status request command
				_device.SendCommand(byteArray);

				// get any response (maybe a status response or a homed response)
				byte[] response = _device.WaitForUnknownReply(250);
				// get response as a message structure
				object returnObject = ObjectFromData(response);
				if (returnObject != null)
				{
					// if a motor status then check the homing bit
					if (returnObject is MotorStatus)
					{
						homed = ((((MotorStatus)returnObject)._status & 0x0400) != 0);
					}
					// if a simple message structure then check for homed message
					if (returnObject is MessageStruct)
					{
						homed = ((MessageStruct)returnObject)._messageId == DeviceMessages.MGMSG_MOT_MOVE_HOMED;
					}
				}
			}
			return true;
		}
	}
}
