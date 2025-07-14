using GraphQL;
using GraphQL.Client.Http;
using LockScreenApp.LoginModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

namespace LockScreenApp.Services;
public class AuthenticationService
{
    private readonly GraphQLHttpClient _graphQLClient;

    public AuthenticationService(GraphQLHttpClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var graphQLRequest = new GraphQLRequest
            {
                Query = @"
                    mutation signIn($username: String, $password: String) {
                        signInGraphNet(username: $username, password: $password) {
                            accounts {
                                id
                                fullName
                            }
                            token
                            refreshToken
                            isVerifyOTP
                        }
                    }",
                Variables = new
                {
                    username = request.Username,
                    password = request.Password
                }
            };
            var response = await _graphQLClient.SendQueryAsync<SignInGraphNetResponse>(graphQLRequest);

            if (response.Errors != null && response.Errors.Any())
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = string.Join(", ", response.Errors.Select(e => e.Message))
                };
            }
            var signInData = response.Data;
            return new LoginResponse
            {
                Success = true,
                Accounts = signInData.Accounts,
                Token = signInData.Token,
                RefreshToken = signInData.RefreshToken,
                IsVerifyOTP = signInData.IsVerifyOTP
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login: " + ex.Message
            };
        }
    }
}
