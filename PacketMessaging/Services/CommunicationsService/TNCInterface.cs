using System;
using System.Collections.Generic;
using System.IO;
using FormControlBaseClass;
using MessageFormControl;
using System.Diagnostics;
using MetroLog;
using System.Threading.Tasks;
using Windows.UI.Popups;
using System.Runtime.CompilerServices;
using PacketMessaging.Models;
using Windows.Devices.SerialCommunication;

namespace PacketMessaging.Services.CommunicationsService
{
	public class TNCInterface : CommunicationsServiceBase
    {
		private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<TNCInterface>();

		enum ConnectState
        {
            ConnectStateNone,
            ConnectStatePrepareTNCType,
            ConnectStatePrepare,
			ConnectStateBBSTryConnect,
			ConnectStateBBSConnected,
            ConnectStatePost,
            ConnectStateDisconnected,
			ConnectStateConverseMode
        }
        ConnectState _connectState;

        List<PacketMessage> _packetMessagesSent = new List<PacketMessage>();
        List<PacketMessage> _packetMessagesReceived = new List<PacketMessage>();
        List<PacketMessage> _packetMessagesToSend;

        string _messageBBS = "";
        bool _forceReadBulletins = false;
        string[] _Areas;
        TNCDevice _tncDevice = null;
  //      SerialDevice _serialPort = null;
		//DataReader _serialReader;
		//DataWriter _serialPort;
		SerialPort _serialPort;

		const string _BBSPrompt = ") >\r\n";
		string _TNCPrompt = "cmd:";
		bool _error = false;		// Disconnect if an error is detected

		//const byte send = 0x5;

        public TNCInterface(string messageBBS, ref TNCDevice tncDevice, bool forceReadBulletins, string[] areas, ref List<PacketMessage> packetMessagesToSend) : base(log)
        {
            _messageBBS = messageBBS;
			_tncDevice = tncDevice;
			_TNCPrompt = _tncDevice.Prompts.Command;
            _forceReadBulletins = forceReadBulletins;
            _Areas = areas;
            _packetMessagesToSend = packetMessagesToSend;
			//_serialPort = new SerialPort(ref tncDevice);
		}

		public List<PacketMessage> PacketMessagesSent => _packetMessagesSent;

        public List<PacketMessage> PacketMessagesReceived => _packetMessagesReceived;

        public DateTime BBSConnectTime
        { get; set; }

		public DateTime BBSDisconnectTime
		{ get; set; }

		//public ConnectedDialog ConnectDlg
  //      { get; set; }


		public bool Cancel
		{ get { return _error; } set { _error = value; } }

