using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using PacketMessaging.Services.CommunicationsService;
using MetroLog;
using Windows.Devices.Bluetooth;
using System.Runtime.CompilerServices;
using System.Linq;
using Windows.Devices.Bluetooth.Rfcomm;
using PacketMessaging.ViewModels;
using PacketMessaging.Models;
using PacketMessaging.Converters;
using Windows.UI.Popups;

namespace PacketMessaging.Views
{
    public sealed partial class SettingsPage : Page
	{
		Template10.Services.SerializationService.ISerializationService _SerializationService;
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<SettingsPage>();

		private ViewModels.SharedData sharedData = ViewModels.SharedData.SharedDataInstance;
		private ObservableCollection<DeviceListEntry> listOfDevices;

        private List<SerialDevice> _listOfSerialDevices;
		private ObservableCollection<SerialDevice> CollectionOfSerialDevices;
		private List<DeviceInformation> _listOfBluetoothDevices;
		private ObservableCollection<DeviceInformation> CollectionOfBluetoothDevices;
		ComportComparer comportComparer;

        private ObservableCollection<uint> listOfBaudRates;
		private ObservableCollection<ushort> listOfDataBits;

		private string _otherDeviceSelector;
		private string _bluetoothDeviceSelector;
		private Dictionary<DeviceWatcher, String> mapDeviceWatchersToDeviceSelector;

		private Boolean watchersSuspended;
		private Boolean watchersStarted;

		// Has all the devices enumerated by the device watcher?
		private Boolean isAllDevicesEnumerated;

		private SuspendingEventHandler appSuspendEventHandler;
		private EventHandler<Object> appResumeEventHandler;

		RfcommDeviceService _service;
		private MainPage rootPage = MainPage.Current;

        //public ViewModels.SettingsPageViewModel  TNCPartViewModel ViewModel => this.DataContext as TNCPartViewModel;

        // Identity settings
        public static ObservableCollection<TacticalCallsignData> listOfTacticallsignsArea;

		//public static Dictionary<string, TacticalCallsignData> _tacticalCallsignDataDictionary;
		TacticalCallsignData _tacticalCallsignData;

		bool _bluetoothOnChanged = false;
		bool _comPortChanged = false;
		bool _comNameChanged = false;
		bool _comportBaudRateChanged = false;
		bool _databitsChanged = false;
		bool _stopbitsChanged = false;
		bool _parityChanged = false;
		bool _handshakeChanged = false;
		bool _comportSettingsChanged = false;
		bool _initCommandsPreChanged = false;
		bool _initCommandsPostChanged = false;
		bool _initCommandsChanged = false;
		bool _commandsConnectChanged = false;
		bool _commandsChanged = false;
		bool _promptsCommandChanged = false;
		bool _promptsTimeoutChanged = false;
		bool _promptsConnectedChanged = false;
		bool _promptsDisconnectedChanged = false;
		bool _promptsChanged = false;

		//bool _isTNCAppBarSaveEnabled = false;
		//string _comPort;
		//Visibility _isBluetoothDevicesVisible = Visibility.Collapsed;

		public SettingsPage()
		{
			InitializeComponent();
			_SerializationService = Template10.Services.SerializationService.SerializationService.Json;

			ViewModels.SharedData sharedData = ViewModels.SharedData.SharedDataInstance;

            ObservableCollection<BBSData> bbsDataCollection = new ObservableCollection<BBSData>();
			foreach (BBSData bbsData in sharedData.BbsArray.BBSDataArray)
			{
				bbsDataCollection.Add(bbsData);
			}
			BBSDataCollection.Source = bbsDataCollection;
			comboBoxBBS.SelectedValue = sharedData.CurrentBBS;

			ObservableCollection<TNCDevice> tncDeviceCollection = new ObservableCollection<TNCDevice>(sharedData.TncDeviceArray.TNCDevices);
			//foreach (TNCDevice tncDevice in sharedData.TncDeviceArray.TNCDevices)
			//{
			//	tncDeviceCollection.Add(tncDevice);
			//}
			DeviceListSource.Source = tncDeviceCollection;
			comboBoxTNCs.SelectedValue = sharedData.CurrentTNCDevice;

			ObservableCollection<Profile> profileCollection = new ObservableCollection<Profile>();
			foreach (Profile profile in sharedData.ProfileArray?.Profiles)
			{
				profileCollection.Add(profile);
			}
			ProfilesCollection.Source = profileCollection;

            // Serial ports
			listOfDevices = new ObservableCollection<DeviceListEntry>();
            _listOfSerialDevices = new List<SerialDevice>();
            CollectionOfSerialDevices = new ObservableCollection<SerialDevice>();
			_listOfBluetoothDevices = new List<DeviceInformation>();
			CollectionOfBluetoothDevices = new ObservableCollection<DeviceInformation>();
			comportComparer = new ComportComparer();

            mapDeviceWatchersToDeviceSelector = new Dictionary<DeviceWatcher, String>();
			watchersStarted = false;
			watchersSuspended = false;
			isAllDevicesEnumerated = false;

			listOfBaudRates = new ObservableCollection<uint>();
			for (uint i = 1200; i < 39000; i *= 2)
			{
				listOfBaudRates.Add(i);
			}
			BaudRateListSource.Source = listOfBaudRates;

			// data bits
			listOfDataBits = new ObservableCollection<ushort>() { 7, 8 };
			DataBitsListSource.Source = listOfDataBits;

			// Parity
			foreach (SerialParity item in Enum.GetValues(typeof(SerialParity)))
			{
				comboBoxParity.Items.Add(item);
			}

			foreach (SerialStopBitCount item in Enum.GetValues(typeof(SerialStopBitCount)))
			{
				comboBoxStopBits.Items.Add(item);
			}

			foreach (SerialHandshake item in Enum.GetValues(typeof(SerialHandshake)))
			{
				comboBoxFlowControl.Items.Add(item);
			}

			appBarSaveTNC.IsEnabled = false;

			// Identity initialization
			listOfTacticallsignsArea = new ObservableCollection<TacticalCallsignData>();
			foreach (var callsignData in App._tacticalCallsignDataDictionary.Values)
			{
				listOfTacticallsignsArea.Add(callsignData);
			}
			TacticalCallsignsAreaSource.Source = listOfTacticallsignsArea;

			distributionListName.ItemsSource = DistributionListArray.Instance.GetDistributionLists();
			distributionListAddItem.IsEnabled = false;
			distributionListItems.IsReadOnly = true;
		}

		// LogHelper
		public void LogHelper(LogLevel logLevel, string message, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    log.Trace($"{sourceLineNumber}|{message}");
                    break;
                case LogLevel.Debug:
                    log.Debug($"{sourceLineNumber}|{message}");
                    break;
                case LogLevel.Info:
                    log.Info($"{sourceLineNumber}|{message}");
                    break;
                case LogLevel.Warn:
                    log.Warn($"{sourceLineNumber}|{message}");
                    break;
                case LogLevel.Error:
                    log.Error($"{sourceLineNumber}|{message}");
                    break;
                case LogLevel.Fatal:
                    log.Fatal($"{sourceLineNumber}|{message}");
                    break;
            }
        }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
			MyPivot.SelectedIndex = index;


