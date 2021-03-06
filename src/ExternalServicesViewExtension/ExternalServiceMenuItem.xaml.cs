﻿using System.Windows;
using System.Windows.Controls;

namespace Dynamo.ExternalServices.Extensions
{
    /// <summary>
    /// Interaction logic for ExternalServiceMenuItem.xaml
    /// </summary>
    public partial class ExternalServiceMenuItem : MenuItem
    {
        public ExternalServiceMenuItem()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;

            var service = DataContext as IExternalService;
            if (service == null) return;

            if (string.IsNullOrEmpty(service.AccessToken))
            {
                var login = new LoginForm(service);
                login.Show();
            }
            else
            {
                service.Logout();
            }
        }
    }
}
