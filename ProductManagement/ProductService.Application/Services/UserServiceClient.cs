using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsUserValidAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/Users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return false;
        }
    }


}
