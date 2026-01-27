namespace MealPlanner.Domain.ShoppingList;

public interface IQuantityParser
{
    bool TryParse(string quantity, out double result);
    string Format(double quantity);
    string Combine(string existingQuantity, string existingUnit, string newQuantity, string newUnit);
}
