using System;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Utils;
using Windows.Storage;
using Windows.UI.Xaml;

namespace PacketMessaging.Services.FormsPageService
{
    public class FormsPageService
    {
        public static FormsPageService Instance { get; }

        static FormsPageService()
        {
            // implement singleton pattern
            Instance = Instance ?? new FormsPageService();
        }

        Template10.Services.SettingsService.ISettingsHelper _helper;
        private FormsPageService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        public string TestFileName
        {
            get { return _helper.Read<string>(nameof(TestFileName), ""); }
            set { _helper.Write(nameof(TestFileName), value.ToString()); }
        }
    }
}