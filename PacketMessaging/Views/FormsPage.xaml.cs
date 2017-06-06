using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FormControlBaseClass;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using MetroLog;
using Windows.UI.Popups;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.Devices.SerialCommunication;
using Windows.ApplicationModel;
using System.Collections.ObjectModel;

namespace PacketMessaging.Views
{
	//public static class Tools
	//{
	//	public static bool CheckTypeIsSubClassOf(Type sub, Type super)
	//	{
	//		return super == sub || super.GetTypeInfo().IsAssignableFrom(sub.GetTypeInfo());
	//	}

	//	public static async Task<List<Assembly>> GetAssemblyList()
	//	{
	//		List<Assembly> assemblies = new List<Assembly>();

	//		var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
	//		//var files = await folder.GetFilesAsync();
	//		//var result = folder.CreateFileQuery();
	//		var files = ViewModels.SharedData.filesInInstalledLocation;
	//		if (files == null)
	//			return assemblies;

	//		foreach (var file in files.Where(file => file.FileType == ".dll" && file.Name.Contains("FormControl")))
	//		{
	//			try
	//			{
	//				assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
	//			}
	//			catch (Exception ex)
	//			{
	//				Debug.WriteLine(ex.Message);
	//			}

	//		}

	//		return assemblies;
	//	}
	//}


	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class FormsPage : Page
	{
		private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<FormsPage>();

		public enum ReturnResult
		{
			Cancel = 0,
			Send = 2,
			Save = 4
		}
		MainPage rootPage = MainPage.Current;


		//bool WriteBytesAvailable = false;

		List<Control> _formFieldsList = new List<Control>();

		Template10.Services.SerializationService.ISerializationService _SerializationService;

		//Compositor _compositor;

		//string _openFilePath;
		//PacForms pacForm;
		PacketMessage _packetMessage;
		bool _loadMessage = false;

		FormControlBase _packetForm;
		SendFormDataControl _packetAddressForm;

		public static Task<List<Assembly>> AssemblyList;
		//bool _forceReadBulletins = false;

		// Serial Device
		//private SuspendingEventHandler appSuspendEventHandler;
		//private EventHandler<Object> appResumeEventHandler;

		//private Collection<DeviceListEntry> listOfDevices;

		//private Dictionary<DeviceWatcher, String> mapDeviceWatchersToDeviceSelector;
		//private Boolean watchersSuspended;
		//private Boolean watchersStarted;

		// Has all the devices enumerated by the device watcher?
		//private Boolean isAllDevicesEnumerated;

		//List<PacketMessage> _packetMessagesReceived = new List<PacketMessage>();
		//List<PacketMessage> _packetMessagesToSend = new List<PacketMessage>();

		public ReturnResult DialogAction { get; private set; }
		//public string MessageSubject { get { return messageSubject.Text; } set { messageSubject.Text = value; } }
		//public string MessageBBS { get { return messageBBS.Text; } set { messageBBS.Text = value; } }
		//public string MessageTNC { get { return messageTNC.Text; } set { messageTNC.Text = value; }  }
		//public string MessageFormName { get; set; }
		//public string MessageFrom { get { return messageFrom.Text; } set { messageFrom.Text = value; } }
		//public string MessageTo { get { return messageTo.Text; } set { messageTo.Text = value; } }
		public string MessageNumber { get { return _packetForm.MessageNo; } set { _packetForm.MessageNo = value; } }

		public FormsPage()
		{
			this.InitializeComponent();

			_SerializationService = Template10.Services.SerializationService.SerializationService.Json;

			//_packetMessage = new PacketMessage();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			//DeviceListSource.Source = listOfDevices;
			if (e.Parameter == null)
				return;

			int index = 0;
			var packetMessagePath = (string)_SerializationService.Deserialize((string)e.Parameter);
			_packetMessage = PacketMessage.Open(packetMessagePath);
			_loadMessage = true;
			foreach (PivotItem pivotItem in MyPivot.Items)
			{
				string pivotItemName = pivotItem.Name.Replace('_', '-');	// required since city-scan is not a valid PivotItem name
				if (pivotItemName == _packetMessage.PacFormName)
				{
					MyPivot.SelectedIndex = index;
					//CreatePacketForm();
					//FillFormFromPacketMessage();
					break;
				}
				index++;
			}
		}

		public void ScanControls(DependencyObject panelName)
		{
			var childrenCount = VisualTreeHelper.GetChildrenCount(panelName);

			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject control = VisualTreeHelper.GetChild(panelName, i);
				if (control is StackPanel || control is Grid || control is Border || control is RelativePanel)
				{
					ScanControls(control);
				}
				else if (control is TextBox || control is ComboBox || control is CheckBox)
				{
					_formFieldsList.Add((Control)control);
				}
			}
		}