		private async Task<string> KPC3PlusAsync()
		{
			string readText = await _serialPort.ReadLineAsync();       // This appears to be a dummy read for KPC3
			Debug.WriteLine(readText);
			log.Info(readText);

			readText = await _serialPort.ReadLineAsync();
			Debug.WriteLine(readText);
			log.Info(readText);

			readText = await _serialPort.ReadLineAsync();
			Debug.WriteLine(readText);
			log.Info(readText);

			readText = await _serialPort.ReadLineAsync();
			Debug.WriteLine(readText);
			log.Info(readText);

			string readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
			Debug.WriteLine(readCmdText + _TNCPrompt);   // First cmd:

			await _serialPort.WriteAsync("D\r");
			//Thread.Sleep(100);

			readText = await _serialPort.ReadLineAsync();
			Debug.WriteLine(readText);
			log.Info(readCmdText + _TNCPrompt + readText);

			readText = await _serialPort.ReadLineAsync();
			Debug.WriteLine(readText);
			log.Info(readText);

			readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
			Debug.WriteLine(readCmdText + _TNCPrompt);

			await _serialPort.WriteAsync("b\r");

			readText = await _serialPort.ReadLineAsync();       // Command
			Debug.WriteLine(readText);
			log.Info(readCmdText + _TNCPrompt + readText);

			readText = await _serialPort.ReadLineAsync();       // Result for b
			Debug.WriteLine(readText);
			log.Info(readText);

			readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
			Debug.WriteLine(readCmdText + _TNCPrompt);

			await _serialPort.WriteAsync("Echo on\r");

			readText = await _serialPort.ReadLineAsync();       // Read command
			Debug.WriteLine(readText);
			log.Info(readCmdText + _TNCPrompt + readText);

			readText = await _serialPort.ReadLineAsync();       // Result for Echo on
			Debug.WriteLine(readText);
			log.Info(readText);

			readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
			Debug.WriteLine(readCmdText + _TNCPrompt);

			await _serialPort.WriteAsync("my " + ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign + "\r");

			readText = await _serialPort.ReadLineAsync();       // Read command
			Debug.WriteLine(readText);
			log.Info(readCmdText + _TNCPrompt + readText);
			// Note no command response

			readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
			Debug.WriteLine(readCmdText + _TNCPrompt);

			await _serialPort.WriteAsync("Mon off\r");

			readText = await _serialPort.ReadLineAsync();       // Read command
			Debug.WriteLine(readText);
			log.Info(readCmdText + _TNCPrompt + readText);

			readText = await _serialPort.ReadLineAsync();       // Result for Mon off
			Debug.WriteLine(readText);
			log.Info(readText);

			readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
			Debug.WriteLine(readCmdText + _TNCPrompt);

			DateTime dateTime = DateTime.Now;
			string dayTime = $"{dateTime.Year - 2000:d2}{dateTime.Month:d2}{dateTime.Day:d2}{dateTime.Hour:d2}{dateTime.Minute:d2}{dateTime.Second:d2}";
			await _serialPort.WriteAsync("daytime " + dayTime + "\r");

			readText = await _serialPort.ReadLineAsync();       // Read command
			Debug.WriteLine(readText);
			log.Info(readCmdText + _TNCPrompt + readText);
			// Note no command response

			readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);      // Ready for precommands
			Debug.WriteLine(readCmdText + _TNCPrompt);
			return readCmdText;
		}

		private async Task<bool> KenwoodAsync()
        {
			LogHelper(LogLevel.Trace, "Initializing Kenwood TNC");

			bool success = true;
			string readText = "";
			//int index = -1;
			try
			{
				await _serialPort.WriteWithEchoAsync("\r");
				readText = await _serialPort.ReadBufferAsync();

				//_serialPort.Write("D\r");

				//string readText = _serialPort.ReadLine();
				//log.Info(readCmdText + _TNCPrompt + readText);

				//readText = _serialPort.ReadLine();
				//log.Info(readText);

				//readCmdText = _serialPort.ReadTo(_TNCPrompt);

				//_serialPort.Write("b\r");

				//redo:
				//await _serialPort.WriteAsync("\r\n");
				//readText = await _serialPort.ReadBufferAsync();
				//if (!readText.EndsWith(_TNCPrompt))
				//{
				//	await _serialPort.WriteAsync("\r\n");
				//	await _serialPort.ReadToAsync(_TNCPrompt);
				//}


				while (!readText.EndsWith(_TNCPrompt))
				{
					//readText = await _serialPort.ReadBufferAsync(); // read line and possibly prompt
					await _serialPort.WriteLineAsync("");
					readText = await _serialPort.ReadBufferAsync(); // read line and possibly prompt
					LogHelper(LogLevel.Info, $"r1 {readText}");
					if (readText.EndsWith(_TNCPrompt))
						break;
					readText = await _serialPort.ReadBufferAsync();
					LogHelper(LogLevel.Info, $"r2: {readText}");
					if (readText.EndsWith(_TNCPrompt))
						break;
					readText = await _serialPort.ReadBufferAsync();
				}

				//LogHelper(LogLevel.Info, $"Write: {"D\r"}");
				await _serialPort.WriteWithEchoAsync("D\r");
				//await _serialPort.WriteAsync("D\r");
				//readText = await _serialPort.ReadToAsync(_TNCPrompt);
				readText = await _serialPort.ReadBufferAsync();
				//string readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
				//string readCmdText = await _serialPort.ReadBufferAsync();
				//LogHelper(LogLevel.Info, $"D: {readCmdText}");

				await _serialPort.WriteWithEchoAsync("b\r");
				//await _serialPort.WriteAsync("b\r");
				//readText = await _serialPort.ReadToAsync(_TNCPrompt);
				readText = await _serialPort.ReadBufferAsync();
				//LogHelper(LogLevel.Info, $"br: {readText}");
				//readCmdText = await _serialPort.ReadBufferAsync();
				//LogHelper(LogLevel.Info, $"b: {readCmdText}");

				await _serialPort.WriteWithEchoAsync("Echo on\r");
				//readText = await _serialPort.ReadToAsync(_TNCPrompt);
				readText = await _serialPort.ReadBufferAsync();
				//LogHelper(LogLevel.Info, $"Buffer: {readText}");

				await _serialPort.WriteAsync("my " + ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign + "\r");
				//readText = await _serialPort.ReadToAsync(_TNCPrompt);
				readText = await _serialPort.ReadBufferAsync();
				LogHelper(LogLevel.Info, $"Buffer: {readText}");

				await _serialPort.WriteAsync("Mon off\r");
				//readText = await _serialPort.ReadToAsync(_TNCPrompt);
				readText = await _serialPort.ReadBufferAsync();
				LogHelper(LogLevel.Info, $"Buffer: {readText}");

				DateTime dateTime = DateTime.Now;
				string dayTime = $"{dateTime.Year - 2000:d2}{dateTime.Month:d2}{dateTime.Day:d2}{dateTime.Hour:d2}{dateTime.Minute:d2}{dateTime.Second:d2}";
				await _serialPort.WriteAsync("daytime " + dayTime + "\r\n");
				//readText = await _serialPort.ReadToAsync(_TNCPrompt);       // Ready for precommands
				readText = await _serialPort.ReadBufferAsync();
				LogHelper(LogLevel.Info, $"Buffer: {readText}");
			}
			catch (SerialPortException e)
			{
				LogHelper(LogLevel.Error, $"Kenwood initialization exception. {e.Message}");
				success = false;
			}
			LogHelper(LogLevel.Trace, $"Done Initializing Kenwood TNC. Result: {(success == false ? "Error" : "Succeded")}");
			return success;
		}

