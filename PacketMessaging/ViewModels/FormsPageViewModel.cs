using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Template10.Services.SettingsService;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using FormControlBaseClass;


namespace PacketMessaging.ViewModels
{
	public class FormsPageViewModel : ViewModelBase
	{
		public FormsPageViewModel()
		{
			if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
				//PacMessage = null;
			}
		}

        //private PacketMessage _packetMessage = null;
        //public PacketMessage PacMessage { get { return _packetMessage; } set { Set(ref _packetMessage, value); } }

        //public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        //{
        //    PacMessage = (suspensionState.ContainsKey(nameof(PacMessage))) ? (PacketMessage)suspensionState[nameof(PacMessage)] : (PacketMessage)parameter;
        //    string formName = PacMessage.PacFormName;
        //    await Task.CompletedTask;
        //}
    }
}