			// Begin watching out for events
			StartHandlingAppEvents();

			// Initialize the desired device watchers so that we can watch for when devices are connected/removed
			InitializeDeviceWatchers();
			StartDeviceWatchers();

			//ComPortListSource.Source = CollectionOfSerialDevices;

			dataPath.Text = ApplicationData.Current.LocalFolder.Path;
			comboBoxBBS.SelectedValue = sharedData.CurrentBBS;
			comboBoxTNCs.SelectedValue = sharedData.CurrentTNCDevice;
			//ViewModels.SettingsPageViewModel.TNCPartViewModel.TNCChanged = false;
            if (MyPivot.SelectedIndex == 0)
            {
                SettingsCommandBar.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
		{
			TNCSaveAsCurrent();		// Must be called before the device watchers are stopped

			StopDeviceWatchers();
            StopHandlingAppEvents();
            // We no longer care about the device being connected
            EventHandlerForDevice.Current.OnDeviceConnected = null;
            EventHandlerForDevice.Current.OnDeviceClose = null;

            //if (profileChanged)
            //{
            //	ProfileSave();
            //}
            IdentitySave();
		}

		//private async void Page_LoadedAsync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		//{
		//	if (ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort.IsBluetooth)
		//	{
		//		try
		//		{
		//			//DeviceInformationCollection DeviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
		//			//_service = await RfcommDeviceService.FromIdAsync(DeviceInfoCollection[0].Id);
		//			//CollectionOfBluetoothDevices = new ObservableCollection<DeviceInformation>(DeviceInfoCollection);
		//			//ComNameListSource.Source = CollectionOfBluetoothDevices;

		//		}
		//		catch (Exception exp)
		//		{
		//			LogHelper(LogLevel.Error, $"{ exp.Message }");
		//		}
		//	}
		//}

#region Identity
		private void ComboBoxTacticalCallsignArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_tacticalCallsignData = (TacticalCallsignData)e.AddedItems[0];

			_tacticalCallsignData.TacticalCallsignsChanged = false;
			ViewModels.IdentityPartViewModel._tacticalCallsignData = _tacticalCallsignData;
			if (_tacticalCallsignData.TacticalCallsigns != null)
			{
				ObservableCollection<TacticalCall> listOfTacticallsigns = new ObservableCollection<TacticalCall>();
				foreach (var callsignData in _tacticalCallsignData.TacticalCallsigns.TacticalCallsignsArray)
				{
					listOfTacticallsigns.Add(callsignData);
				}
				TacticalCallsignsSource.Source = listOfTacticallsigns;
			}
			if (_tacticalCallsignData.AreaName == "Other")
			{
				textBoxTacticalCallsign.Visibility = Visibility.Visible;
				comboBoxTacticalCallsign.Visibility = Visibility.Collapsed;
				comboBoxAdditionalText.SelectedItem = null;
			}
			else
			{
				textBoxTacticalCallsign.Visibility = Visibility.Collapsed;
				comboBoxTacticalCallsign.Visibility = Visibility.Visible;
			}
		}

		private void textBoxTacticalCallsign_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (textBoxTacticalCallsign.Text.Length == 6)
				SettingsPageViewModel.IdentityPartViewModel.TacticalCallsignOther = textBoxTacticalCallsign.Text;
		}

		//private void comboBoxTacticalCallsign_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	TacticalCall callsignData = (TacticalCall)e.AddedItems[0];

		//	ViewModels.IdentityPartViewModel._callsignData = callsignData;
		//}

		private void IdentitySave()
		{
			//ViewModels.SharedData._profileArray.Save(ApplicationData.Current.LocalFolder.Path);
			foreach (var tacticalCallsignType in App._tacticalCallsignDataDictionary.Values)
			{
				if (tacticalCallsignType.TacticalCallsignsChanged)
					tacticalCallsignType.TacticalCallsigns.SaveAsync(tacticalCallsignType.FileName);
			}
		}

		private void ButtonSetAllPrimary_Click(object sender, RoutedEventArgs e)
		{
            AddressBook.Instance.UsePrimaryBBSForAll();
            //ContactsCVS.Source = AddressBook.Instance.GetContactsGrouped();
        }
        #endregion

        #region Profiles
        private bool _bbsChanged = false;
        private bool _tncChanged = false;
        private bool _defaultToChanged = false;

        private void ComboBoxBBS_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedBBS = (BBSData)e.AddedItems[0];
			sharedData.CurrentBBS = selectedBBS;
			//ViewModels.SharedData._currentProfile.BBS = selectedBBS.Name;
			textBoxDescription.Text = selectedBBS.Description;
			textBoxFrequency1.Text = selectedBBS.Frequency1;
			textBoxFrequency2.Text = selectedBBS.Frequency2;
            if (sharedData.CurrentProfile.BBS != selectedBBS.Name)
            {
                _bbsChanged = true;
            }
            else
            {
                _bbsChanged = false;
            }
			profileSave.IsEnabled = _bbsChanged | _tncChanged | _defaultToChanged;
		}

		private void ComboBoxTNCs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedTNCDexice = (TNCDevice)e.AddedItems[0];
			sharedData.CurrentTNCDevice = selectedTNCDexice;
			//ViewModels.SharedData._currentProfile.TNC = selectedTNCDexice.Name;
			if (sharedData.CurrentProfile.TNC != selectedTNCDexice.Name)
			{
                _defaultToChanged = true;
			}
            else
            {
                _defaultToChanged = false;
            }
			profileSave.IsEnabled = _bbsChanged | _tncChanged | _defaultToChanged;
		}

        private void TextBoxTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sharedData.CurrentProfile.SendTo != ((TextBox)sender).Text)
            {
                _tncChanged = true;
            }
            else
            {
                _tncChanged = false;
            }
            profileSave.IsEnabled = _bbsChanged | _tncChanged | _defaultToChanged;
        }

        private void ComboBoxProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach (Profile profile in e.RemovedItems)
			{
				profile.Selected = false;
			}

			try
			{
				Profile profile = (Profile)((ComboBox)sender).SelectedItem;
				if (profile != null)
				{
					//comboBoxTNCs.SelectedValuePath = "Name";
					comboBoxTNCs.SelectedValue = profile.TNC;
					//BBSSelectedValue = profile.BBS;
					comboBoxBBS.SelectedValue = profile.BBS;
					textBoxTo.Text = profile.SendTo;
					//MessageTo = profile.SendTo;
					sharedData.CurrentProfile = profile;
					profile.Selected = true;
				}
                _bbsChanged = false;
                _tncChanged = false;
                _defaultToChanged = false;

                profileSave.IsEnabled = false;
			}
			catch (Exception ex)
			{
				string s = ex.ToString();
			}
		}

		private void ProfileSave()
		{
			//Profile newProfile = null;
			int index = comboBoxProfiles.SelectedIndex;
			Profile profile = sharedData.ProfileArray.Profiles[index];
			if (comboBoxProfiles.Visibility == Visibility.Collapsed)
			{
				//	newProfile = new Profile();

				profile.Name = textBoxNewProfileName.Text;
				//	newProfile.BBS = comboBoxBBS.SelectedValue as string;
				//	newProfile.TNC = comboBoxTNCs.SelectedValue as string;
				//	newProfile.SendTo = textBoxTo.Text;
				//	newProfile.Selected = true;

				//	ViewModels.SharedData._profileArray.Profiles.SetValue(newProfile, ViewModels.SharedData._profileArray.Profiles.Length - 1);
				comboBoxProfiles.Visibility = Visibility.Visible;
				textBoxNewProfileName.Visibility = Visibility.Collapsed;
			}
			//else
			//{
			//int index = comboBoxProfiles.SelectedIndex;
			//Profile profile = ViewModels.SharedData._profileArray.Profiles[index];

			profile.BBS = comboBoxBBS.SelectedValue as string;
			profile.TNC = comboBoxTNCs.SelectedValue as string;
			profile.SendTo = textBoxTo.Text;
			profile.Selected = true;

			sharedData.ProfileArray.Profiles.SetValue(profile, index);
			//}

			sharedData.ProfileArray.Save(ApplicationData.Current.LocalFolder.Path);

            _bbsChanged = false;
            _tncChanged = false;
            _defaultToChanged = false;
            profileSave.IsEnabled = false;
		}

		private void ProfileSave_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ProfileSave();
		}

		private void ProfileSettingsAdd_Click(object sender, RoutedEventArgs e)
		{
			profileSave.IsEnabled = true;

			comboBoxProfiles.Visibility = Visibility.Collapsed;
			textBoxNewProfileName.Visibility = Visibility.Visible;

			var length = sharedData.ProfileArray.Profiles.Length;
			Profile[] tempProfileArray = new Profile[length + 1];
			sharedData.ProfileArray.Profiles.CopyTo(tempProfileArray, 0);
			Profile newProfile = new Profile()
			{
				BBS = comboBoxBBS.SelectedValue as string,
				TNC = comboBoxTNCs.SelectedValue as string,
                SendTo = textBoxTo.Text
            };
            tempProfileArray.SetValue(newProfile, length);
			sharedData.ProfileArray.Profiles = tempProfileArray;

			ObservableCollection<Profile> profileCollection = new ObservableCollection<Profile>();
			foreach (Profile profile in sharedData.ProfileArray.Profiles)
			{
				profile.Selected = false;
				profileCollection.Add(profile);
			}
			sharedData.ProfileArray.Profiles[length].Selected = true;
			ProfilesCollection.Source = profileCollection;
			comboBoxProfiles.SelectedIndex = length;
		}

		private void ProfileSettingsDelete_Click(object sender, RoutedEventArgs e)
		{
			int index = comboBoxProfiles.SelectedIndex;
			var length = sharedData.ProfileArray.Profiles.Length;
			Profile[] tempProfileArray = new Profile[length - 1];

			ObservableCollection<Profile> profileCollection = new ObservableCollection<Profile>();
			for (int i = 0, j = 0; i < length; i++)
			{
				if (i != index)
				{
					tempProfileArray.SetValue(sharedData.ProfileArray?.Profiles[i], j);
					profileCollection.Add(sharedData.ProfileArray?.Profiles[i]);
					j++;
				}
			}
			ProfilesCollection.Source = profileCollection;
			sharedData.ProfileArray.Profiles = tempProfileArray;

			comboBoxProfiles.SelectedIndex = Math.Max(index - 1, 0);
			profileSave.IsEnabled = true;
		}