		// Create a packetMessage from the filled out form
		public void CreatePacketMessage()
		{
            _packetMessage = new PacketMessage()
            {
                BBSName = _packetAddressForm.MessageBBS,
                TNCName = _packetAddressForm.MessageTNC,
                FormFieldArray = _packetForm.CreateFormFieldsInXML(),
                PacFormName = _packetForm.PacFormName,
                MessageFrom = _packetAddressForm.MessageFrom,
                MessageTo = _packetAddressForm.MessageTo,
                MessageNumber = _packetForm.MessageNo
            };
            AddressBook.AddAddressAsync(_packetMessage.MessageTo);
            string subject = _packetForm.CreateSubject();
			// subject is "null" for Simple Message, otherwise use the form generated subject line
			_packetMessage.Subject = (subject ?? _packetAddressForm.MessageSubject );
			//MessageSubject = _packetMessage.MessageSubject;
			_packetMessage.CreateFileName();
		}

		public void FillFormFromPacketMessage()
		{
			_packetAddressForm.MessageBBS = _packetMessage.BBSName;
			_packetAddressForm.MessageTNC = _packetMessage.TNCName;
			_packetForm.FillFormFromFormFields(_packetMessage.FormFieldArray);
			_packetAddressForm.MessageFrom = _packetMessage.MessageFrom;
			_packetAddressForm.MessageTo = _packetMessage.MessageTo;
			MessageNumber = _packetMessage.MessageNumber;
			_packetAddressForm.MessageSubject = _packetMessage.Subject;
		}

		//async void CreatePacketForm()
		//{
		//	_packetForm = CreateFormControlInstance(_packetMessage.PacFormName);
		//	if (_packetForm == null)
		//	{
		//		MessageDialog messageDialog = new MessageDialog($"Form {_packetMessage.PacFormName} not found");
		//		await messageDialog.ShowAsync();
		//		return;
		//	}
		//	_packetAddressForm = new SendFormDataControl();

		//	if (_packetMessage.PacFormName == "SimpleMessage")
		//	{
		//		messageFormPanel.Children.Clear();
		//		messageFormPanel.Children.Insert(0, _packetAddressForm);
		//		messageFormPanel.Children.Insert(1, _packetForm);
		//	}
		//	else
		//	{
		//		Form213Panel.Children.Clear();
		//		Form213Panel.Children.Insert(0, _packetForm);
		//		Form213Panel.Children.Insert(1, _packetAddressForm);

		//		_packetForm.eventSubjectChanged += FormControl_SubjectChange;

		//		//switch (_packetMessage.PacFormName)
		//		//{
		//		//	case "CERT-DA-MTVUniversal-message":
		//		//		//((ICS213MVControl)_packetForm).eventTacticalCallsign += ICS213MVControl_TacticalCallsignChange;
		//		//		((ICS213MVControl)_packetForm).CERTPositions = IdentitySettings.mtvCERTTacticalCallList;
		//		//		((ICS213MVControl)_packetForm).TacticalCallsign = Properties.Settings.Default.TacticalCallsign;
		//		//		break;
		//		//}
		//		DateTime now = DateTime.Now;
		//		_packetForm.OperatorDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year:d2}";
		//		_packetForm.OperatorTime = $"{now.Hour:d2}{now.Minute:d2}";
		//		//packetFormPanel.Children.Add(_packetForm);
		//	}
		//	MessageNumber = ViewModels.SettingsPageViewModel.GetMessageNumberPacket();
		//}

		//public void ProcessFormByName(string formName)
		//{
		//	TNCDevice tncDevice;
		//	if (MainPage._tncTypes.TryGetValue(_currentProfile.TNC, out tncDevice))
		//	{
		//		BBSData bbs;
		//		if (MainPage._bbsTypes.TryGetValue(_currentProfile.BBS, out bbs))
		//		{
		//			PacketMessage packetMessage = new PacketMessage();
		//			packetMessage.PacFormName = formName;
		//			packetMessage.MessageNumber = MainPage.GetMessageNumberPacket();
		//			packetMessage.BBSConnectName = bbs.ConnectName;
		//			packetMessage.TNCName = tncDevice.Name;
		//			packetMessage.MessageFrom = Properties.Settings.Default.UseTacticalCallsign ? Properties.Settings.Default.TacticalCallsign : Properties.Settings.Default.UserCallSign;

