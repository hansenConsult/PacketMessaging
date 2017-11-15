
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
		private CancellationTokenSource _ReadCancellationTokenSource;
		private Object _ReadCancelLock = new Object();

		//private Boolean IsReadTaskPending;
		//private uint ReadBytesCounter = 0;
		//DataReader _DataReaderObject = null;
		string _readBytesBuffer = "";

		// Track Write Operation
		private CancellationTokenSource _WriteCancellationTokenSource;
		private Object _WriteCancelLock = new Object();

		//private Boolean IsWriteTaskPending;
		//private uint WriteBytesCounter = 0;
		DataWriter _DataWriteObject = null;

		//bool WriteBytesAvailable = false;

		SerialDevice _serialDevice = null;

		TNCDevice _tncDevice;

		public SerialPort(ref TNCDevice tncDevice) : base(log)
		{
			_tncDevice = tncDevice;

			//mapDeviceWatchersToDeviceSelector = new Dictionary<DeviceWatcher, String>();
			//watchersStarted = false;
			//watchersSuspended = false;

			//isAllDevicesEnumerated = false;

			//// Begin watching out for events
			//StartHandlingAppEvents();

			//// Initialize the desired device watchers so that we can watch for when devices are connected/removed
			//InitializeDeviceWatchers();
			//StartDeviceWatchers();
		}


		///// <summary>
		///// Creates a DeviceListEntry for a device and adds it to the list of devices in the UI
		///// </summary>
		///// <param name="deviceInformation">DeviceInformation on the device to be added to the list</param>
		///// <param name="deviceSelector">The AQS used to find this device</param>
		//private void AddDeviceToList(DeviceInformation deviceInformation, String deviceSelector)
		//{
		//	// search the device list for a device with a matching interface ID
		//	var match = FindDevice(deviceInformation.Id);

		//	// Add the device if it's new
		//	if (match == null)
		//	{
		//		// Create a new element for this device interface, and queue up the query of its
		//		// device information
		//		match = new DeviceListEntry(deviceInformation, deviceSelector);

		//		// Add the new element to the end of the list of devices
		//		listOfDevices.Add(match);
		//	}
		//}			


		//private void RemoveDeviceFromList(String deviceId)
		//{
		//	// Removes the device entry from the internal list; therefore the UI
		//	var deviceEntry = FindDevice(deviceId);

		//	listOfDevices.Remove(deviceEntry);
		//}

		//private void ClearDeviceEntries()
		//{
		//	listOfDevices.Clear();
		//}

		/// <summary>
		/// Searches through the existing list of devices for the first DeviceListEntry that has
		/// the specified device Id.
		/// </summary>
		/// <param name="deviceId">Id of the device that is being searched for</param>
		/// <returns>DeviceListEntry that has the provided Id; else a nullptr</returns>
		//private DeviceListEntry FindDevice(String deviceId)
		//{
		//	if (deviceId != null)
		//	{
		//		foreach (DeviceListEntry entry in listOfDevices)
		//		{
		//			if (entry.DeviceInformation.Id == deviceId)
		//			{
		//				return entry;
		//			}
		//		}
		//	}

		//	return null;
		//}



		public TimeSpan ReadTimeout
		{ get { return EventHandlerForDevice.Current.Device.ReadTimeout; } set { EventHandlerForDevice.Current.Device.ReadTimeout = value; } }

		public TimeSpan WriteTimeout
		{ get { return EventHandlerForDevice.Current.Device.WriteTimeout; } set { EventHandlerForDevice.Current.Device.WriteTimeout = value; } }


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
				if (_ReadCancellationTokenSource != null)
				{
					if (!_ReadCancellationTokenSource.IsCancellationRequested)
					{
						_ReadCancellationTokenSource.Cancel();

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
				if (_WriteCancellationTokenSource != null)
				{
					if (!_WriteCancellationTokenSource.IsCancellationRequested)
					{
						_WriteCancellationTokenSource.Cancel();

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

			//uint ReadBufferLength = 1024;
			DataReader dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);

			while (!endOfMessage)
			{
				// Don't start any IO if we canceled the task
				lock (_ReadCancelLock)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// Cancellation Token will be used so we can stop the task operation explicitly
					// The completion function should still be called so that we can properly handle a canceled task
					dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

					loadAsyncTask = dataReaderObject.LoadAsync((uint)readToIncluding.Length).AsTask(cancellationToken);
				}
				uint bytesRead = await loadAsyncTask;
				if (bytesRead > 0)
				{
					_readBytesBuffer += dataReaderObject.ReadString(bytesRead);
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
				else if (readToIncluding != null && readToIncluding.Length == 0)
				{
					continue;
				}
			}
			dataReaderObject.DetachStream();
			dataReaderObject = null;

			readBytes = readBytes.Replace('\0', ' ');
			//LogHelper(LogLevel.Info, $"Read: {readBytes}");
			return readBytes;
		}

		private async Task WriteAsync(CancellationToken cancellationToken, string stringToWrite)
		{
			Task<UInt32> storeAsyncTask;

			if (!string.IsNullOrEmpty(stringToWrite))
			{
				char[] buffer = new char[stringToWrite.Length];
				stringToWrite.CopyTo(0, buffer, 0, stringToWrite.Length);
				String InputString = new string(buffer);
				_DataWriteObject.WriteString(InputString);
				stringToWrite = "";

				// Don't start any IO if we canceled the task
				lock (_WriteCancelLock)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// Cancellation Token will be used so we can stop the task operation explicitly
					// The completion function should still be called so that we can properly handle a canceled task
					storeAsyncTask = _DataWriteObject.StoreAsync().AsTask(cancellationToken);
				}

				UInt32 bytesWritten = await storeAsyncTask;
				if (bytesWritten > 0)
				{
					//LogHelper(LogLevel.Info, $"Write: {InputString.Substring(0, (int)bytesWritten)}");
					//writeBytes += InputString.Substring(0, (int)bytesWritten) + '\n';
					//WriteBytesCounter += bytesWritten;
					//UpdateWriteBytesCounterView();
				}
			}
			else
			{
				LogHelper(LogLevel.Warn, $"{_tncDevice.CommPort.Comport} is not connected");
				Utilities.ShowMessageDialogAsync($"{_tncDevice.CommPort.Comport} is not connected");
			}
		}

		public async Task<string> ReadToTimeoutAsync() => await ReadAsync("");		// This is mostly for debugging

		public async Task<string> ReadLineAsync() => await ReadToAsync("\r\n");

		public async Task<string> ReadToAsync(string text) => await ReadAsync(text);

		public async Task WriteLineAsync(string s)
		{
			await WriteAsync(s + "\r\n");
		}

		public async Task<string> ReadAsync(string readToIncluding)
		{
			string readString = "";

			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					_ReadCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.ReadTimeout);
					readString = await ReadAsync(_ReadCancellationTokenSource.Token, readToIncluding);
				}
				catch (OperationCanceledException)
				{
					OperationCanceledException exception = new OperationCanceledException("Read timeout");
					LogHelper(LogLevel.Fatal, $"Read timeout");
					throw exception;
				}
				catch (Exception e)
				{
					LogHelper(LogLevel.Fatal, $"{e.Message.ToString()}");
				}
				finally
				{
					ResetReadCancellationTokenSource();
					//_DataReaderObject.DetachStream();
					//_DataReaderObject = null;
				}
			}
			else
			{
				Utilities.ShowMessageDialogAsync("Device not connected");
				LogHelper(LogLevel.Error, $"Device not connected");
			}

			//LogHelper(LogLevel.Info, $"Read: {readString}");

			return readString;
		}

		public async Task WriteAsync(string s)
		{
			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					//LogHelper(LogLevel.Info, $"{s}");

					// We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
					// update the button states until after the write completes.
					_DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
					_WriteCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.WriteTimeout);
					await WriteAsync(_WriteCancellationTokenSource.Token, s);
				}
				catch (OperationCanceledException /*exception*/)
				{
					NotifyWriteTaskCanceledAsync();
				}
				catch (Exception exception)
				{
					LogHelper(LogLevel.Fatal, $"{exception.Message}");
				}
				finally
				{
					ResetWriteCancellationTokenSource();
					_DataWriteObject.DetachStream();
					_DataWriteObject = null;
				}
			}
			else
			{
				await Utilities.ShowMessageDialogAsync($"{_tncDevice.CommPort.Comport} is not connected");
			}
		}

		public async Task<string> ReadBufferAsync()
		{
			string readString = "";
			uint ReadBufferLength = 4096 * 4;
			byte[] ReadBuffer = new byte[ReadBufferLength];
			byte[] buffer;
			DataReader dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);


			if (EventHandlerForDevice.Current.IsDeviceConnected)
			{
				try
				{
					dataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
					dataReaderObject.UnicodeEncoding = UnicodeEncoding.Utf8;

					dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
					await dataReaderObject.LoadAsync(ReadBufferLength);
					//_ReadCancellationTokenSource.CancelAfter(EventHandlerForDevice.Current.Device.ReadTimeout);
					uint bytesReceived = EventHandlerForDevice.Current.Device.BytesReceived;

					//LogHelper(LogLevel.Trace, $"bytes received: {bytesReceived}");

					if ((bytesReceived > 0) && (bytesReceived <= ReadBufferLength))
					{
						buffer = new byte[bytesReceived + 2];
						readString = dataReaderObject.ReadString(bytesReceived);
					}

					//await ReadAsync(_ReadCancellationTokenSource.Token, readToIncluding);
				}
				catch (OperationCanceledException)
				{
					OperationCanceledException exception = new OperationCanceledException("Read cancelled");
					LogHelper(LogLevel.Fatal, $"Read cancelled {readString}");
					throw exception;
				}
				catch (Exception e)
				{
					LogHelper(LogLevel.Error, $"{e.Message.ToString()}");
				}
				finally
				{
					ResetReadCancellationTokenSource();
					dataReaderObject.DetachStream();
					dataReaderObject = null;
				}
			}
			else
			{
				//Utilities.ShowMessageDialogAsync($"{_tncDevice.CommPort.Comport} is not connected");
				LogHelper(LogLevel.Error, $"{_tncDevice.CommPort.Comport} is not connected");
			}

			//LogHelper(LogLevel.Info, $"Buffer: {readString}");

			return readString;
		}

		public async Task OpenAsync()
		{
			LogHelper(LogLevel.Info, $"Opening Comport {_tncDevice.CommPort.Comport}");

			var aqsFilter = SerialDevice.GetDeviceSelector(_tncDevice.CommPort.Comport);
			var devices = await DeviceInformation.FindAllAsync(aqsFilter);
			//LogHelper(LogLevel.Info, $"Device to open: {devices[0].Id}");
			if (devices.Count > 0)
			{
				DeviceInformation device = devices[0];
				//LogHelper(LogLevel.Info, $"Comport: {comport}");

				if (device != null)
				{
					// Create an EventHandlerForDevice to watch for the device we are connecting to
					EventHandlerForDevice.CreateNewEventHandlerForDevice();

					// Get notified when the device was successfully connected to or about to be closed
					EventHandlerForDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
					//EventHandlerForDevice.Current.OnDeviceClose = this.OnDeviceClosing;

					// It is important that the FromIdAsync call is made on the UI thread because the consent prompt, when present,
					// can only be displayed on the UI thread. Since this method is invoked by the UI, we are already in the UI thread.
					Boolean openSuccess = await EventHandlerForDevice.Current.OpenDeviceAsync(device, aqsFilter);

					if (openSuccess)
					{
						_serialDevice =  EventHandlerForDevice.Current.Device;

						_serialDevice.BaudRate = (uint)_tncDevice.CommPort?.Baudrate;
						_serialDevice.StopBits = (SerialStopBitCount)_tncDevice.CommPort?.Stopbits;
						_serialDevice.DataBits = Convert.ToUInt16(_tncDevice.CommPort.Databits);
						_serialDevice.Parity = (SerialParity)_tncDevice.CommPort?.Parity;
						_serialDevice.Handshake = (SerialHandshake)_tncDevice.CommPort.Flowcontrol;
						_serialDevice.ReadTimeout = new TimeSpan(0, 0, 0, 5, 0);
						_serialDevice.WriteTimeout = new TimeSpan(0, 0, 0, 5, 0);

						ResetReadCancellationTokenSource();
						ResetWriteCancellationTokenSource();

						LogHelper(LogLevel.Info, $"Comport open. {_serialDevice.PortName}, {_serialDevice.BaudRate}, {_serialDevice.DataBits}, {_serialDevice.StopBits}, {_serialDevice.Parity}, {_serialDevice.Handshake}");
					}
					else
					{
						LogHelper(LogLevel.Error, $"Failed to open comport {device}");
					}
				}
			}
		}

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
					LogHelper(LogLevel.Info, "Connected to - " + EventHandlerForDevice.Current.Device.PortName);
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
			_ReadCancellationTokenSource = new CancellationTokenSource();

			// Hook the cancellation callback (called whenever Task.cancel is called)
			_ReadCancellationTokenSource.Token.Register(() => NotifyReadCancelingTaskAsync());
		}

		private void ResetWriteCancellationTokenSource()
		{
			// Create a new cancellation token source so that can cancel all the tokens again
			_WriteCancellationTokenSource = new CancellationTokenSource();

			// Hook the cancellation callback (called whenever Task.cancel is called)
			_WriteCancellationTokenSource.Token.Register(() => NotifyWriteCancelingTaskAsync());
		}

		/// <summary>
		/// Print a status message saying we are canceling a task and disable all buttons to prevent multiple cancel requests.
		/// <summary>
		private async void NotifyReadCancelingTaskAsync()
		{
			// Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
			// before any of the IO completion callbacks get a chance to modify the UI; that way this method
			// will never get the opportunity to overwrite UI changes made by IO callbacks
			await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
				new DispatchedHandler(() =>
				{
					//ReadButton.IsEnabled = false;
					//ReadCancelButton.IsEnabled = false;

					//if (!IsNavigatedAway)
					//{
					//	rootPage.NotifyUser("Canceling Read... Please wait...", NotifyType.StatusMessage);
					//}
				}));
		}

		private async void NotifyWriteCancelingTaskAsync()
		{
			// Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
			// before any of the IO completion callbacks get a chance to modify the UI; that way this method
			// will never get the opportunity to overwrite UI changes made by IO callbacks
			await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
				new DispatchedHandler(() =>
				{
					//WriteButton.IsEnabled = false;
					//WriteCancelButton.IsEnabled = false;

					//if (!IsNavigatedAway)
					//{
					//	rootPage.NotifyUser("Canceling Write... Please wait...", NotifyType.StatusMessage);
					//}
				}));
		}

		/// <summary>
		/// Notifies the UI that the operation has been canceled
		/// </summary>
		private async void NotifyReadTaskCanceledAsync()
		{
			await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
				new DispatchedHandler(() =>
				{
					//if (!IsNavigatedAway)
					//{
					//	rootPage.NotifyUser("Read request has been canceled", NotifyType.StatusMessage);
					//}
				}));

		}

		private async void NotifyWriteTaskCanceledAsync()
		{
			await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
				new DispatchedHandler(() =>
				{
					//if (!IsNavigatedAway)
					//{
					//	rootPage.NotifyUser("Write request has been canceled", NotifyType.StatusMessage);
					//}
				}));
		}

		public void Dispose()
		{
			if (_ReadCancellationTokenSource != null)
			{
				_ReadCancellationTokenSource.Dispose();
				_ReadCancellationTokenSource = null;
			}

			if (_WriteCancellationTokenSource != null)
			{
				_WriteCancellationTokenSource.Dispose();
				_WriteCancellationTokenSource = null;
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
