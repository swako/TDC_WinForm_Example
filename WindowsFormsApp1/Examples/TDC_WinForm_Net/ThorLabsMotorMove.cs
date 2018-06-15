using System.Threading;

namespace TDC_WinForm_Net
{
	/// <summary> Thorlabs motor move. </summary>
	/// <seealso cref="T:TDC_WinForm_Net.ThorlabsMotor"/>
	public class ThorlabsMotorMove : ThorlabsMotor
	{
		/// <summary> Constructor. </summary>
		/// <param name="device"> The device. </param>
		public ThorlabsMotorMove( ThorlabsDevice device )
			: base(device)
		{
		}

		/// <summary> Move relative the given number of steps. </summary>
		/// <param name="moveStep"> The number of steps to move. </param>
		/// <returns> success message. </returns>
		public string MoveRelative(int moveStep)
		{
			// Start moving
			bool complete;
			if (!StartMoving(moveStep, out complete))
			{
				return "Error Jogging device";
			}
			if (complete)
			{
				return string.Format("Moved {0} steps", moveStep);
			}
			Thread.Sleep(50);

			// wait for move to complete
			WaitForMovingComplete();

			return string.Format("Moved {0} steps", moveStep);
		}

		/// <summary> Starts a move operation. </summary>
		/// <param name="step">	    Amount to increment by. </param>
		/// <param name="complete"> [out] The complete. </param>
		/// <returns> true if it succeeds, false if it fails. </returns>
		private bool StartMoving(int step, out bool complete)
		{
			complete = false;

			// gets the MotorMoveRelative command structure as a byte[] array
			MotorMoveRelative motorMessage = new MotorMoveRelative(step);
			byte[] byteArray = Utilities.SerializeMessage(motorMessage);

			// flush any existing messages and send move message
			_device.FlushMessages();
			if (!_device.SendCommand(byteArray))
			{
				return false;
			}

			// get a Thorlabs simple message to request a status as a byte[] array
			MessageStruct statusMessage = ThorlabsDevice.SimpleMessageStruct(DeviceMessages.MGMSG_MOT_REQ_DCSTATUSUPDATE); // NOTE - some devices use 0x0480 as status request
			byteArray = Utilities.SerializeMessage(statusMessage);

			// loop until device is moving
			bool started = false;
			while (!started && !complete)
			{
				// request status
				_device.SendCommand(byteArray);

				// get any response (maybe a status response or a move complete response)
				byte[] response = _device.WaitForUnknownReply(250);
				// get response as a message structure
				object returnObject = ObjectFromData(response);
				if (returnObject != null)
				{
					// if a motor status then check for moving bit
					if (returnObject is MotorStatus)
					{
						started = ((((MotorStatus)returnObject)._status & 0x00f0) != 0);
					}
					// if a simple message structure then check for move complete message
					if (returnObject is MessageStruct)
					{
						complete = ((MessageStruct)returnObject)._messageId == DeviceMessages.MGMSG_MOT_MOVE_COMPLETE;
					}
				}
			}

			return true;
		}

		/// <summary> Wait for moving complete. </summary>
		/// <returns> true if it succeeds, false if it fails. </returns>
		private bool WaitForMovingComplete()
		{
			// get a Thorlabs simple message to request a status as a byte[] array
			MessageStruct statusMessage = ThorlabsDevice.SimpleMessageStruct(DeviceMessages.MGMSG_MOT_REQ_DCSTATUSUPDATE); // NOTE - some devices use 0x0480 as status request
			byte[] byteArray = Utilities.SerializeMessage(statusMessage);

			// loop until move complete
			bool moved = false;
			while (!moved)
			{
				// send status request command
				_device.SendCommand(byteArray);

				// get any response (maybe a status response or a move complete response)
				byte[] response = _device.WaitForUnknownReply(250);
				// get response as a message structure
				object returnObject = ObjectFromData(response);
				if (returnObject != null)
				{
					// if a motor status then check for moving bit
					if (returnObject is MotorStatus)
					{
						moved = ((((MotorStatus)returnObject)._status & 0x00f0) == 0);
					}
					// if a simple message structure then check for move complete message
					if (returnObject is MessageStruct)
					{
						moved = ((MessageStruct)returnObject)._messageId == DeviceMessages.MGMSG_MOT_MOVE_COMPLETE;
					}
				}
			}
			return true;
		}
	}
}
