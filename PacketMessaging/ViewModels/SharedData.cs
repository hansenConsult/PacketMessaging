using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.Storage;

namespace PacketMessaging.ViewModels
{
    public class SharedData : ViewModelBase
    {
        private static readonly Object singletonCreationLock = new Object();
        static volatile SharedData sharedData = null;

        private BBSDefinitions bbsArray = new BBSDefinitions();
        private BBSData currentBBS = new BBSData();

        private TNCDeviceArray tncDeviceArray = new TNCDeviceArray();
        private TNCDevice currentTNCDevice;
		private TNCDevice savedTNCDevice;

		private ProfileArray profileArray = new ProfileArray();
        private Profile currentProfile = new Profile();

        private SharedData()
        {

        }

        public static SharedData SharedDataInstance
        {
            get
            {
                if (sharedData == null)
                {
                    lock (singletonCreationLock)
                    {
                        if (sharedData == null)
                        {
                            sharedData = new SharedData();
                        }
                    }
                }

                return sharedData;
            }
        }

        //public static BBSDefinitions _bbsArray;
        public BBSDefinitions BbsArray { get => bbsArray; set => bbsArray = value; }
        public BBSData CurrentBBS { get => currentBBS; set => currentBBS = value; }

        public TNCDeviceArray TncDeviceArray { get => tncDeviceArray; set => tncDeviceArray = value; }
        public TNCDevice CurrentTNCDevice { get => currentTNCDevice; set => currentTNCDevice = value; }
		public TNCDevice SavedTNCDevice { get => savedTNCDevice; set => savedTNCDevice = value; }

		public ProfileArray ProfileArray { get => profileArray; set => profileArray = value; }
        public Profile CurrentProfile { get => currentProfile; set => currentProfile = value; }

        //public static Dictionary<string, Profile> _profileTypes;

		public static IReadOnlyList<StorageFile> filesInInstalledLocation;

		public static string[] _Areas;
		public static bool _forceReadBulletins;
	}
}
