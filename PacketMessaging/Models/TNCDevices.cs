﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Devices.SerialCommunication;
using Windows.Storage;
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
    public partial class TNCDeviceArray
    {
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<TNCDeviceArray>();

        public static string tncFileName = "TNCData.xml";

		private TNCDevice[] deviceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Device")]
        public TNCDevice[] TNCDevices
        {
            get
            {
                return this.deviceField;
            }
            set
            {
                this.deviceField = value;
            }
        }

		public static async Task<TNCDeviceArray> OpenAsync(StorageFolder localFolder)
		{
			TNCDeviceArray tncDeviceArray = null;

			try
			{
				var storageItem = await localFolder.TryGetItemAsync(tncFileName);
				if (storageItem == null)
				{
					// Copy the file from the install folder to the local folder
					var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
					var storageFile = await folder.GetFileAsync(tncFileName);
					if (storageFile != null)
					{
						await storageFile.CopyAsync(localFolder, tncFileName, NameCollisionOption.FailIfExists);
					}
				}

				StorageFile file = await localFolder.GetFileAsync(tncFileName);

				using (FileStream stream = new FileStream(file.Path, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(TNCDeviceArray));
						tncDeviceArray = (TNCDeviceArray)serializer.Deserialize(reader);
					}
				}
			}
			catch (FileNotFoundException e)
			{
				Debug.WriteLine($"File not found: {e.Message}");
			}
			catch (Exception e)
			{
				Debug.WriteLine($"Failed to read TNC data file {e}");
			}
			if (tncDeviceArray == null)
			{
				//System.Windows.MessageDialog.Show(tncFileName + " missing");
				//log.Error(tncFileName + " missing");
			}

			return tncDeviceArray;
		}

        public async Task SaveAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                var storageItem = await localFolder.TryGetItemAsync(tncFileName);
                if (storageItem != null)
                {
                    using (Stream writer = new FileStream(storageItem.Path, FileMode.Create))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TNCDeviceArray));           
                        serializer.Serialize(writer, this);
                    }
                }
                else
                {
                    log.Error($"Folder not found {localFolder.Path}");
                }
            }
            catch (Exception e)
            {
                log.Error($"Error saving file {tncFileName}, {e}");
            }
        }

        //public static void CreateBackupFile()
        //{
        //    //Create a backup
        //    string backupFilePath = tncFilePath + ".bak";
        //    File.Copy(tncFilePath, backupFilePath, true);
        //    File.Delete(tncFilePath);
        //}
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TNCDevice
    {
        private TNCDevicePrompts promptsField;

        private TNCDeviceCommands commandsField;

        private TNCDeviceInitCommands initCommandsField;

        private TNCDeviceCommPort commPortField;

        private string nameField;

        private bool selectedField;


        /// <remarks/>
        public TNCDevicePrompts Prompts
        {
            get
            {
                return this.promptsField;
            }
            set
            {
                this.promptsField = value;
            }
        }

        /// <remarks/>
        public TNCDeviceCommands Commands
        {
            get
            {
                return this.commandsField;
            }
            set
            {
                this.commandsField = value;
            }
        }

        /// <remarks/>
        public TNCDeviceInitCommands InitCommands
        {
            get
            {
                return this.initCommandsField;
            }
            set
            {
                this.initCommandsField = value;
            }
        }

        /// <remarks/>
        public TNCDeviceCommPort CommPort
        {
            get
            {
                return this.commPortField;
            }
            set
            {
                this.commPortField = value;
            }
        }

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

		public TNCDevice()
		{
			Name = "";
			Prompts = new TNCDevicePrompts()
			{
				Command = "",
				Connected = "",
				Disconnected = "",
				Timeout = ""
			};
			Commands = new TNCDeviceCommands()
			{
				Connect = "",
				Conversmode = "",
				Datetime = "",
				MyCall = "",
				Retry = ""
			};
			InitCommands = new TNCDeviceInitCommands()
			{
				Precommands = "",
				Postcommands = ""
			};
			CommPort = new TNCDeviceCommPort()
			{
				IsBluetooth = false,
				BluetoothName = "",
				DeviceId = "",
				Comport = "",
				Baudrate = 9600,
				Databits = 8,
				Parity = SerialParity.None,
				Stopbits = SerialStopBitCount.One,
				Flowcontrol = SerialHandshake.RequestToSend
			};
		}

		public TNCDevice(TNCDevice tncDevice)
		{
			Prompts = new TNCDevicePrompts()
			{
				Command = tncDevice.Prompts?.Command,
				Connected = tncDevice.Prompts?.Connected,
				Disconnected = tncDevice.Prompts?.Disconnected,
				Timeout = tncDevice.Prompts?.Timeout
			};
			Commands = new TNCDeviceCommands()
			{
				Connect = tncDevice.Commands?.Connect,
				Conversmode = tncDevice.Commands?.Conversmode,
				Datetime = tncDevice.Commands?.Datetime,
				MyCall = tncDevice.Commands?.MyCall,
				Retry = tncDevice.Commands?.Retry
			};
			InitCommands = new TNCDeviceInitCommands()
			{
				Precommands = tncDevice.InitCommands?.Precommands,
				Postcommands = tncDevice.InitCommands?.Postcommands
			};
			CommPort = new TNCDeviceCommPort()
			{
				IsBluetooth = tncDevice.CommPort.IsBluetooth,
				BluetoothName = tncDevice.CommPort?.BluetoothName,
				DeviceId = tncDevice.CommPort?.DeviceId,
				Comport = tncDevice.CommPort?.Comport,
				Baudrate = tncDevice.CommPort.Baudrate,
				Databits = tncDevice.CommPort.Databits,
				Parity = tncDevice.CommPort.Parity,
				Stopbits = tncDevice.CommPort.Stopbits,
				Flowcontrol = tncDevice.CommPort.Flowcontrol
			};
			Name = tncDevice.Name;
		}

		public override string ToString() => Name;
	}

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TNCDevicePrompts
    {

        private string commandField;

        private string timeoutField;

		private string connectedField;

		private string disconnectedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Command
        {
            get
            {
                return this.commandField;
            }
            set
            {
                this.commandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Timeout
        {
            get
            {
                return this.timeoutField;
            }
            set
            {
                this.timeoutField = value;
            }
        }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Connected
		{
			get
			{
				return this.connectedField;
			}
			set
			{
				this.connectedField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
        public string Disconnected
        {
            get
            {
                return this.disconnectedField;
            }
            set
            {
                this.disconnectedField = value;
            }
        }

		public TNCDevicePrompts()
		{
		}
	}

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TNCDeviceCommands
    {

        private string myCallField;

        private string connectField;

        private string retryField;

        private string conversmodeField;

        private string datetimeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MyCall
        {
            get
            {
                return this.myCallField;
            }
            set
            {
                this.myCallField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Connect
        {
            get
            {
                return this.connectField;
            }
            set
            {
                this.connectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Retry
        {
            get
            {
                return this.retryField;
            }
            set
            {
                this.retryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Conversmode
        {
            get
            {
                return this.conversmodeField;
            }
            set
            {
                this.conversmodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Datetime
        {
            get
            {
                return this.datetimeField;
            }
            set
            {
                this.datetimeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TNCDeviceInitCommands
    {

        private string precommandsField;

        private string postcommandsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Precommands
        {
            get
            {
                return this.precommandsField;
            }
            set
            {
                this.precommandsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Postcommands
        {
            get
            {
                return this.postcommandsField;
            }
            set
            {
                this.postcommandsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TNCDeviceCommPort
    {
		private bool isBluetoothField;

		private string bluetoothNameField;

		private string deviceIdField;

		private string comportField;

        private uint baudrateField;

        private ushort databitsField;

        private SerialParity parityField;

        private SerialStopBitCount stopbitsField;

        private SerialHandshake flowcontrolField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool IsBluetooth
		{
			get
			{
				return this.isBluetoothField;
			}
			set
			{
				this.isBluetoothField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string BluetoothName
		{
			get
			{
				return this.bluetoothNameField;
			}
			set
			{
				this.bluetoothNameField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string DeviceId
		{
			get
			{
				return this.deviceIdField;
			}
			set
			{
				this.deviceIdField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
        public string Comport
        {
            get
            {
                return this.comportField;
            }
            set
            {
                this.comportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint Baudrate
        {
            get
            {
                return this.baudrateField;
            }
            set
            {
                this.baudrateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Databits
        {
            get
            {
                return this.databitsField;
            }
            set
            {
                this.databitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SerialParity Parity
        {
            get
            {
                return this.parityField;
            }
            set
            {
                this.parityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SerialStopBitCount Stopbits
        {
            get
            {
                return this.stopbitsField;
            }
            set
            {
                this.stopbitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SerialHandshake Flowcontrol
        {
            get
            {
                return this.flowcontrolField;
            }
            set
            {
                this.flowcontrolField = value;
            }
        }
    }
}