		private async void SendMessageAsync(PacketMessage packetMessage)
		{
			_serialPort.ReadTimeout = new TimeSpan(0, 1, 0, 0, 0);
			try
			{
				await _serialPort.WriteAsync("SP " + packetMessage.MessageTo + "\r");
				await _serialPort.WriteAsync(packetMessage.Subject + "\r");
				await _serialPort.WriteAsync(packetMessage.MessageBody + "\r\x1a\r\x05");

				string readText = await _serialPort.ReadLineAsync();       // Read SP
				//Debug.WriteLine(readText);
				//LogHelper(LogLevel.Info, readText);

				readText = await _serialPort.ReadToAsync(_BBSPrompt);      // read response
				//Debug.WriteLine(readText + _BBSPrompt);
                //LogHelper(LogLevel.Info, readText);

				readText = await _serialPort.ReadToAsync("\n");         // Next command
				//Debug.WriteLine(readText + "\n");
                //LogHelper(LogLevel.Info, readText

				DateTime dateTime = DateTime.Now;
				packetMessage.SentTime = dateTime;
				packetMessage.SentTimeDisplay = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";
				_packetMessagesSent.Add(packetMessage);
			}
			catch (Exception e)
			{
				log.Error("Send message exception:", e);
				_error = true;
			}
			_serialPort.ReadTimeout = new TimeSpan(0, 0, 0, 5, 0);
		}

