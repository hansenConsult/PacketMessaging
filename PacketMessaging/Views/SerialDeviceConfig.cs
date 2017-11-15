//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace PacketMessaging.Views
{
	public class DeviceProperties
    {
        public const String DeviceInstanceId = "System.Devices.DeviceInstanceId";
    }

    public class DeviceConfiguration
    {
		public uint BaudRate = (uint)ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort?.Baudrate;

        public SerialParity Parity = (SerialParity)ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort?.Parity;

        public SerialStopBitCount StopBits = (SerialStopBitCount)ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort?.Stopbits;

        public UInt16 DataBits = (UInt16)ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort?.Databits;

        public SerialHandshake Handshake = (SerialHandshake)ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort?.Flowcontrol;

        public const Boolean BreakSignalState_false = false;
        public const Boolean BreakSignalState_true = true;

        public const Boolean IsDataTerminalReady_false = false;
        public const Boolean IsDataTerminalReady_true = true;

        public const Boolean IsRequestToSendEnabled_false = false;
        public const Boolean IsRequestToSendEnabled_true = true;

		async Task<SerialDevice> GetSerialDeviceAsync()
		{
			string portname = ViewModels.SharedData.SharedDataInstance.CurrentTNCDevice.CommPort.Comport;
			String aqs = SerialDevice.GetDeviceSelector(portname);

			var myDevices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs, null);

			if (myDevices.Count == 0)
			{
				MainPage.Current.ShowMessageBox("Device not found!");
				return null;
			}

			//SerialDevice device = await SerialDevice.FromIdAsync(myDevices[0].Id);
			return await SerialDevice.FromIdAsync(myDevices[0].Id);
		}
	}
    
}
