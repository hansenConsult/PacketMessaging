using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.IO;
using Windows.Storage;
using FormControlBaseClass;
using System.Collections.ObjectModel;

namespace PacketMessaging.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		Services.SettingsServices.SettingsService _settings;

		public static MainPageViewModel _MainPageViewModel { get; } = new MainPageViewModel();

#pragma warning disable CS0414 // The field 'MainPageViewModel.startup' is assigned but its value is never used
		static bool startup = true;	// Forms dies without startup defined
#pragma warning restore CS0414 // The field 'MainPageViewModel.startup' is assigned but its value is never used

		public MainPageViewModel()
		{
			if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
			}
			else
			{
				_settings = Services.SettingsServices.SettingsService.Instance;
			}
		}

		//public double GridViewItemWidth
		//{
		//	get => _settings.GridViewItemWidth; 
		//	set { _settings.GridViewItemWidth = value; base.RaisePropertyChanged(); }
		//}




#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
		public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
		{

			//await Task.CompletedTask;

			//if (_settings.DisplayIdentityAtStartup && startup)
			//{
			//	NavigationService?.Navigate(typeof(Views.SettingsPage), 1);
			//	startup = false;
			//}
		}

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
			await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
		{
            var selfNav = args.TargetPageType == typeof(Views.MainPage);
            if (selfNav)
            {
                args.Cancel = true;
                return;
            }

            //args.Cancel = false;
            await Task.CompletedTask;
        }

        //public void GotoSettings() =>
        //	NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        //public void GotoForms() =>
        //	NavigationService.Navigate(typeof(Views.FormsPage), 1);

        //public void GotoAbout() =>
        //	NavigationService.Navigate(typeof(Views.SettingsPage), 2);

    }
}

