
using System;

using Windows.UI.Core;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

using MetroLog;
using System.Threading;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using PacketMessaging.Views;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PacketMessaging.Models;

namespace PacketMessaging.Services.CommunicationsService
{
	class SerialPort : CommunicationsServiceBase, IDisposable
	{
		private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<SerialPort>();

		// Pointer back to the main page
		private MainPage rootPage = MainPage.Current;

		// Track Read Operation
		private CancellationTokenSource _readCancellationTokenSource;
		private Object _ReadCancelLock = new Object();
		DataReader _dataReaderObject = null;

		string _readBytesBuffer = "";

		// Track Write Operation
		private CancellationTokenSource _writeCancellationTokenSource;
		private Object _WriteCancelLock = new Object();
		DataWriter _dataWriteObject = null;

		SerialDevice _serialDevice = null;


		public SerialPort() : base(log)
		{
			_serialDevice = EventHandlerForDevice.Current.Device;

			ResetReadCancellationTokenSource();
			ResetWriteCancellationTokenSource();
		}


		public TimeSpan ReadTimeout
		{
			get => EventHandlerForDevice.Current.Device.ReadTimeout;
			set => EventHandlerForDevice.Current.Device.ReadTimeout = value;
		}

		public TimeSpan WriteTimeout
		{
			get => EventHandlerForDevice.Current.Device.WriteTimeout;
			set => EventHandlerForDevice.Current.Device.WriteTimeout = value;
		}


		/// <summary>
		/// Print a status message saying we are canceling a task and disable all buttons to prevent multiple cancel requests.
		/// <summary>

		//public void ClearInputBuffer()
		//{
		//	EventHandlerForDevice.Current.Device.
		//}

		private void CancelReadTask()
		{
			lock (_ReadCancelLock)
			{
				if (_readCancellationTokenSource != null)
				{
					if (!_readCancellationTokenSource.IsCancellationRequested)
					{
						_readCancellationTokenSource.Cancel();

						// Existing IO already has a local copy of the old cancellation token so this reset won't affect it
						ResetReadCancellationTokenSource();
					}
				}
			}
		}

		private void CancelWriteTask()
		{
			lock (_WriteCancelLock)
			{
				if (_writeCancellationTokenSource != null)
				{
					if (!_writeCancellationTokenSource.IsCancellationRequested)
					{
						_writeCancellationTokenSource.Cancel();

						// Existing IO already has a local copy of the old cancellation token so this reset won't affect it
						ResetWriteCancellationTokenSource();
					}
				}
			}
		}
		private void CancelAllIoTasks()
		{
			CancelReadTask();
			CancelWriteTask();
		}

		private void CancelRead()
		{
			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				CancelReadTask();
				LogHelper(LogLevel.Info, $"Cancel Read {EventHandlerForDevice.Current.Device}");
			}
			else
			{
				Utilities.NotifyDeviceNotConnectedAsync();
				LogHelper(LogLevel.Info, $"Cancel Read. Device not connected. {EventHandlerForDevice.Current.Device}");
			}
		}

