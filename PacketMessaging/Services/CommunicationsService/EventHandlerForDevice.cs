//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

using Windows.ApplicationModel;
using Windows.Foundation;

using Windows.UI.Core;
using Windows.UI.Xaml;
using MetroLog;
using PacketMessaging.Views;

namespace PacketMessaging.Services.CommunicationsService
{
    /// <summary>
    /// The purpose of this class is to demonstrate the expected application behavior for app events 
    /// such as suspension and resume or when the device is disconnected. In addition to handling
    /// the SerialDevice, the app's state should also be saved upon app suspension (will not be demonstrated here).
    /// 
    /// This class will also demonstrate how to handle device watcher events.
    /// 
    /// For simplicity, this class will only allow at most one device to be connected at any given time. In order
    /// to make this class support multiple devices, make this class a non-singleton and create multiple instances
    /// of this class; each instance should watch one connected device.
    /// </summary>
    public class EventHandlerForDevice : CommunicationsServiceBase
    {
		private static ILogger _log = LogManagerFactory.DefaultLogManager.GetLogger<EventHandlerForDevice>();

		/// <summary>
		/// Used to synchronize threads to avoid multiple instantiations of eventHandlerForDevice.
		/// </summary>
		private static volatile EventHandlerForDevice eventHandlerForDevice;

        /// <summary>
        /// Used to synchronize threads to avoid multiple instantiations of eventHandlerForDevice.
        /// </summary>
        private static readonly Object singletonCreationLock = new Object();

        private String _deviceSelector;
        private DeviceWatcher _deviceWatcher;

        private DeviceInformation _deviceInformation;
        private DeviceAccessInformation _deviceAccessInformation;
        private SerialDevice device;

        private SuspendingEventHandler _appSuspendEventHandler;
        private EventHandler<Object> _appResumeEventHandler;

        private TypedEventHandler<EventHandlerForDevice, DeviceInformation> _deviceCloseCallback;
        private TypedEventHandler<EventHandlerForDevice, DeviceInformation> _deviceConnectedCallback;

        private TypedEventHandler<DeviceWatcher, DeviceInformation> _deviceAddedEventHandler;
        private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> _deviceRemovedEventHandler;
        private TypedEventHandler<DeviceAccessInformation, DeviceAccessChangedEventArgs> _deviceAccessEventHandler;

		private TypedEventHandler<SerialDevice, ErrorReceivedEventArgs> _deviceErrorEventHandler;

        private Boolean _watcherSuspended;
        private Boolean _watcherStarted;
        private Boolean _isEnabledAutoReconnect;

        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private MainPage rootPage = MainPage.Current;

        /// <summary>
        /// Enforces the singleton pattern so that there is only one object handling app events
        /// as it relates to the SerialDevice because this sample app only supports communicating with one device at a time. 
        ///
        /// An instance of EventHandlerForDevice is globally available because the device needs to persist across scenario pages.
        ///
        /// If there is no instance of EventHandlerForDevice created before this property is called,
        /// an EventHandlerForDevice will be created.
        /// </summary>
        public static EventHandlerForDevice Current
        {
            get
            {
                if (eventHandlerForDevice == null)
                {
                    lock (singletonCreationLock)
                    {
                        if (eventHandlerForDevice == null)
                        {
							CreateNewEventHandlerForDevice();
                        }
                    }
                }
                return eventHandlerForDevice;
            }
        }
        /// <summary>
        /// Creates a new instance of EventHandlerForDevice, enables auto reconnect, and uses it as the Current instance.
        /// </summary>
        public static void CreateNewEventHandlerForDevice()
        {
            eventHandlerForDevice = new EventHandlerForDevice(_log);
        }

        public TypedEventHandler<EventHandlerForDevice, DeviceInformation> OnDeviceClose
        {
            get
            {
                return _deviceCloseCallback;
            }

            set
            {
                _deviceCloseCallback = value;
            }
        }

        public TypedEventHandler<EventHandlerForDevice, DeviceInformation> OnDeviceConnected
        {
            get
            {
                return _deviceConnectedCallback;
            }

            set
            {
                _deviceConnectedCallback = value;
            }
        }

		public Boolean IsDeviceConnected => (device != null);

		public SerialDevice Device => device;

		/// <summary>
		/// This DeviceInformation represents which device is connected or which device will be reconnected when
		/// the device is plugged in again (if IsEnabledAutoReconnect is true);.
		/// </summary>
		public DeviceInformation DeviceInformation
        {
            get
            {
                return _deviceInformation;
            }
        }

        /// <summary>
        /// Returns DeviceAccessInformation for the device that is currently connected using this EventHandlerForDevice
        /// object.
        /// </summary>
        public DeviceAccessInformation DeviceAccessInformation
        {
            get
            {
                return _deviceAccessInformation;
            }
        }

