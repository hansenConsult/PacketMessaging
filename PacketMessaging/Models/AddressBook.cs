﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using MetroLog;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using PacketMessaging.Models;

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 

namespace PacketMessaging.Models
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
	//[System.SerializableAttribute()]
	//[System.Diagnostics.DebuggerStepThroughAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(Namespace = "", IsNullable = false)]
    public sealed class AddressBook
	{
        private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<AddressBook>();

        private Dictionary<string, AddressBookEntry> _addressDictionary;
        private const string addressBookFileName = "addressBook.xml";

        private AddressBookEntry[] _addressEntriesField;
        private AddressBookEntry[] _userAddressEntriesField;

        private static volatile AddressBook _instance;
        private static object _syncRoot = new Object();

        private AddressBook() { }

        /// <remarks/>
        [XmlElement("AddressEntry")]
		public AddressBookEntry[] AddressEntries
		{
			get => _addressEntriesField;
			set => _addressEntriesField = value;
		}

        /// <remarks/>
        [XmlElement("AddressEntry")]
        public AddressBookEntry[] UserAddressEntries
        {
            get => _userAddressEntriesField;
            set => _userAddressEntriesField = value;
        }

		public string UserBBS
		{ get; set; }

        public static AddressBook Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new AddressBook();
                    }
                }
                return _instance;
            }
        }

        public Dictionary<string, AddressBookEntry> AddressBookDictionary
        {
            get => _addressDictionary;
        }

        public async Task OpenAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                var storageItem = await localFolder.TryGetItemAsync(addressBookFileName);
                if (storageItem == null)
                    return;

                StorageFile file = await localFolder.GetFileAsync(addressBookFileName);
                using (FileStream reader = new FileStream(file.Path, FileMode.Open))
                {
                    if (UserAddressEntries == null)
                    {
                        UserAddressEntries = new AddressBookEntry[0];
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(AddressBookEntry[]));
                    UserAddressEntries = (AddressBookEntry[])serializer.Deserialize(reader);
                }
            }
            catch (FileNotFoundException e)
            {
                log.Error($"Open Address Book file failed: {e.Message}");
            }
            catch (Exception e)
            {
                log.Error($"Error opening {e.Message} {e}");
            }
        }


        public async void SaveAsync()
        {
            if (UserAddressEntries == null || UserAddressEntries.Length == 0)
                return;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile file = await localFolder.CreateFileAsync(addressBookFileName, CreationCollisionOption.ReplaceExisting);
                using (StreamWriter writer = new StreamWriter(new FileStream(file.Path, FileMode.OpenOrCreate)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AddressBookEntry[]));
                    serializer.Serialize(writer, UserAddressEntries);
                }
            }
            catch (Exception e)
            {
                log.Error($"Error saving {addressBookFileName} {e}");
            }
        }

        public void CreateAddressBook()
        {
            _addressDictionary = new Dictionary<string, AddressBookEntry>();
            foreach (var tacticalCallsignData in App._tacticalCallsignDataDictionary.Values)
            {
				if (tacticalCallsignData.TacticalCallsigns == null)
					continue;

                foreach (TacticalCall tacticalCall in tacticalCallsignData.TacticalCallsigns.TacticalCallsignsArray)
                {
                    AddressBookEntry entry = new AddressBookEntry()
                    {
                        Callsign = tacticalCall.TacticalCallsign.ToUpper(),
                        NameDetail = tacticalCall.AgencyName,
                        BBSPrimary = tacticalCall.PrimaryBBS,
                        BBSSecondary = tacticalCall.SecondaryBBS,
                        BBSPrimaryActive = true // tacticalCall.PrimaryBBSActive
                    };
                    if (tacticalCall.SecondaryBBS.Length == 0 && tacticalCall.SecondaryBBSActive)
                    {
                        tacticalCall.PrimaryBBSActive = true;
                        //tacticalCall.SecondaryBBSActive = false;
                    }

                    string activeBBS = tacticalCall.PrimaryBBSActive ? tacticalCall.PrimaryBBS : tacticalCall.SecondaryBBS;
                    _addressDictionary.TryGetValue(entry.Callsign, out AddressBookEntry newEntry);
                    if (newEntry == null)
                    {
                        _addressDictionary.Add(entry.Callsign, entry);
                    }
                }
            }
            // Add user enries to the dictionary
            if (UserAddressEntries != null)
            {
                foreach (var entry in UserAddressEntries)
                {
                    _addressDictionary.TryGetValue(entry.Callsign, out AddressBookEntry newEntry);
                    if (newEntry == null)
                    {
                        _addressDictionary.Add(entry.Callsign, entry);
                    }
                }
            }
        }

        //public static void UpdateEntry(TacticalCall tacticalCall)
        //{
        //    AddressBookEntry entry = _addresBook[tacticalCall.TacticalCallsign];
        //    string activeBBS = tacticalCall.PrimaryBBSActive ? tacticalCall.PrimaryBBS : tacticalCall.SecondaryBBS;
        //    //entry.Address = entry.Callsign + '@' + activeBBS + ".ampr.org";
        //}

        public string GetBBS(string callsign)
        {
            _addressDictionary.TryGetValue(callsign.ToUpper(), out AddressBookEntry entry);

            return (entry == null ? "" : entry.BBSPrimaryActive ? entry.BBSPrimary : entry.BBSSecondary);
        }

		public string GetAddress(string callsign)
        {
            _addressDictionary.TryGetValue(callsign.ToUpper(), out AddressBookEntry entry);
            if (entry != null)
            {
                string bbs = entry.BBSPrimaryActive ? entry.BBSPrimary : entry.BBSSecondary;
				if (!string.IsNullOrEmpty(bbs))
				{
					if (!string.IsNullOrEmpty(UserBBS))
					{
						if (UserBBS == bbs)
						{
							return entry.Callsign;
						}
						else
						{
							return entry.Callsign + '@' + bbs;
						}
					}
					else
					{
						return entry.Callsign + '@' + bbs + "ampr.org";
					}
				}
				else
				{
					return entry.Callsign;
				}
			}
            return null;
        }

		public List<string> GetCallsigns(string partialCallsign)
		{
			List<string> matches = new List<string>();

			foreach (var key in _addressDictionary.Keys)
			{
				if (key.StartsWith(partialCallsign.ToUpper()))
				{
					matches.Add(key);
				}
			}
			return matches;
		}

		public List<string> GetAddressItems(string partialAddress)
        {
            List<string> matches = new List<string>();

            foreach (var key in _addressDictionary.Keys)
            {
                if (key.StartsWith(partialAddress.ToUpper()))
                {
                    matches.Add(_addressDictionary[key].Address);
                }
            }
            return matches;
        }

		public List<string> GetAddressNames(string partialAddress)
		{
			List<string> matches = new List<string>();

			foreach (var key in _addressDictionary.Keys)
			{
				if (key.StartsWith(partialAddress.ToUpper()))
				{
					matches.Add(_addressDictionary[key].Callsign);
				}
			}
			return matches;
		}

		private bool ValidateBBS(string bbs)
        {
            string bbsStringIndex = bbs.Substring(1, 1);
            int BBSIndex = Convert.ToInt32(bbsStringIndex);
            if (bbs.StartsWith("w") && bbs.EndsWith("xsc") && BBSIndex > 0 && BBSIndex < 6)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateAddressBookEntry(string callsign, bool usePrimaryBBS)
        {
            _addressDictionary.TryGetValue(callsign, out AddressBookEntry oldAddressBookEntry);
            oldAddressBookEntry.BBSPrimaryActive = usePrimaryBBS;
            SaveAsync();
        }

        public void UpdateAddressBookEntry(AddressBookEntry addressBookEntry)
        {
            _addressDictionary.TryGetValue(addressBookEntry.Callsign, out AddressBookEntry oldAddressBookEntry);
            _addressDictionary.Remove(oldAddressBookEntry.Callsign);
            _addressDictionary.Add(addressBookEntry.Callsign, addressBookEntry);
        }

        public bool AddAddressAsync(AddressBookEntry addressBookEntry)
        {
            // Validate entries
            int index = addressBookEntry.Callsign.IndexOf('@');
            if (index < 0)
                return false;

            _addressDictionary.TryGetValue(addressBookEntry.Callsign, out AddressBookEntry oldAddressBookEntry);
            if (oldAddressBookEntry == null)
            {
                string temp = addressBookEntry.Callsign.Substring(index + 1);
                temp = temp.ToLower();
                index = temp.IndexOf('.');
                if (index < 0)
                    return false;

                //temp = temp.Substring(0, index);
                //bool result = ValidateBBS(temp);        // BBS in address
                //result &= ValidateBBS(addressBookEntry.BBSPrimary.ToLower());       // Primary BBS
                ////result &= ValidateBBS(addressBookEntry.BBSSecondary.ToLower());     // Secondary BBS can be undefined
                //if (!result)
                //    return false;

                _addressDictionary.Add(addressBookEntry.Callsign, addressBookEntry);
                // Insert in userAddressBook
                if (UserAddressEntries == null)
                {
                    UserAddressEntries = new AddressBookEntry[0];
                }
                var addressBookEntryList = UserAddressEntries.ToList();
                addressBookEntryList.Add(addressBookEntry);
                UserAddressEntries = addressBookEntryList.ToArray();
                SaveAsync();
				return true;
			}
			else
			{
				return false;
			}
		}

        public void AddAddressAsync(string address, string bbsPrimary = "", string bbsSecondary = "", bool primaryActive = true)
        {
            // extract callsign
            int index = address.IndexOf('@');
			if (index < 0)
				return;

			string callsign = address.Substring(0, index);
            _addressDictionary.TryGetValue(callsign, out AddressBookEntry addressBookEntry);
            if (addressBookEntry == null)
            {
                string temp = address.Substring(index + 1);
                temp = temp.ToLower();
                index = temp.IndexOf('.');
				if (index < 0)
					return;

				temp = temp.Substring(0, index);
                if (temp.StartsWith("w") && temp.EndsWith("xsc"))
                {
                    bbsPrimary = temp;
                }
                AddressBookEntry entry = new AddressBookEntry()
                {
                    Callsign = callsign,
                    NameDetail = address,
                    BBSPrimary = bbsPrimary,
                    BBSSecondary = bbsPrimary,
                    BBSPrimaryActive = primaryActive
                };
                _addressDictionary.Add(callsign, entry);
                // Insert in userAddressBook
                if (UserAddressEntries == null)
                {
                    UserAddressEntries = new AddressBookEntry[0];
                }
                var addressBookEntryList = UserAddressEntries.ToList();
                addressBookEntryList.Add(entry);
                UserAddressEntries = addressBookEntryList.ToArray();
                SaveAsync();
            }
        }

        public void DeleteAddress(AddressBookEntry addressBookEntry)
        {
            if (UserAddressEntries != null)
            {
                var addressBookEntryList = UserAddressEntries.ToList();
                addressBookEntryList?.Remove(addressBookEntry);
                UserAddressEntries = addressBookEntryList.ToArray();
                SaveAsync();
            }
            _addressDictionary.Remove(addressBookEntry.Callsign);
        }

        public void UpdateForBBSStatusChange(string bbs, bool bbsStatusUp)
        {
            List<AddressBookEntry> changedEntries = new List<AddressBookEntry>();
            foreach (AddressBookEntry entry in _addressDictionary.Values)
            {
                if (bbsStatusUp)
                {
                    if (!entry.BBSPrimaryActive && entry.BBSPrimary == bbs)
                    {
                        entry.BBSPrimaryActive = true;
                        changedEntries.Add(entry);
                    }
                    //else if (entry.BBSPrimaryActive && entry.BBSSecondary == bbs)
                    //{
                    //    entry.BBSPrimaryActive = false;
                    //}
                }
                else
                {
                    if (entry.BBSPrimaryActive && entry.BBSPrimary == bbs)
                    {
                        entry.BBSPrimaryActive = false;
                        changedEntries.Add(entry);
                    }
                }
            }
            foreach (AddressBookEntry entry in changedEntries)
            {
                _addressDictionary[entry.Callsign] = entry;
            }
        }

        public void UsePrimaryBBSForAll()
        {
            List<AddressBookEntry> changedEntries = new List<AddressBookEntry>();
            foreach (AddressBookEntry entry in _addressDictionary.Values)
            {
                entry.BBSPrimaryActive = true;
                changedEntries.Add(entry);
            }
            foreach (AddressBookEntry entry in changedEntries)
            {
                _addressDictionary[entry.Callsign] = entry;
            }
            //CreateAddressBook();
        }

        public ObservableCollection<AddressBookEntry> GetContacts()
        {
            ObservableCollection<AddressBookEntry> contacts = new ObservableCollection<AddressBookEntry>();

            foreach (AddressBookEntry entry in _addressDictionary.Values)
            {
                contacts.Add(entry);
            }
            return contacts;
        }

		public ObservableCollection<string> GetContactNames()
		{
			ObservableCollection<string> contactNames = new ObservableCollection<string>();

			foreach (AddressBookEntry entry in _addressDictionary.Values)
			{
				contactNames.Add(entry.Callsign);
			}
			return contactNames;
		}

		public ObservableCollection<GroupInfoList> GetContactsGrouped()
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();

            var query = from item in GetContacts()
                        group item by item.Callsign.Substring(0, 1) into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                GroupInfoList info = new GroupInfoList();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                info.Sort();
                groups.Add(info);
            }

            return groups;
        }

    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[XmlType(AnonymousType = true)]
    public partial class AddressBookEntry : IComparable
    {
        private string callsignField;

        private string nameField;

        private string bBSPrimaryField;

        private string bBSSecondaryField;

        private bool bbsPrimaryActiveField = true;

        /// <remarks/>
        [XmlAttribute()]
        public string Callsign
        {
            get
            {
                return this.callsignField;
            }
            set
            {
                this.callsignField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string NameDetail
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }


        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BBSPrimary
        {
            get
            {
                return this.bBSPrimaryField;
            }
            set
            {
                this.bBSPrimaryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BBSSecondary
        {
            get
            {
                return this.bBSSecondaryField;
            }
            set
            {
                this.bBSSecondaryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool BBSPrimaryActive
        {
            get
            {
                return this.bbsPrimaryActiveField;
            }
            set
            {
                this.bbsPrimaryActiveField = value;

                //AddressBook._addresBook.TryGetValue(Callsign, out AddressBookEntry entry);
                //if (entry != null)
                //{
                //    AddressBook._addresBook[Callsign].bbsPrimaryActiveField = value;
                //    //ContactsCVS.Source = AddressBook.GetContactsGrouped();
                //}
            }
        }

        public string Address
        {
            get
            {
                string retval;
                if (string.IsNullOrEmpty(BBSPrimary))
                {
                    retval = Callsign;
                }
                else
                {
					retval = Callsign + '@' + (BBSPrimaryActive ? BBSPrimary : BBSSecondary);// + ".ampr.org";
                }
                return retval;
            }
        }

        public override string ToString()
        {
            return Callsign;
        }

        int IComparable.CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToString());
        }
    }
}
