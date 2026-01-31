namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Represents a void type, used for handlers that don't return a meaningful value.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    /// <summary>
    /// Gets the single value of the Unit type.
    /// </summary>
    public static readonly Unit Value = new();

    public bool Equals(Unit other) => true;

    public override bool Equals(object? obj) => obj is Unit;

    public override int GetHashCode() => 0;

    public override string ToString() => "()";

    public int CompareTo(Unit other) => 0;

    public int CompareTo(object? obj) => 0;

    public static bool operator ==(Unit left, Unit right) => true;

    public static bool operator !=(Unit left, Unit right) => false;
}
