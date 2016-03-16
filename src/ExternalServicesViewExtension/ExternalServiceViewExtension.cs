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
using ExternalServiceInterfaces;

namespace ExternalServicesViewExtension
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

            var servicesMenu = new MenuItem() {Header = "External Services"};
            menu.Items.Add(servicesMenu);

            foreach (var service in OAuthServices.Instance.Services)
            {
                var serviceMenuItem = new ExternalServiceMenuItem {DataContext = service};
                servicesMenu.Items.Add(serviceMenuItem);
            }
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
            var authData = value[1] as IOAuthAuthenticationData;

            if (authData == null)
            {
                return serviceName + " : Login...";
            }

            return serviceName + " : Logout (" + authData.CurrentUserName + ")";
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
