using ErrorOr;

namespace MealPlanner.Application.Admin;

public static class AdminErrors
{
    public static Error UsernameAlreadyExists =>
        Error.Conflict("Admin.UsernameAlreadyExists", "A user with this username already exists.");

    public static Error PasswordRequirementsNotMet(IEnumerable<string> errors) =>
        Error.Validation("Admin.PasswordRequirementsNotMet", string.Join(" ", errors));
}
