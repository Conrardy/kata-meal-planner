using System.Text;

namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Generates documentation skeleton markdown for undocumented endpoints.
/// </summary>
public sealed class DocumentationSkeletonGenerator
{
    /// <summary>
    /// Generates a markdown skeleton for undocumented endpoints.
    /// </summary>
    /// <param name="undocumentedEndpoints">List of undocumented endpoints.</param>
    /// <returns>Markdown content for the documentation skeleton.</returns>
    public string Generate(IReadOnlyList<EndpointInfo> undocumentedEndpoints)
    {
        if (undocumentedEndpoints.Count == 0)
        {
            return "No undocumented endpoints found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("# Documentation Skeleton for New Endpoints");
        sb.AppendLine();
        sb.AppendLine("The following endpoints need documentation. Copy and customize each section into `endpoints.md`.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var endpoint in undocumentedEndpoints)
        {
            sb.AppendLine(GenerateEndpointSkeleton(endpoint));
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a single endpoint documentation skeleton.
    /// </summary>
    private static string GenerateEndpointSkeleton(EndpointInfo endpoint)
    {
        var hasPathParams = endpoint.RoutePath.Contains('{');
        var isAuthRequired = endpoint.RoutePath.StartsWith("/api/v1/") &&
                            !endpoint.RoutePath.Contains("/auth/");

        var sb = new StringBuilder();
        sb.AppendLine($"### {endpoint.HttpMethod} `{endpoint.RoutePath}` - [Description]");
        sb.AppendLine();
        sb.AppendLine("[Brief description of what this endpoint does.]");
        sb.AppendLine();
        sb.AppendLine($"**Authentification** : {(isAuthRequired ? "Bearer JWT" : "Aucune")}");
        sb.AppendLine();

        // Path parameters section
        if (hasPathParams)
        {
            sb.AppendLine("**Paramètres de chemin** :");
            sb.AppendLine();
            sb.AppendLine("| Paramètre | Type | Description |");
            sb.AppendLine("|-----------|------|-------------|");
            sb.AppendLine("| `[param]` | `[Type]` | [Description] |");
            sb.AppendLine();
        }

        // Request body for POST/PUT/PATCH
        if (endpoint.HttpMethod is "POST" or "PUT" or "PATCH")
        {
            sb.AppendLine("**Corps de la requête** :");
            sb.AppendLine();
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"property\": \"value\"");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        // Response section
        var successCode = endpoint.HttpMethod switch
        {
            "POST" => "201 Created",
            "DELETE" => "204 No Content",
            "PATCH" => "204 No Content",
            _ => "200 OK"
        };

        sb.AppendLine($"**Réponse** `{successCode}` :");
        sb.AppendLine();

        if (!successCode.Contains("204"))
        {
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"property\": \"value\"");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        // Error codes table
        sb.AppendLine("**Codes d'erreur** :");
        sb.AppendLine();
        sb.AppendLine("| Code | Description |");
        sb.AppendLine("|------|-------------|");

        if (successCode.Contains("200"))
            sb.AppendLine("| `200` | Succès |");
        if (successCode.Contains("201"))
            sb.AppendLine("| `201` | Ressource créée |");
        if (successCode.Contains("204"))
            sb.AppendLine("| `204` | Opération réussie |");

        sb.AppendLine("| `400` | Erreur de validation |");

        if (isAuthRequired)
            sb.AppendLine("| `401` | Token manquant ou invalide |");

        if (hasPathParams)
            sb.AppendLine("| `404` | Ressource non trouvée |");

        sb.AppendLine();

        // Example curl command
        sb.AppendLine("```bash");
        var curlMethod = endpoint.HttpMethod != "GET" ? $"-X {endpoint.HttpMethod} " : "";
        var curlAuth = isAuthRequired ? "\n  -H \"Authorization: Bearer <token>\"" : "";
        var curlBody = endpoint.HttpMethod is "POST" or "PUT" or "PATCH"
            ? "\n  -H \"Content-Type: application/json\" \\\n  -d '{\"property\": \"value\"}'"
            : "";
        sb.AppendLine($"curl {curlMethod}http://localhost:5000{endpoint.RoutePath} \\{curlAuth}{curlBody}");
        sb.AppendLine("```");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates skeleton and writes it to a file.
    /// </summary>
    /// <param name="undocumentedEndpoints">List of undocumented endpoints.</param>
    /// <param name="outputPath">Path to write the skeleton file.</param>
    public void GenerateToFile(IReadOnlyList<EndpointInfo> undocumentedEndpoints, string outputPath)
    {
        var content = Generate(undocumentedEndpoints);
        File.WriteAllText(outputPath, content);
    }
}
