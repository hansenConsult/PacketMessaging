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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace PacketMessaging.Services.CommunicationsService
{
    /// <summary>
    /// This class is intended to host any functionality that will be shared among different
    /// scenario/pages such as common error messages.
    /// </summary>
    public class Utilities
    {
		/// <summary>
		/// Prints an error message stating that device is not connected
		/// </summary>
		public static async void NotifyDeviceNotConnectedAsync()
		{
			await ShowMessageDialogAsync("Device is not connected, please select a plugged in device to try the scenario again");
		}

		public static async Task ShowMessageDialogAsync(string dialogMessage)
		{
			ContentDialog contentDialog = new ContentDialog()
			{
				Title = "Packet Message",
				Content = dialogMessage,
				CloseButtonText = "Close"
			};
			await contentDialog.ShowAsync();

			//MessageDialog messageDialog = new MessageDialog(dialogMessage);
			//await messageDialog.ShowAsync();
		}

		public static async void ShowMessageDialogAsync(string dialogMessage, string title)
		{
			ContentDialog contentDialog = new ContentDialog()
			{ 
				Title = title,
				Content = dialogMessage,
				CloseButtonText = "Close"
			};
		await contentDialog.ShowAsync();
	}
}
}
