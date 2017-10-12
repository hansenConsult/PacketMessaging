using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using FormControlBaseClass;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PacketMessaging.Views
{
	public sealed partial class SendFormDataControl : UserControl
	{
		List<FormControl> _formFieldsList = new List<FormControl>();


		public SendFormDataControl()
		{
			this.InitializeComponent();

			ScanControls(messageInfo);

			MessageBBS = ViewModels.SharedData.SharedDataInstance.CurrentProfile.BBS;
			MessageTNC = ViewModels.SharedData.SharedDataInstance.CurrentProfile.TNC;
			MessageTo = ViewModels.SharedData.SharedDataInstance.CurrentProfile.SendTo;
			if (ViewModels.SettingsPageViewModel.IdentityPartViewModel.UseTacticalCallsign)
			{
				MessageFrom = ViewModels.SettingsPageViewModel.IdentityPartViewModel.TacticalCallsign;
			}
			else
			{
				MessageFrom = ViewModels.SettingsPageViewModel.IdentityPartViewModel.UserCallsign;
			}
		}

		public string MessageBBS
		{
			get => messageBBS.Text;
            set => messageBBS.Text = value == null ? "" : value;
		}

		public string MessageTNC
		{
			get => messageTNC.Text; 
			set => messageTNC.Text = value == null ? "" : value; 
		}

		public string MessageFrom
		{
			get => messageFrom.Text;
			set => messageFrom.Text = value;
		}

		public string MessageTo
		{
			get => messageTo.Text;
			//set => messageTo.Text = value;
            set => messageTo.Text = AddressBook.GetAddress(value);
		}

		public string MessageSubject
		{
			get => messageSubject.Text;
			set => messageSubject.Text = string.IsNullOrEmpty(value) ? "" : value;
		}

		public void ScanControls(DependencyObject panelName)
		{
			var count = VisualTreeHelper.GetChildrenCount(panelName);

			for (int i = 0; i < count; i++)
			{
				DependencyObject control = VisualTreeHelper.GetChild(panelName, i);

				if (control is StackPanel || control is Grid || control is Border || control is RelativePanel)
				{
					ScanControls(control);
				}
				else if (control is TextBox)
				{
                    //FormControl formControl = new FormControl((Control)control);
                    FormControl formControl = new FormControl(control as Control);
                    _formFieldsList.Add(formControl);
				}
			}
		}

		public bool ValidateForm()
		{
			bool result = true;
			foreach (FormControl formControl in _formFieldsList)
			{
				Control control = formControl.InputControl;

				if (control.Tag?.ToString() == "required")
				{
					if (control is TextBox textBox)
					{
						if (textBox.Text.Length == 0)
						{
							textBox.BorderBrush = FormControlBase._redBrush;
							result &= false;
						}
						else
						{
							control.BorderBrush = formControl.BaseBorderColor;
						}
					}
					//else if (control.GetType() == typeof(ComboBox))
					//{
					//	if (((ComboBox)control).SelectedIndex < 0)
					//	{
					//		control.BorderBrush = FormControlBase._redBrush;
					//		result &= false;
					//	}
					//	else
					//	{
					//		control.BorderBrush = FormControlBase._whiteBrush;
					//	}
					//}
				}
			}
			return result;
		}

		private void MessageTo_TextChanged(object sender, TextChangedEventArgs e)
		{
			messageTo.Text = AddressBook.GetAddress(messageTo.Text);
		}
	}
}
