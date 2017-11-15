using FormControlBaseClass;
using MessageFormControl;
using MetroLog;
using PacketMessaging.Views;
using PacketMessaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Email;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Popups;

namespace PacketMessaging.Services.CommunicationsService
{
	public class CommunicationsService : CommunicationsServiceBase
	{
		protected static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<CommunicationsService>();

		//Collection<DeviceListEntry> _listOfDevices;

		public List<PacketMessage> _packetMessagesReceived = new List<PacketMessage>();
		List<PacketMessage> _packetMessagesToSend = new List<PacketMessage>();

        private static readonly Object singletonCreationLock = new Object();
        static volatile CommunicationsService _communicationsService = null;
		static bool _deviceFound = false;

        private CommunicationsService(ILogger log) : base(log)
        {
        }

        public static CommunicationsService CreateInstance()
		{
            if (_communicationsService == null)
            {
                lock (singletonCreationLock)
                {
                    if (_communicationsService == null)
                    {
                        _communicationsService = new CommunicationsService(log);
                    }
                }
            }
			return _communicationsService;
		}

        public async void ProcessReceivedMessagesAsync()
		{
			if (_packetMessagesReceived.Count() > 0)
			{
				foreach (PacketMessage packetMessageOutpost in _packetMessagesReceived)
				{
					FormControlBase formControl = new MessageControl();
                    // test for packet form!!
                    PacketMessage pktMsg = new PacketMessage()
                    {
                        //pktMsg.PacFormType = PacForms.Message;
                        PacFormName = "SimpleMessage",
                        BBSName = packetMessageOutpost.BBSName,
                        TNCName = packetMessageOutpost.TNCName,
                        MessageSize = packetMessageOutpost.MessageSize,
                        Area = packetMessageOutpost.Area,
                        // Save the original message for post processing (tab characters are lost in the displayed message)
                        MessageBody = packetMessageOutpost.MessageBody,
                        //MessageReadOnly = true
                    };
                    string[] msgLines = packetMessageOutpost.MessageBody.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.None);

					bool subjectFound = false;
					for (int i = 0; i < Math.Min(msgLines.Length, 20); i++)
					{
						if (msgLines[i].StartsWith("Date:"))
							pktMsg.JNOSDate = DateTime.Parse(msgLines[i].Substring(10, 21));
						else if (msgLines[i].StartsWith("From:"))
							pktMsg.MessageFrom = msgLines[i].Substring(6);
						else if (msgLines[i].StartsWith("To:"))
							pktMsg.MessageTo = msgLines[i].Substring(4);
						else if (msgLines[i].StartsWith("Cc:"))
						{
							pktMsg.MessageTo += (", " + msgLines[i].Substring(4));
							while (msgLines[i + 1].Length == 0)
							{
								i++;
							}
							if (msgLines[i + 1][0] == ' ')
							{
								pktMsg.MessageTo += msgLines[i + 1].TrimStart(new char[] { ' ' });
							}
						}
						else if (!subjectFound && msgLines[i].StartsWith("Subject:"))
						{
							pktMsg.Subject = msgLines[i].Substring(9);
							//pktMsg.MessageSubject = pktMsg.MessageSubject.Replace('\t', ' ');
							subjectFound = true;
						}
						else if (msgLines[i].StartsWith("# FORMFILENAME:"))
						{
							string html = ".html";
							string formName = msgLines[i].Substring(16).TrimEnd(new char[] { ' ' });
							formName = formName.Substring(0, formName.Length - html.Length);
							pktMsg.PacFormName = formName;

							formControl = Views.FormsPage.CreateFormControlInstanceFromFileName(pktMsg.PacFormName);
							if (formControl == null)
							{
								await Utilities.ShowMessageDialogAsync($"Form {pktMsg.PacFormName} not found");
								return;
							}
                            break;
						}
					}

                    //pktMsg.MessageNumber = GetMessageNumberPacket();		// Filled in BBS connection
                    pktMsg.PacFormType = formControl.PacFormType;    
                    pktMsg.MessageNumber = packetMessageOutpost.MessageNumber;
					pktMsg.FormFieldArray = formControl.ConvertFromOutpost(pktMsg.MessageNumber, ref msgLines);
					pktMsg.ReceivedTime = packetMessageOutpost.ReceivedTime;
					pktMsg.CreateFileName();
					string fileFolder = Views.MainPage.receivedMessagesFolder.Path;
					pktMsg.Save(fileFolder);

					//log.Info($"Message number {pktMsg.MessageNumber} received");
                    LogHelper(LogLevel.Info, $"Message number {pktMsg.MessageNumber} received");

                    // If the received message is a delivery confirmation, update receivers message number in the original sent message
                    if (!string.IsNullOrEmpty(pktMsg.Subject) && pktMsg.Subject.Contains("DELIVERED:"))
					{
						var formField = pktMsg.FormFieldArray.FirstOrDefault(x => x.ControlName == "messageBody");
						if (formField.ControlContent.Contains("!LMI!"))
						{
							string[] searchStrings = new string[] { "Subject: ", "was delivered on ", "Recipient's Local Message ID: " };
							DateTime receiveTime = DateTime.Now;
							string receiversMessageId = "", sendersMessageId = "";
							var messageLines = formField.ControlContent.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
							foreach (string line in messageLines)
							{
								if (line.Contains(searchStrings[0]))
								{
									int indexOfUnderscore = line.IndexOf('_');
									int indexOfDelivered = line.IndexOf(searchStrings[0]);
									if (indexOfUnderscore >= 0)
									{
										sendersMessageId = line.Substring(indexOfDelivered + searchStrings[0].Length, indexOfUnderscore - (indexOfDelivered + searchStrings[0].Length));
									}
								}
								else if (line.Contains(searchStrings[1]))
								{
									int indexOfDeliveryTime = line.IndexOf(searchStrings[1]);
									if (indexOfDeliveryTime >= 0)
									{
										string s = line.Substring(indexOfDeliveryTime + searchStrings[1].Length);
										receiveTime = DateTime.Parse(s);
									}
								}
								else if (line.Contains(searchStrings[2]))
								{
									receiversMessageId = line.Substring(line.IndexOf(searchStrings[2]) + searchStrings[2].Length);
								}
							}

                            //DirectoryInfo diSentFolder = new DirectoryInfo(Views.MainPage._sentMessagesFolder.Path);
                            //foreach (FileInfo fi in diSentFolder.GetFiles())
                            List<string> fileTypeFilter = new List<string>() { ".xml" };
                            //fileTypeFilter.Add(".xml");
                            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);

                            // Get the files in the user's archive folder
                            StorageFileQueryResult results = MainPage._sentMessagesFolder.CreateFileQueryWithOptions(queryOptions);
                            // Iterate over the results
                            IReadOnlyList<StorageFile> files = await results.GetFilesAsync();
                            foreach (StorageFile file in files)
                            {
                                // Update sent form with receivers message number and receive time
                                if (file.Name.Contains(sendersMessageId))
								{
									PacketMessage packetMessage = PacketMessage.Open(file);
									if (packetMessage.MessageNumber == sendersMessageId)
									{
										formField = packetMessage.FormFieldArray.FirstOrDefault(x => x.ControlName == "receiverMsgNo");
										if (formField != null)
										{
											formField.ControlContent = receiversMessageId;
										}
										packetMessage.ReceivedTime = receiveTime;
										packetMessage.Save(MainPage._sentMessagesFolder.Path);
										break;
									}
								}
							}
						}
					}
				}
				//RefreshDataGrid();      // Display newly added messages
			}
		}

		public async void BBSConnectAsync()
		{
			FormControlBase formControl;

            //_listOfDevices = listOfDevices;

            ViewModels.SharedData sharedData =  ViewModels.SharedData.SharedDataInstance;

            TNCDevice tncDevice = sharedData.CurrentTNCDevice;
            if (tncDevice != null)
			{
				BBSData bbs = sharedData.CurrentBBS;
				if (bbs != null)
				{
					// Do we use a bluetooth device?
					if ((bool)sharedData.CurrentTNCDevice.CommPort?.IsBluetooth)
					{
						RfcommDeviceService service = null;
						try
						{
							DeviceInformationCollection DeviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
							foreach (var deviceInfo in DeviceInfoCollection)
							{
								if (deviceInfo.Id.Contains(sharedData.CurrentTNCDevice.CommPort.DeviceId))
								{
									service = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);
									_deviceFound = true;
									break;
								}
							}
						}
						catch (Exception exp)
						{
							LogHelper(LogLevel.Error, $"Error finding bluetooth device. { exp.Message }");
						}
						if (!_deviceFound)
						{
							Utilities.ShowMessageDialogAsync($"TNC not found. Sending the message via e-mail");
						}
						else
						{
							if (_socket != null)
							{
								// Disposing the socket with close it and release all resources associated with the socket
								_socket.Dispose();
							}

							_socket = new StreamSocket();
							bool success = true;
							try
							{
								// Note: If either parameter is null or empty, the call will throw an exception
								await _socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);
							}
							catch (Exception ex)
							{
								success = false;
								LogHelper(LogLevel.Error, "Bluetootn Connect failed:" + ex.Message);
							}
						// If the connection was successful, the RemoteAddress field will be populated
						try
						{
							if (success)
							{
								//System.Diagnostics.Debug.WriteLine(msg);
								//await md.ShowAsync();
							}
						}
						catch (Exception ex)
						{
							LogHelper(LogLevel.Error, $"Overall Connect: { ex.Message}");
							_socket.Dispose();
							_socket = null;
						}
					}
				}
				    // Collect messages to be sent
				    _packetMessagesToSend.Clear();
                    List<string> fileTypeFilter = new List<string>() { ".xml" };
                    QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);

                    // Get the files in the user's archive folder
                    StorageFileQueryResult results = MainPage._unsentMessagesFolder.CreateFileQueryWithOptions(queryOptions);
                    // Iterate over the results
                    IReadOnlyList<StorageFile> files = await results.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        // Add Outpost message format by Filling the MessageBody field in packetMessage. 
                        PacketMessage packetMessage = PacketMessage.Open(file);
                        if (packetMessage == null)
                        {
                            continue;
                        }

						DateTime now = DateTime.Now;

						var operatorDateField = packetMessage.FormFieldArray.Where(formField => formField.ControlName == "operatorDate").FirstOrDefault();
						if (operatorDateField != null)
							operatorDateField.ControlContent = $"{now.Month:d2}/{now.Day:d2}/{now.Year:d2}";

						var operatorTimeField = packetMessage.FormFieldArray.Where(formField => formField.ControlName == "operatorTime").FirstOrDefault();
						if (operatorTimeField != null)
							operatorTimeField.ControlContent = $"{now.Hour:d2}{now.Minute:d2}";
                       
						formControl = Views.FormsPage.CreateFormControlInstance(packetMessage.PacFormType);
						if (formControl == null)
						{
							MessageDialog messageDialog = new MessageDialog($"Form {packetMessage.PacFormName} not found");
							await messageDialog.ShowAsync();
							continue;
						}

						packetMessage.MessageBody = formControl.CreateOutpostData(ref packetMessage);
						packetMessage.MessageSize = packetMessage.Size;
						// Save updated message
						packetMessage.Save(MainPage._unsentMessagesFolder.Path);

						_packetMessagesToSend.Add(packetMessage);
					}
                    if (_packetMessagesToSend.Count == 0)
                    {
                        LogHelper(LogLevel.Info, $"No messages to send.");
                        //return;
                    }

					TNCInterface tncInterface = new TNCInterface(bbs.ConnectName, ref tncDevice, ViewModels.SharedData._forceReadBulletins, ViewModels.SharedData._Areas, ref _packetMessagesToSend); 

					// Send as email if a TNC is not reachable.
					if (!_deviceFound)
					{
						EmailMessage emailMessage;
						try
						{
							foreach (PacketMessage packetMessage in _packetMessagesToSend)
							{
								// Mark message as sent by email
								packetMessage.TNCName = "e-mail";
								emailMessage = new EmailMessage();
								// Create the to field.
								var messageTo = packetMessage.MessageTo.Split(new char[] { ' ', ';' });
								foreach (string address in messageTo)
								{
									var index = address.IndexOf('@');
									if (index > 0)
									{
										index = address.ToLower().IndexOf("ampr.org");
										if (index < 0)
										{
											emailMessage.To.Add(new EmailRecipient(address + ".ampr.org"));
										}
										else
										{
											emailMessage.To.Add(new EmailRecipient(address));
										}
									}
									else
									{
										string to = $"{packetMessage.MessageTo}@{packetMessage.BBSName}.ampr.org";
										emailMessage.To.Add(new EmailRecipient(to));
									}
								}

								emailMessage.Subject = packetMessage.Subject;
								//// Insert \r\n 
								//string temp = packetMessage.MessageBody;
								//string messageBody = temp.Replace("\r", "\r\n");
								//var msgBody = temp.Split(new char[] { '\r' });
								//messageBody = "";
								//foreach (string s in msgBody)
								//{
								//	messageBody += (s + "\r\n");
								//}
								//emailMessage.Body = messageBody;
								emailMessage.Body = packetMessage.MessageBody;

								//IBuffer 
								//var memStream = new InMemoryRandomAccessStream();
								//var randomAccessStreamReference = RandomAccessStreamReference.CreateFromStream(memStream);
								//emailMessage.SetBodyStream(EmailMessageBodyKind.PlainText, randomAccessStreamReference);
								//emailMessage.Body = await memStream.WriteAsync(packetMessage.MessageBody);

								await EmailManager.ShowComposeNewEmailAsync(emailMessage);

								packetMessage.SentTime = DateTime.Now;
								tncInterface.PacketMessagesSent.Add(packetMessage);

								LogHelper(LogLevel.Info, $"Message sent via email {packetMessage.MessageNumber}");
							}
						}
						catch (Exception ex)
						{
							string text = ex.Message;
						}
					}
					else
					{
						//ref List<PacketMessage> packetMessagesReceived, ref List<PacketMessage> packetMessagesToSend, out DateTime bbsConnectTime, out DateTime bbsDisconnectTime
						//string messageBBS, ref TNCDevice tncDevice, bool forceReadBulletins
						LogHelper(LogLevel.Info, $"Connect to: {bbs.ConnectName}");

						//tncInterface.BBSConnect(ref packetMessagesReceived, ref packetMessagesToSend);
						//ConnectedDialog connectDlg = new ConnectedDialog();
						//connectDlg.Owner = Window.GetWindow(this);
						//tncInterface.ConnectDlg = connectDlg;

						await tncInterface.BBSConnectThreadProcAsync();

						//var thread = new Thread(tncInterface.BBSConnectThreadProc);
						//thread.SetApartmentState(ApartmentState.STA);
						//connectDlg.ConnectThread = thread;
						//connectDlg.ConnectError = tncInterface;
						//thread.Start();

						//// Blocks UI while connected to BBS
						//connectDlg.ShowDialog();

						ViewModels.SharedData._forceReadBulletins = false;
						LogHelper(LogLevel.Info, $"Disconnected from: {bbs.ConnectName}. Connect time = {tncInterface.BBSDisconnectTime - tncInterface.BBSConnectTime}");
					}
					// Move sent messages from unsent folder to the Sent folder
					foreach (PacketMessage packetMsg in tncInterface.PacketMessagesSent)
					{
                        LogHelper(LogLevel.Info, $"Message number {packetMsg.MessageNumber} Sent");

						var file = await MainPage._unsentMessagesFolder.CreateFileAsync(packetMsg.FileName, CreationCollisionOption.OpenIfExists);
						await file.DeleteAsync();

						// Do a save to ensure that updates from tncInterface.BBSConnect are saved
						packetMsg.Save(Views.MainPage._sentMessagesFolder.Path);
					}
					_packetMessagesReceived = tncInterface.PacketMessagesReceived;
					ProcessReceivedMessagesAsync();
				}
				else
				{
					MessageDialog messageDialog = new MessageDialog($"Could not find the requested TNC ({ViewModels.SharedData.SharedDataInstance.CurrentProfile.TNC})");
					await messageDialog.ShowAsync();
                    LogHelper(LogLevel.Error, $"Could not find the requested TNC ({ViewModels.SharedData.SharedDataInstance.CurrentProfile.TNC})");
				}
			}
			else
			{
				MessageDialog messageDialog = new MessageDialog($"Could not find the requested BBS ({ViewModels.SharedData.SharedDataInstance.CurrentProfile.BBS}). Check Packet Settings");
				await messageDialog.ShowAsync();
                LogHelper(LogLevel.Error, $"Could not find the requested BBS ({ViewModels.SharedData.SharedDataInstance.CurrentProfile.BBS}). Check Packet Settings");
			}
		}

	}
}