		//			var messageWindow = new PacketFormWindow(ref packetMessage);
		//			//messageWindow.DefaultFolderPath = defaultFolderPath;
		//			DateTime now = DateTime.Now;
		//			messageWindow.MessageDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year:d2}";
		//			messageWindow.MessageTime = $"{now.Hour:d2}{now.Minute:d2}";
		//			//messageWindow.MessageTo = _defaultMessageTo;					
		//			messageWindow.MessageTo = _currentProfile.SendTo;
		//			messageWindow.OperatorCallsign = Properties.Settings.Default.UserCallSign;
		//			messageWindow.OperatorName = Properties.Settings.Default.UserName;
		//			if (messageWindow.ShowDialog() == true)
		//			{
		//				// Get the updated packetMessage
		//				ProcessMessageWindow(ref messageWindow);
		//			}
		//			else
		//			{
		//				// Return the unused message number
		//				MainPage.ReturnMessageNumber();
		//			}
		//		}
		//		else
		//		{
		//			//System.Windows.MessageDialog.Show("The BBS could not be found");
		//			//log.Error("The BBS could not be found in Packet Message Window");
		//		}
		//	}
		//	else
		//	{
		//		//System.Windows.MessageDialog.Show($"Could not find the requested TNC ({_currentProfile.TNC})");
		//		//log.Error($"Could not find the requested TNC ({_currentProfile.TNC})");
		//	}
		//}

		public static FormControlBase CreateFormControlInstance(string controlName)
		{
			FormControlBase formControl = null;
			var files = ViewModels.SharedData.filesInInstalledLocation;
			if (files == null)
				return null;

			Type foundType = null;
			foreach (var file in files.Where(file => file.FileType == ".dll" && file.Name.Contains("FormControl")))
			{
				try
				{
					Assembly assembly = Assembly.Load(new AssemblyName(file.DisplayName));
					foreach (Type classType in assembly.GetTypes())
					{
						var attrib = classType.GetTypeInfo();
						foreach (CustomAttributeData customAttribute in attrib.CustomAttributes.Where(customAttribute => customAttribute.GetType() == typeof(CustomAttributeData)))
						{
							var namedArguments = customAttribute.NamedArguments;
							if (namedArguments.Count > 0)
							{
								var formControlName = namedArguments[0].TypedValue.Value;
								//var arg1 = Enum.Parse(typeof(FormControlAttribute.FormType), namedArguments[1].TypedValue.Value.ToString());
								//var arg2 = namedArguments[2].TypedValue.Value;
								if (formControlName is string frmControlName && frmControlName == controlName)
								{
									foundType = classType;
									break;
								}
							}
						}
						if (foundType != null)
							break;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
				if (foundType != null)
					break;
			}

			if (foundType != null)
			{
				try
				{
					formControl = (FormControlBase)Activator.CreateInstance(foundType);
				}
				catch (Exception e)
				{
					string message = e.Message;
				}
			}

			return formControl;
		}

		void FormControl_SubjectChange(object sender, FormEventArgs e)
		{
			if (e?.SubjectLine?.Length > 0)
			{
				if (_packetMessage != null)
				{
					_packetMessage.Subject = _packetForm.CreateSubject();
					_packetAddressForm.MessageSubject = _packetMessage.Subject;
				}
			}
		}

		private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModels.SettingsPageViewModel.ReturnMessageNumber();

			_packetAddressForm = new SendFormDataControl();
			string pivotItemName = ((PivotItem)((Pivot)sender).SelectedItem).Name.Replace('_', '-');    // required since city-scan is not a valid PivotItem name
			_packetForm = CreateFormControlInstance(pivotItemName);
			if (!_loadMessage)
				_packetMessage = new PacketMessage();
			_packetForm.MessageNo = ViewModels.SettingsPageViewModel.GetMessageNumberPacket();

			//var pivotItem = (PivotItem)((Pivot)sender).SelectedItem;
			if (pivotItemName == "SimpleMessage")
			{
				messageFormPanel.Children.Clear();
				messageFormPanel.Children.Insert(0, _packetAddressForm);
				messageFormPanel.Children.Insert(1, _packetForm);
				_packetAddressForm.MessageSubject = _packetForm.MessageNo + "_O/R_";
			}
			else if (pivotItemName == "Message")
			{
				Form213Panel.Children.Clear();
				Form213Panel.Children.Insert(0, _packetForm);
				Form213Panel.Children.Insert(1, _packetAddressForm);
				_packetAddressForm.MessageSubject = _packetForm.CreateSubject();
			}
			else if (pivotItemName == "city-scan")
			{
				Form213Panel.Children.Clear();
				CityScanPanel.Children.Clear();
				CityScanPanel.Children.Insert(0, _packetForm);
				CityScanPanel.Children.Insert(1, _packetAddressForm);
				_packetAddressForm.MessageSubject = _packetForm.CreateSubject();
			}
			else if (pivotItemName == "EOCLogisticsRequest")
            {
				Form213Panel.Children.Clear();
				LogisticsPanel.Children.Clear();
				LogisticsPanel.Children.Insert(0, _packetForm);
				LogisticsPanel.Children.Insert(1, _packetAddressForm);
				_packetAddressForm.MessageSubject = _packetForm.CreateSubject();
			}
			if (!_loadMessage)
			{
				_packetForm.EventSubjectChanged += FormControl_SubjectChange;
			}
			DateTime now = DateTime.Now;
			_packetForm.MsgDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year - 2000:d2}";
			_packetForm.MsgTime = $"{now.Hour:d2}{now.Minute:d2}";
			_packetForm.OperatorDate = $"{now.Month:d2}/{now.Day:d2}/{now.Year - 2000:d2}";
			_packetForm.OperatorTime = $"{now.Hour:d2}{now.Minute:d2}";
			_packetForm.OperatorName = ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserName;
			_packetForm.OperatorCallsign = ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign;

			if (_loadMessage)
			{
				ViewModels.SettingsPageViewModel.ReturnMessageNumber();	// Use original message number

				FillFormFromPacketMessage();
				_loadMessage = false;
			}
		}