#endregion
#region TNC
		private void ClearDeviceEntries()
		{
			listOfDevices.Clear();
			foreach (SerialDevice serialDevice in _listOfSerialDevices)
			{
				serialDevice.Dispose();
			}
            _listOfSerialDevices.Clear();
            CollectionOfSerialDevices.Clear();
			_listOfBluetoothDevices.Clear();
			CollectionOfBluetoothDevices.Clear();
		}

		private void InitializeDeviceWatchers()
		{
			// Serial devices device selector
			var deviceSelector = SerialDevice.GetDeviceSelector();

			// Create a device watcher to look for instances of the Serial Device that match the device selector
			// used earlier.
			var deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);

			// Allow the EventHandlerForDevice to handle device watcher events that relates or effects our device (i.e. device removal, addition, app suspension/resume)
			AddDeviceWatcher(deviceWatcher, deviceSelector);

			// Bluetooth devices device selector
			_bluetoothDeviceSelector = BluetoothDevice.GetDeviceSelector();
			//_bluetoothDeviceSelector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
			deviceWatcher = DeviceInformation.CreateWatcher(_bluetoothDeviceSelector);
			AddDeviceWatcher(deviceWatcher, _bluetoothDeviceSelector);
        }

        private void StartDeviceWatchers()
		{
			// Start all device watchers
			watchersStarted = true;
			isAllDevicesEnumerated = false;

			foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
			{
				if ((deviceWatcher.Status != DeviceWatcherStatus.Started)
					&& (deviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted))
				{
					deviceWatcher.Start();
				}
			}
		}

		/// <summary>
		/// Stops all device watchers.
		/// </summary>
		private void StopDeviceWatchers()
		{
			// Stop all device watchers
			foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
			{
				if ((deviceWatcher.Status == DeviceWatcherStatus.Started)
					|| (deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted))
				{
					deviceWatcher.Stop();
				}
			}

			// Clear the list of devices so we don't have potentially disconnected devices around
			ClearDeviceEntries();

			watchersStarted = false;
		}

		/// <summary>
		/// Searches through the existing list of devices for the first DeviceListEntry that has
		/// the specified device Id.
		/// </summary>
		/// <param name="deviceId">Id of the device that is being searched for</param>
		/// <returns>DeviceListEntry that has the provided Id; else a nullptr</returns>
		private DeviceListEntry FindDevice(String deviceId)
		{
			if (deviceId != null)
			{
				foreach (DeviceListEntry entry in listOfDevices)
				{
					if (entry.DeviceInformation.Id == deviceId)
					{
						return entry;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// We must stop the DeviceWatchers because device watchers will continue to raise events even if
		/// the app is in suspension, which is not desired (drains battery). We resume the device watcher once the app resumes again.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void OnAppSuspension(Object sender, SuspendingEventArgs args)
		{
			if (watchersStarted)
			{
				watchersSuspended = true;
				StopDeviceWatchers();
			}
			else
			{
				watchersSuspended = false;
			}
		}

		/// <summary>
		/// See OnAppSuspension for why we are starting the device watchers again
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnAppResume(Object sender, Object args)
		{
			if (watchersSuspended)
			{
				watchersSuspended = false;
				StartDeviceWatchers();
			}
		}

		/// <summary>
		/// We will remove the device from the UI
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="deviceInformationUpdate"></param>
		private async void OnDeviceRemovedAsync(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
		{
			await RemoveDeviceFromListAsync(deviceInformationUpdate.Id);
		}

        /// <summary>
        /// This function will add the device to the listOfDevices so that it shows up in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private async void OnDeviceAddedAsync(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            await rootPage.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler( () =>
			   {
				   AddDeviceToListAsync(deviceInformation, mapDeviceWatchersToDeviceSelector[sender]);
			   }));
        }


        /// <summary>
        /// Notify the UI whether or not we are connected to a device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnDeviceEnumerationComplete(DeviceWatcher sender, Object args)
        {
            isAllDevicesEnumerated = true;

            //await rootPage.Dispatcher.RunAsync(
            //    CoreDispatcherPriority.Normal,
            //    new DispatchedHandler(() =>
            //    {
            //        isAllDevicesEnumerated = true;

            //    // If we finished enumerating devices and the device has not been connected yet, the OnDeviceConnected method
            //    // is responsible for selecting the device in the device list (UI); otherwise, this method does that.
            //    if (EventHandlerForDevice.Current.IsDeviceConnected)
            //        {
            //            SelectDeviceInList(EventHandlerForDevice.Current.DeviceInformation.Id);

            //            ButtonDisconnectFromDevice.Content = ButtonNameDisconnectFromDevice;

            //        //rootPage.NotifyUser("Connected to - " +
            //        //					EventHandlerForDevice.Current.DeviceInformation.Id, NotifyType.StatusMessage);

            //        EventHandlerForDevice.Current.ConfigureCurrentlyConnectedDevice();
            //        }
            //        else if (EventHandlerForDevice.Current.IsEnabledAutoReconnect && EventHandlerForDevice.Current.DeviceInformation != null)
            //        {
            //        // We will be reconnecting to a device
            //        ButtonDisconnectFromDevice.Content = ButtonNameDisableReconnectToDevice;

            //        //rootPage.NotifyUser("Waiting to reconnect to device -  " + EventHandlerForDevice.Current.DeviceInformation.Id, NotifyType.StatusMessage);
            //    }
            //        else
            //        {
            //        //rootPage.NotifyUser("No device is currently connected", NotifyType.StatusMessage);
            //    }
            //    }));
        }

        private void StartHandlingAppEvents()
		{
			appSuspendEventHandler = new SuspendingEventHandler(this.OnAppSuspension);
			appResumeEventHandler = new EventHandler<Object>(this.OnAppResume);

			// This event is raised when the app is exited and when the app is suspended
			App.Current.Suspending += appSuspendEventHandler;

			App.Current.Resuming += appResumeEventHandler;
		}

		private void StopHandlingAppEvents()
		{
			// This event is raised when the app is exited and when the app is suspended
			App.Current.Suspending -= appSuspendEventHandler;

			App.Current.Resuming -= appResumeEventHandler;
		}

		/// <summary>
		/// Registers for Added, Removed, and Enumerated events on the provided deviceWatcher before adding it to an internal list.
		/// </summary>
		/// <param name="deviceWatcher"></param>
		/// <param name="deviceSelector">The AQS used to create the device watcher</param>
		private void AddDeviceWatcher(DeviceWatcher deviceWatcher, String deviceSelector)
		{
			deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAddedAsync);
			deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemovedAsync);
			deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(this.OnDeviceEnumerationComplete);

			mapDeviceWatchersToDeviceSelector.Add(deviceWatcher, deviceSelector);
		}

		/// <summary>
		/// Selects the item in the UI's listbox that corresponds to the provided device id. If there are no
		/// matches, we will deselect anything that is selected.
		/// </summary>
		/// <param name="deviceIdToSelect">The device id of the device to select on the list box</param>
		//private void SelectDeviceInList(String deviceIdToSelect)
		//{
		//	// Don't select anything by default.
		//	ConnectDevices.SelectedIndex = -1;

		//	for (int deviceListIndex = 0; deviceListIndex < listOfDevices.Count; deviceListIndex++)
		//	{
		//		if (listOfDevices[deviceListIndex].DeviceInformation.Id == deviceIdToSelect)
		//		{
		//			ConnectDevices.SelectedIndex = deviceListIndex;

		//			break;
		//		}
		//	}
		//}

		private async void AddDeviceToListAsync(DeviceInformation deviceInformation, string deviceSelector)
		//private void AddDeviceToListAsync(DeviceInformation deviceInformation, string deviceSelector)
        {
            try
            {
                // search the device list for a device with a matching interface ID
                var match = FindDevice(deviceInformation.Id);

				// Add the device if it's new
				if (match == null)
				{
					// Create a new element for this device interface, and queue up the query of its
					// device information
					match = new DeviceListEntry(deviceInformation, deviceSelector);

					// Add the new element to the end of the list of devices
					listOfDevices.Add(match);

					//SerialDevice serialDevice = await SerialDevice.FromIdAsync(deviceInformation.Id);
					//if (serialDevice != null)
					//{
					//	//string name = serialDevice.PortName;
					//	_listOfSerialDevices.Add(serialDevice);
					//	// Sort list
					//	_listOfSerialDevices = _listOfSerialDevices.OrderBy(s => s.PortName, comportComparer).ToList();
					//	CollectionOfSerialDevices = new ObservableCollection<SerialDevice>(_listOfSerialDevices);
					//	ComPortListSource.Source = CollectionOfSerialDevices;
					//}


					if (deviceInformation.Pairing.IsPaired)
					{
						// Bluetooth device
						try
						{
							if (deviceInformation.Kind == DeviceInformationKind.AssociationEndpoint)
							{
								_listOfBluetoothDevices.Add(deviceInformation);
								CollectionOfBluetoothDevices = new ObservableCollection<DeviceInformation>(_listOfBluetoothDevices);
								ComNameListSource.Source = CollectionOfBluetoothDevices;
							}
						}
						catch (Exception ex)
						{
							LogHelper(LogLevel.Error, $"Overall Connect: { ex.Message}");
						}
					}
					else
					{
						// USB serial port
						SerialDevice serialDevice = await SerialDevice.FromIdAsync(deviceInformation.Id);
						if (serialDevice != null)
						{
							//string name = serialDevice.PortName;
							_listOfSerialDevices.Add(serialDevice);
							// Sort list
							_listOfSerialDevices = _listOfSerialDevices.OrderBy(s => s.PortName, comportComparer).ToList();
							CollectionOfSerialDevices = new ObservableCollection<SerialDevice>(_listOfSerialDevices);
							ComPortListSource.Source = CollectionOfSerialDevices;
						}
					}
                }
            }
            catch (Exception e)
            {
#if DEBUG
                LogHelper(LogLevel.Error, $"{e.Message}, DeviceId = {deviceInformation.Id}");
#endif
            }
        }

		private async Task RemoveDeviceFromListAsync(String deviceId)
		{
			// Removes the device entry from the internal list; therefore the UI
			var deviceEntry = FindDevice(deviceId);

			listOfDevices.Remove(deviceEntry);

			SerialDevice serialDevice = await SerialDevice.FromIdAsync(deviceId);
			if (serialDevice != null)
			{
				_listOfSerialDevices.Remove(serialDevice);
				// Sort list
				_listOfSerialDevices = _listOfSerialDevices.OrderBy(s => s.PortName, comportComparer).ToList();
				CollectionOfSerialDevices = new ObservableCollection<SerialDevice>(_listOfSerialDevices);
				ComPortListSource.Source = CollectionOfSerialDevices;
			}
		}

		private void ConnectDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if ((PivotItem)PivotTNC.SelectedItem == null)
            //    return;

            if (e.AddedItems.Count > 0)
            {
                TNCDevice tncDevice = null;
                var TNCDevices = e.AddedItems;
                if (TNCDevices.Count == 1)
                {
                    tncDevice = (TNCDevice)TNCDevices[0];
                    sharedData.CurrentTNCDevice = tncDevice;
					sharedData.SavedTNCDevice = new TNCDevice(tncDevice);
                }
                else
                {
                    return;
                }
                textBoxInitCommandsPre.Text = tncDevice.InitCommands.Precommands;
                textBoxInitCommandsPost.Text = tncDevice.InitCommands.Postcommands;

				//SerialDevice serialDevice = null;
				//UseBluetooth = tncDevice.CommPort.IsBluetooth;
				toggleSwitchBluetooth.IsOn = tncDevice.CommPort.IsBluetooth;
				SetComboBoxVisicility();

				foreach (DeviceInformation deviceInfo in CollectionOfBluetoothDevices)
				{
					if (deviceInfo.Name == tncDevice.CommPort.BluetoothName)
					{
						comboBoxComName.SelectedItem = deviceInfo;
						break;
					}
				}
				foreach (SerialDevice device in CollectionOfSerialDevices)
				{
					if (device.PortName == tncDevice.CommPort.Comport)
					{
						comboBoxComPort.SelectedItem = device;
						break;
					}
				}
				//if (serialDevice != null)
				{
					//comboBoxComPort.SelectedItem = serialDevice;
					comboBoxBaudRate.SelectedValue = tncDevice.CommPort.Baudrate;
                    comboBoxDatabits.SelectedValue = tncDevice.CommPort.Databits;

                    int i = 0;
                    var values = Enum.GetValues(typeof(SerialParity));
                    for (; i < values.Length; i++)
                    {
                        if ((SerialParity)values.GetValue(i) == tncDevice.CommPort.Parity) break;
                    }
                    comboBoxParity.SelectedIndex = i;
                    //ViewModels.SettingsPageViewModel.TNCPartViewModel.SelectedParity = tncDevice.CommPort.Parity;

                    values = Enum.GetValues(typeof(SerialHandshake));
                    for (i = 0; i < values.Length; i++)
                    {
                        if ((SerialStopBitCount)values.GetValue(i) == tncDevice.CommPort.Stopbits) break;
                    }
                    comboBoxStopBits.SelectedIndex = i;

                    //ViewModels.SettingsPageViewModel.TNCPartViewModel.SelectedStopBits = tncDevice.CommPort.Stopbits;
                    values = Enum.GetValues(typeof(SerialHandshake));
                    for (i = 0; i < values.Length; i++)
                    {
                        if ((SerialHandshake)values.GetValue(i) == tncDevice.CommPort.Flowcontrol)
                        {
                            break;
                        }
                    }
                    comboBoxFlowControl.SelectedIndex = i;
                }
				//else
				//{
				//	MessageDialog messageDialog = new MessageDialog("Com port not found. \nIs the TNC plugged in?");
				//	await messageDialog.ShowAsync();
				//}
				TNCPromptsCommand = tncDevice.Prompts.Command;
				TNCPromptsTimeout = tncDevice.Prompts.Timeout;
				//textBoxPrompsCommand.Text = tncDevice.Prompts.Command;
				//textBoxPromptsTimeout.Text = tncDevice.Prompts.Timeout;
				textBoxPromptsConnected.Text = tncDevice.Prompts.Connected;
                textBoxPromptsDisconnected.Text = tncDevice.Prompts.Disconnected;

				TNCCommandsConnect = tncDevice.Commands.Connect;
				//textBoxCommandsConnect.Text = tncDevice.Commands.Connect;
                textBoxCommandsConversMode.Text = tncDevice.Commands.Conversmode;
                textBoxCommandsMyCall.Text = tncDevice.Commands.MyCall;
                textBoxCommandsRetry.Text = tncDevice.Commands.Retry;
                textBoxCommandsDateTime.Text = tncDevice.Commands.Datetime;
            }
        }

		private void TNCSaveAsCurrent()
		{
			TNCDevice tncDevice = sharedData.CurrentTNCDevice;

			if (_initCommandsChanged)
			{
				tncDevice.InitCommands.Precommands = textBoxInitCommandsPre.Text;
				tncDevice.InitCommands.Postcommands = textBoxInitCommandsPost.Text;
			}

			if (_comportSettingsChanged)
			{
				tncDevice.CommPort.IsBluetooth = toggleSwitchBluetooth.IsOn;
				tncDevice.CommPort.BluetoothName = comboBoxComName.SelectedValue as string;
				tncDevice.CommPort.DeviceId = ((DeviceInformation)comboBoxComName.SelectedItem)?.Id as string;
				tncDevice.CommPort.Comport = comboBoxComPort.SelectedValue as string;
				tncDevice.CommPort.Baudrate = (uint)comboBoxBaudRate.SelectedValue;
				tncDevice.CommPort.Databits = (ushort)comboBoxDatabits.SelectedValue;
				tncDevice.CommPort.Stopbits = (SerialStopBitCount)comboBoxStopBits.SelectedValue;
				tncDevice.CommPort.Parity = (SerialParity)comboBoxParity.SelectedValue;
				tncDevice.CommPort.Flowcontrol = (SerialHandshake)comboBoxFlowControl.SelectedValue;
			}

			if (_promptsChanged)
			{
				tncDevice.Prompts.Command = textBoxPrompsCommand.Text;
				tncDevice.Prompts.Timeout = textBoxPromptsTimeout.Text;
				tncDevice.Prompts.Connected = textBoxPromptsConnected.Text;
				tncDevice.Prompts.Disconnected = textBoxPromptsDisconnected.Text;
			}

			if (_commandsChanged)
			{
				tncDevice.Commands.Connect = textBoxCommandsConnect.Text;
				tncDevice.Commands.Conversmode = textBoxCommandsConversMode.Text;
				tncDevice.Commands.MyCall = textBoxCommandsMyCall.Text;
				tncDevice.Commands.Retry = textBoxCommandsRetry.Text;
				tncDevice.Commands.Datetime = textBoxCommandsDateTime.Text;
			}
        }

		private async void appBarSaveTNC_ClickAsync(object sender, RoutedEventArgs e)
		{
			appBarSaveTNC.IsEnabled = false;

			TNCSaveAsCurrent();
			for (int i = 0; i < sharedData.TncDeviceArray.TNCDevices.Length; i++)
			{
				if (sharedData.TncDeviceArray.TNCDevices[i].Name == sharedData.CurrentTNCDevice.Name)
				{
					sharedData.TncDeviceArray.TNCDevices[i].InitCommands.Precommands = sharedData.CurrentTNCDevice.InitCommands.Precommands;
					sharedData.TncDeviceArray.TNCDevices[i].InitCommands.Postcommands = sharedData.CurrentTNCDevice.InitCommands.Postcommands;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.IsBluetooth = sharedData.CurrentTNCDevice.CommPort.IsBluetooth;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.BluetoothName = sharedData.CurrentTNCDevice.CommPort.BluetoothName;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.DeviceId = sharedData.CurrentTNCDevice.CommPort.DeviceId;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.Comport = sharedData.CurrentTNCDevice.CommPort.Comport;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.Baudrate = sharedData.CurrentTNCDevice.CommPort.Baudrate;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.Databits = sharedData.CurrentTNCDevice.CommPort.Databits;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.Stopbits = sharedData.CurrentTNCDevice.CommPort.Stopbits;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.Parity = sharedData.CurrentTNCDevice.CommPort.Parity;
					sharedData.TncDeviceArray.TNCDevices[i].CommPort.Flowcontrol = sharedData.CurrentTNCDevice.CommPort.Flowcontrol;
					sharedData.TncDeviceArray.TNCDevices[i].Prompts.Command = sharedData.CurrentTNCDevice.Prompts.Command;
					sharedData.TncDeviceArray.TNCDevices[i].Prompts.Timeout = sharedData.CurrentTNCDevice.Prompts.Timeout;
					sharedData.TncDeviceArray.TNCDevices[i].Prompts.Connected = sharedData.CurrentTNCDevice.Prompts.Connected;
					sharedData.TncDeviceArray.TNCDevices[i].Prompts.Disconnected = sharedData.CurrentTNCDevice.Prompts.Disconnected;
					sharedData.TncDeviceArray.TNCDevices[i].Commands.Connect = textBoxCommandsConnect.Text;
					sharedData.TncDeviceArray.TNCDevices[i].Commands.Conversmode = sharedData.CurrentTNCDevice.Commands.Conversmode;
					sharedData.TncDeviceArray.TNCDevices[i].Commands.MyCall = sharedData.CurrentTNCDevice.Commands.MyCall;
					sharedData.TncDeviceArray.TNCDevices[i].Commands.Retry = sharedData.CurrentTNCDevice.Commands.Retry;
					sharedData.TncDeviceArray.TNCDevices[i].Commands.Datetime = sharedData.CurrentTNCDevice.Commands.Datetime;

					break;
				}
			}

			await sharedData.TncDeviceArray.SaveAsync();
			sharedData.SavedTNCDevice = new TNCDevice(sharedData.CurrentTNCDevice);
		}

		//public bool IsTNCAppBarSaveEnabled
		//{
		//	get => _isTNCAppBarSaveEnabled;//_comportSettingsChanged | _initCommandsChanged | _commandsChanged | _promptsChanged;
		//	set => _isTNCAppBarSaveEnabled = value;
		//}

		//public Visibility ShowBluetoothDevices
		//{
		//	get => _isBluetoothDevicesVisible;
		//	set { _isBluetoothDevicesVisible = value; }
		//}

		//public Visibility NotShowBluetoothDevices
		//{
		//	get => ShowBluetoothDevices == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		//}
		private void SetComboBoxVisicility()
		{
			if (toggleSwitchBluetooth.IsOn)
			{
				//ShowBluetoothDevices = Visibility.Visible;
				comboBoxComName.Visibility = Visibility.Visible;
				comboBoxComPort.Visibility = Visibility.Collapsed;
			}
			else
			{
				//ShowBluetoothDevices = Visibility.Collapsed;
				comboBoxComName.Visibility = Visibility.Collapsed;
				comboBoxComPort.Visibility = Visibility.Visible;
			}
		}

		private void toggleSwitchBluetooth_Toggled(object sender, RoutedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.CommPort.IsBluetooth != ((ToggleSwitch)sender).IsOn)
			{
				_bluetoothOnChanged = true;
			}
			else
			{
				_bluetoothOnChanged = false;
			}
			if (((ToggleSwitch)sender).IsOn)
			{
				//ShowBluetoothDevices = Visibility.Visible;
				comboBoxComName.Visibility = Visibility.Visible;
				comboBoxComPort.Visibility = Visibility.Collapsed;
			}
			else
			{
				//ShowBluetoothDevices = Visibility.Collapsed;
				comboBoxComName.Visibility = Visibility.Collapsed;
				comboBoxComPort.Visibility = Visibility.Visible;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void comboBoxComPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!toggleSwitchBluetooth.IsOn && sharedData.SavedTNCDevice.CommPort.Comport != (string)((ComboBox)sender).SelectedValue)
			{
				_comPortChanged = true;
			}
			else
			{
				_comPortChanged = false;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged;
			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
				| _commandsChanged | _promptsChanged;
		}

		private void comboBoxComName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (toggleSwitchBluetooth.IsOn && sharedData.SavedTNCDevice.CommPort.BluetoothName != (string)((ComboBox)sender).SelectedValue)
			{
				_comNameChanged = true;
			}
			else
			{
				_comNameChanged = false;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void comboBoxBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.CommPort.Baudrate != (uint)((ComboBox)sender).SelectedValue)
				_comportBaudRateChanged = true;
			else
				_comportBaudRateChanged = false;

			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void comboBoxDatabits_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.CommPort.Databits != (ushort)((ComboBox)sender).SelectedValue)
			{
				_databitsChanged = true;
			}
			else
			{
				_databitsChanged = false;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void comboBoxParity_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.CommPort.Parity != (SerialParity)((ComboBox)sender).SelectedValue)
			{
				_parityChanged = true;
			}
			else
			{
				_parityChanged = false;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void comboBoxStopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.CommPort.Stopbits != (SerialStopBitCount)((ComboBox)sender).SelectedValue)
			{
				_stopbitsChanged = true;
			}
			else
			{
				_stopbitsChanged = false;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void comboBoxFlowControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.CommPort.Flowcontrol != (SerialHandshake)((ComboBox)sender).SelectedValue)
			{
				_handshakeChanged = true;
			}
			else
			{
				_handshakeChanged = false;
			}
			_comportSettingsChanged = _bluetoothOnChanged | _comNameChanged | _comPortChanged | _comportBaudRateChanged
					| _databitsChanged | _stopbitsChanged | _parityChanged | _handshakeChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void textBoxInitCommandsPre_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.InitCommands.Precommands != (string)((TextBox)sender).Text)
			{
				_initCommandsPreChanged = true;
			}
			else
			{
				_initCommandsPreChanged = false;
			}
			_initCommandsChanged = _initCommandsPreChanged | _initCommandsPostChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void textBoxInitCommandsPost_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.InitCommands.Postcommands != (string)((TextBox)sender).Text)
			{
				_initCommandsPostChanged = true;
			}
			else
			{
				_initCommandsPostChanged = false;
			}
			_initCommandsChanged = _initCommandsPreChanged | _initCommandsPostChanged;

			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		public string TNCPromptsCommand
		{
			get { return sharedData.CurrentTNCDevice.Prompts.Command; }
			set
			{
				sharedData.CurrentTNCDevice.Prompts.Command = value;
				if (sharedData.SavedTNCDevice.Prompts.Command != value)
				{
					_promptsCommandChanged = true;
				}
				else
				{
					_promptsCommandChanged = false;
				}
				_promptsChanged = _promptsCommandChanged | _promptsTimeoutChanged | _promptsConnectedChanged 
						| _promptsDisconnectedChanged;

				appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
						| _commandsChanged | _promptsChanged;
			}
		}

		public string TNCPromptsTimeout
		{
			get => sharedData.CurrentTNCDevice.Prompts.Timeout;
			set
			{
				sharedData.CurrentTNCDevice.Prompts.Timeout = value;
				if (sharedData.SavedTNCDevice.Prompts.Timeout != value)
				{
					_promptsTimeoutChanged = true;
				}
				else
				{
					_promptsTimeoutChanged = false;
				}
				_promptsChanged = _promptsCommandChanged | _promptsTimeoutChanged | _promptsConnectedChanged | _promptsDisconnectedChanged;
				appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
						| _commandsChanged | _promptsChanged;
			}
		}

		//private void textBoxPromptsTimeout_TextChanged(object sender, TextChangedEventArgs e)
		//{
		//	if (sharedData.SavedTNCDevice.Prompts.Timeout != (string)((TextBox)sender).Text)
		//	{
		//		_promptsChanged = true;
		//	}
		//	else
		//	{
		//		_promptsChanged = false;
		//	}
		//	appBarSaveTNC.IsEnabled = _bluetoothOnChanged | _comportSettingsChanged | _initCommandsChanged
		//		| _commandsChanged | _promptsChanged;
		//}

		private void textBoxPromptsConnected_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.Prompts.Connected != (string)((TextBox)sender).Text)
			{
				_promptsConnectedChanged = true;
			}
			else
			{
				_promptsConnectedChanged = false;
			}
			_promptsChanged = _promptsCommandChanged | _promptsTimeoutChanged | _promptsConnectedChanged | _promptsDisconnectedChanged;
			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		private void textBoxPromptsDisconnected_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sharedData.SavedTNCDevice.Prompts.Disconnected != (string)((TextBox)sender).Text)
			{
				_promptsDisconnectedChanged = true;
			}
			else
			{
				_promptsDisconnectedChanged = false;
			}
			_promptsChanged = _promptsCommandChanged | _promptsTimeoutChanged | _promptsConnectedChanged | _promptsDisconnectedChanged;
			appBarSaveTNC.IsEnabled = _comportSettingsChanged | _initCommandsChanged
					| _commandsChanged | _promptsChanged;
		}

		public string TNCCommandsConnect
		{
			get => sharedData.CurrentTNCDevice.Commands.Connect;
			set 
			{
				sharedData.CurrentTNCDevice.Commands.Connect = value;
				if (sharedData.SavedTNCDevice.Commands.Connect != value)
					_commandsConnectChanged = true;
				else
					_commandsConnectChanged = false;

				_commandsChanged = _commandsConnectChanged;
				//appBarSaveTNC.IsEnabled = _bluetoothOnChanged | _comportSettingsChanged | _initCommandsChanged
				//	| _commandsChanged | _promptsChanged;
			}
		}
