namespace MealPlanner.Domain.Auth;

public interface ISeededUserRepository
{
    SeededUser? FindByUsername(string username);
    bool ValidatePassword(string username, string password);
    IReadOnlyList<SeededUser> GetAllUsers();
}
