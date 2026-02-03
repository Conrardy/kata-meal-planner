using ErrorOr;
using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Admin;

public sealed record CreateUserCommand(string Username, string Password) : IRequest<ErrorOr<CreateUserResponse>>;

public sealed record CreateUserResponse(Guid UserId, string Username);
