using System;

namespace PacketMessaging.Services.CommunicationsService
{
	public class SerialPortException : Exception
	{
		string _message;
		Exception _innerException;

		public string Message => _message;

		//
		// Summary:
		//     Initializes a new instance of the SerialPortException class with
		//     a system-supplied error message.
		public SerialPortException()
		{
			_message = "Unspecified serial port exception";
		}

		//
		// Summary:
		//     Initializes a new instance of the SerialPortException class with
		//     a specified error message.
		//
		// Parameters:
		//   message:
		//     A System.String that describes the error.
		public SerialPortException(string message)
		{
			_message = message;
		}

		//
		// Summary:
		//     Initializes a new instance of the System.OperationCanceledException class with
		//     a specified error message and a reference to the inner exception that is the
		//     cause of this exception.
		//
		// Parameters:
		//   message:
		//     The error message that explains the reason for the exception.
		//
		//   innerException:
		//     The exception that is the cause of the current exception. If the innerException
		//     parameter is not null, the current exception is raised in a catch block that
		//     handles the inner exception.
		public SerialPortException(string message, Exception innerException)
		{
			_message = message;
			_innerException = innerException;
		}
	}
}
