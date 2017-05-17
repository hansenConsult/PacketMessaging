using System;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Utils;
using Windows.Storage;
using Windows.UI.Xaml;

namespace PacketMessaging.Services.SettingsServices
{
	public class SettingsService
	{
		public static SettingsService Instance { get; }

		static SettingsService()
		{
			// implement singleton pattern
			Instance = Instance ?? new SettingsService();
		}

		Template10.Services.SettingsService.ISettingsHelper _helper;
		private SettingsService()
		{
			_helper = new Template10.Services.SettingsService.SettingsHelper();
		}

		public bool UseShellBackButton
		{
			get { return _helper.Read<bool>(nameof(UseShellBackButton), true); }
			set
			{
				_helper.Write(nameof(UseShellBackButton), value);
				BootStrapper.Current.NavigationService.Dispatcher.Dispatch(() =>
				{
					BootStrapper.Current.ShowShellBackButton = value;
					BootStrapper.Current.UpdateShellBackButton();
					BootStrapper.Current.NavigationService.Refresh();
				});
			}
		}

		public ApplicationTheme AppTheme
		{
			get
			{
				var theme = ApplicationTheme.Light;
				var value = _helper.Read<string>(nameof(AppTheme), theme.ToString());
				return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
			}
			set
			{
				_helper.Write(nameof(AppTheme), value.ToString());
				(Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
				Views.Shell.HamburgerMenu.RefreshStyles(value);
			}
		}

		public string UserCallsign
		{
			get { return _helper.Read<string>(nameof(UserCallsign), ""); }
			set { _helper.Write(nameof(UserCallsign), value.ToString()); }
		}

		public bool UseTacticalCallsign
		{
			get { return _helper.Read<bool>(nameof(UseTacticalCallsign), false); }
			set { _helper.Write(nameof(UseTacticalCallsign), value); }
		}

		public string UserName
		{
			get { return _helper.Read<string>(nameof(UserName), ""); }
			set { _helper.Write(nameof(UserName), value); }
		}

		public string UserCallSign
		{
			get { return _helper.Read<string>(nameof(UserCallSign), ""); }
			set { _helper.Write(nameof(UserCallSign), value); }
		}

		public string UserMsgPrefix
		{
			get { return _helper.Read<string>(nameof(UserMsgPrefix), UserCallSign.Length > 3 ? UserCallSign.Substring(UserCallSign.Length - 3) : ""); }
			set { _helper.Write(nameof(UserMsgPrefix), value); }
		}

		public string TacticalCallsign
		{
			get { return _helper.Read<string>(nameof(TacticalCallsign), ""); }
			set { _helper.Write(nameof(TacticalCallsign), value); }
		}

		public int TacticalCallsignAreaSelectedIndex
		{
			get { return _helper.Read<int>(nameof(TacticalCallsignAreaSelectedIndex), 0); }
			set { _helper.Write(nameof(TacticalCallsignAreaSelectedIndex), value); }
		}

		public int TacticalCallsignSelectedIndex
		{
			get { return _helper.Read<int>(nameof(TacticalCallsignSelectedIndex), 0); }
			set { _helper.Write(nameof(TacticalCallsignSelectedIndex), value); }
		}

		public string TacticalCallsignOther
		{
			get { return _helper.Read<string>(nameof(TacticalCallsignOther), ""); }
			set { _helper.Write(nameof(TacticalCallsignOther), value); }
		}

		public string TacticalMsgPrefix
		{
			get { return _helper.Read<string>(nameof(TacticalMsgPrefix), ""); }
			set { _helper.Write(nameof(TacticalMsgPrefix), value); }
		}

		public string TacticalPrimary
		{
			get { return _helper.Read<string>(nameof(TacticalPrimary), ""); }
			set { _helper.Write(nameof(TacticalPrimary), value); }
		}

		public bool TacticalPrimaryActive
		{
			get { return _helper.Read<bool>(nameof(TacticalPrimaryActive), true); }
			set { _helper.Write(nameof(TacticalPrimaryActive), value); }
		}

		public string TacticalSecondary
		{
			get { return _helper.Read<string>(nameof(TacticalSecondary), ""); }
			set { _helper.Write(nameof(TacticalSecondary), value); }
		}

		public bool TacticalSecondaryActive
		{
			get { return _helper.Read<bool>(nameof(TacticalSecondaryActive), true); }
			set { _helper.Write(nameof(TacticalSecondaryActive), value); }
		}

		public bool DisplayIdentityAtStartup
		{
			get { return _helper.Read<bool>(nameof(DisplayIdentityAtStartup), false); }
			set { _helper.Write(nameof(DisplayIdentityAtStartup), value); }
		}

		public int MessageNumber
		{
			get { return _helper.Read<int>(nameof(MessageNumber), 100); }
			set { _helper.Write(nameof(MessageNumber), value); }
		}

		public TimeSpan CacheMaxDuration
		{
			get { return _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
			set
			{
				_helper.Write(nameof(CacheMaxDuration), value);
				BootStrapper.Current.CacheMaxDuration = value;
			}
		}


		public int ProfileSelectedIndex
		{
			get { return _helper.Read<int>(nameof(ProfileSelectedIndex), 0); }
			set { _helper.Write(nameof(ProfileSelectedIndex), value); }
		}

		public string MessageTo
		{
			get { return _helper.Read<string>(nameof(MessageTo), ""); }
			set { _helper.Write(nameof(MessageTo), value); }
		}

		public string JNOSAreas
		{
			get { return _helper.Read<string>(nameof(JNOSAreas), "XSCPERM, XSCEVENT"); }
			set { _helper.Write(nameof(JNOSAreas), value); }
		}

#region TNC
		public int TNCDeviceSelectedIndex
		{
			get { return _helper.Read<int>(nameof(TNCDeviceSelectedIndex), 0); }
			set { _helper.Write(nameof(TNCDeviceSelectedIndex), value); }
		}

#endregion
#region MainPage
		//public double GridViewItemWidth
		//{
		//	get { return _helper.Read(nameof(GridViewItemWidth), 218.0); }
		//	set { _helper.Write(nameof(GridViewItemWidth), value); }
		//}
		#endregion
	}
}

