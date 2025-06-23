using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Services;
using FareCalculator.Visualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FareCalculator.Visualization;

/// <summary>
/// Demo program to showcase metro system visualization capabilities.
/// </summary>
public class VisualizationDemo
{
    /// <summary>
    /// Runs the visualization demo.
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a host with all the necessary services
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configure options
                services.Configure<FareCalculationOptions>(
                    context.Configuration.GetSection(FareCalculationOptions.SectionName));
                services.Configure<GeographyOptions>(
                    context.Configuration.GetSection(GeographyOptions.SectionName));
                services.Configure<List<FareCalculator.Models.Station>>(
                    context.Configuration.GetSection(StationOptions.SectionName));
                services.Configure<MetroLineOptions>(
                    context.Configuration.GetSection(MetroLineOptions.SectionName));

                // Register services
                services.AddScoped<IStationService, StationService>();
                services.AddScoped<IMetroLineService, MetroLineService>();
                services.AddScoped<MetroMapGenerator>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        // Get the map generator service
        var generator = host.Services.GetRequiredService<MetroMapGenerator>();
        
        Console.WriteLine("=== Metro System Visualization Demo ===\n");
        
        // Generate all visualization types
        await DemoMermaidDiagram(generator);
        await DemoAsciiMap(generator);
        await DemoFareExplanation(generator);
        
        Console.WriteLine("\n=== Demo Complete ===");
        Console.WriteLine("Files have been generated in the 'docs/generated' directory.");
        Console.WriteLine("You can copy these into your documentation.");
    }

    private static async Task DemoMermaidDiagram(MetroMapGenerator generator)
    {
        Console.WriteLine("1. Generating Mermaid Diagram for Documentation...\n");
        
        var mermaidDiagram = await generator.GenerateMermaidDiagramAsync();
        
        // Save to file
        Directory.CreateDirectory("docs/generated");
        await File.WriteAllTextAsync("docs/generated/metro-system-map.md", mermaidDiagram);
        
        Console.WriteLine("✓ Mermaid diagram generated!");
        Console.WriteLine("  → Saved to: docs/generated/metro-system-map.md");
        Console.WriteLine("  → This can be embedded in GitHub/GitLab markdown documentation");
        Console.WriteLine();
    }

    private static async Task DemoAsciiMap(MetroMapGenerator generator)
    {
        Console.WriteLine("2. Generating ASCII Map for Text Documentation...\n");
        
        var asciiMap = await generator.GenerateAsciiMapAsync();
        
        // Save to file
        await File.WriteAllTextAsync("docs/generated/metro-system-ascii.txt", asciiMap);
        
        Console.WriteLine("✓ ASCII map generated!");
        Console.WriteLine("  → Saved to: docs/generated/metro-system-ascii.txt");
        Console.WriteLine("  → This can be included in README files or plain text docs");
        Console.WriteLine();
        
        // Show a preview
        Console.WriteLine("Preview:");
        Console.WriteLine("--------");
        var lines = asciiMap.Split('\n');
        for (int i = 0; i < Math.Min(15, lines.Length); i++)
        {
            Console.WriteLine(lines[i]);
        }
        if (lines.Length > 15)
        {
            Console.WriteLine("... (truncated for demo)");
        }
        Console.WriteLine();
    }

    private static async Task DemoFareExplanation(MetroMapGenerator generator)
    {
        Console.WriteLine("3. Generating Fare Structure Documentation...\n");
        
        var fareExplanation = await generator.GenerateFareExplanationAsync();
        
        // Save to file
        await File.WriteAllTextAsync("docs/generated/fare-structure.txt", fareExplanation);
        
        Console.WriteLine("✓ Fare explanation generated!");
        Console.WriteLine("  → Saved to: docs/generated/fare-structure.txt");
        Console.WriteLine("  → This explains the complete fare calculation logic");
        Console.WriteLine();
        
        // Show a preview
        Console.WriteLine("Preview:");
        Console.WriteLine("--------");
        var lines = fareExplanation.Split('\n');
        for (int i = 0; i < Math.Min(20, lines.Length); i++)
        {
            Console.WriteLine(lines[i]);
        }
        if (lines.Length > 20)
        {
            Console.WriteLine("... (truncated for demo)");
        }
        Console.WriteLine();
    }
} 