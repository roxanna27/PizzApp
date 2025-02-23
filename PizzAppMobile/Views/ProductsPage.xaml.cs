using PizzAppMobile.Models;
using PizzAppMobile.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzAppMobile.Views
{
    public partial class ProductsPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();
        public List<Product> Products { get; set; } = new List<Product>();

        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        /// <summary>
        /// 🔹 Încarcă produsele din API
        /// </summary>
        private async void LoadProducts()
        {
            try
            {
                Products = await _apiService.GetProductsAsync();
                if (Products.Count == 0)
                {
                    await DisplayAlert("Eroare", "Nu s-au putut încărca produsele.", "OK");
                    return;
                }
                ProductListView.ItemsSource = Products;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Eroare", $"A apărut o problemă: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// 🔹 Adaugă produsul în coș
        /// </summary>
        private async void OnAddToCartClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Product product)
            {
                bool success = await _apiService.AddToCartAsync(product.Id, 1);
                if (success)
                    await DisplayAlert("Succes", "Produs adăugat în coș!", "OK");
                else
                    await DisplayAlert("Eroare", "Nu s-a putut adăuga în coș.", "OK");
            }
        }

        /// <summary>
        /// 🔹 Navighează la pagina coșului
        /// </summary>
        private async void OnViewCartClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//CartPage");
        }
    }
}
