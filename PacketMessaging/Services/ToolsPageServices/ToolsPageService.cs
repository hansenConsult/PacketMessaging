using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketMessaging.Services.ToolsPageServices
{
    class ToolsPageService
    {
        public static ToolsPageService Instance { get; }

        static ToolsPageService()
        {
            // implement singleton pattern
            Instance = Instance ?? new ToolsPageService();
        }

        Template10.Services.SettingsService.ISettingsHelper _helper;
        private ToolsPageService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        public string UserName
        {
            get { return _helper.Read<string>(nameof(UserName), ""); }
            set { _helper.Write(nameof(UserName), value); }
        }

    }
}
