using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using LockScreenApp.LoginModels;

namespace LockScreenApp.Services;
public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthenticationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var endpoint = _configuration["ApiSettings:LoginEndpoint"];
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            
            return result;
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = "An error occurred during login." };
        }
    }
}
