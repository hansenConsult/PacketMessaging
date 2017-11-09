using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Devices.SerialCommunication;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using PacketMessaging.Models;

namespace PacketMessaging.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
	{
		static bool _activeMessageNumber = false;

		public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
		public static IdentityPartViewModel IdentityPartViewModel { get; } = new IdentityPartViewModel();
		public static PacketSettingsPartViewModel PacketSettingsPartViewModel { get; } = new PacketSettingsPartViewModel();
		public static TNCPartViewModel TNCPartViewModel { get; } = new TNCPartViewModel();
        public static AddressBookPartViewModel AddressBookPartViewModel { get; } = new AddressBookPartViewModel();
		public DistributionListsPartViewModel distributionListsPartViewModel { get; } = new DistributionListsPartViewModel();
		public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();

		public static string GetMessageNumberPacket() => GetMessageNumber() + "P";



        private static string GetMessageNumber()
		{
			string messageNumber = "100";

			if (IdentityPartViewModel.UseTacticalCallsign)
			{
				int number = IdentityPartViewModel.MessageNumber;
				messageNumber = IdentityPartViewModel.TacticalMsgPrefix + "-" + number.ToString();
				//messageNumber = TextBox + "-" + number.ToString();
				number++;
				IdentityPartViewModel.MessageNumber = number;
			}
			else
			{
				int number = IdentityPartViewModel.MessageNumber;
				messageNumber = IdentityPartViewModel.UserMsgPrefix + "-" + number.ToString();
				number++;
				IdentityPartViewModel.MessageNumber = number;
			}
			_activeMessageNumber = true;
			return messageNumber;
		}

		public static void ReturnMessageNumber()
		{
			// Only remove the last message number created
			if (!_activeMessageNumber)
				return;

			//string messageNumber;

			if (IdentityPartViewModel.UseTacticalCallsign)
			{
				int number = IdentityPartViewModel.MessageNumber;
				number--;
				//_messageNumber = IdentityPartViewModel.TacticalMsgPrefix + "-" + number.ToString();
				IdentityPartViewModel.MessageNumber = number;
			}
			else
			{
				int number = IdentityPartViewModel.MessageNumber;
				number--;
				//_messageNumber = IdentityPartViewModel.UserMsgPrefix + "-" + messageNumber.ToString();
				IdentityPartViewModel.MessageNumber = number;
			}
			_activeMessageNumber = false;
		}

    }

    public class SettingsPartViewModel : ViewModelBase
    {
        Services.SettingsServices.SettingsService _settings;

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = Services.SettingsServices.SettingsService.Instance;
            }
        }

        public bool UseShellBackButton
        {
            get { return _settings.UseShellBackButton; }
            set { _settings.UseShellBackButton = value; base.RaisePropertyChanged(); }
        }

        public bool UseLightThemeButton
        {
            get { return _settings.AppTheme.Equals(ApplicationTheme.Light); }
            set { _settings.AppTheme = value ? ApplicationTheme.Light : ApplicationTheme.Dark; base.RaisePropertyChanged(); }
        }

        private string _BusyText = "Please wait...";
        public string BusyText
        {
            get { return _BusyText; }
            set
            {
                Set(ref _BusyText, value);
                _ShowBusyCommand.RaiseCanExecuteChanged();
            }
        }

        DelegateCommand _ShowBusyCommand;
        public DelegateCommand ShowBusyCommand
            => _ShowBusyCommand ?? (_ShowBusyCommand = new DelegateCommand(async () =>
            {
                Views.Busy.SetBusy(true, _BusyText);
                await Task.Delay(5000);
                Views.Busy.SetBusy(false);
            }, () => !string.IsNullOrEmpty(BusyText)));

        public bool W1XSCStatusUp
        {
            get
            {
                bool temp = _settings.W1XSCStatusUp;
                AddressBook.Instance.UpdateForBBSStatusChange("W1XSC", temp);
                return temp;
            }
            set
            {
                _settings.W1XSCStatusUp = value;
                base.RaisePropertyChanged();
                AddressBook.Instance.UpdateForBBSStatusChange("W1XSC", W1XSCStatusUp);
            }
        }

        public bool W2XSCStatusUp
        {
            get
            {
                bool temp = _settings.W2XSCStatusUp;
                AddressBook.Instance.UpdateForBBSStatusChange("W2XSC", temp);
                return temp;
            }
            set
            {
                _settings.W2XSCStatusUp = value;
                base.RaisePropertyChanged();
                AddressBook.Instance.UpdateForBBSStatusChange("W2XSC", W2XSCStatusUp);
            }
        }

        public bool W3XSCStatusUp
        {
            get
            {
                bool temp = _settings.W3XSCStatusUp;
                AddressBook.Instance.UpdateForBBSStatusChange("W3XSC", temp);
                return temp;
            }
            set
            {
                _settings.W3XSCStatusUp = value;
                base.RaisePropertyChanged();
                AddressBook.Instance.UpdateForBBSStatusChange("W3XSC", W3XSCStatusUp);
            }
        }

        public bool W4XSCStatusUp
        {
            get
            {
                bool temp = _settings.W4XSCStatusUp;
                AddressBook.Instance.UpdateForBBSStatusChange("W4XSC", temp);
                return temp;
            }
            set
            {
                _settings.W4XSCStatusUp = value;
                base.RaisePropertyChanged();
                AddressBook.Instance.UpdateForBBSStatusChange("W4XSC", W4XSCStatusUp);
            }
        }
        public bool W5XSCStatusUp
        {
            get
            {
                bool temp = _settings.W5XSCStatusUp;
                AddressBook.Instance.UpdateForBBSStatusChange("W5XSC", temp);
                return temp;
            }
            set
            {
                _settings.W5XSCStatusUp = value;
                base.RaisePropertyChanged();
                AddressBook.Instance.UpdateForBBSStatusChange("W5XSC", W5XSCStatusUp);
            }
        }
    }

    public class IdentityPartViewModel : ViewModelBase
	{
		Services.SettingsServices.SettingsService _settings;

		public static TacticalCall _callsignData;
		public static TacticalCallsignData _tacticalCallsignData;

		public IdentityPartViewModel()
		{
			if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
				// designtime
			}
			else
			{
				_settings = Services.SettingsServices.SettingsService.Instance;
				//TacticalCallsigns = _TacticalCallsigns;
			}
		}

		public string UserCallsign
		{
			get { return _settings.UserCallsign; }
			set
			{
				_settings.UserCallsign = value;
				base.RaisePropertyChanged();
				if (_settings.UserCallsign.Length > 3)
				{
					UserMsgPrefix = _settings.UserCallsign.Substring(_settings.UserCallsign.Length - 3, 3);
				}
			}
		}

		public string UserName
		{
			get { return _settings.UserName; }
			set { _settings.UserName = value; base.RaisePropertyChanged(); }
		}

		public string UserMsgPrefix
		{
			get { return _settings.UserMsgPrefix; }
			set { _settings.UserMsgPrefix = value; base.RaisePropertyChanged(); }
		}

		public bool UseTacticalCallsign
		{
			get { return _settings.UseTacticalCallsign; }
			set { _settings.UseTacticalCallsign = value; base.RaisePropertyChanged(); }
		}

		public string TacticalCallsign
		{
			get { return _settings.TacticalCallsign; }
			set { _settings.TacticalCallsign = value; base.RaisePropertyChanged(); }
		}

		public int TacticalCallsignAreaSelectedIndex
		{
			get { return _settings.TacticalCallsignAreaSelectedIndex; }
			set
			{
				_settings.TacticalCallsignAreaSelectedIndex = value;
				base.RaisePropertyChanged();
			}
		}

		public int TacticalCallsignSelectedIndex
		{
			get { return _settings.TacticalCallsignSelectedIndex; }
			set
			{
				if (value == -1)
				{

				}
				else
				{
					_settings.TacticalCallsignSelectedIndex = value;

					if (Views.SettingsPage.listOfTacticallsignsArea[_settings.TacticalCallsignAreaSelectedIndex].TacticalCallsigns != null)
					{
						_callsignData = Views.SettingsPage.listOfTacticallsignsArea[_settings.TacticalCallsignAreaSelectedIndex].TacticalCallsigns.TacticalCallsignsArray[value];
						TacticalCallsign = _callsignData.TacticalCallsign;
						TacticalMsgPrefix = _callsignData.Prefix;
						TacticalPrimary = _callsignData.PrimaryBBS;
						TacticalPrimaryActive = _callsignData.PrimaryBBSActive;
						TacticalSecondary = _callsignData.SecondaryBBS;
						TacticalSecondaryActive = _callsignData.SecondaryBBSActive;
					}
					base.RaisePropertyChanged();
				}
			}
		}

		public string TacticalCallsignOther
		{
			get => _settings.TacticalCallsignOther;
			set
			{
				_settings.TacticalCallsignOther = value;
				base.RaisePropertyChanged();
				TacticalCallsign = _settings.TacticalCallsignOther;
				TacticalMsgPrefix = TacticalCallsign.Substring(3, 3);
			}
		}

		public string TacticalMsgPrefix
		{
			get { return _settings.TacticalMsgPrefix; }
			set { _settings.TacticalMsgPrefix = value; base.RaisePropertyChanged(); }
		}

		public string TacticalPrimary
		{
			get { return _settings.TacticalPrimary; }
			set { _settings.TacticalPrimary = value; base.RaisePropertyChanged(); }
		}

		public bool TacticalPrimaryActive
		{
			get { return _settings.TacticalPrimaryActive; }
			set
			{
				_settings.TacticalPrimaryActive = value;
				if (_callsignData == null)
					_callsignData = Views.SettingsPage.listOfTacticallsignsArea[TacticalCallsignAreaSelectedIndex].TacticalCallsigns.TacticalCallsignsArray[TacticalCallsignSelectedIndex];

				_callsignData.PrimaryBBSActive = TacticalPrimaryActive;
				_tacticalCallsignData.TacticalCallsignsChanged = true;
				//AddressBook.UpdateEntry(_callsignData);
				base.RaisePropertyChanged();
			}
		}

		public string TacticalSecondary
		{
			get { return _settings.TacticalSecondary; }
			set { _settings.TacticalSecondary = value; base.RaisePropertyChanged(); }
		}

		public bool TacticalSecondaryActive
		{
			get { return _settings.TacticalSecondaryActive; }
			set
			{
				_settings.TacticalSecondaryActive = value;
				if (_callsignData == null)
					_callsignData = Views.SettingsPage.listOfTacticallsignsArea[TacticalCallsignAreaSelectedIndex].TacticalCallsigns.TacticalCallsignsArray[TacticalCallsignSelectedIndex];

				_callsignData.SecondaryBBSActive = TacticalSecondaryActive;
				_tacticalCallsignData.TacticalCallsignsChanged = true;
				//AddressBook.UpdateEntry(_callsignData);
				base.RaisePropertyChanged();
			}
		}

		public bool DisplayIdentityAtStartup
		{
			get { return _settings.DisplayIdentityAtStartup; }
			set { _settings.DisplayIdentityAtStartup = value; base.RaisePropertyChanged(); }
		}

		public int MessageNumber
		{
			get { return _settings.MessageNumber; }
			set { _settings.MessageNumber = value; base.RaisePropertyChanged(); }
		}

	}

	public class PacketSettingsPartViewModel : ViewModelBase
	{
		Services.SettingsServices.SettingsService _settings;
		ObservableCollection<Profile> _profileCollection = new ObservableCollection<Profile>();

		public PacketSettingsPartViewModel()
		{
			if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
				// designtime
			}
			else
			{
				_settings = Services.SettingsServices.SettingsService.Instance;
			}
		}

		public int ProfileSelectedIndex
		{
			get
			{
				return _settings.ProfileSelectedIndex;
			}
			set
			{
				if (value < 0)
					_settings.ProfileSelectedIndex = 0;
				else
				{
					_settings.ProfileSelectedIndex = value;
				}
				MessageTo = SharedData.SharedDataInstance.ProfileArray.Profiles[_settings.ProfileSelectedIndex].SendTo;
				base.RaisePropertyChanged();
			}
		}

		public string MessageTo
		{
			get { return _settings.MessageTo; }
			set { _settings.MessageTo = value; base.RaisePropertyChanged(); }
		}

		public bool SendReceivedReceipt
		{ get; set; }
	}

	public class TNCPartViewModel : ViewModelBase
	{
		Services.SettingsServices.SettingsService _settings;
		//string _TNCPrompsCommand;

		//SerialParity _SelectedParity;
		//SerialStopBitCount _SelectedStopBits;
		//SerialHandshake _SelectedHandshake;
		bool _tncChanged = false;


		public TNCPartViewModel()
		{
			_settings = Services.SettingsServices.SettingsService.Instance;
		}

		public int TNCDeviceSelectedIndex
        {
			get { return _settings.TNCDeviceSelectedIndex; }
			set
			{
				_settings.TNCDeviceSelectedIndex = value;
				base.RaisePropertyChanged();
			}
		}

		public bool TNCChanged
		{
			get => _tncChanged;
			set { _tncChanged = value; base.RaisePropertyChanged(); }
		}

	}

    public class AddressBookPartViewModel : ViewModelBase
    {
        Services.SettingsServices.SettingsService _settings;

        string _addressBookCallsign = "KZ6DM";

        public AddressBookPartViewModel()
        {
            _settings = Services.SettingsServices.SettingsService.Instance;
        }

        public string AddressBookCallsign
        { get => _addressBookCallsign; set { _addressBookCallsign = value; base.RaisePropertyChanged(); } }
    }

	public class DistributionListsPartViewModel : ViewModelBase
	{
		Services.SettingsServices.SettingsService _settings;

		DistributionListArray distributionLists = DistributionListArray.Instance;


		public DistributionListsPartViewModel()
		{
			_settings = Services.SettingsServices.SettingsService.Instance;
		}

		public string DistributionListName
		{ get => _settings.DistributionListName; set { _settings.DistributionListName = value; base.RaisePropertyChanged(); } }

		public string DistributionListItems
		{ get => _settings.DistributionListItems; set { _settings.DistributionListItems = value; base.RaisePropertyChanged(); } }
	}

	public class AboutPartViewModel : ViewModelBase
	{
		public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

		public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

		public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

		public string Version
		{
			get
			{
				var v = Windows.ApplicationModel.Package.Current.Id.Version;
				return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
			}
		}

		public Uri RateMe => new Uri("http://aka.ms/template10");
	}
}

