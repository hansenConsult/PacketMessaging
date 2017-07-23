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
using Windows.Graphics.Printing;
using Windows.UI.Popups;

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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Printing
            if (!PrintManager.IsSupported())
            {
                // Remove the print button
                //InvokePrintingButton.Visibility = Visibility.Collapsed;

                // Inform user that Printing is not supported
                var messageDialog = new MessageDialog("Printing is not supported.");
                await messageDialog.ShowAsync();
            }
            //    PacMessage = (suspensionState.ContainsKey(nameof(PacMessage))) ? (PacketMessage)suspensionState[nameof(PacMessage)] : (PacketMessage)parameter;
            //    string formName = PacMessage.PacFormName;
            //    await Task.CompletedTask;
        }
    }
}