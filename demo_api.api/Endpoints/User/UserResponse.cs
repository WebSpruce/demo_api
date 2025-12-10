namespace demo_api.api.Endpoints.User;

public record UserLoginResponse(string AccessToken, string RefreshToken);