		private void CancelWrite()
		{
			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				CancelWriteTask();
				LogHelper(LogLevel.Info, $"Cancel Write {EventHandlerForDevice.Current.Device}");
			}
			else
			{
				Utilities.NotifyDeviceNotConnectedAsync();
				LogHelper(LogLevel.Info, $"Cancel Write. Device not connected {EventHandlerForDevice.Current.Device}");

			}
		}

		private async Task<string> ReadAsync(CancellationToken cancellationToken, string readToIncluding)
		{
			string readBytes = "";
			_readBytesBuffer = "";
			bool endOfMessage = false;
			Task<UInt32> loadAsyncTask;

			uint readBufferLength = 1024;
			_dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);

			while (!endOfMessage)
			{
				// Don't start any IO if we canceled the task
				lock (_ReadCancelLock)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// Cancellation Token will be used so we can stop the task operation explicitly
					// The completion function should still be called so that we can properly handle a canceled task
					_dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

					loadAsyncTask = _dataReaderObject.LoadAsync(readBufferLength).AsTask(cancellationToken);
				}
				uint bytesRead = await loadAsyncTask;
				if (bytesRead > 0)
				{
					_readBytesBuffer += _dataReaderObject.ReadString(bytesRead);
				}
				readBytes = _readBytesBuffer;
				LogHelper(LogLevel.Info, $"count: {bytesRead} Read: {readBytes}");

				if (!string.IsNullOrEmpty(readToIncluding))
				{
					int index = _readBytesBuffer.IndexOf(readToIncluding);
					if (index >= 0)
					{
						readBytes = _readBytesBuffer.Substring(0, index + readToIncluding.Length);
						_readBytesBuffer = _readBytesBuffer.Substring(index + readToIncluding.Length);
						LogHelper(LogLevel.Info, $"ReadBytesBuffer: {_readBytesBuffer}");
						endOfMessage = true;
					}
				}
				else 
				{
					endOfMessage = true;
					continue;
				}
			}
			readBytes = readBytes.Replace('\0', ' ');
			LogHelper(LogLevel.Info, $"Read: {readBytes}");
			return readBytes;
		}

		private async Task WriteAsync(CancellationToken cancellationToken, string stringToWrite)
		{
			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				if (!string.IsNullOrEmpty(stringToWrite))
				{
					Task<UInt32> storeAsyncTask;

					char[] buffer = new char[stringToWrite.Length];
					stringToWrite.CopyTo(0, buffer, 0, stringToWrite.Length);
					String InputString = new string(buffer);
					_dataWriteObject.WriteString(InputString);
					stringToWrite = "";

					// Don't start any IO if we canceled the task
					lock (_WriteCancelLock)
					{
						cancellationToken.ThrowIfCancellationRequested();

						// Cancellation Token will be used so we can stop the task operation explicitly
						// The completion function should still be called so that we can properly handle a canceled task
						storeAsyncTask = _dataWriteObject.StoreAsync().AsTask(cancellationToken);
					}

					UInt32 bytesWritten = await storeAsyncTask;
					if (bytesWritten > 0)
					{
						//LogHelper(LogLevel.Info, $"WriteAsync(): {bytesWritten.ToString()}");
						//writeBytes += InputString.Substring(0, (int)bytesWritten) + '\n';
					}
				}
			}
			else
			{
				LogHelper(LogLevel.Warn, $"{_serialDevice.PortName} is not connected");
				await Utilities.ShowMessageDialogAsync($"{_serialDevice.PortName} is not connected");
			}
		}

		public async Task<string> ReadToTimeoutAsync() => await ReadAsync("");		// This is mostly for debugging

		public async Task<string> ReadLineAsync() => await ReadToAsync("\r");

		public async Task<string> ReadToAsync(string text) => await ReadAsync(text);

		public async Task WriteLineAsync(string s)
		{
			await WriteAsync(s + "\r");
		}

		public async Task<string> ReadAsync(string readToIncluding = null)
		{
			string readString = "";

			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					_readCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.ReadTimeout);
					readString = await ReadAsync(_readCancellationTokenSource.Token, readToIncluding);
				}
				catch (OperationCanceledException)
				{
					OperationCanceledException exception = new OperationCanceledException("Read timeout");
					LogHelper(LogLevel.Fatal, $"Read timeout");
					throw new SerialPortException("Read timeout");
				}
				catch (Exception e)
				{
					LogHelper(LogLevel.Fatal, $"{e.Message.ToString()}");
					throw new SerialPortException( e.Message);
				}
				finally
				{
					ResetReadCancellationTokenSource();
					_dataReaderObject.DetachStream();
					_dataReaderObject = null;
				}
			}
			else
			{
				await Utilities.ShowMessageDialogAsync("Device not connected");
				LogHelper(LogLevel.Error, $"Device not connected");
			}

			//LogHelper(LogLevel.Info, $"Read: {readString}");

			return readString;
		}

		public async Task WriteWithEchoAsync(string s)
		{
			string echoString = "";
			foreach (char c in s)
			{
				await WriteAsync(c.ToString());
				string echoChar = await ReadCharAsync();
				echoString += echoChar;
			}
			LogHelper(LogLevel.Info, $"WriteWithEcho - {echoString}");
		}


		public async Task WriteAsync(string s)
		{
			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					//LogHelper(LogLevel.Info, $"Write {s}");

					_dataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
					_writeCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.WriteTimeout);
					await WriteAsync(_writeCancellationTokenSource.Token, s);
				}
				catch (OperationCanceledException )
				{
					//NotifyWriteTaskCanceledAsync();
					LogHelper(LogLevel.Info, $"WriteAsync has been cancelled");
					throw new SerialPortException("WriteAsync has been cancelled");
				}
				catch (Exception exception)
				{
					LogHelper(LogLevel.Fatal, $"{exception.Message}");
					throw new SerialPortException(exception.Message);
				}
				finally
				{
					ResetWriteCancellationTokenSource();
					_dataWriteObject.DetachStream();
					_dataWriteObject = null;
				}
			}
			else
			{
				await Utilities.ShowMessageDialogAsync($"{_serialDevice.PortName} is not connected");
			}
		}

		private async Task<string> ReadCharAsync(CancellationToken cancellationToken)
		{
			Task<UInt32> loadAsyncTask;

			uint ReadBufferLength = 1;
			string stringRead = "";

			// Don't start any IO if we canceled the task
			lock (_ReadCancelLock)
			{
				cancellationToken.ThrowIfCancellationRequested();

				// Cancellation Token will be used so we can stop the task operation explicitly
				// The completion function should still be called so that we can properly handle a canceled task
				_dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
				loadAsyncTask = _dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
			}
			UInt32 bytesRead = await loadAsyncTask;
			if (bytesRead > 0)
			{
				stringRead = _dataReaderObject.ReadString(bytesRead);
			}
			return stringRead;
		}

		private async Task<string> ReadAsync(CancellationToken cancellationToken)
		{
			Task<UInt32> loadAsyncTask;

			uint ReadBufferLength = 256;
			uint totalBytesRead = 0;
			string stringRead = "";
			bool endFound = false;

			//LogHelper(LogLevel.Trace, $"ReadAsync()");
			while (!endFound)
			{
				// Don't start any IO if we canceled the task
				lock (_ReadCancelLock)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// Cancellation Token will be used so we can stop the task operation explicitly
					// The completion function should still be called so that we can properly handle a canceled task
					_dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
					loadAsyncTask = _dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
				}				
				UInt32 bytesRead = await loadAsyncTask;
				if (bytesRead > 0)
				{
					stringRead += _dataReaderObject.ReadString(bytesRead);
					totalBytesRead += bytesRead;
					if (stringRead.EndsWith("cmd:"))
					{
						endFound = true;
					}
				}
			}
			LogHelper(LogLevel.Info, $"Read completed - {totalBytesRead.ToString()}, {stringRead}");
			return stringRead;
		}

		public async Task<string> ReadCharAsync()
		{
			string readString = "";

			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					_dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);

					_readCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.ReadTimeout);
					readString = await ReadCharAsync(_readCancellationTokenSource.Token);
				}
				catch (OperationCanceledException /*exception*/)
				{
					LogHelper(LogLevel.Fatal, $"Read timeout");
					throw new SerialPortException("Read timeout");
				}
				catch (Exception exception)
				{
					LogHelper(LogLevel.Info, exception.Message.ToString() + $" {EventHandlerForDevice.Current.Device.PortName}");
					throw new SerialPortException(exception.Message);
				}
				finally
				{
					ResetReadCancellationTokenSource();
					_dataReaderObject.DetachStream();
					//_dataReaderObject.Dispose();
					_dataReaderObject = null;
				}

			}
			else
			{
				//Utilities.ShowMessageDialogAsync($"{_tncDevice.CommPort.Comport} is not connected");
				LogHelper(LogLevel.Error, $"{_serialDevice.PortName} is not connected");
			}

			//LogHelper(LogLevel.Info, $"Buffer: {readString}");

			return readString;
		}

		public async Task<string> ReadBufferAsync()
		{
			string readString = "";

			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					_dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);

					_readCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.ReadTimeout);
					readString = await ReadAsync(_readCancellationTokenSource.Token);
				}
				catch (OperationCanceledException /*exception*/)
				{
					OperationCanceledException exception = new OperationCanceledException("Read timeout");
					LogHelper(LogLevel.Fatal, $"Read timeout");
					throw new SerialPortException("Read timeout");
				}
				catch (Exception exception)
				{
					LogHelper(LogLevel.Info, exception.Message.ToString() + $" {EventHandlerForDevice.Current.Device.PortName}");
					throw new SerialPortException(exception.Message, exception);
				}
				finally
				{
					ResetReadCancellationTokenSource();
					_dataReaderObject.DetachStream();
					//_dataReaderObject.Dispose();
					_dataReaderObject = null;
				}

				//try
				//{
				//	dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
				//	dataReaderObject.UnicodeEncoding = UnicodeEncoding.Utf8;

				//	dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
				//	await dataReaderObject.LoadAsync(ReadBufferLength);
				//	//_ReadCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.ReadTimeout);
				//	uint bytesReceived = EventHandlerForDevice.Current.Device.BytesReceived;

				//	//LogHelper(LogLevel.Trace, $"bytes received: {bytesReceived}");

				//	if ((bytesReceived > 0) && (bytesReceived <= ReadBufferLength))
				//	{
				//		buffer = new byte[bytesReceived + 2];
				//		readString = dataReaderObject.ReadString(bytesReceived);
				//	}

				//	//await ReadAsync(_ReadCancellationTokenSource.Token, readToIncluding);
				//}
				//catch (OperationCanceledException)
				//{
				//	OperationCanceledException exception = new OperationCanceledException("Read cancelled");
				//	LogHelper(LogLevel.Fatal, $"Read cancelled {readString}");
				//	throw exception;
				//}
				//catch (Exception e)
				//{
				//	LogHelper(LogLevel.Error, $"{e.Message.ToString()}");
				//}
				//finally
				//{
				//	ResetReadCancellationTokenSource();
				//	dataReaderObject.DetachStream();
				//	dataReaderObject = null;
				//}
			}
			else
			{
				//Utilities.ShowMessageDialogAsync($"{_tncDevice.CommPort.Comport} is not connected");
				LogHelper(LogLevel.Error, $"{_serialDevice.PortName} is not connected");
			}

			LogHelper(LogLevel.Trace, $"Buffer: {readString}");

			return readString;
		}

		//public async Task OpenAsync()
		//{
		//	LogHelper(LogLevel.Info, $"Opening Comport {_serialDevice.PortName}");

		//	var aqsFilter = SerialDevice.GetDeviceSelector(_serialDevice.PortName);
		//	var devices = await DeviceInformation.FindAllAsync(aqsFilter);
		//	//LogHelper(LogLevel.Info, $"Device to open: {devices[0].Id}");
		//	if (devices.Count > 0)
		//	{
		//		DeviceInformation device = devices[0];
		//		//LogHelper(LogLevel.Info, $"Comport: {comport}");

		//		if (device != null)
		//		{
		//			// Create an EventHandlerForDevice to watch for the device we are connecting to
		//			EventHandlerForDevice.CreateNewEventHandlerForDevice();

		//			// Get notified when the device was successfully connected to or about to be closed
		//			EventHandlerForDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
		//			//EventHandlerForDevice.Current.OnDeviceClose = this.OnDeviceClosing;

		//			// It is important that the FromIdAsync call is made on the UI thread because the consent prompt, when present,
		//			// can only be displayed on the UI thread. Since this method is invoked by the UI, we are already in the UI thread.
		//			Boolean openSuccess = await EventHandlerForDevice.Current.OpenDeviceAsync(device, aqsFilter);

		//			if (openSuccess)
		//			{
		//				_serialDevice =  EventHandlerForDevice.Current.Device;

		//				_serialDevice.BaudRate = (uint)_tncDevice.CommPort?.Baudrate;
		//				_serialDevice.StopBits = (SerialStopBitCount)_tncDevice.CommPort?.Stopbits;
		//				_serialDevice.DataBits = Convert.ToUInt16(_tncDevice.CommPort.Databits);
		//				_serialDevice.Parity = (SerialParity)_tncDevice.CommPort?.Parity;
		//				_serialDevice.Handshake = (SerialHandshake)_tncDevice.CommPort.Flowcontrol;
		//				_serialDevice.ReadTimeout = new TimeSpan(0, 0, 10); // hours, min, sec
		//				_serialDevice.WriteTimeout = new TimeSpan(0, 0, 10);

		//				ResetReadCancellationTokenSource();
		//				ResetWriteCancellationTokenSource();

		//				LogHelper(LogLevel.Info, $"Comport open. {_serialDevice.PortName}, {_serialDevice.BaudRate}, {_serialDevice.DataBits}, {_serialDevice.StopBits}, {_serialDevice.Parity}, {_serialDevice.Handshake}");
		//			}
		//			else
		//			{
		//				LogHelper(LogLevel.Error, $"Failed to open comport {_serialDevice.PortName}");
		//			}
		//		}
		//	}
		//}

		/// <summary>
		/// If all the devices have been enumerated, select the device in the list we connected to. Otherwise let the EnumerationComplete event
		/// from the device watcher handle the device selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="deviceInformation"></param>
		private void OnDeviceConnected(EventHandlerForDevice sender, DeviceInformation deviceInformation)
			{
				// Find and select our connected device
				//if (isAllDevicesEnumerated)
				//{
				//	SelectDeviceInList(EventHandlerForDevice.Current.DeviceInformation.Id);

				//	//ButtonDisconnectFromDevice.Content = ButtonNameDisconnectFromDevice;
				//}

				if (EventHandlerForDevice.Current.Device.PortName != "")
				{
					LogHelper(LogLevel.Info, "Connected to - " + _serialDevice.PortName);
				}
				else
				{
				LogHelper(LogLevel.Info, "Connected to - " +
										EventHandlerForDevice.Current.DeviceInformation.Id);
				}
			}

		private void ResetReadCancellationTokenSource()
		{
			// Create a new cancellation token source so that can cancel all the tokens again
			_readCancellationTokenSource = new CancellationTokenSource();

			// Hook the cancellation callback (called whenever Task.cancel is called)
			//_readCancellationTokenSource.Token.Register(() => NotifyReadCancelingTaskAsync());
		}

		private void ResetWriteCancellationTokenSource()
		{
			// Create a new cancellation token source so that can cancel all the tokens again
			_writeCancellationTokenSource = new CancellationTokenSource();

			// Hook the cancellation callback (called whenever Task.cancel is called)
			//_writeCancellationTokenSource.Token.Register(() => NotifyWriteCancelingTaskAsync());
		}

		/// <summary>
		/// Print a status message saying we are canceling a task and disable all buttons to prevent multiple cancel requests.
		/// <summary>
		//private async void NotifyReadCancelingTaskAsync()
		//{
		//	// Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
		//	// before any of the IO completion callbacks get a chance to modify the UI; that way this method
		//	// will never get the opportunity to overwrite UI changes made by IO callbacks
		//	await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
		//		new DispatchedHandler(() =>
		//		{
		//			//ReadButton.IsEnabled = false;
		//			//ReadCancelButton.IsEnabled = false;

		//			//if (!IsNavigatedAway)
		//			//{
		//			//	rootPage.NotifyUser("Canceling Read... Please wait...", NotifyType.StatusMessage);
		//			//}
		//		}));
		//}

		//private async void NotifyWriteCancelingTaskAsync()
		//{
		//	// Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
		//	// before any of the IO completion callbacks get a chance to modify the UI; that way this method
		//	// will never get the opportunity to overwrite UI changes made by IO callbacks
		//	await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
		//		new DispatchedHandler(() =>
		//		{
		//			//WriteButton.IsEnabled = false;
		//			//WriteCancelButton.IsEnabled = false;

		//			//if (!IsNavigatedAway)
		//			//{
		//			//	rootPage.NotifyUser("Canceling Write... Please wait...", NotifyType.StatusMessage);
		//			//}
		//		}));
		//}

		///// <summary>
		///// Notifies the UI that the operation has been canceled
		///// </summary>
		//private async void NotifyReadTaskCanceledAsync()
		//{
		//	await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
		//		new DispatchedHandler(() =>
		//		{
		//			//if (!IsNavigatedAway)
		//			//{
		//			//	rootPage.NotifyUser("Read request has been canceled", NotifyType.StatusMessage);
		//			//}
		//		}));

		//}

		//private async void NotifyWriteTaskCanceledAsync()
		//{
		//	await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
		//		new DispatchedHandler(() =>
		//		{
		//			//if (!IsNavigatedAway)
		//			//{
		//			//	rootPage.NotifyUser("Write request has been canceled", NotifyType.StatusMessage);
		//			//}
		//		}));
		//}

		public void Dispose()
		{
			if (_readCancellationTokenSource != null)
			{
				_readCancellationTokenSource.Dispose();
				_readCancellationTokenSource = null;
			}

			if (_writeCancellationTokenSource != null)
			{
				_writeCancellationTokenSource.Dispose();
				_writeCancellationTokenSource = null;
			}

			_serialDevice.Dispose();
		}

		///// <summary>
		///// The device was closed. If we will autoreconnect to the device, reflect that in the UI
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="deviceInformation"></param>
		//private async void OnDeviceClosing(EventHandlerForDevice sender, DeviceInformation deviceInformation)
		//{
		//	await rootPage.Dispatcher.RunAsync(
		//		CoreDispatcherPriority.Normal,
		//		new DispatchedHandler(() =>
		//		{
		//		// We were connected to the device that was unplugged, so change the "Disconnect from device" button
		//		// to "Do not reconnect to device"
		//		if (ButtonDisconnectFromDevice.IsEnabled && EventHandlerForDevice.Current.IsEnabledAutoReconnect)
		//			{
		//				ButtonDisconnectFromDevice.Content = ButtonNameDisableReconnectToDevice;
		//			}
		//		}));
		//}

	}

}