        /// <summary>
        /// DeviceSelector AQS used to find this device
        /// </summary>
        public String DeviceSelector
        {
            get => _deviceSelector;
        }

        /// <summary>
        /// True if EventHandlerForDevice will attempt to reconnect to the device once it is plugged into the computer again
        /// </summary>
        public Boolean IsEnabledAutoReconnect
        {
            get
            {
                return _isEnabledAutoReconnect;
            }
            set
            {
                _isEnabledAutoReconnect = value;
            }
        }

        /// <summary>
        /// This method opens the device using the WinRT Serial API. After the device is opened, save the device
        /// so that it can be used across scenarios.
        ///
        /// It is important that the FromIdAsync call is made on the UI thread because the consent prompt can only be displayed
        /// on the UI thread.
        /// 
        /// This method is used to reopen the device after the device reconnects to the computer and when the app resumes.
        /// </summary>
        /// <param name="deviceInfo">Device information of the device to be opened</param>
        /// <param name="deviceSelector">The AQS used to find this device</param>
        /// <returns>True if the device was successfully opened, false if the device could not be opened for well known reasons.
        /// An exception may be thrown if the device could not be opened for extraordinary reasons.</returns>
        public async Task<Boolean> OpenDeviceAsync(DeviceInformation deviceInfo, String deviceSelector)
        {
            device = await SerialDevice.FromIdAsync(deviceInfo.Id);

            Boolean successfullyOpenedDevice = false;
            String notificationMessage = null;

            // Device could have been blocked by user or the device has already been opened by another app.
            if (device != null)
            {
                successfullyOpenedDevice = true;

                _deviceInformation = deviceInfo;
                _deviceSelector = deviceSelector;

                notificationMessage = "Device " + _deviceInformation.Id + " opened";

				// Notify registered callback handle that the device has been opened
				_deviceConnectedCallback?.Invoke(this, _deviceInformation);

				if (_appSuspendEventHandler == null || _appResumeEventHandler == null)
                {
                    RegisterForAppEvents();
                }

                // Register for DeviceAccessInformation.AccessChanged event and react to any changes to the
                // user access after the device handle was opened.
                if (_deviceAccessEventHandler == null)
                {
                    RegisterForDeviceAccessStatusChange();
                }

                // Create and register device watcher events for the device to be opened unless we're reopening the device
                if (_deviceWatcher == null)
                {
                    _deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);

                    RegisterForDeviceWatcherEvents();
                }

                if (!_watcherStarted)
                {
                    // Start the device watcher after we made sure that the device is opened.
                    StartDeviceWatcher();
                }

				if (_deviceErrorEventHandler == null)
				{
					RegisterForDeviceError();
				}
            }
            else
            {
                successfullyOpenedDevice = false;

                var deviceAccessStatus = DeviceAccessInformation.CreateFromId(deviceInfo.Id).CurrentStatus;

                if (deviceAccessStatus == DeviceAccessStatus.DeniedByUser)
                {
                    notificationMessage = "Access to the device was blocked by the user : " + device.PortName;
                }
                else if (deviceAccessStatus == DeviceAccessStatus.DeniedBySystem)
                {
                    // This status is most likely caused by app permissions (did not declare the device in the app's package.appxmanifest)
                    // This status does not cover the case where the device is already opened by another app.
                    notificationMessage = "Access to the device was blocked by the system : " + device.PortName;
                }
                else
                {
                    // Most likely the device is opened by another app, but cannot be sure
                    notificationMessage = "Unknown error, possibly opened by another app : " + device.PortName;
                }
            }
            if (!successfullyOpenedDevice)
            {
                await Utilities.ShowMessageDialogAsync(notificationMessage);
                LogHelper(LogLevel.Error, notificationMessage);
            }

			return successfullyOpenedDevice;
        }

        /// <summary>
        /// Closes the device, stops the device watcher, stops listening for app events, and resets object state to before a device
        /// was ever connected.
        /// </summary>
        public void CloseDevice()
        {
			if (_deviceErrorEventHandler != null)
			{
				UnregisterFromDeviceError();
			}

			if (IsDeviceConnected)
            {
                CloseCurrentlyConnectedDevice();
            }

            if (_deviceWatcher != null)
            {
                if (_watcherStarted)
                {
                    StopDeviceWatcher();

                    UnregisterFromDeviceWatcherEvents();
                }

                _deviceWatcher = null;
            }

            if (_deviceAccessInformation != null)
            {
                UnregisterFromDeviceAccessStatusChange();

                _deviceAccessInformation = null;
            }

            if (_appSuspendEventHandler != null || _appResumeEventHandler != null)
            {
                UnregisterFromAppEvents();
            }

            _deviceInformation = null;
            _deviceSelector = null;

            _deviceConnectedCallback = null;
            _deviceCloseCallback = null;

            _isEnabledAutoReconnect = false;
        }

