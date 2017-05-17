
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PacketMessaging;
using Windows.Storage;

namespace UnitTestPacketMessaging
{
    [TestClass]
    public class UnitTestProfiles
    {
		public ProfileArray Await { get; private set; }

		[TestMethod]
        public void TestMethodProfiles()
        {
        }

		[TestMethod]
		public async System.Threading.Tasks.Task ProfilesTestOpenAsync()
		{
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			ProfileArray profileArray = await ProfileArray.OpenAsync(localFolder);

			Assert.IsNotNull(profileArray);
		}

	}
}