#endregion // TNC
#region AddressBook
        private void EditAddressBookEntryContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {

        }
		#endregion // AddressBook
#region Distribution List
		enum DistributionListState
		{
			None,
			Edit,
			Add,
			Delete
		}
		DistributionListState _distributionListState = DistributionListState.None;

		private void DistributionListName_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			sender.Text = args.SelectedItem.ToString();
			distributionListItems.Text = DistributionListArray.Instance.DistributionListsDict[sender.Text];
		}

		private void DistributionListName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			// Only get results when it was a user typing, 
			// otherwise assume the value got filled in by TextMemberPath 
			// or the handler for SuggestionChosen.
			if (string.IsNullOrEmpty(distributionListName.Text))
			{
				sender.ItemsSource = DistributionListArray.Instance.GetDistributionListNames();
				return;
			}
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				//Set the ItemsSource to be your filtered dataset
				sender.ItemsSource = DistributionListArray.Instance.GetDistributionListNames(distributionListName.Text);
			}
		}

		private void DistributionListAddItem_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			sender.Text = args.SelectedItem.ToString();
			if (!distributionListItems.Text.Contains(sender.Text))
			{
				if (string.IsNullOrEmpty(distributionListItems.Text))
				{
					distributionListItems.Text = $"{sender.Text}";
					//DistributionListItems.Text = $"{sender.Text}";
				}
				else
				{
					distributionListItems.Text += $", {sender.Text}";
				}
			}
			sender.Text = "";
		}

		private void DistributionListAddItem_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			// Only get results when it was a user typing, 
			// otherwise assume the value got filled in by TextMemberPath 
			// or the handler for SuggestionChosen.
			if (string.IsNullOrEmpty(distributionListAddItem.Text))
			{
				sender.ItemsSource = null;
				return;
			}

			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				//Set the ItemsSource to be your filtered dataset
				sender.ItemsSource = AddressBook.Instance.GetAddressNames(distributionListAddItem.Text);
			}
		}
