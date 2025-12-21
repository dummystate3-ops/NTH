using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public class RecipeService : IRecipeService
{
    private static readonly string[] CookingMethods =
    {
        "Sauté",
        "Roast",
        "Simmer",
        "Sheet Pan Roast",
        "One-Pot Simmer",
        "Stir-Fry",
        "Pressure Cook",
        "Slow Roast"
    };

    private static readonly string[] Difficulties = { "Easy", "Moderate", "Chef-Level" };

    private readonly IAiService _aiService;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(IAiService aiService, ILogger<RecipeService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RecipeResponse> GenerateAsync(RecipeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ingredients = NormalizeIngredients(request.Ingredients);
        if (ingredients.Count == 0)
        {
            throw new InvalidOperationException("Add at least one ingredient to generate a recipe.");
        }

        // Try AI-powered full recipe generation first
        try
        {
            var aiRecipe = await GenerateAiRecipeAsync(ingredients, request, cancellationToken);
            if (aiRecipe != null)
            {
                _logger.LogInformation("Successfully generated AI-powered recipe: {Title}", aiRecipe.Title);
                return aiRecipe;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI recipe generation failed, falling back to algorithmic generation");
        }

        // Fallback to algorithmic generation
        return GenerateAlgorithmicRecipe(ingredients, request);
    }

    private async Task<RecipeResponse?> GenerateAiRecipeAsync(List<string> ingredients, RecipeRequest request, CancellationToken cancellationToken)
    {
        var dietaryInfo = request.DietaryPreference != "none"
            ? $" The recipe must be {request.DietaryPreference} and include appropriate substitutions."
            : string.Empty;
        var allergyInfo = !string.IsNullOrWhiteSpace(request.Allergies)
            ? $" STRICTLY AVOID these allergens: {request.Allergies}. Offer safe alternatives if necessary."
            : string.Empty;
        var cuisineInfo = !string.IsNullOrWhiteSpace(request.Cuisine)
            ? $" Make it {request.Cuisine} cuisine with authentic flavor cues."
            : string.Empty;
        var timeInfo = request.CookingTimePreference switch
        {
            "quick" => " Keep total cook time under 30 minutes.",
            "medium" => " Aim for a 45 minute cook time.",
            "long" => " Slow cooking instructions (60-90 minutes) are welcome.",
            _ => string.Empty
        };

        var prompt = $@"
Create a DELICIOUS yet practical recipe using these main ingredients: {string.Join(", ", ingredients)}.
Servings: {request.Servings}.{dietaryInfo}{allergyInfo}{cuisineInfo}{timeInfo}

IMPORTANT: Be creative but realistic. Use common cookware, accessible pantry staples, and give precise measurements.

Respond in this EXACT format (use | as separator):
TITLE: [Creative recipe name under 60 characters]
CUISINE: [Specific cuisine type]
DIFFICULTY: [Easy/Moderate/Chef-Level]
PREP_TIME: [realistic minutes]
COOK_TIME: [realistic minutes]
TOTAL_TIME: [PREP_TIME + COOK_TIME]
INGREDIENTS:
- [precise quantity] [ingredient 1] [optional prep note]
- [quantity] [ingredient 2]
(list 6-12 ingredients with accurate measurements)
STEPS:
1. [Clear, actionable step with timing if helpful]
2. [Next step]
(Provide 5-8 numbered steps total)
NUTRITION_PER_SERVING:
Calories: [estimate] | Protein: [number]g | Carbs: [number]g | Fat: [number]g | Fiber: [number]g | Sugar: [number]g | Sodium: [number]mg
CHEF_TIPS: [2-3 professional tips for success, storage, or variations]";

        var aiRequest = new AiDraftRequest
        {
            Prompt = prompt,
            Tone = "professional",
            Length = "medium"
        };

        var response = await _aiService.DraftAsync(aiRequest);
        
        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return null;
        }

        return ParseAiRecipeResponse(response.Content, request);
    }

    private RecipeResponse? ParseAiRecipeResponse(string content, RecipeRequest request)
    {
        try
        {
            var recipe = new RecipeResponse
            {
                Servings = request.Servings,
                DietaryPreference = request.DietaryPreference,
                GeneratedAt = DateTime.UtcNow,
                RecipeId = Guid.NewGuid().ToString("N")[..8]
            };
            recipe.Title = ExtractValue(content, @"TITLE:\s*(.+?)(?:\n|CUISINE:|$)") ?? "Creative Kitchen Creation";
            recipe.Cuisine = ExtractValue(content, @"CUISINE:\s*(.+?)(?:\n|DIFFICULTY:|$)") ?? "Fusion";
            recipe.Difficulty = NormalizeDifficulty(ExtractValue(content, @"DIFFICULTY:\s*(Easy|Moderate|Chef-Level|Hard|Intermediate)") ?? "Moderate");
            recipe.PrepMinutes = ParseTime(ExtractValue(content, @"PREP_TIME:\s*(\d+)"), 15);
            recipe.CookMinutes = ParseTime(ExtractValue(content, @"COOK_TIME:\s*(\d+)"), 25);
            recipe.TotalMinutes = ParseTime(ExtractValue(content, @"TOTAL_TIME:\s*(\d+)"), recipe.PrepMinutes + recipe.CookMinutes);

            recipe.Ingredients = ParseSection(content, "INGREDIENTS:", "STEPS:|NUTRITION|NUTRITION_PER_SERVING|CHEF_TIPS|CHEF_NOTES")
                .Select(line => line.Trim().TrimStart('-', '*', '•', ' '))
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.Length > 2)
                .Take(15)
                .ToList();

            recipe.Steps = ParseSection(content, "STEPS:", "NUTRITION|NUTRITION_PER_SERVING|CHEF_TIPS|CHEF_NOTES")
                .Select(line => Regex.Replace(line, @"^\d+[\.)]\s*", "").Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.Length > 10)
                .Take(12)
                .ToList();

            recipe.Nutrition = ParseNutrition(content) ?? EstimateNutrition(recipe.Ingredients, recipe.Servings, recipe.DietaryPreference ?? "none");
            recipe.Notes = ExtractValue(content, @"CHEF_TIPS:\s*(.+?)(?:\n|$)|CHEF_NOTES:\s*(.+?)(?:\n|$)") ?? "Season to taste and enjoy!";

            recipe.Tags = BuildTags(recipe, request);
            recipe.ImagePrompt = BuildImagePrompt(recipe, request);
            recipe.EquipmentNotes = InferEquipmentNotes(recipe);

            if (recipe.Ingredients.Count < 3 || recipe.Steps.Count < 3)
            {
                _logger.LogWarning("AI response missing critical sections (Ingredients: {Ingredients}, Steps: {Steps})", recipe.Ingredients.Count, recipe.Steps.Count);
                return null;
            }

            return recipe;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI recipe response");
            return null;
        }
    }

    private RecipeResponse GenerateAlgorithmicRecipe(List<string> ingredients, RecipeRequest request)
    {
        var cuisine = PickCuisine(request.Cuisine, ingredients);
        var servings = Math.Clamp(request.Servings, 1, 20);
        var prepMinutes = CalculatePrepTime(ingredients.Count, request.CookingTimePreference);
        var cookMinutes = DetermineCookMinutes(request.CookingTimePreference, cuisine);
        var difficulty = DetermineDifficulty(ingredients.Count, cookMinutes, request.CookingTimePreference);
        var title = GenerateCreativeTitle(ingredients, cuisine, request.DietaryPreference);
        var totalMinutes = prepMinutes + cookMinutes;

        var recipe = new RecipeResponse
        {
            Title = title,
            Cuisine = cuisine,
            Servings = servings,
            DietaryPreference = request.DietaryPreference,
            PrepMinutes = prepMinutes,
            CookMinutes = cookMinutes,
            TotalMinutes = totalMinutes,
            Difficulty = difficulty,
            Ingredients = GenerateIngredientList(ingredients, servings, request.DietaryPreference),
            Steps = GenerateCookingSteps(ingredients, cuisine, cookMinutes, request),
            Nutrition = EstimateNutrition(ingredients, servings, request.DietaryPreference),
            Notes = GenerateHelpfulNotes(ingredients, request),
            IsFallbackRecipe = true,
            GeneratedAt = DateTime.UtcNow,
            RecipeId = Guid.NewGuid().ToString("N")[..8]
        };

        recipe.Tags = BuildTags(recipe, request);
        recipe.ImagePrompt = BuildImagePrompt(recipe, request);
        recipe.EquipmentNotes = InferEquipmentNotes(recipe);

        return recipe;
    }

    private static List<string> NormalizeIngredients(IEnumerable<string> input)
    {
        return input
            .Select(i => i?.Trim())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(i!.ToLowerInvariant()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
    }

    private static string PickCuisine(string? requestedCuisine, IReadOnlyList<string> ingredients)
    {
        if (!string.IsNullOrWhiteSpace(requestedCuisine))
        {
            return requestedCuisine.Trim();
        }

        var first = ingredients.FirstOrDefault() ?? "House";
        return first.Contains("rice", StringComparison.OrdinalIgnoreCase)
            ? "Asian Fusion"
            : first.Contains("pasta", StringComparison.OrdinalIgnoreCase)
                ? "Italian"
                : "Chef's Choice";
    }

    private static int CalculatePrepTime(int ingredientCount, string? preference)
    {
        var baseTime = Math.Clamp((int)Math.Ceiling(ingredientCount * 3.5), 8, 30);
        return preference?.ToLowerInvariant() switch
        {
            "quick" => Math.Min(baseTime, 15),
            "long" => baseTime + 5,
            _ => baseTime
        };
    }

    private static int DetermineCookMinutes(string? preference, string cuisine)
    {
        var normalized = preference?.ToLowerInvariant();
        return normalized switch
        {
            "quick" => 20,
            "medium" => 40,
            "long" => 70,
            _ => cuisine.Contains("roast", StringComparison.OrdinalIgnoreCase) ? 50 : 35
        };
    }

    private static string DetermineDifficulty(int ingredientCount, int cookMinutes, string? preference)
    {
        if (cookMinutes <= 25 && ingredientCount <= 6)
        {
            return "Easy";
        }

        if (cookMinutes >= 60 || ingredientCount >= 12 || string.Equals(preference, "long", StringComparison.OrdinalIgnoreCase))
        {
            return "Chef-Level";
        }

        return "Moderate";
    }

    private static string GenerateCreativeTitle(List<string> ingredients, string cuisine, string dietaryPreference)
    {
        var hero = ingredients.FirstOrDefault() ?? "Harvest";
        var methods = new[] { "Sauté", "Roast", "Simmer", "Sheet Pan", "Stir-Fry", "Bake" };
        var adjectives = new[] { "Crispy", "Silky", "Zesty", "Herbed", "Smoky", "Bright" };
        var random = new Random(HashCode.Combine(hero, cuisine));
        var adjective = dietaryPreference != "none" ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dietaryPreference) : adjectives[random.Next(adjectives.Length)];
        var method = methods[random.Next(methods.Length)];
        return $"{adjective} {cuisine} {hero} {method}".Trim();
    }

    private static List<string> GenerateIngredientList(List<string> ingredients, int servings, string dietaryPreference)
    {
        var measures = new[] { "cup", "tbsp", "tsp", "clove", "oz", "slice" };
        var rng = new Random(HashCode.Combine(servings, ingredients.Count));
        var lines = new List<string>();

        foreach (var ingredient in ingredients.Take(12))
        {
            var quantity = Math.Round(rng.NextDouble() * 1.2 + 0.5, 2);
            var measure = measures[rng.Next(measures.Length)];
            var plural = quantity > 1 && !measure.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "s" : string.Empty;
            lines.Add($"{quantity:0.##} {measure}{plural} {ingredient}");
        }

        lines.Add("Sea salt and cracked pepper to taste");
        lines.Add(dietaryPreference switch
        {
            "vegan" => "2 tbsp extra-virgin olive oil",
            "vegetarian" => "2 tbsp butter or olive oil",
            "keto" => "1 tbsp ghee or avocado oil",
            _ => "1 tbsp finishing oil or butter"
        });

        return lines;
    }

    private static List<string> GenerateCookingSteps(List<string> ingredients, string cuisine, int cookMinutes, RecipeRequest request)
    {
        var hero = ingredients.FirstOrDefault() ?? "ingredients";
        var steps = new List<string>
        {
            $"Prep: Rinse and pat dry all produce. Dice aromatics finely and cut {hero} into even pieces for consistent cooking.",
            $"Flavor base: Warm a heavy skillet with oil, then bloom garlic, onion, and {cuisine} spices until fragrant.",
            $"Build: Layer hearty items first, then fold in tender vegetables/proteins. Deglaze with stock or citrus.",
            $"Simmer: Cover and cook for {cookMinutes - 10} minutes, stirring occasionally so nothing sticks.",
            "Finish: Adjust seasoning, add fresh herbs, and rest the dish for 2 minutes before serving.",
            "Serve: Spoon into warm bowls or plates and garnish with reserved herbs or crunchy toppings."
        };

        if (!string.IsNullOrWhiteSpace(request.Allergies))
        {
            steps.Add($"Allergy note: Swap or omit any ingredients that conflict with {request.Allergies}.");
        }

        if (!string.Equals(request.DietaryPreference, "none", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add($"Dietary tip: Keep it {request.DietaryPreference} by choosing appropriate broth and fats.");
        }

        return steps;
    }

    private static string GenerateHelpfulNotes(List<string> ingredients, RecipeRequest request)
    {
        var hero = ingredients.FirstOrDefault() ?? "the main ingredient";
        var dietary = request.DietaryPreference != "none" ? $" Keep it {request.DietaryPreference} by using compliant stock and toppings." : string.Empty;
        var allergy = !string.IsNullOrWhiteSpace(request.Allergies) ? $" Avoid {request.Allergies} by choosing safe substitutes." : string.Empty;
        return $"Finish with fresh citrus or herbs over {hero} for brightness.{dietary}{allergy}".Trim();
    }

    private static string? ExtractValue(string content, string pattern)
    {
        var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!match.Success)
        {
            return null;
        }

        for (var i = 1; i < match.Groups.Count; i++)
        {
            if (match.Groups[i].Success && !string.IsNullOrWhiteSpace(match.Groups[i].Value))
            {
                return match.Groups[i].Value.Trim();
            }
        }

        return null;
    }

    private static List<string> ParseSection(string content, string startMarker, string endMarkerPattern)
    {
        var pattern = $@"{startMarker}\s*(.+?)(?:(?={endMarkerPattern})|$)";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new List<string>();
        }

        return match.Groups[1].Value
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    private static string NormalizeDifficulty(string value)
    {
        var normalized = value.ToLowerInvariant() switch
        {
            "easy" => "Easy",
            "moderate" => "Moderate",
            "chef-level" => "Chef-Level",
            "hard" => "Chef-Level",
            "intermediate" => "Moderate",
            _ => "Moderate"
        };
        return normalized;
    }

    private static int ParseTime(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }

    private static RecipeNutrition? ParseNutrition(string content)
    {
        var calories = ExtractValue(content, @"Calories:\s*(\d+)");
        var protein = ExtractValue(content, @"Protein:\s*(\d+)");
        var carbs = ExtractValue(content, @"Carbs:\s*(\d+)");
        var fat = ExtractValue(content, @"Fat:\s*(\d+)");

        if (calories == null || protein == null || carbs == null || fat == null)
        {
            return null;
        }

        return new RecipeNutrition
        {
            Calories = ParseTime(calories, 350),
            ProteinGrams = ParseTime(protein, 20),
            CarbsGrams = ParseTime(carbs, 30),
            FatGrams = ParseTime(fat, 15),
            FiberGrams = ParseTime(ExtractValue(content, @"Fiber:\s*(\d+)") ?? "4", 4),
            SugarGrams = ParseTime(ExtractValue(content, @"Sugar:\s*(\d+)") ?? "6", 6),
            SodiumMilligrams = ParseTime(ExtractValue(content, @"Sodium:\s*(\d+)") ?? "520", 520)
        };
    }

    private static List<string> BuildTags(RecipeResponse recipe, RecipeRequest request)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(recipe.Cuisine)) tags.Add(recipe.Cuisine);
        if (!string.IsNullOrWhiteSpace(request.DietaryPreference) && !"none".Equals(request.DietaryPreference, StringComparison.OrdinalIgnoreCase))
            tags.Add(request.DietaryPreference);
        tags.Add(recipe.Difficulty);

        if (!string.IsNullOrWhiteSpace(request.CookingTimePreference) && !request.CookingTimePreference.Equals("any", StringComparison.OrdinalIgnoreCase))
        {
            tags.Add(request.CookingTimePreference + " cook time");
        }

        if (request.Ingredients.Count > 0)
        {
            tags.Add(request.Ingredients.First());
        }

        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t))
            .ToList();
    }

    private static string BuildImagePrompt(RecipeResponse recipe, RecipeRequest request)
    {
        var hero = request.Ingredients.FirstOrDefault() ?? "seasonal produce";
        var cuisine = string.IsNullOrWhiteSpace(recipe.Cuisine) ? "modern" : recipe.Cuisine;
        return $"{cuisine} style plated dish highlighting {hero}, soft natural lighting, editorial food photography, 50mm lens";
    }

    private static string? InferEquipmentNotes(RecipeResponse recipe)
    {
        var combined = string.Join(' ', recipe.Steps).ToLowerInvariant();
        var equipment = new List<string>();
        if (combined.Contains("sheet")) equipment.Add("sheet pan");
        if (combined.Contains("oven")) equipment.Add("oven-safe pan");
        if (combined.Contains("grill")) equipment.Add("grill or grill pan");
        if (combined.Contains("blender")) equipment.Add("blender");
        if (combined.Contains("skillet")) equipment.Add("skillet");

        if (equipment.Count == 0)
        {
            return null;
        }

        var unique = equipment.Distinct().ToList();
        return $"Recommended equipment: {string.Join(", ", unique)}.";
    }

    private static RecipeNutrition EstimateNutrition(IReadOnlyList<string> ingredients, int servings, string dietaryPreference)
    {
        var baseCalories = 320 + ingredients.Count * 18;
        if (string.Equals(dietaryPreference, "keto", StringComparison.OrdinalIgnoreCase))
        {
            baseCalories += 60;
        }

        return new RecipeNutrition
        {
            Calories = Math.Max(250, baseCalories / Math.Max(servings, 1)),
            ProteinGrams = 12 + ingredients.Count,
            CarbsGrams = Math.Max(10, 35 - (dietaryPreference.Equals("keto", StringComparison.OrdinalIgnoreCase) ? 15 : 0)),
            FatGrams = 10 + ingredients.Count / 2,
            FiberGrams = 4 + Math.Max(1, ingredients.Count / 3),
            SugarGrams = 6 + Math.Max(0, ingredients.Count / 4),
            SodiumMilligrams = 450 + ingredients.Count * 25
        };
    }
}
