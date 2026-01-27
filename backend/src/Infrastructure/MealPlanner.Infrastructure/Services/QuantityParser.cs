using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Infrastructure.Services;

public sealed class QuantityParser : IQuantityParser
{
    public bool TryParse(string quantity, out double result)
    {
        result = 0;

        if (quantity.Contains('/'))
        {
            var parts = quantity.Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var numerator) &&
                double.TryParse(parts[1], out var denominator) &&
                denominator != 0)
            {
                result = numerator / denominator;
                return true;
            }
            return false;
        }

        if (quantity.ToLowerInvariant().Contains("to taste"))
        {
            return false;
        }

        return double.TryParse(quantity, out result);
    }

    public string Format(double quantity)
    {
        if (Math.Abs(quantity - Math.Round(quantity)) < 0.01)
        {
            return ((int)Math.Round(quantity)).ToString();
        }
        return quantity.ToString("0.##");
    }

    public string Combine(string existingQuantity, string existingUnit, string newQuantity, string newUnit)
    {
        if (TryParse(existingQuantity, out var existingNum) &&
            TryParse(newQuantity, out var newNum) &&
            existingUnit == newUnit)
        {
            var totalQuantity = existingNum + newNum;
            return Format(totalQuantity);
        }

        return $"{existingQuantity} + {newQuantity}";
    }
}
