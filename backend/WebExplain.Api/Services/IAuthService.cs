using WebExplain.Api.DTOs;

namespace WebExplain.Api.Services;

public class EmailAlreadyRegisteredException : Exception;

public class InvalidCredentialsException : Exception;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