		private void ICS213Control_Loaded(object sender, RoutedEventArgs e)
		{

		}
#region SendMessage
		private async void AppBarSend_ClickAsync(object sender, RoutedEventArgs e)
		{
			if (_packetForm.ValidateForm() == false)
			{
				MessageDialog messageDialog = new MessageDialog("Please fill out areas in red.\nPress \"Send\" again to continue.", "Form Error");
				await messageDialog.ShowAsync();

				return;
			}
			if (_packetAddressForm.ValidateForm() == false)
			{
				MessageDialog messageDialog = new MessageDialog("Missing BBS name, TNC name or To/From. \nAdd the missing information and press \"Send\" again to continue.", "Form Error");
				await messageDialog.ShowAsync();
				return;
			}

			CreatePacketMessage();
			DateTime dateTime = DateTime.Now;
			_packetMessage.CreateTime = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";


			_packetMessage.Save(MainPage.unsentMessagesFolder.Path);
			Services.CommunicationsService.CommunicationsService communicationsService = Services.CommunicationsService.CommunicationsService.CreateInstance();
			communicationsService.BBSConnectAsync();
		}


#endregion SendMessage

		private void AppBarSave_Click(object sender, RoutedEventArgs e)
		{
			CreatePacketMessage();
			DateTime dateTime = DateTime.Now;
			_packetMessage.CreateTime = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";

			_packetMessage.Save(MainPage._draftMessagesFolder.Path);
			_packetForm.MessageNo = ViewModels.SettingsPageViewModel.GetMessageNumberPacket();
		}

		//private async void appBarOpen_Click(object sender, RoutedEventArgs e)
		//{
		//	StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", MainPage.draftMessagesFolder);

		//	FileOpenPicker fileOpenPicker = new FileOpenPicker();
		//	fileOpenPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
		//	fileOpenPicker.FileTypeFilter.Add(".xml");
		//	StorageFile file = await fileOpenPicker.PickSingleFileAsync();
		//	if (file != null)
		//	{
		//		_packetMessage = PacketMessage.Open(MainPage.draftMessagesFolder.Path + @"\" + file.Name);

		//		foreach (PivotItem pivotItem in MyPivot.Items)
		//		{
		//			if (pivotItem.Name == _packetMessage.PacFormName)
		//			{
		//				MyPivot.SelectedItem = pivotItem;
		//				CreatePacketForm();
		//				FillFormFromPacketMessage();
		//				break;
		//			}
		//		}
		//	}
		//}
	}
}