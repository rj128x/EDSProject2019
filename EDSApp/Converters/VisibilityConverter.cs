using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EDSApp.Converters
{
	public class VisibilityConverter : IValueConverter {
		#region IValueConverter Members
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is Boolean) {
				try {
					bool val = (Boolean)value;
					return val ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
				} catch {
					return System.Windows.Visibility.Collapsed;
				}
			} else if (value is Int32) {
				try {
					int val = (Int32)value;
					return val>0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
				} catch {
					return System.Windows.Visibility.Collapsed;
				}
			}
			else if (value is String) {
				try {
					string val = (String)value;
					return !String.IsNullOrEmpty(val) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
				}
				catch {
					return System.Windows.Visibility.Collapsed;
				}
			}
			else return value == null ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			try {
				System.Windows.Visibility val = (System.Windows.Visibility)value;
				return val == System.Windows.Visibility.Visible;
			} catch {
				return false;
			}
		}

		#endregion
	}
}
