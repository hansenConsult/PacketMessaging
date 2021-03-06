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
using System.IO;
using System.Xml.Serialization;
using FormControlBaseClass;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MetroLog;

namespace PacketMessaging.Models
{

    // 
    // This source code was auto-generated by xsd, Version=4.0.30319.33440.
    // 


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class BBSDefinitions : ICollectionViewFactory
	{
		private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<BBSDefinitions>();

		public static string bbsFileName = "BBSData.xml";

		private DateTime? revisionTimeField;

        private BBSData[] bBSField;

		public DateTime? RevisionTime
		{
			get { return revisionTimeField; }
			set { revisionTimeField = value; }
		}

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BBS")]
        public BBSData[] BBSDataArray
        {
            get
            {
                return this.bBSField;
            }
            set
            {
                this.bBSField = value;
            }
        }

		public static async Task<BBSDefinitions> OpenAsync(StorageFolder localFolder1)
		{
			BBSDefinitions bbsDataArray = null;

			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			try
			{
				var storageItem = await localFolder.TryGetItemAsync(bbsFileName);
				if (storageItem == null)
				{
					// Copy the file from the install folder to the local folder
					var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
					var storageFile = await folder.GetFileAsync(bbsFileName);
					if (storageFile != null)
					{
						await storageFile.CopyAsync(localFolder, bbsFileName, Windows.Storage.NameCollisionOption.FailIfExists);
					}
				}

				StorageFile file = await localFolder.GetFileAsync(bbsFileName);

				using (FileStream reader = new FileStream(file.Path, FileMode.Open))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(BBSDefinitions));
					bbsDataArray = (BBSDefinitions)serializer.Deserialize(reader);
				}
			}
			catch (FileNotFoundException e)
			{
				Debug.WriteLine($"Open BBSData file failed: {e.Message}");
			}
			catch (Exception e)
			{
				Debug.WriteLine($"Error opening {e.Message} {e}");
			}
			return bbsDataArray;
		}

		public void Save(string filePath)
		{
			string bbsFilePath = filePath + bbsFileName;

			try
			{
				using (StreamWriter writer = new StreamWriter(new FileStream(bbsFilePath, FileMode.OpenOrCreate)))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(BBSDefinitions));
					serializer.Serialize(writer, this);
				}
			}
			catch (Exception e)
			{
				log.Error($"Error saving {filePath} {e}");
			}
		}

		public static BBSDefinitions CreateFromBulletin(ref PacketMessage bbsBulletin)
		{
			string bulletin = bbsBulletin.MessageBody;

			if (bulletin == null)
				return null;

			int start = 0;
			int start2 = 0;
			bulletin = bulletin.Substring(start);

			start = bulletin.IndexOf("---------");
			start2 += start;
			string bbsInfo = bulletin.Substring(start);
			start = bbsInfo.IndexOf("\n");
			start2 += start;
			bbsInfo = bbsInfo.Substring(start + 1);
			int end = bbsInfo.IndexOf('*');
			bbsInfo = bbsInfo.Substring(0, end);

			BBSData bbsData;
			var lines = bbsInfo.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			int bbsCount = lines.Length;
			BBSData[] bbsdataArray = new BBSData[lines.Length + 1];
			int i = 0;
			for (; i < lines.Length; i++)
			{
				var callsign = lines[i].Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

				bbsData = new BBSData();
				bbsData.Name = callsign[0];
				bbsData.ConnectName = callsign[1];
				bbsData.Frequency1 = callsign[2];
				bbsData.Frequency2 = callsign[3];
				bbsData.Selected = false;

				bbsdataArray[i] = bbsData;
			}
			// Location
			bbsInfo = bulletin.Substring(start2 + end);
			start = bbsInfo.IndexOf("---------");
			bbsInfo = bbsInfo.Substring(start);
			start = bbsInfo.IndexOf("\n");
			bbsInfo = bbsInfo.Substring(start + 1);
			string description = "Santa Clara County ARES/RACES PacketSystem. ";
			lines = bbsInfo.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			for (i = 0; i < bbsCount; i++)
			{
				var callsign = lines[i].Split(new string[] { ",", "      ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
				bbsdataArray[i].Description = description + callsign[1];
			}
			bbsdataArray[0].Selected = true;

			// Add training BBS
			bbsData = new BBSData();
			bbsData.Name = "W5XSC";
			bbsData.ConnectName = "W5XSC-1";
			bbsdataArray[i] = bbsData;

			BBSDefinitions bbsDefinitions = new BBSDefinitions();
			bbsDefinitions.RevisionTime = bbsBulletin.JNOSDate;
			bbsDefinitions.BBSDataArray = bbsdataArray;

			return bbsDefinitions;
		}

		//ICollectionView CreateView()
		//{
		//	throw new NotImplementedException();
		//	//return new MyListCollectionView(this);
		//	//BBSDataArray.CreateView();
		//	//return (BBSData[]) CreateView();


		//}

		ICollectionView ICollectionViewFactory.CreateView()
		{
			throw new NotImplementedException();
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class BBSData
    {

        private string nameField;

        private string connectNameField;

        private string descriptionField;

        private string frequency1Field;

        private string frequency2Field;

        private string secondaryField;

        private bool selectedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
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
        public string ConnectName
        {
            get
            {
                return this.connectNameField;
            }
            set
            {
                this.connectNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Frequency1
        {
            get
            {
                return this.frequency1Field;
            }
            set
            {
                this.frequency1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Frequency2
        {
            get
            {
                return this.frequency2Field;
            }
            set
            {
                this.frequency2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Secondary
        {
            get
            {
                return this.secondaryField;
            }
            set
            {
                this.secondaryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Selected
        {
            get
            {
                return this.selectedField;
            }
            set
            {
                this.selectedField = value;
            }
        }

		//public override string ToString() => Name;

	}
}