#endregion
		// Event handling
		private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch ((MyPivot.SelectedItem as PivotItem).Name)
			{
				case "pivotItemAddressBook":
					ContactsCVS.Source = AddressBook.Instance.GetContactsGrouped();
					appBarSettingsSave.Visibility = Visibility.Collapsed;
					SettingsCommandBar.Visibility = Visibility.Visible;
					break;
				case "pivotItemDistributionLists":
					//ContactsCVS.Source = AddressBook.Instance.GetContactsGrouped();
					appBarSettingsSave.Visibility = Visibility.Visible;
					appBarSettingsSave.IsEnabled = DistributionListArray.Instance.DataChanged;
					appBarSettingsEdit.Visibility = Visibility.Visible;
					appBarsettingsDelete.Visibility = Visibility.Visible;
					SettingsCommandBar.Visibility = Visibility.Visible;
					break;
				default:
					SettingsCommandBar.Visibility = Visibility.Collapsed;
					break;
			}
        }

        private AddressBookEntry GetAddressBookEntryEditData()
        {
            AddressBookEntry addressBookEntry = new AddressBookEntry();
            StackPanel panel = editAddressBookEntryContentDialog.Content as StackPanel;
            var panels = panel.Children;
            var subPanels = (panels[0] as StackPanel).Children;
            addressBookEntry.Callsign = (subPanels[1] as TextBox).Text;
            subPanels = (panels[1] as StackPanel).Children;
            addressBookEntry.NameDetail = (subPanels[1] as TextBox).Text;
            subPanels = (panels[2] as StackPanel).Children;
            addressBookEntry.BBSPrimary = (subPanels[1] as ComboBox).SelectedItem as string;
            subPanels = (panels[3] as StackPanel).Children;
            addressBookEntry.BBSSecondary = (subPanels[1] as ComboBox).SelectedItem as string;
            return addressBookEntry;
        }

        private void SetAddressBookEntryEditData(AddressBookEntry addressBookEntry)
        {
            StackPanel panel = editAddressBookEntryContentDialog.Content as StackPanel;
            var panels = panel.Children;
            var subPanels = (panels[0] as StackPanel).Children;
            (subPanels[1] as TextBox).Text = addressBookEntry.Callsign;
            subPanels = (panels[1] as StackPanel).Children;
            (subPanels[1] as TextBox).Text = addressBookEntry.NameDetail;
            subPanels = (panels[2] as StackPanel).Children;
            (subPanels[1] as ComboBox).SelectedItem = addressBookEntry.BBSPrimary;
            subPanels = (panels[3] as StackPanel).Children;
            (subPanels[1] as ComboBox).SelectedItem = addressBookEntry.BBSSecondary;
        }

        private async void AppBarEdit_ClickAsync(object sender, RoutedEventArgs e)
        {
            switch ((MyPivot.SelectedItem as PivotItem).Name)
            {
                case "pivotItemAddressBook":
                    AddressBookEntry addressBookEntry = addressBookListView.SelectedItem as AddressBookEntry;
                    SetAddressBookEntryEditData(addressBookEntry);
                    editAddressBookEntryContentDialog.PrimaryButtonText = "Save";
                    ContentDialogResult result = await editAddressBookEntryContentDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
						AddressBook addressBook = AddressBook.Instance;
						addressBookEntry = GetAddressBookEntryEditData();
                        bool success = addressBook.AddAddressAsync(addressBookEntry);
                        ContactsCVS.Source = addressBook.GetContactsGrouped();
                    }
                    break;
				case "pivotItemDistributionLists":
					_distributionListState = DistributionListState.Edit;
					distributionListAddItem.IsEnabled = true;
					distributionListItems.IsReadOnly = false;
					appBarSettingsSave.IsEnabled = true;
					break;

			}
		}


        private void AppBarDelete_Click(object sender, RoutedEventArgs e)
        {
            switch ((MyPivot.SelectedItem as PivotItem).Name)
            {
                case "pivotItemAddressBook":
                    AddressBook addressBook = AddressBook.Instance;
                    AddressBookEntry addressBookEntry = addressBookListView.SelectedItem as AddressBookEntry;
                    addressBook.DeleteAddress(addressBookEntry);
                    ContactsCVS.Source = addressBook.GetContactsGrouped();
                    break;
				case "pivotItemDistributionLists":
					_distributionListState = DistributionListState.Delete;
					appBarSettingsSave.IsEnabled = true;
					DistributionListArray.Instance.RemoveDistributionList(distributionListName.Text);
					break;
            }
        }

        private async void AppBarAdd_ClickAsync(object sender, RoutedEventArgs e)
        {
            AddressBookEntry addressBookEntry = null;

            switch ((MyPivot.SelectedItem as PivotItem).Name)
            {
                case "pivotItemAddressBook":
                    AddressBook addressBook = AddressBook.Instance;
                    AddressBookEntry emptyEntry = new AddressBookEntry()
                    {
                        Callsign = "",
                        NameDetail = "",
                        BBSPrimary = "",
                        BBSSecondary = "",
                        BBSPrimaryActive = true
                    };
                    SetAddressBookEntryEditData(emptyEntry);
                    editAddressBookEntryContentDialog.Title = "Add Address Book Entry";
                    editAddressBookEntryContentDialog.PrimaryButtonText = "Add";
                    ContentDialogResult result = await editAddressBookEntryContentDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        addressBookEntry = GetAddressBookEntryEditData();
                        bool success = addressBook.AddAddressAsync(addressBookEntry);
						if (!success)
						{
							await Utilities.ShowMessageDialogAsync("Error adding a new address book entry.");
						}
					}
                    ContactsCVS.Source = addressBook.GetContactsGrouped();
                    break;
				case "pivotItemDistributionLists":
					_distributionListState = DistributionListState.Add;
					distributionListName.Text = "";
					distributionListItems.Text = "";
					appBarSettingsSave.IsEnabled = true;
					distributionListAddItem.IsEnabled = true;
					distributionListItems.IsReadOnly = false;
					break;
			}
		}

        private async void AppBarSave_ClickAsync(object sender, RoutedEventArgs e)
        {
            switch ((MyPivot.SelectedItem as PivotItem).Name)
            {
                case "pivotItemAddressBook":
                    break;
				case "pivotItemDistributionLists":
					if (_distributionListState == DistributionListState.Add)
					{
						// Must not exist
						if (DistributionListArray.Instance.DistributionListsDict.TryGetValue(distributionListName.Text, out string items))
						{
							Utilities.ShowMessageDialogAsync("The Distribution List already exists.", "DistributionList List Error");
							return;
						}
						DistributionList list = new DistributionList()
						{
							DistributionListName = distributionListName.Text,
							DistributionListItems = distributionListItems.Text
						};
						DistributionListArray.Instance.AddDistributionList(list);
					}
					else if (_distributionListState == DistributionListState.Edit)
					{
						// Must exist
						if (!DistributionListArray.Instance.DistributionListsDict.TryGetValue(distributionListName.Text, out string items))
						{
							Utilities.ShowMessageDialogAsync("The Distribution List does not exist.", "DistributionList List Error");
							return;
						}
						DistributionList list = new DistributionList()
						{
							DistributionListName = distributionListName.Text,
							DistributionListItems = distributionListItems.Text
						};
						DistributionListArray.Instance.UpdateDistributionList(list);
					}
					else if (_distributionListState == DistributionListState.Delete)
					{
						DistributionListArray.Instance.RemoveDistributionList(distributionListName.Text);
						distributionListName.Text = "";
						distributionListItems.Text = "";
					}
					await DistributionListArray.Instance.SaveAsync();

					_distributionListState = DistributionListState.None;
					appBarSettingsSave.IsEnabled = false;
					distributionListAddItem.IsEnabled = false;
					distributionListItems.IsReadOnly = true;
					break;
			}

        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {            
            if (sender is ToggleSwitch)
            {
                ToggleSwitch toggleSwitch = sender as ToggleSwitch;
                if (toggleSwitch.IsHitTestVisible && !(toggleSwitch.FocusState == FocusState.Unfocused))
                {
                    AddressBook addressBook = AddressBook.Instance;

                    Grid parent = toggleSwitch.Parent as Grid;
                    string callsign = (parent.Children[1] as TextBlock).Text;
                    if (string.IsNullOrEmpty(callsign))
                        return;

                    addressBook.UpdateAddressBookEntry(callsign, toggleSwitch.IsOn);
                    ContactsCVS.Source = addressBook.GetContactsGrouped();
                }
            }
        }

	}
}
