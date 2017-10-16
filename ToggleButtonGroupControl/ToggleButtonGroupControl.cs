using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace ToggleButtonGroupControl
{
    public sealed class ToggleButtonGroup : Control
	{
		static SolidColorBrush _redBrush = new SolidColorBrush(Colors.Red);
		static SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
		static SolidColorBrush _blackBrush = new SolidColorBrush(Colors.Black);


		IList<RadioButton> _radioButtonGroup = new List<RadioButton>();

		static ToggleButtonGroup()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButtonGroup), new FrameworkPropertyMetadata(typeof(ToggleButtonGroup)));
		}

        //public ToggleButtonGroup(List<RadioButton> radioButtonList, string groupName)
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButtonGroup), new FrameworkPropertyMetadata(typeof(ToggleButtonGroup)));

        //    foreach (RadioButton radioButton in radioButtonList)
        //    {
        //        if (radioButton.GroupName == groupName)
        //        {
        //            radioButtonGroup.Add(radioButton);
        //            radioButton.IsChecked = false;
        //        }
        //    }
        //    SetBorderBrush(_blackBrush);
        //}

        public IList<RadioButton> RadioButtonGroup
        { get => _radioButtonGroup; }

        public static readonly DependencyProperty
            CheckedControlNameProperty = DependencyProperty.Register("CheckedControlName", typeof(string),
                typeof(ToggleButtonGroup), new PropertyMetadata(null, new PropertyChangedCallback(OnSelectionChanged)));

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string newval = e.NewValue as string;
            (d as ToggleButtonGroup).SetRadioButtonCheckedState(e.NewValue as string);
        }

        public string CheckedControlName
        {
            get { return (string)GetValue(CheckedControlNameProperty); }
            set { SetValue(CheckedControlNameProperty, value); }
        }
        //{ get { return GetRadioButtonCheckedState(); } set { SetRadioButtonCheckedState(value); } }

        public void Initialize(IList<RadioButton> radioButtonList, string groupName)
			{
				foreach (RadioButton radioButton in radioButtonList)
				{
					if (radioButton.GroupName == groupName)
					{
						_radioButtonGroup.Add(radioButton);
						radioButton.IsChecked = false;
					}
				}
			}

			//delegate string GetRadioButtonChecked(RadioButton radioButton);
			//string GetRadioButton(RadioButton radioButton)
			//{
			//	if ((bool)radioButton.IsChecked)
			//	{
			//		return radioButton.Name;
			//	}
			//	return null;
			//}

			public string GetRadioButtonCheckedState()
			{
				foreach (RadioButton radioButton in _radioButtonGroup)
				{
					//if ((radioButton.Dispatcher.CheckAccess()))
					//{
						if ((bool)radioButton.IsChecked)
						{
							return radioButton.Name;
						}
					//}
					//else
					//{
					//	string retval = (string)radioButton.Dispatcher.Invoke(DispatcherPriority.Normal, new GetRadioButtonChecked(GetRadioButton), radioButton);
					//	if (retval != null && retval?.Length != 0)
					//		return retval;
					//}
				}
				return null;
			}

			//delegate void SetRadioButtonChecked(RadioButton radioButton, string name);
			//void SetRadioButton(RadioButton radioButton, string name)
			//{
			//	if (radioButton.Name == name)
			//	{
			//		radioButton.IsChecked = true;
			//	}
			//	else
			//	{
			//		radioButton.IsChecked = false;
			//	}
			//}

			public void SetRadioButtonCheckedState(string name)
			{
				if (name == null || name.Length == 0)
					return;

				// First character must be lower case
				string firstCharacter = name.Substring(0, 1);
				firstCharacter = firstCharacter.ToLower();
				string AllButFirst = name.Substring(1);
				string nameWithLowercase = firstCharacter + AllButFirst;

				foreach (RadioButton radioButton in _radioButtonGroup)
				{
					//if ((radioButton.Dispatcher.CheckAccess()))
					//{
						if (radioButton.Name == nameWithLowercase)
						{
							radioButton.IsChecked = true;
							break;
						}
					else
					{
						radioButton.IsChecked = false;
					}
				//}
				//	else
				//	{
				//		radioButton.Dispatcher.Invoke(DispatcherPriority.Normal, new SetRadioButtonChecked(SetRadioButton), radioButton, nameWithLowercase);
				//	}
				}
			}

        public bool Validate()
        {
            if (GetRadioButtonCheckedState() == null)
            {
                foreach (RadioButton radioButton in _radioButtonGroup)
                {
                    radioButton.Foreground = _redBrush;
                }
                return false;
            }
            else
            {
                foreach (RadioButton radioButton in _radioButtonGroup)
                {
                    radioButton.Foreground = _blackBrush;
                }
                return true;
            }
        }

    }
}

