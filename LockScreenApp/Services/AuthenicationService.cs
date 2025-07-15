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
                                roleName
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
            var graphQLResponse = await _graphQLClient.SendQueryAsync<SignInGraphNetResponse>(graphQLRequest);

            // Kiểm tra lỗi trước
            if (graphQLResponse.Errors != null && graphQLResponse.Errors.Any())
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = string.Join(", ", graphQLResponse.Errors.Select(e => e.Message))
                };
            }
            var result = graphQLResponse.Data?.signInGraphNet;

            if (result == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "No data returned from server."
                };
            }

            return new LoginResponse
            {
                Success = true,
                Accounts = result.Accounts,
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                IsVerifyOTP = result.IsVerifyOTP
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
