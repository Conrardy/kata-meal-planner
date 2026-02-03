using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Admin;

public sealed record GetUsersQuery : IRequest<GetUsersResponse>;

public sealed record GetUsersResponse(IReadOnlyList<UserDto> Users);

public sealed record UserDto(Guid Id, string Username, bool IsAdmin);