		private void SendMessageReceipts()
		{
			if (ViewModels.SettingsPageViewModel.PacketSettingsPartViewModel.SendReceivedReceipt)
			{
				// do not send received receipt for receive receipt messages
				foreach (PacketMessage pktMsg in _packetMessagesReceived)
				{
					if (pktMsg.Area.Length > 0)			// Do not send receipt for bulletins
						continue;

					try
					{
						// Find the Subject line
						string[] msgLines = pktMsg.MessageBody.Split(new string[] { "\r\n" }, StringSplitOptions.None);
						for (int i = 0; i < Math.Min(msgLines.Length, 10); i++)
						{
							if (msgLines[i].StartsWith("Date:"))
							{
								pktMsg.JNOSDate = DateTime.Parse(msgLines[i].Substring(10, 21));
								pktMsg.JNOSDateDisplay = $"{pktMsg.JNOSDate.Month:d2}/{pktMsg.JNOSDate.Date:d2}/{pktMsg.JNOSDate.Year - 2000:d2} {pktMsg.JNOSDate.Hour:d2}:{pktMsg.JNOSDate.Minute:d2}";
							}
							else if (msgLines[i].StartsWith("From:"))
								pktMsg.MessageFrom = msgLines[i].Substring(6);
							else if (msgLines[i].StartsWith("To:"))
								pktMsg.MessageTo = msgLines[i].Substring(4);
							else if (msgLines[i].StartsWith("Subject:"))
							{
								if (msgLines[i].Length > 10)
								{
									pktMsg.Subject = msgLines[i].Substring(9);
								}
								break;
							}
						}

						if (!pktMsg.Subject.Contains("DELIVERED:") && pktMsg.Area.Length == 0)
						{
                            PacketMessage receiptMessage = new PacketMessage()
                            {
                                PacFormName = "SimpleMessage",
                                MessageNumber = ViewModels.SettingsPageViewModel.GetMessageNumberPacket(),
                                BBSName = _messageBBS,
                                TNCName = _tncDevice.Name,
                                MessageTo = pktMsg.MessageFrom,
                                MessageFrom = ViewModels.SettingsPageViewModel.IdentityPartViewModel.UseTacticalCallsign ? ViewModels.SettingsPageViewModel.IdentityPartViewModel.TacticalCallsign : ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign,

                                Subject = $"DELIVERED: {pktMsg.Subject}"
                            };
                            FormField[] formFields = new FormField[1];

                            FormField formField = new FormField()
                            {
                                ControlName = "messageBody",
                                ControlContent = $"!LMI!{pktMsg.MessageNumber}!DR!{pktMsg.ReceivedTime?.ToString("G")}\r\n"
                            };
                            formField.ControlContent += "Your Message\r\n";
							formField.ControlContent += $"To: {pktMsg.MessageTo}\r\n";
							formField.ControlContent += $"Subject: {pktMsg.Subject}\r\n";
							//formField.ControlContent += $"was delivered on {pktMsg.MessageReceiveTime.ToShortDateString()} {pktMsg.MessageReceiveTime.ToShortTimeString()}\r\n";
							formField.ControlContent += $"was delivered on {pktMsg.ReceivedTime?.ToString("G")}\r\n";
							formField.ControlContent += $"Recipient's Local Message ID: {pktMsg.MessageNumber}\r\n";
							formFields[0] = formField;

							receiptMessage.FormFieldArray = formFields;
							MessageControl packetForm = new MessageControl();
							receiptMessage.MessageBody = packetForm.CreateOutpostData(ref receiptMessage);
							receiptMessage.CreateFileName();
							DateTime dateTime = DateTime.Now;
							receiptMessage.SentTime = dateTime;
							receiptMessage.SentTimeDisplay = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";
							receiptMessage.MessageSize = receiptMessage.Size;
							log.Info(receiptMessage.MessageBody);   // Disable if not testing
							//SendMessage(ref receiptMessage);		// Disabled for testing
							_packetMessagesSent.Add(receiptMessage);
						}
					}
					catch (Exception e)
					{
						log.Error("Delivered message exception: ", e);
						_error = true;
					}
				}
			}
		}

