using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace FareCalculator.Visualization;

/// <summary>
/// Generates visual representations of the metro system for documentation purposes.
/// </summary>
public class MetroMapGenerator
{
    private readonly ILogger<MetroMapGenerator> _logger;
    private readonly IStationService _stationService;
    private readonly IMetroLineService _metroLineService;
    private readonly FareCalculationOptions _fareOptions;
    private readonly MetroLineOptions _metroLineOptions;

    public MetroMapGenerator(
        ILogger<MetroMapGenerator> logger,
        IStationService stationService,
        IMetroLineService metroLineService,
        IOptions<FareCalculationOptions> fareOptions,
        IOptions<MetroLineOptions> metroLineOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stationService = stationService ?? throw new ArgumentNullException(nameof(stationService));
        _metroLineService = metroLineService ?? throw new ArgumentNullException(nameof(metroLineService));
        _fareOptions = fareOptions?.Value ?? throw new ArgumentNullException(nameof(fareOptions));
        _metroLineOptions = metroLineOptions?.Value ?? throw new ArgumentNullException(nameof(metroLineOptions));
    }

    /// <summary>
    /// Generates a Mermaid diagram representation of the metro system.
    /// </summary>
    public async Task<string> GenerateMermaidDiagramAsync()
    {
        var stations = (await _stationService.GetAllStationsAsync()).ToList();
        var metroLines = (await _metroLineService.GetAllMetroLinesAsync()).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart TD");
        sb.AppendLine("    %% Metro System Map - Concentric Zone Layout");
        sb.AppendLine();

        // Create concentric zone subgraphs
        sb.AppendLine("    %% Concentric Zone Layout");
        sb.AppendLine("    subgraph ZoneC [\"🟠 Zone C - Outer Ring\"]");
        sb.AppendLine("        subgraph ZoneB [\"🟢 Zone B - Middle Ring\"]");
        sb.AppendLine("            subgraph ZoneA [\"🔵 Zone A - Central\"]");
        
        // Zone A stations
        var zoneAStations = stations.Where(s => s.Zone == "A").ToList();
        foreach (var station in zoneAStations)
        {
            var stationName = station.Name.Replace(" ", "<br/>");
            sb.AppendLine($"                S{station.Id}[\"{stationName}\"]");
        }
        
        sb.AppendLine("            end");
        
        // Zone B stations
        var zoneBStations = stations.Where(s => s.Zone == "B").ToList();
        foreach (var station in zoneBStations)
        {
            var stationName = station.Name.Replace(" ", "<br/>");
            sb.AppendLine($"            S{station.Id}[\"{stationName}\"]");
        }
        
        sb.AppendLine("        end");
        
        // Zone C stations
        var zoneCStations = stations.Where(s => s.Zone == "C").ToList();
        foreach (var station in zoneCStations)
        {
            var stationName = station.Name.Replace(" ", "<br/>");
            sb.AppendLine($"        S{station.Id}[\"{stationName}\"]");
        }
        
        sb.AppendLine("    end");
        sb.AppendLine();

        // Metro line routes with thick colored arrows
        sb.AppendLine("    %% Metro Line Routes - Colored Lines");
        
        // Red Line (Express) - use thick arrows
        var redLine = metroLines.FirstOrDefault(ml => ml.Code == "RL");
        if (redLine != null)
        {
            var redStations = stations.Where(s => s.MetroLineIds.Contains(redLine.Id)).OrderBy(s => s.Id).ToList();
            sb.AppendLine("    %% Red Line - Express Service");
            for (int i = 0; i < redStations.Count - 1; i++)
            {
                sb.AppendLine($"    S{redStations[i].Id} ==>|RL| S{redStations[i + 1].Id}");
            }
        }
        
        // Blue Line (Local) - use thick arrows
        var blueLine = metroLines.FirstOrDefault(ml => ml.Code == "BL");
        if (blueLine != null)
        {
            var blueStations = stations.Where(s => s.MetroLineIds.Contains(blueLine.Id)).OrderBy(s => s.Id).ToList();
            sb.AppendLine("    %% Blue Line - Local Service");
            for (int i = 0; i < blueStations.Count - 1; i++)
            {
                sb.AppendLine($"    S{blueStations[i].Id} ==>|BL| S{blueStations[i + 1].Id}");
            }
        }
        
        // Green Line (Local) - use thick arrows
        var greenLine = metroLines.FirstOrDefault(ml => ml.Code == "GL");
        if (greenLine != null)
        {
            var greenStations = stations.Where(s => s.MetroLineIds.Contains(greenLine.Id)).OrderBy(s => s.Id).ToList();
            sb.AppendLine("    %% Green Line - Local Service");
            for (int i = 0; i < greenStations.Count - 1; i++)
            {
                sb.AppendLine($"    S{greenStations[i].Id} ==>|GL| S{greenStations[i + 1].Id}");
            }
        }
        sb.AppendLine();

        // Zone styling for concentric appearance
        sb.AppendLine("    %% Zone Styling");
        sb.AppendLine("    style ZoneA fill:#e3f2fd,stroke:#1976d2,stroke-width:3px");
        sb.AppendLine("    style ZoneB fill:#e8f5e8,stroke:#388e3c,stroke-width:3px");
        sb.AppendLine("    style ZoneC fill:#fff3e0,stroke:#f57c00,stroke-width:3px");
        sb.AppendLine();

        // Station styling based on zone and transfer status
        sb.AppendLine("    %% Station Styling");
        
        // Zone A stations
        foreach (var station in zoneAStations)
        {
            var fillColor = station.IsTransferStation ? "#1e40af" : "#3b82f6";
            sb.AppendLine($"    style S{station.Id} fill:{fillColor},stroke:#1e3a8a,stroke-width:3px,color:#ffffff");
        }
        
        // Zone B stations
        foreach (var station in zoneBStations)
        {
            var fillColor = station.IsTransferStation ? "#15803d" : "#22c55e";
            sb.AppendLine($"    style S{station.Id} fill:{fillColor},stroke:#14532d,stroke-width:3px,color:#ffffff");
        }
        
        // Zone C stations
        foreach (var station in zoneCStations)
        {
            var fillColor = station.IsTransferStation ? "#ea580c" : "#f97316";
            sb.AppendLine($"    style S{station.Id} fill:{fillColor},stroke:#9a3412,stroke-width:3px,color:#ffffff");
        }
        sb.AppendLine();

        // Link styling for metro lines only (no geographic links to count)
        sb.AppendLine("    %% Metro Line Colors");
        int linkIndex = 0;
        
        // Red Line links (red)
        if (redLine != null)
        {
            var redStations = stations.Where(s => s.MetroLineIds.Contains(redLine.Id)).OrderBy(s => s.Id).ToList();
            for (int i = 0; i < redStations.Count - 1; i++)
            {
                sb.AppendLine($"    linkStyle {linkIndex} stroke:#dc2626,stroke-width:8px");
                linkIndex++;
            }
        }
        
        // Blue Line links (blue)
        if (blueLine != null)
        {
            var blueStations = stations.Where(s => s.MetroLineIds.Contains(blueLine.Id)).OrderBy(s => s.Id).ToList();
            for (int i = 0; i < blueStations.Count - 1; i++)
            {
                sb.AppendLine($"    linkStyle {linkIndex} stroke:#2563eb,stroke-width:8px");
                linkIndex++;
            }
        }
        
        // Green Line links (green)
        if (greenLine != null)
        {
            var greenStations = stations.Where(s => s.MetroLineIds.Contains(greenLine.Id)).OrderBy(s => s.Id).ToList();
            for (int i = 0; i < greenStations.Count - 1; i++)
            {
                sb.AppendLine($"    linkStyle {linkIndex} stroke:#16a34a,stroke-width:8px");
                linkIndex++;
            }
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>
    /// Generates an ASCII art representation of the metro system.
    /// </summary>
    public async Task<string> GenerateAsciiMapAsync()
    {
        var stations = (await _stationService.GetAllStationsAsync()).ToList();
        var metroLines = (await _metroLineService.GetAllMetroLinesAsync()).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Metro System Map");
        sb.AppendLine("================");
        sb.AppendLine();

        // Zone layout
        sb.AppendLine("Concentric Zone Layout:");
        sb.AppendLine("┌─────────────────────────────────────────┐");
        sb.AppendLine("│              Zone C (Outer)            │");
        sb.AppendLine("│  ┌───────────────────────────────────┐  │");
        sb.AppendLine("│  │           Zone B (Middle)        │  │");
        sb.AppendLine("│  │  ┌─────────────────────────────┐  │  │");
        sb.AppendLine("│  │  │      Zone A (Central)      │  │  │");
        sb.AppendLine("│  │  └─────────────────────────────┘  │  │");
        sb.AppendLine("│  └───────────────────────────────────┘  │");
        sb.AppendLine("└─────────────────────────────────────────┘");
        sb.AppendLine();

        // Stations by zone
        var stationsByZone = stations.GroupBy(s => s.Zone).OrderBy(g => g.Key);
        foreach (var zoneGroup in stationsByZone)
        {
            sb.AppendLine($"Zone {zoneGroup.Key} Stations:");
            foreach (var station in zoneGroup.OrderBy(s => s.Id))
            {
                var transferIndicator = station.IsTransferStation ? " [TRANSFER]" : "";
                var lines = string.Join(", ", metroLines.Where(ml => station.MetroLineIds.Contains(ml.Id))
                                                       .Select(ml => ml.Code));
                sb.AppendLine($"  {station.Id}. {station.Name} ({lines}){transferIndicator}");
            }
            sb.AppendLine();
        }

        // Metro lines
        sb.AppendLine("Metro Lines:");
        foreach (var metroLine in metroLines.OrderBy(ml => ml.Id))
        {
            var lineStations = stations.Where(s => s.MetroLineIds.Contains(metroLine.Id))
                                     .OrderBy(s => s.Id)
                                     .Select(s => s.Name);
            sb.AppendLine($"{metroLine.Code} - {metroLine.Name} ({metroLine.LineType})");
            sb.AppendLine($"     Route: {string.Join(" → ", lineStations)}");
            sb.AppendLine($"     Fare Multiplier: {metroLine.FareMultiplier:F1}x");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates an HTML/SVG visualization of the metro system.
    /// </summary>
    public async Task<string> GenerateHtmlVisualizationAsync()
    {
        var stations = (await _stationService.GetAllStationsAsync()).ToList();
        var metroLines = (await _metroLineService.GetAllMetroLinesAsync()).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("    <title>Metro System Map</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
        sb.AppendLine("        h1 { color: #333; text-align: center; margin-bottom: 30px; }");
        sb.AppendLine("        .map-container { display: flex; gap: 20px; margin-bottom: 30px; }");
        sb.AppendLine("        .map-svg { flex: 2; }");
        sb.AppendLine("        .legend { flex: 1; }");
        sb.AppendLine("        .zone { fill: none; stroke: #666; stroke-width: 2; stroke-dasharray: 5,5; }");
        sb.AppendLine("        .station { cursor: pointer; }");
        sb.AppendLine("        .station-circle { r: 8; }");
        sb.AppendLine("        .transfer-station .station-circle { r: 12; stroke-width: 3; }");
        sb.AppendLine("        .station-text { font-size: 12px; text-anchor: middle; }");
        sb.AppendLine("        .metro-line { stroke-width: 4; fill: none; }");
        sb.AppendLine("        .legend-item { margin-bottom: 15px; padding: 10px; border-radius: 5px; background: #f9f9f9; }");
        sb.AppendLine("        .legend-line { width: 30px; height: 4px; display: inline-block; margin-right: 10px; vertical-align: middle; }");
        sb.AppendLine("        .fare-table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        sb.AppendLine("        .fare-table th, .fare-table td { border: 1px solid #ddd; padding: 8px; text-align: center; }");
        sb.AppendLine("        .fare-table th { background-color: #f2f2f2; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine("        <h1>Metro System Map & Fare Guide</h1>");
        sb.AppendLine();
        sb.AppendLine("        <div class=\"map-container\">");
        sb.AppendLine("            <svg class=\"map-svg\" width=\"600\" height=\"400\" viewBox=\"0 0 600 400\">");

        // Draw zones
        sb.AppendLine("                <!-- Zones -->");
        sb.AppendLine("                <circle class=\"zone\" cx=\"300\" cy=\"200\" r=\"80\" />"); // Zone A
        sb.AppendLine("                <circle class=\"zone\" cx=\"300\" cy=\"200\" r=\"140\" />"); // Zone B  
        sb.AppendLine("                <circle class=\"zone\" cx=\"300\" cy=\"200\" r=\"180\" />"); // Zone C
        sb.AppendLine("                <text x=\"300\" y=\"130\" text-anchor=\"middle\" font-size=\"14\" fill=\"#666\">Zone A</text>");
        sb.AppendLine("                <text x=\"300\" y=\"70\" text-anchor=\"middle\" font-size=\"14\" fill=\"#666\">Zone B</text>");
        sb.AppendLine("                <text x=\"300\" y=\"30\" text-anchor=\"middle\" font-size=\"14\" fill=\"#666\">Zone C</text>");

        // Position stations in a circular layout
        var stationPositions = CalculateStationPositions(stations);

        // Draw metro line connections
        sb.AppendLine("                <!-- Metro Lines -->");
        foreach (var metroLine in metroLines)
        {
            var lineStations = stations.Where(s => s.MetroLineIds.Contains(metroLine.Id))
                                     .OrderBy(s => s.Id).ToList();
            
            if (lineStations.Count > 1)
            {
                var pathData = "M";
                for (int i = 0; i < lineStations.Count; i++)
                {
                    var station = lineStations[i];
                    var pos = stationPositions[station.Id];
                    pathData += $" {pos.X},{pos.Y}";
                    if (i < lineStations.Count - 1) pathData += " L";
                }
                
                sb.AppendLine($"                <path class=\"metro-line\" d=\"{pathData}\" stroke=\"{metroLine.Color}\" />");
            }
        }

        // Draw stations
        sb.AppendLine("                <!-- Stations -->");
        foreach (var station in stations)
        {
            var pos = stationPositions[station.Id];
            var transferClass = station.IsTransferStation ? " transfer-station" : "";
            var primaryLine = metroLines.FirstOrDefault(ml => station.MetroLineIds.Contains(ml.Id));
            var fillColor = primaryLine?.Color ?? "#666";

            sb.AppendLine($"                <g class=\"station{transferClass}\" data-station-id=\"{station.Id}\">");
            sb.AppendLine($"                    <circle class=\"station-circle\" cx=\"{pos.X}\" cy=\"{pos.Y}\" fill=\"{fillColor}\" stroke=\"white\" stroke-width=\"2\" />");
            sb.AppendLine($"                    <text class=\"station-text\" x=\"{pos.X}\" y=\"{pos.Y + 20}\" fill=\"#333\">{station.Name}</text>");
            sb.AppendLine("                </g>");
        }

        sb.AppendLine("            </svg>");
        sb.AppendLine();
        sb.AppendLine("            <div class=\"legend\">");
        sb.AppendLine("                <h3>Metro Lines</h3>");

        foreach (var metroLine in metroLines)
        {
            sb.AppendLine("                <div class=\"legend-item\">");
            sb.AppendLine($"                    <span class=\"legend-line\" style=\"background-color: {metroLine.Color};\"></span>");
            sb.AppendLine($"                    <strong>{metroLine.Code} - {metroLine.Name}</strong><br>");
            sb.AppendLine($"                    Type: {metroLine.LineType}<br>");
            sb.AppendLine($"                    Fare Multiplier: {metroLine.FareMultiplier:F1}x<br>");
            sb.AppendLine($"                    Hours: {metroLine.OperatingHours}");
            sb.AppendLine("                </div>");
        }

        sb.AppendLine("                <div class=\"legend-item\">");
        sb.AppendLine("                    <strong>Station Types:</strong><br>");
        sb.AppendLine("                    🚇 Regular Station<br>");
        sb.AppendLine("                    ⭕ Transfer Station<br>");
        sb.AppendLine("                    🚉 Terminal Station");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");

        // Fare table
        sb.AppendLine(await GenerateFareTableHtmlAsync());

        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a fare explanation table in HTML format.
    /// </summary>
    public async Task<string> GenerateFareTableHtmlAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <h3>Fare Structure</h3>");
        sb.AppendLine("        <table class=\"fare-table\">");
        sb.AppendLine("            <thead>");
        sb.AppendLine("                <tr>");
        sb.AppendLine("                    <th>Zone Combination</th>");
        sb.AppendLine("                    <th>Base Fare</th>");
        sb.AppendLine("                    <th>Red Line (1.2x)</th>");
        sb.AppendLine("                    <th>Blue/Green Line (1.0x)</th>");
        sb.AppendLine("                    <th>Yellow Line (0.8x)</th>");
        sb.AppendLine("                </tr>");
        sb.AppendLine("            </thead>");
        sb.AppendLine("            <tbody>");

        var zoneCombinations = new[]
        {
            ("A-A", 1), ("A-B", 1), ("A-C", 2),
            ("B-B", 1), ("B-C", 1), ("C-C", 1)
        };

        foreach (var (combo, zones) in zoneCombinations)
        {
            var baseFare = _fareOptions.GetZoneBasedFare(zones);
            sb.AppendLine("                <tr>");
            sb.AppendLine($"                    <td>{combo}</td>");
            sb.AppendLine($"                    <td>${baseFare:F2}</td>");
            sb.AppendLine($"                    <td>${baseFare * 1.2m:F2}</td>");
            sb.AppendLine($"                    <td>${baseFare * 1.0m:F2}</td>");
            sb.AppendLine($"                    <td>${baseFare * 0.8m:F2}</td>");
            sb.AppendLine("                </tr>");
        }

        sb.AppendLine("            </tbody>");
        sb.AppendLine("        </table>");

        sb.AppendLine("        <h4>Additional Charges</h4>");
        sb.AppendLine("        <ul>");
        sb.AppendLine("            <li><strong>Transfer Penalty:</strong> $0.25 - $0.75 depending on lines</li>");
        sb.AppendLine("            <li><strong>Peak Hour Surcharge:</strong> 25% (7-9 AM, 5-7 PM weekdays)</li>");
        sb.AppendLine("            <li><strong>Off-Peak Discount:</strong> 10% (10 PM - 6 AM)</li>");
        sb.AppendLine("        </ul>");

        sb.AppendLine("        <h4>Passenger Discounts</h4>");
        sb.AppendLine("        <ul>");
        foreach (var discount in _fareOptions.PassengerDiscounts)
        {
            if (discount.Value > 0)
            {
                sb.AppendLine($"            <li><strong>{discount.Key}:</strong> {discount.Value:P0} discount</li>");
            }
        }
        sb.AppendLine("        </ul>");

        return sb.ToString();
    }

    /// <summary>
    /// Calculates positions for stations in a circular layout.
    /// </summary>
    private Dictionary<int, (int X, int Y)> CalculateStationPositions(List<Station> stations)
    {
        var positions = new Dictionary<int, (int X, int Y)>();
        var centerX = 300;
        var centerY = 200;

        var stationsByZone = stations.GroupBy(s => s.Zone).ToDictionary(g => g.Key, g => g.ToList());

        // Zone A: Inner circle
        if (stationsByZone.ContainsKey("A"))
        {
            var zoneAStations = stationsByZone["A"];
            for (int i = 0; i < zoneAStations.Count; i++)
            {
                var angle = (2 * Math.PI * i) / zoneAStations.Count;
                var x = centerX + (int)(60 * Math.Cos(angle));
                var y = centerY + (int)(60 * Math.Sin(angle));
                positions[zoneAStations[i].Id] = (x, y);
            }
        }

        // Zone B: Middle circle
        if (stationsByZone.ContainsKey("B"))
        {
            var zoneBStations = stationsByZone["B"];
            for (int i = 0; i < zoneBStations.Count; i++)
            {
                var angle = (2 * Math.PI * i) / zoneBStations.Count + Math.PI / 6; // Offset slightly
                var x = centerX + (int)(120 * Math.Cos(angle));
                var y = centerY + (int)(120 * Math.Sin(angle));
                positions[zoneBStations[i].Id] = (x, y);
            }
        }

        // Zone C: Outer circle
        if (stationsByZone.ContainsKey("C"))
        {
            var zoneCStations = stationsByZone["C"];
            for (int i = 0; i < zoneCStations.Count; i++)
            {
                var angle = (2 * Math.PI * i) / zoneCStations.Count + Math.PI / 4; // Offset slightly
                var x = centerX + (int)(160 * Math.Cos(angle));
                var y = centerY + (int)(160 * Math.Sin(angle));
                positions[zoneCStations[i].Id] = (x, y);
            }
        }

        return positions;
    }

    public async Task<string> GenerateFareExplanationAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Fare Calculation Guide");
        sb.AppendLine("=====================");
        sb.AppendLine();

        sb.AppendLine("Base Fare Structure:");
        sb.AppendLine("-------------------");
        foreach (var zoneFare in _fareOptions.ZoneBasedFares.OrderBy(f => f.Key))
        {
            sb.AppendLine($"  {zoneFare.Key} zone(s): ${zoneFare.Value:F2}");
        }
        sb.AppendLine();

        sb.AppendLine("Metro Line Multipliers:");
        sb.AppendLine("----------------------");
        var metroLines = await _metroLineService.GetAllMetroLinesAsync();
        foreach (var line in metroLines.OrderBy(l => l.Code))
        {
            sb.AppendLine($"  {line.Code} ({line.Name}): {line.FareMultiplier:F1}x multiplier");
        }
        sb.AppendLine();

        sb.AppendLine("Passenger Discounts:");
        sb.AppendLine("-------------------");
        foreach (var discount in _fareOptions.PassengerDiscounts.Where(d => d.Value > 0))
        {
            sb.AppendLine($"  {discount.Key}: {discount.Value:P0} discount");
        }
        sb.AppendLine();

        sb.AppendLine("Time-Based Adjustments:");
        sb.AppendLine("----------------------");
        sb.AppendLine($"  Peak Hours Surcharge: {_fareOptions.TimeBasedRules.PeakHours.Surcharge:P0}");
        sb.AppendLine($"    Weekday Morning: {_fareOptions.TimeBasedRules.PeakHours.WeekdayMorningStart}:00-{_fareOptions.TimeBasedRules.PeakHours.WeekdayMorningEnd}:00");
        sb.AppendLine($"    Weekday Evening: {_fareOptions.TimeBasedRules.PeakHours.WeekdayEveningStart}:00-{_fareOptions.TimeBasedRules.PeakHours.WeekdayEveningEnd}:00");
        sb.AppendLine($"  Off-Peak Discount: {_fareOptions.TimeBasedRules.OffPeakHours.Discount:P0}");
        sb.AppendLine($"    Night Hours: {_fareOptions.TimeBasedRules.OffPeakHours.NightStart}:00-{_fareOptions.TimeBasedRules.OffPeakHours.NightEnd}:00");
        sb.AppendLine();

        sb.AppendLine("Transfer Penalties:");
        sb.AppendLine("------------------");
        foreach (var penalty in _metroLineOptions.TransferPenalties)
        {
            sb.AppendLine($"  {penalty.Key}: ${penalty.Value:F2}");
        }

        return sb.ToString();
    }
} 