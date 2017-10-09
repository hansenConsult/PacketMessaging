using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;


namespace PacketMessaging.ViewModels
{

	class ToolsPageViewModel : ViewModelBase
	{
		public ToolsPageLogFilePartViewModel SettingsPartViewModel { get; } = new ToolsPageLogFilePartViewModel();


        public class ToolsPageLogFilePartViewModel : ViewModelBase
        {
            Services.SettingsServices.SettingsService _settings;

            public ToolsPageLogFilePartViewModel()
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
        }
        public class ToolsPageTestReceivePartViewModel : ViewModelBase
        {
            Services.SettingsServices.SettingsService _settings;

            public ToolsPageTestReceivePartViewModel()
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
        }

	}
}