        private EventHandlerForDevice(ILogger log) : base(log)
        {
            _watcherStarted = false;
            _watcherSuspended = false;
            _isEnabledAutoReconnect = false;
        }

        /// <summary>
        /// This method demonstrates how to close the device properly using the WinRT Serial API.
        ///
        /// When the SerialDevice is closing, it will cancel all IO operations that are still pending (not complete).
        /// The close will not wait for any IO completion callbacks to be called, so the close call may complete before any of
        /// the IO completion callbacks are called.
        /// The pending IO operations will still call their respective completion callbacks with either a task 
        /// canceled error or the operation completed.
        /// </summary>
        private void CloseCurrentlyConnectedDevice()
        {
            if (device != null)
            {
                // Notify callback that we're about to close the device
                _deviceCloseCallback?.Invoke(this, _deviceInformation);

				// Save the port name for logging info
				string comport = device.PortName;
                // This closes the handle to the device
                device.Dispose();

                device = null;

                LogHelper(LogLevel.Info, $"{comport} is closed");
            }
        }

        /// <summary>
        /// Register for app suspension/resume events. See the comments
        /// for the event handlers for more information on the exact device operation performed.
        ///
        /// We will also register for when the app exists so that we may close the device handle.
        /// </summary>
        private void RegisterForAppEvents()
        {
            _appSuspendEventHandler = new SuspendingEventHandler(EventHandlerForDevice.Current.OnAppSuspension);
            _appResumeEventHandler = new EventHandler<Object>(EventHandlerForDevice.Current.OnAppResume);

            // This event is raised when the app is exited and when the app is suspended
            App.Current.Suspending += _appSuspendEventHandler;

            App.Current.Resuming += _appResumeEventHandler;
        }

        private void UnregisterFromAppEvents()
        {
            // This event is raised when the app is exited and when the app is suspended
            App.Current.Suspending -= _appSuspendEventHandler;
            _appSuspendEventHandler = null;

            App.Current.Resuming -= _appResumeEventHandler;
            _appResumeEventHandler = null;
        }

        /// <summary>
        /// Register for Added and Removed events.
        /// Note that, when disconnecting the device, the device may be closed by the system before the OnDeviceRemoved callback is invoked.
        /// </summary>
        private void RegisterForDeviceWatcherEvents()
        {
            _deviceAddedEventHandler = new TypedEventHandler<DeviceWatcher, DeviceInformation>(OnDeviceAddedAsync);

            _deviceRemovedEventHandler = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);

            _deviceWatcher.Added += _deviceAddedEventHandler;