		private async void ReceiveMessagesAsync(string area)
		{
			string readText;
			//string readCmdText;
			_serialPort.ReadTimeout = new TimeSpan(0, 0, 0, 10, 0);
			try
			{
				if (area.Length != 0)
				{
					// Read bulletins
					await _serialPort.WriteAsync($"A {area}\r\x05");        // A XSCPERM
					readText = await _serialPort.ReadToAsync(_BBSPrompt);        // read response
					//Debug.WriteLine(readText + _BBSPrompt);
					//log.Info(readText + _BBSPrompt);

					//readText = await _serialPort.ReadTo("\n");         // Next command
					//Debug.WriteLine(readText + "\n");

					if (!_forceReadBulletins && readText.Contains("0 messages"))
					{
						//log.Info("Skip read bulletin 1");
						return;
					}
					if (!_forceReadBulletins && readText.Contains("0 new"))
					{
						//log.Info("Skip read bulletin 2");
						return;
					}
					log.Info($"Read bulletin {_forceReadBulletins.ToString()}");
					await _serialPort.WriteAsync("LA\r");
				}
				else
				{
					//log.Info($"Timeout = {_serialPort.ReadTimeout}");        // For testing
					await _serialPort.WriteAsync("LM\r");
				}
				readText = await _serialPort.ReadToAsync(_BBSPrompt);      // read response
				//Debug.WriteLine(readText + _BBSPrompt);
				//log.Info(readText);

				//readCmdText = await _serialPort.ReadTo("\n");         // Next command
				//Debug.WriteLine(readCmdText + "\n");

				// read messages
				string[] lines = readText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				bool firstMessageDescriptionDetected = false;
				foreach (string line in lines)
				{
					if (_error)
						break;

					if (line[0] != '(' && (line[0] == '>' || firstMessageDescriptionDetected))
					{
						//string lineCopy = line.Substring(1);        // Remove the first character which may be ' ' or '>'
						string lineCopy = line.TrimStart(new char[] { ' ', '>' });

						firstMessageDescriptionDetected = true;
						string[] lineSections = lineCopy.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						//Debug.WriteLineLine("lineSections length: " + lineSections.Length);
						if (char.IsLetter(lineSections[1][0]))        // No more messages in the list. Not sure this works!
							break;

                        PacketMessage packetMessage = new PacketMessage()
                        {
                            BBSName = _messageBBS,
                            TNCName = _tncDevice.Name,
                            MessageNumber = ViewModels.SettingsPageViewModel.GetMessageNumberPacket(),
                            Area = area,
                            MessageSize = Convert.ToInt32(lineSections[6])
                        };
                        //Debug.WriteLineLine(packetMessage.MessageSize.ToString());
                        int msgIndex = Convert.ToInt32(lineSections[1]);
						//Debug.WriteLineLine(msgIndex.ToString());

						await _serialPort.WriteAsync("R " + msgIndex + "\r\x05");
						readText = await _serialPort.ReadToAsync(_BBSPrompt);      // read response
						//Debug.WriteLine(readText + _BBSPrompt);
						//log.Info(readText + _BBSPrompt);

						//readCmdText = await _serialPort.ReadTo("\n");     // Next command
						//Debug.WriteLine(readCmdText + "\n");

						packetMessage.MessageBody = readText.Substring(0, readText.Length - _BBSPrompt.Length); // Remove beginning of prompt
						DateTime dateTime = DateTime.Now;
						packetMessage.ReceivedTime = dateTime;
						packetMessage.ReceivedTimeDisplay = $"{dateTime.Month:d2}/{dateTime.Date:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";
						_packetMessagesReceived.Add(packetMessage);

						if (area.Length == 0)
						{
							// Delete the received message from the BBS
							await _serialPort.WriteAsync("K " + msgIndex + "\r\x05");
							readText = await _serialPort.ReadToAsync(_BBSPrompt);      // read response
							//Debug.WriteLine(readText + _BBSPrompt);
							//log.Info(readText + _BBSPrompt);

							//readCmdText = await _serialPort.ReadTo("\n");       // Read rest of prompt
							//Debug.WriteLine(readCmdText + "\n");
						}
					}
				}
			}
			catch (Exception e)
			{
				LogHelper(LogLevel.Error, $"Receive message exception: {e.Message}");
				//_serialPort.DiscardInBuffer();
				//_serialPort.DiscardOutBuffer();
				_error = true;
			}
			_serialPort.ReadTimeout = new TimeSpan(0, 0, 0, 5, 0);

		}

		//void SerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		//{
		//    Debug.WriteLine($"SerialPort exception: {e.EventType.ToString()}");
		//    log.Error($"SerialPort Error: {e.EventType.ToString()}");
		//    //MessageDialog.ShowAsync($"SerialPort Error: {e.EventType.ToString()}", "TNC Connect Error", MessageBoxButton.OK);
		//    _serialPort.Close();
		//    return;
		//}

		//delegate void CloseDialogWindow(Window window);
		//void CloseWindow(Window window) => window.Close();

