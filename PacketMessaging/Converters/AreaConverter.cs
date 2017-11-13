using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using FormControlBaseClass;

//namespace PacketMessaging.Views
namespace PacketMessaging.Converters
{
	public sealed class AreaConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
		{
			if (value != null && (((string)value).ToLower() == "xscperm" || ((string)value).ToLower() == "xscevent"))
			{
				//((PacketMessage)value).Area = "B";
				//return value;
				return "B";
			}
			else if (value == null)
			{
				//((PacketMessage)value).Area = "";
				//return value;
				return "";
			}
			else
			{
				//return ((PacketMessage)value).Area;
				return value;
			}
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class DateTimeConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
			{
				return "";
			}
			DateTime dateTime = (DateTime)value;
			string date = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";
			return date;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class ComboBoxItemConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language) => value;

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language) => value;
	}

    public sealed class VisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
			if (string.IsNullOrEmpty(value as string))
				return Windows.UI.Xaml.Visibility.Collapsed;
            else
				return Windows.UI.Xaml.Visibility.Visible;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

	//public sealed class BBSToggleEnabled : IValueConverter
	//{
	//	object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
	//	{
	//		return true;
	//	}

	//	object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
	//	{
	//		throw new NotImplementedException();
	//	}
	//}

	public class ComportComparer : IComparer<string>
    {
        int IComparer<string>.Compare(string x, string y)
        {
            // Only compare the port number
            string comString = "COM";
            string x1 = x.Substring(comString.Length);
            string y1 = y.Substring(comString.Length);

            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    int retval = x1.Length.CompareTo(y1.Length);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        return x1.CompareTo(y1);
                    }
                }
            }
        }
    }
}
