using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FareCalculator.Visualization;

/// <summary>
/// Console application for generating metro system visualizations.
/// </summary>
public class MapGeneratorApp
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MapGeneratorApp> _logger;

    public MapGeneratorApp(IServiceProvider serviceProvider, ILogger<MapGeneratorApp> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the map generator application.
    /// </summary>
    public async Task RunAsync(string[] args)
    {
        _logger.LogInformation("Metro Map Generator started");

        try
        {
            var generator = _serviceProvider.GetRequiredService<MetroMapGenerator>();

            if (args.Length == 0)
            {
                await ShowMenuAsync(generator);
            }
            else
            {
                await ProcessCommandLineArgs(generator, args);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating metro map visualization");
            Console.WriteLine($"Error: {ex.Message}");
        }

        _logger.LogInformation("Metro Map Generator finished");
    }

    /// <summary>
    /// Shows interactive menu for generating visualizations.
    /// </summary>
    private async Task ShowMenuAsync(MetroMapGenerator generator)
    {
        while (true)
        {
            Console.WriteLine("\n=== Metro Map Generator ===");
            Console.WriteLine("1. Generate Mermaid Diagram");
            Console.WriteLine("2. Generate ASCII Map");
            Console.WriteLine("3. Generate Fare Explanation");
            Console.WriteLine("4. Generate All (save to files)");
            Console.WriteLine("5. Exit");
            Console.Write("\nSelect option (1-5): ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await GenerateAndDisplayMermaid(generator);
                    break;
                case "2":
                    await GenerateAndDisplayAscii(generator);
                    break;
                case "3":
                    await GenerateAndDisplayFareExplanation(generator);
                    break;
                case "4":
                    await GenerateAllToFiles(generator);
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Processes command line arguments for batch generation.
    /// </summary>
    private async Task ProcessCommandLineArgs(MetroMapGenerator generator, string[] args)
    {
        var format = args[0].ToLower();
        var outputFile = args.Length > 1 ? args[1] : null;

        switch (format)
        {
            case "mermaid":
                var mermaidOutput = await generator.GenerateMermaidDiagramAsync();
                await OutputResult(mermaidOutput, outputFile, "metro-map.md");
                break;

            case "ascii":
                var asciiOutput = await generator.GenerateAsciiMapAsync();
                await OutputResult(asciiOutput, outputFile, "metro-map.txt");
                break;

            case "fare":
                var fareOutput = await generator.GenerateFareExplanationAsync();
                await OutputResult(fareOutput, outputFile, "fare-guide.txt");
                break;

            case "all":
                await GenerateAllToFiles(generator, outputFile);
                break;

            default:
                Console.WriteLine("Usage: dotnet run [mermaid|ascii|fare|all] [output-directory]");
                Console.WriteLine("  mermaid - Generate Mermaid diagram");
                Console.WriteLine("  ascii   - Generate ASCII map");
                Console.WriteLine("  fare    - Generate fare explanation");
                Console.WriteLine("  all     - Generate all formats");
                break;
        }
    }

    private async Task GenerateAndDisplayMermaid(MetroMapGenerator generator)
    {
        Console.WriteLine("\nGenerating Mermaid Diagram...\n");
        var result = await generator.GenerateMermaidDiagramAsync();
        Console.WriteLine(result);
    }

    private async Task GenerateAndDisplayAscii(MetroMapGenerator generator)
    {
        Console.WriteLine("\nGenerating ASCII Map...\n");
        var result = await generator.GenerateAsciiMapAsync();
        Console.WriteLine(result);
    }

    private async Task GenerateAndDisplayFareExplanation(MetroMapGenerator generator)
    {
        Console.WriteLine("\nGenerating Fare Explanation...\n");
        var result = await generator.GenerateFareExplanationAsync();
        Console.WriteLine(result);
    }

    private async Task GenerateAllToFiles(MetroMapGenerator generator, string? outputDir = null)
    {
        outputDir ??= "docs/generated";
        Directory.CreateDirectory(outputDir);

        Console.WriteLine($"\nGenerating all visualizations to {outputDir}...");

        // Generate Mermaid diagram
        var mermaidOutput = await generator.GenerateMermaidDiagramAsync();
        var mermaidFile = Path.Combine(outputDir, "metro-map.md");
        await File.WriteAllTextAsync(mermaidFile, mermaidOutput);
        Console.WriteLine($"✓ Mermaid diagram saved to {mermaidFile}");

        // Generate ASCII map
        var asciiOutput = await generator.GenerateAsciiMapAsync();
        var asciiFile = Path.Combine(outputDir, "metro-map.txt");
        await File.WriteAllTextAsync(asciiFile, asciiOutput);
        Console.WriteLine($"✓ ASCII map saved to {asciiFile}");

        // Generate fare explanation
        var fareOutput = await generator.GenerateFareExplanationAsync();
        var fareFile = Path.Combine(outputDir, "fare-guide.txt");
        await File.WriteAllTextAsync(fareFile, fareOutput);
        Console.WriteLine($"✓ Fare guide saved to {fareFile}");

        // Generate combined documentation
        var combinedOutput = await GenerateCombinedDocumentation(generator);
        var combinedFile = Path.Combine(outputDir, "metro-system-documentation.md");
        await File.WriteAllTextAsync(combinedFile, combinedOutput);
        Console.WriteLine($"✓ Combined documentation saved to {combinedFile}");

        Console.WriteLine($"\nAll files generated successfully in {outputDir}");
    }

    private async Task<string> GenerateCombinedDocumentation(MetroMapGenerator generator)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("# Metro System Documentation");
        sb.AppendLine();
        sb.AppendLine("This document provides a comprehensive overview of the metro system including");
        sb.AppendLine("network topology, fare structure, and operational information.");
        sb.AppendLine();
        
        sb.AppendLine("## System Map");
        sb.AppendLine();
        sb.AppendLine(await generator.GenerateMermaidDiagramAsync());
        sb.AppendLine();
        
        sb.AppendLine("## Network Overview");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(await generator.GenerateAsciiMapAsync());
        sb.AppendLine("```");
        sb.AppendLine();
        
        sb.AppendLine("## Fare Information");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(await generator.GenerateFareExplanationAsync());
        sb.AppendLine("```");
        sb.AppendLine();
        
        sb.AppendLine("---");
        sb.AppendLine($"*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        
        return sb.ToString();
    }

    private async Task OutputResult(string content, string? outputFile, string defaultFileName)
    {
        if (outputFile != null)
        {
            var directory = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await File.WriteAllTextAsync(outputFile, content);
            Console.WriteLine($"Output saved to {outputFile}");
        }
        else
        {
            Console.WriteLine(content);
        }
    }
} 