		//public void CloseDlgWindow(Window window)
		//{
		//    if ((window.Dispatcher.CheckAccess()))
		//    {
		//        window.Close();
		//    }
		//    else
		//    {
		//        window.Dispatcher.Invoke(DispatcherPriority.Normal, new CloseDialogWindow(CloseWindow), window);
		//    }
		//}

		public async Task<bool> BBSConnectThreadProcAsync()
		{
			_packetMessagesSent.Clear();
			_packetMessagesReceived.Clear();

			SerialDevice serialDevice = EventHandlerForDevice.Current.Device;

			serialDevice.BaudRate = (uint)_tncDevice.CommPort?.Baudrate;
			serialDevice.StopBits = (SerialStopBitCount)_tncDevice.CommPort?.Stopbits;
			serialDevice.DataBits = Convert.ToUInt16(_tncDevice.CommPort.Databits);
			serialDevice.Parity = (SerialParity)_tncDevice.CommPort?.Parity;
			serialDevice.Handshake = (SerialHandshake)_tncDevice.CommPort.Flowcontrol;
			serialDevice.ReadTimeout = new TimeSpan(0, 0, 10); // hours, min, sec
			serialDevice.WriteTimeout = new TimeSpan(0, 0, 10);

			_serialPort = new SerialPort();


            //_serialPort.RtsEnable = _TncDevice.CommPort.Flowcontrol == _serialPort.Handshake.RequestToSend ? true : false;
            //_serialPort..ErrorReceived += new SerialErrorReceivedEventHandler(SerialPortErrorReceived);
            LogHelper(LogLevel.Info, "\n");
			LogHelper(LogLevel.Info, $"{DateTime.Now.ToString()}");
            LogHelper(LogLevel.Info, $"{_tncDevice.Name}: {serialDevice.PortName}, {serialDevice.BaudRate}, {serialDevice.DataBits}, {serialDevice.StopBits}, {serialDevice.Parity}, {serialDevice.Handshake}");
			try
			{
				_connectState = ConnectState.ConnectStateNone;

				string readText = "";
				string readCmdText = "";

				try
				{
					_connectState = ConnectState.ConnectStatePrepareTNCType;
					if (_tncDevice.Name == "XSC_Kantronics_KPC3-Plus")
					{
						readCmdText = await KPC3PlusAsync();
					}
					else if (_tncDevice.Name == "XSC_Kenwood_TM-D710A" || _tncDevice.Name == "XSC_Kenwood_TH-D72A")
					{
						bool success = await KenwoodAsync();
						if (!success)
						{
							throw new IOException();
							//return false;
						}
					}
					// Send Precommands
					string preCommands = _tncDevice.InitCommands.Precommands;
					string[] preCommandLines = preCommands.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
					_connectState = ConnectState.ConnectStatePrepare;
					foreach (string commandLine in preCommandLines)
					{
						await _serialPort.WriteAsync(commandLine + "\r\n");

						//readText = await _serialPort.ReadLine();       // Read command
						//log.Info(readCmdText + _TNCPrompt + readText);

						readText = await _serialPort.ReadToAsync(_TNCPrompt);		// Result for command
						//log.Info(readText);

						//readCmdText = await _serialPort.ReadTo(_TNCPrompt);		// Next command
					}
				}
				catch (Exception e)
				{
					if (e is IOException)
					{
						await Utilities.ShowMessageDialogAsync("Looks like the USB cable to the TNC is disconnected, or the radio is off", "TNC Connect Error");
						return false;
					}

					LogHelper(LogLevel.Error, $"{e.Message}");
					await Utilities.ShowMessageDialogAsync("Failed setting up the TNC");
					return false;
				}
				// Connect to JNOS
				TimeSpan readTimeout = _serialPort.ReadTimeout;
				_serialPort.ReadTimeout = new TimeSpan(0, 0, 0, 60, 0);
				BBSConnectTime = DateTime.Now;
				_connectState = ConnectState.ConnectStateBBSTryConnect;
				await _serialPort.WriteAsync("connect " + _messageBBS + "-1" + "\r\x05");

				//readText = await _serialPort.ReadLineAsync();        // Read command

				string readConnectText = await _serialPort.ReadLineAsync();    // Read command response
				if (readConnectText.ToLower().Contains(_tncDevice.Prompts.Timeout.ToLower()))
				{
					Utilities.ShowMessageDialogAsync("Timeout connecting to the BBS.\nIs the BBS connect name and frequency correct?\nIs the antenna connected.\nThe BBS may be out of reach.", "BBS Connect Error");

					goto Disconnect;
				}

				_connectState = ConnectState.ConnectStateBBSConnected;
				readConnectText = await _serialPort.ReadToAsync(_BBSPrompt);      // read connect response
				//Debug.WriteLine(readConnectText + _BBSPrompt);
				//log.Info(readText + "\n" + readConnectText + _BBSPrompt);
				_serialPort.ReadTimeout = readTimeout;

                // Test
                await _serialPort.WriteAsync("B\r\x05");               // Disconnect from BBS (JNOS)
                readText = await _serialPort.ReadToAsync(_TNCPrompt);

                //readText = await _serialPort.ReadTo(_BBSPrompt);    // Next command
                //Debug.WriteLine(readText + "\n");

                await _serialPort.WriteAsync("XM 0\r\x05");
				readText = await _serialPort.ReadToAsync(_BBSPrompt);      // Read command
				//Debug.WriteLine(readText);
				//log.Info(readText);

				//readCmdText = await _serialPort.ReadLine();   // Read prompt
				//Debug.WriteLine(readCmdText);
				//log.Info(readCmdText);

				// Send messages
				foreach (PacketMessage packetMessage in _packetMessagesToSend)
				{
					_error = true;
					if (_error)
						break;

					if (packetMessage.BBSName == _messageBBS)
					{
						SendMessageAsync(packetMessage);
						//_serialPort.ReadTimeout = new TimeSpan(0, 0, 4, 0, 0);
						//try
						//{
						//	await _serialPort.Write("SP " + packetMessage.MessageTo + "\r");
						//	await _serialPort.Write(packetMessage.MessageSubject + "\r");
						//	await _serialPort.Write(packetMessage.MessageBody + "\r\x1a\r\x05");

						//	readText = await _serialPort.ReadLine();       // Read SP
						//	Debug.WriteLine(readText);
						//	log.Info(readText);

						//	readText = await _serialPort.ReadTo(") >");      // read response
						//	Debug.WriteLine(readText + _BBSPrompt);
						//	log.Info(readText + _BBSPrompt);

						//	readText = await _serialPort.ReadTo("\n");         // Next command
						//	Debug.WriteLine(readText + "\n");

						//	packetMessage.MessageSentTime = DateTime.Now;
						//	_packetMessagesSent.Add(packetMessage);
						//}
						//catch (Exception e)
						//{
						//	log.Error("Send message exception:", e);
						//	//_serialPort.DiscardInBuffer();
						//	//_serialPort.DiscardOutBuffer();
						//	_error = true;
						//}
						//_serialPort.ReadTimeout = new TimeSpan(0, 0, 0, 5, 0);
					}
				}

				if (!readConnectText.Contains("0 messages") && !_error)
				{
//					ReceiveMessages("");
				}

				if (_Areas != null && !_error)
				{
					foreach (string area in _Areas)
					{
//						ReceiveMessages(area);
					}
				}
				//SendMessageReceipts();					// Send message receipts

				await _serialPort.WriteAsync("B\r\x05");               // Disconnect from BBS (JNOS)

				//readText = await _serialPort.ReadLine();           // Read command
				//Debug.WriteLine(readText);
				//log.Info(readText);
Disconnect:
				//readText = await _serialPort.ReadLine();           // Read disconnect response
				//Debug.WriteLine(readText);
				//log.Info(readText);

				BBSDisconnectTime = DateTime.Now;
				//serialPort.Write(cmd, 0, 1);            // Ctrl-C to return to cmd mode. NOT for Kenwood

				SendMessageReceipts();          // TODO testing

				_serialPort.ReadTimeout = new TimeSpan(0, 0, 0, 5, 0);
				readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);      // Next command
				//Debug.WriteLine(readCmdText + _TNCPrompt);

				// Send PostCommands
				string postCommands = _tncDevice.InitCommands.Postcommands;
				string[] postCommandLines = postCommands.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				_connectState = ConnectState.ConnectStatePost;
				foreach (string commandLine in postCommandLines)
				{
					await _serialPort.WriteAsync(commandLine + "\r");

					readText = await _serialPort.ReadLineAsync();              // Read command
					//log.Info(readCmdText + _TNCPrompt + readText);

					//readText = await _serialPort.ReadLine();              // Command result
					//log.Info(readText);

					readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);   // Next command
				}
				// Enter converse mode and send FCC call sign
				_connectState = ConnectState.ConnectStateConverseMode;
				await _serialPort.WriteAsync(_tncDevice.Commands.Conversmode + "\r");
				readText = await _serialPort.ReadLineAsync();       // Read command
				//log.Info(readCmdText + _TNCPrompt + readText);