            _deviceWatcher.Removed += _deviceRemovedEventHandler;
        }

        private void UnregisterFromDeviceWatcherEvents()
        {
            _deviceWatcher.Added -= _deviceAddedEventHandler;
            _deviceAddedEventHandler = null;

            _deviceWatcher.Removed -= _deviceRemovedEventHandler;
            _deviceRemovedEventHandler = null;
        }

        /// <summary>
        /// Listen for any changed in device access permission. The user can block access to the device while the device is in use.
        /// If the user blocks access to the device while the device is opened, the device's handle will be closed automatically by
        /// the system; it is still a good idea to close the device explicitly so that resources are cleaned up.
        /// 
        /// Note that by the time the AccessChanged event is raised, the device handle may already be closed by the system.
        /// </summary>
        private void RegisterForDeviceAccessStatusChange()
        {
            // Enable the following registration ONLY if the Serial device under test is non-internal.
            //

            _deviceAccessInformation = DeviceAccessInformation.CreateFromId(_deviceInformation.Id);
            _deviceAccessEventHandler = new TypedEventHandler<DeviceAccessInformation, DeviceAccessChangedEventArgs>(OnDeviceAccessChangedAsync);
            _deviceAccessInformation.AccessChanged += _deviceAccessEventHandler;
        }

        private void UnregisterFromDeviceAccessStatusChange()
        {
            _deviceAccessInformation.AccessChanged -= _deviceAccessEventHandler;

            _deviceAccessEventHandler = null;
        }

		private void RegisterForDeviceError()
		{
			_deviceErrorEventHandler = new TypedEventHandler<SerialDevice, ErrorReceivedEventArgs>(OnDeviceError);
			Device.ErrorReceived += _deviceErrorEventHandler;
		}

		private void UnregisterFromDeviceError()
		{
			Device.ErrorReceived -= _deviceErrorEventHandler;

			_deviceErrorEventHandler = null;
		}

		private void StartDeviceWatcher()
        {
            _watcherStarted = true;

            if ((_deviceWatcher.Status != DeviceWatcherStatus.Started)
                && (_deviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted))
            {
                _deviceWatcher.Start();
            }
        }

        private void StopDeviceWatcher()
        {
            if ((_deviceWatcher.Status == DeviceWatcherStatus.Started)
                || (_deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted))
            {
                _deviceWatcher.Stop();
            }

            _watcherStarted = false;
        }

        /// <summary>
        /// If a Serial object has been instantiated (a handle to the device is opened), we must close it before the app 
        /// goes into suspension because the API automatically closes it for us if we don't. When resuming, the API will
        /// not reopen the device automatically, so we need to explicitly open the device in the app (Scenario1_ConnectDisconnect).
        ///
        /// Since we have to reopen the device ourselves when the app resumes, it is good practice to explicitly call the close
        /// in the app as well (For every open there is a close).
        /// 
        /// We must stop the DeviceWatcher because it will continue to raise events even if
        /// the app is in suspension, which is not desired (drains battery). We resume the device watcher once the app resumes again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnAppSuspension(Object sender, SuspendingEventArgs args)
        {
            if (_watcherStarted)
            {
                _watcherSuspended = true;
                StopDeviceWatcher();
            }
            else
            {
                _watcherSuspended = false;
            }

            CloseCurrentlyConnectedDevice();
        }

        /// <summary>
        /// When resume into the application, we should reopen a handle to the Serial device again. This will automatically
        /// happen when we start the device watcher again; the device will be re-enumerated and we will attempt to reopen it
        /// if IsEnabledAutoReconnect property is enabled.
        /// 
        /// See OnAppSuspension for why we are starting the device watcher again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void OnAppResume(Object sender, Object args)
        {
            if (_watcherSuspended)
            {
                _watcherSuspended = false;
                StartDeviceWatcher();
            }
        }

        /// <summary>
        /// Close the device that is opened so that all pending operations are canceled properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformationUpdate"></param>
        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            if (IsDeviceConnected && (deviceInformationUpdate.Id == _deviceInformation.Id))
            {
                // The main reasons to close the device explicitly is to clean up resources, to properly handle errors,
                // and stop talking to the disconnected device.
                CloseCurrentlyConnectedDevice();
            }
        }

		/// <summary>
		/// Open the device that the user wanted to open if it hasn't been opened yet and auto reconnect is enabled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="deviceInfo"></param>
		private async void OnDeviceAddedAsync(DeviceWatcher sender, DeviceInformation deviceInfo)
		{
			if ((_deviceInformation != null) && (deviceInfo.Id == _deviceInformation.Id) && !IsDeviceConnected && _isEnabledAutoReconnect)
			{
				await rootPage.Dispatcher.RunAsync(
					CoreDispatcherPriority.Normal,
					new DispatchedHandler(async () =>
					{
						await OpenDeviceAsync(_deviceInformation, _deviceSelector);

						// Any app specific device initialization should be done here because we don't know the state of the device when it is re-enumerated.
					}));
			}
		}

		/// <summary>
		/// Close the device if the device access was denied by anyone (system or the user) and reopen it if permissions are allowed again
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private async void OnDeviceAccessChangedAsync(DeviceAccessInformation sender, DeviceAccessChangedEventArgs eventArgs)
		{
			if ((eventArgs.Status == DeviceAccessStatus.DeniedBySystem)
				|| (eventArgs.Status == DeviceAccessStatus.DeniedByUser))
			{
				CloseCurrentlyConnectedDevice();
			}
			else if ((eventArgs.Status == DeviceAccessStatus.Allowed) && (_deviceInformation != null) && _isEnabledAutoReconnect)
			{
				await rootPage.Dispatcher.RunAsync(
					CoreDispatcherPriority.Normal,
					new DispatchedHandler(async () =>
					{
						await OpenDeviceAsync(_deviceInformation, _deviceSelector);

						// Any app specific device initialization should be done here because we don't know the state of the device when it is re-enumerated.
					}));
			}
		}

		private void OnDeviceError(SerialDevice sender, ErrorReceivedEventArgs eventArgs)
		{
			if (eventArgs.Error == SerialError.BufferOverrun)
			{
				LogHelper(LogLevel.Error, "Character Buffer Overrun");
			}
			else if (eventArgs.Error == SerialError.ReceiveFull)
			{
                LogHelper(LogLevel.Error, "Receive Buffer Full");
			}
			else if (eventArgs.Error == SerialError.ReceiveParity)
			{
                LogHelper(LogLevel.Error, "Receive Parity Error");
			}
			else if (eventArgs.Error == SerialError.TransmitFull)
			{
                LogHelper(LogLevel.Error, "Transmit Buffer Full");
			}
		}
	}
}
