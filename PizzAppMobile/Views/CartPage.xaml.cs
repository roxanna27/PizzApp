using PizzAppMobile.Models;
using PizzAppMobile.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzAppMobile.Views
{
    public partial class CartPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public CartPage()
        {
            InitializeComponent();
            BindingContext = this; // 🔹 Legăm pagina de date
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCartItems();
        }

        /// <summary>
        /// 🔹 Încarcă produsele din coș de la API și actualizează UI-ul.
        /// </summary>
        private async Task LoadCartItems()
        {
            try
            {
                Console.WriteLine("🔍 Se încearcă încărcarea coșului...");

                CartItems = await _apiService.GetCartItemsAsync();
                if (CartItems == null || CartItems.Count == 0)
                {
                    Console.WriteLine("⚠️ Coșul este gol.");
                    CartListView.ItemsSource = null;
                    return;
                }

                Console.WriteLine($"✅ Coș încărcat cu {CartItems.Count} produse.");
                foreach (var item in CartItems)
                {
                    Console.WriteLine($"🛒 Produs: {item.Product.Name} | Cantitate: {item.Quantity} | Preț: {item.Product.Price}");
                }

                CartListView.ItemsSource = null; // Reset UI
                await Task.Delay(100); // Pauză pentru refresh
                CartListView.ItemsSource = CartItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la încărcarea coșului: {ex.Message}");
                await DisplayAlert("Eroare", "Nu s-au putut încărca produsele din coș.", "OK");
            }
        }

        /// <summary>
        /// 🔹 Elimină produsul selectat din coș.
        /// </summary>
        private async void OnRemoveFromCartClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CartItem cartItem)
            {
                bool success = await _apiService.RemoveFromCartAsync(cartItem.ProductId);
                if (success)
                {
                    await DisplayAlert("Succes", "Produs eliminat din coș!", "OK");
                    await LoadCartItems();
                }
                else
                {
                    await DisplayAlert("Eroare", "Nu s-a putut elimina produsul.", "OK");
                }
            }
        }

        /// <summary>
        /// 🔹 Revine la meniul de produse.
        /// </summary>
        private async void OnGoBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ProductsPage");
        }

        /// <summary>
        /// 🔹 Plasează comanda și golește coșul.
        /// </summary>
        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            if (CartItems == null || CartItems.Count == 0)
            {
                await DisplayAlert("Eroare", "Coșul este gol!", "OK");
                return;
            }

            bool orderPlaced = await _apiService.PlaceOrderAsync();
            if (orderPlaced)
            {
                await DisplayAlert("Succes", "Comanda a fost plasată!", "OK");
                await LoadCartItems();
                await Shell.Current.GoToAsync("//ProductsPage");
            }
            else
            {
                await DisplayAlert("Eroare", "Nu s-a putut plasa comanda!", "OK");
            }
        }
    }
}