				string fccId = $"FCC Station ID = {ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign}";
				await _serialPort.WriteAsync(fccId + "\r");
				readText = await _serialPort.ReadLineAsync();
				//log.Info(readText);
				await _serialPort.WriteAsync("\x03\r");                        // Ctrl-C exits converse mode
				readCmdText = await _serialPort.ReadToAsync(_TNCPrompt);
				//log.Info(readCmdText + _TNCPrompt);
			}
			catch (Exception e)
			{
				//Debug.WriteLineLine($"Serial port exception: {e.GetType().ToString()}");
				log.Error($"Serial port exception. Connect state: {Enum.Parse(typeof(ConnectState), _connectState.ToString())}.", e);
				if (_connectState == ConnectState.ConnectStateBBSTryConnect)
				{
					//await _serialPort.Write("XM 0\r\x05");
					await Utilities.ShowMessageDialogAsync("It appears that the radio is tuned to the wrong frequency,\nor the BBS was out of reach", "BBS Connect Error");
					throw;
				}
				else if (_connectState == ConnectState.ConnectStatePrepareTNCType)
				{
					//MessageDialog.Show("Unable to connect to the TNC.\nIs the TNC on?\nFor Kenwood; is the radio in \"packet12\" mode?", "BBS Connect Error", MessageBoxButton.OK);
				}
				else if (_connectState == ConnectState.ConnectStateConverseMode)
				{
					await Utilities.ShowMessageDialogAsync($"Error sending FCC Identification - {ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign}.", "TNC Converse Error");
				}
				else if (_connectState == ConnectState.ConnectStateBBSConnected)
				{
					await _serialPort.WriteAsync("B\r\x05");
					log.Error("BBS Error Disconnect");
				}
				//else if (e.Message.Contains("not exist"))
				else if (e is IOException)
				{
					await Utilities.ShowMessageDialogAsync("Looks like the USB cable to the TNC is disconnected, or the radio is off", "TNC Connect Error");
				}
				else if (e is UnauthorizedAccessException)
				{
					await Utilities.ShowMessageDialogAsync($"The COM Port ({_tncDevice.CommPort.Comport}) is in use by another application. ", "TNC Connect Error");
				}
				else
				{
					log.Fatal($"Connect state: {Enum.Parse(typeof(ConnectState), _connectState.ToString())} Exception: {e.Message}");
				}
                //_serialPort.Debug.WriteLine("B\r\n");
                await Utilities.ShowMessageDialogAsync("Failed to communicate with the TNC");
                return false;
			}
			finally
			{
			}

			//CloseDlgWindow(ConnectDlg);
			return true;
		}

		//#region IDisposable Support
		//private bool disposedValue = false; // To detect redundant calls

		//protected virtual void Dispose(bool disposing)
		//{
		//	if (!disposedValue)
		//	{
		//		if (disposing)
		//		{
		//			// TODO: dispose managed state (managed objects).
		//			_serialPort.Dispose();
		//		}

		//		// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
		//		// TODO: set large fields to null.

		//		disposedValue = true;
		//	}
		//}

		//// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		//// ~TNCInterface() {
		////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		////   Dispose(false);
		//// }

		//// This code added to correctly implement the disposable pattern.
		//void IDisposable.Dispose()
		//{
		//	// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//	Dispose(true);
		//	// TODO: uncomment the following line if the finalizer is overridden above.
		//	// GC.SuppressFinalize(this);
		//}
		//#endregion

	}
}
