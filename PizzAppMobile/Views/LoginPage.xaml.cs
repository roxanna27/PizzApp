using PizzAppMobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace PizzAppMobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Eroare", "Introduceți email și parolă!", "OK");
                return;
            }

            string jwtToken = await _apiService.LoginAsync(email, password);

            if (string.IsNullOrEmpty(jwtToken))
            {
                await DisplayAlert("Eroare", "Autentificare eșuată!", "OK");
                return;
            }

            await SecureStorage.SetAsync("jwt_token", jwtToken);
            await Shell.Current.GoToAsync("//ProductsPage");
        }
    }
}
