using PizzAppMobile.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace PizzAppMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://localhost:7107/api/";

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        private StringContent GetStringContent(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        /// 🔹 **Autentificare utilizator și obținere token JWT**
        public async Task<string> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var response = await _httpClient.PostAsync($"{BASE_URL}auth/login", GetStringContent(loginData));

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Autentificare eșuată: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrEmpty(result?.Token))
                {
                    await SecureStorage.SetAsync("jwt_token", result.Token);
                    Console.WriteLine("✅ Autentificare reușită!");
                    return result.Token;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la autentificare: {ex.Message}");
                return null;
            }
        }

        /// 🔹 **Obține token-ul JWT stocat**
        private async Task<string> GetTokenAsync()
        {
            return await SecureStorage.GetAsync("jwt_token");
        }

        /// 🔹 **Obține lista de produse din API**
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}products");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Eroare la încărcarea produselor: {response.StatusCode}");
                    return new List<Product>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la încărcarea produselor: {ex.Message}");
                return new List<Product>();
            }
        }

        /// 🔹 **Adaugă un produs în coș**
        public async Task<bool> AddToCartAsync(int productId, int quantity)
        {
            try
            {
                string jwtToken = await GetTokenAsync();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    Console.WriteLine("⚠️ Utilizator neautentificat. Nu se poate adăuga produs în coș.");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var cartItem = new { ProductId = productId, Quantity = quantity };
                var response = await _httpClient.PostAsync($"{BASE_URL}cart", GetStringContent(cartItem));

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Produs adăugat în coș.");
                    return true;
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Eroare API la adăugarea în coș: {response.StatusCode} - {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la adăugarea în coș: {ex.Message}");
                return false;
            }
        }

        /// 🔹 **Obține conținutul coșului utilizatorului**
        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            try
            {
                string jwtToken = await GetTokenAsync();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    Console.WriteLine("⚠️ Utilizator neautentificat. Nu se poate accesa coșul.");
                    return new List<CartItem>();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.GetAsync($"{BASE_URL}cart");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Eroare API la obținerea coșului: {response.StatusCode} - {errorContent}");
                    return new List<CartItem>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ JSON returnat de API: {json}");

                return JsonSerializer.Deserialize<List<CartItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CartItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la încărcarea coșului: {ex.Message}");
                return new List<CartItem>();
            }
        }

        /// 🔹 **Elimină un produs din coș**
        public async Task<bool> RemoveFromCartAsync(int productId)
        {
            try
            {
                string jwtToken = await GetTokenAsync();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    Console.WriteLine("⚠️ Utilizator neautentificat. Nu se poate elimina produs din coș.");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.DeleteAsync($"{BASE_URL}cart/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Produs eliminat din coș.");
                    return true;
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Eroare API la eliminarea din coș: {response.StatusCode} - {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la ștergerea din coș: {ex.Message}");
                return false;
            }
        }

        /// 🔹 **Plasează o comandă**
        public async Task<bool> PlaceOrderAsync()
        {
            try
            {
                string jwtToken = await GetTokenAsync();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    Console.WriteLine("⚠️ Utilizator neautentificat. Nu se poate plasa comanda.");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.PostAsync($"{BASE_URL}orders/place", null);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Comandă plasată cu succes.");
                    return true;
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Eroare API la plasarea comenzii: {response.StatusCode} - {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la plasarea comenzii: {ex.Message}");
                return false;
            }
        }

        private class LoginResponse
        {
            public string Token { get; set; }
        }
    }
}
