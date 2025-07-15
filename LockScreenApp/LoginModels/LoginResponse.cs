namespace LockScreenApp.LoginModels;
public class GraphQLResponse<T>
{
    public T Data { get; set; }
    public GraphQLError[] Errors { get; set; }
}

public class GraphQLError
{
    public string Message { get; set; }
}

public class SignInGraphNetResponse
{
    public Account Accounts { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool IsVerifyOTP { get; set; }
}

public class Account
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone {  get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; }
    public Account Accounts { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool IsVerifyOTP { get; set; }
}