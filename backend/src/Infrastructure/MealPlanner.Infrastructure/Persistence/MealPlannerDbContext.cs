using System.Text.Json;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class MealPlannerDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<PlannedMeal> PlannedMeals => Set<PlannedMeal>();
    public DbSet<UserPreferencesEntity> UserPreferences => Set<UserPreferencesEntity>();
    public DbSet<ShoppingListStateEntity> ShoppingListStates => Set<ShoppingListStateEntity>();

    public MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRecipe(modelBuilder);
        ConfigurePlannedMeal(modelBuilder);
        ConfigureUserPreferences(modelBuilder);
        ConfigureShoppingListState(modelBuilder);
    }

    private static void ConfigureRecipe(ModelBuilder modelBuilder)
    {
        var stringListComparer = new ValueComparer<IReadOnlyList<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var ingredientListComparer = new ValueComparer<IReadOnlyList<Ingredient>>(
            (c1, c2) => SerializeForComparison(c1) == SerializeForComparison(c2),
            c => SerializeForComparison(c).GetHashCode(),
            c => DeserializeIngredients(SerializeForComparison(c)));

        var stepsListComparer = new ValueComparer<IReadOnlyList<CookingStep>>(
            (c1, c2) => SerializeForComparison(c1) == SerializeForComparison(c2),
            c => SerializeForComparison(c).GetHashCode(),
            c => DeserializeSteps(SerializeForComparison(c)));

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.ToTable("recipes");
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(r => r.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(r => r.Description).HasColumnName("description").HasMaxLength(1000);

            entity.Property(r => r.MealType)
                .HasColumnName("meal_type")
                .HasMaxLength(20)
                .HasConversion(
                    v => v.Value,
                    v => MealType.FromString(v));

            entity.Property(r => r.Tags)
                .HasColumnName("tags")
                .HasColumnType("text[]")
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList())
                .Metadata.SetValueComparer(stringListComparer);

            entity.Property(r => r.Ingredients)
                .HasColumnName("ingredients")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => SerializeJson(v),
                    v => DeserializeIngredients(v))
                .Metadata.SetValueComparer(ingredientListComparer);

            entity.Property(r => r.Steps)
                .HasColumnName("steps")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => SerializeJson(v),
                    v => DeserializeSteps(v))
                .Metadata.SetValueComparer(stepsListComparer);
        });
    }

    private static void ConfigurePlannedMeal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlannedMeal>(entity =>
        {
            entity.ToTable("planned_meals");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id).HasColumnName("id");
            entity.Property(m => m.Date).HasColumnName("date");
            entity.Property(m => m.RecipeId).HasColumnName("recipe_id");

            entity.Property(m => m.MealType)
                .HasColumnName("meal_type")
                .HasMaxLength(20)
                .HasConversion(
                    v => v.Value,
                    v => MealType.FromString(v));

            entity.HasOne(m => m.Recipe)
                .WithMany()
                .HasForeignKey(m => m.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => new { m.Date, m.MealType }).IsUnique();
        });
    }

    private static void ConfigureUserPreferences(ModelBuilder modelBuilder)
    {
        var stringListComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<UserPreferencesEntity>(entity =>
        {
            entity.ToTable("user_preferences");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.DietaryPreference).HasColumnName("dietary_preference").HasMaxLength(50);
            entity.Property(p => p.MealsPerDay).HasColumnName("meals_per_day");
            entity.Property(p => p.PlanLength).HasColumnName("plan_length");
            entity.Property(p => p.IncludeLeftovers).HasColumnName("include_leftovers");
            entity.Property(p => p.AutoGenerateShoppingList).HasColumnName("auto_generate_shopping_list");

            entity.Property(p => p.Allergies)
                .HasColumnName("allergies")
                .HasColumnType("text[]")
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList())
                .Metadata.SetValueComparer(stringListComparer);

            entity.Property(p => p.ExcludedIngredients)
                .HasColumnName("excluded_ingredients")
                .HasColumnType("text[]")
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList())
                .Metadata.SetValueComparer(stringListComparer);
        });
    }

    private static void ConfigureShoppingListState(ModelBuilder modelBuilder)
    {
        var dictionaryComparer = new ValueComparer<Dictionary<string, bool>>(
            (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count && !c1.Except(c2).Any(),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
            c => new Dictionary<string, bool>(c));

        var customItemsComparer = new ValueComparer<List<CustomItemData>>(
            (c1, c2) => SerializeForComparison(c1) == SerializeForComparison(c2),
            c => SerializeForComparison(c).GetHashCode(),
            c => DeserializeCustomItems(SerializeForComparison(c)));

        modelBuilder.Entity<ShoppingListStateEntity>(entity =>
        {
            entity.ToTable("shopping_list_states");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.StartDate).HasColumnName("start_date");
            entity.Property(s => s.EndDate).HasColumnName("end_date");

            entity.Property(s => s.CheckedItems)
                .HasColumnName("checked_items")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => SerializeJson(v),
                    v => DeserializeDictionary(v))
                .Metadata.SetValueComparer(dictionaryComparer);

            entity.Property(s => s.CustomItems)
                .HasColumnName("custom_items")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => SerializeJson(v),
                    v => DeserializeCustomItems(v))
                .Metadata.SetValueComparer(customItemsComparer);

            entity.HasIndex(s => s.StartDate).IsUnique();
        });
    }

    private static string SerializeJson<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string SerializeForComparison<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static List<Ingredient> DeserializeIngredients(string json) =>
        JsonSerializer.Deserialize<List<Ingredient>>(json, JsonOptions) ?? [];

    private static List<CookingStep> DeserializeSteps(string json) =>
        JsonSerializer.Deserialize<List<CookingStep>>(json, JsonOptions) ?? [];

    private static Dictionary<string, bool> DeserializeDictionary(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, bool>>(json, JsonOptions) ?? new();

    private static List<CustomItemData> DeserializeCustomItems(string json) =>
        JsonSerializer.Deserialize<List<CustomItemData>>(json, JsonOptions) ?? [];
}
