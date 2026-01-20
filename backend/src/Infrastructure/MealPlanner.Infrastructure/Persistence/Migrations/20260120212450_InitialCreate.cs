using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    meal_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ingredients = table.Column<string>(type: "jsonb", nullable: false),
                    steps = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shopping_list_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    checked_items = table.Column<string>(type: "jsonb", nullable: false),
                    custom_items = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_list_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dietary_preference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    allergies = table.Column<string[]>(type: "text[]", nullable: false),
                    meals_per_day = table.Column<int>(type: "integer", nullable: false),
                    plan_length = table.Column<int>(type: "integer", nullable: false),
                    include_leftovers = table.Column<bool>(type: "boolean", nullable: false),
                    auto_generate_shopping_list = table.Column<bool>(type: "boolean", nullable: false),
                    excluded_ingredients = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "planned_meals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    meal_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_meals", x => x.id);
                    table.ForeignKey(
                        name: "FK_planned_meals_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_planned_meals_date_meal_type",
                table: "planned_meals",
                columns: new[] { "date", "meal_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planned_meals_recipe_id",
                table: "planned_meals",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_list_states_start_date",
                table: "shopping_list_states",
                column: "start_date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planned_meals");

            migrationBuilder.DropTable(
                name: "shopping_list_states");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "recipes");
        }
    }
}
