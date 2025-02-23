using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace PizzAppMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            AddLogoutButton();
        }

        private void AddLogoutButton()
        {
            ToolbarItem logoutItem = new ToolbarItem
            {
                Text = "Logout",
                Order = ToolbarItemOrder.Secondary,
                Priority = 0
            };
            logoutItem.Clicked += OnLogoutClicked;
            this.Items.Add(logoutItem);
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Sigur doriți să vă deconectați?", "Da", "Nu");
            if (confirm)
            {
                SecureStorage.Remove("jwt_token"); // Ștergem token-ul de autentificare
                await Shell.Current.GoToAsync("//LoginPage"); // Navigăm către LoginPage
            }
        }
    }
}
