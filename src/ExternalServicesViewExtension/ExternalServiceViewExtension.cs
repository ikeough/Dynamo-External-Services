using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Wpf.Extensions;

namespace Dynamo.ExternalServices.Extensions
{
    public class ExternalServicesViewExtension : IViewExtension
    {
        public void Dispose()
        {
            
        }

        public void Startup(ViewStartupParams p)
        {
            
        }

        public void Loaded(ViewLoadedParams p)
        {
            var view = (DynamoView)p.DynamoWindow;

            var menu = FindVisualChildren<Menu>(view).FirstOrDefault();
            if (menu == null) return;

            var servicesMenu = new ExternalServicesMenu ();
            menu.Items.Add(servicesMenu);
        }

        public void Shutdown()
        {
            
        }

        public string UniqueId
        {
            get { return Guid.NewGuid().ToString(); }
        }

        public string Name
        {
            get { return "External Services"; }
        }

        internal static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }

    public class OAuthServiceLoginStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var serviceName = value[0].ToString();
            var userName = value[1] == null ? string.Empty : value[1].ToString();
            var token = value[2];

            return serviceName + (token == null? " : Login" : " : Logout (" + userName + ")");
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OAuthServiceAuthenticationMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Authenticate With " + value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
