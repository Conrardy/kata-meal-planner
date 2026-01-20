using ErrorOr;

namespace MealPlanner.Application.Auth;

public static class AuthErrors
{
    public static Error InvalidCredentials =>
        Error.Validation("Auth.InvalidCredentials", "Invalid email or password.");

    public static Error EmailAlreadyExists =>
        Error.Conflict("Auth.EmailAlreadyExists", "A user with this email already exists.");

    public static Error InvalidRefreshToken =>
        Error.Validation("Auth.InvalidRefreshToken", "The refresh token is invalid or expired.");

    public static Error UserNotFound =>
        Error.NotFound("Auth.UserNotFound", "User not found.");

    public static Error PasswordRequirementsNotMet(IEnumerable<string> errors) =>
        Error.Validation("Auth.PasswordRequirementsNotMet", string.Join(" ", errors));
}
