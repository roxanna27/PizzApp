using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzAppMobile.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 🔹 Navigare către pagina meniului
        /// </summary>
        private async void OnMenuClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductsPage");
        }

        /// <summary>
        /// 🔹 Navigare către coșul de cumpărături (necesită autentificare)
        /// </summary>
        private async void OnCartClicked(object sender, EventArgs e)
        {
            string jwtToken = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrEmpty(jwtToken))
            {
                await DisplayAlert("Eroare", "Trebuie să fiți autentificat pentru a accesa coșul.", "OK");
                return;
            }

            await Shell.Current.GoToAsync("CartPage");
        }
    }
}
