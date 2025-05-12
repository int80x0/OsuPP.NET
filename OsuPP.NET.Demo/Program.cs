using System;
using System.IO;
using OsuPP.NET.Calculators;
using OsuPP.NET.Examples;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OsuPP.NET - osu! Performance Points Calculator");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }
            
            string command = args[0].ToLower();
            
            switch (command)
            {
                case "calc":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Missing beatmap path.");
                        ShowUsage();
                        return;
                    }
                    
                    CalculatePP(args[1], args);
                    break;
                
                case "examples":
                    RunExamples();
                    break;
                
                case "convert":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Error: Missing beatmap path or mode.");
                        ShowUsage();
                        return;
                    }
                    
                    ConvertMap(args[1], args[2]);
                    break;
                
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowUsage();
                    break;
            }
        }
        
        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  OsuPP.NET.Demo calc <beatmap.osu> [options]");
            Console.WriteLine("  OsuPP.NET.Demo convert <beatmap.osu> <mode>");
            Console.WriteLine("  OsuPP.NET.Demo examples");
            Console.WriteLine();
            Console.WriteLine("Options for calc:");
            Console.WriteLine("  --mods <mods>       Specify mods (e.g., HDHR, HDDT)");
            Console.WriteLine("  --acc <accuracy>    Specify accuracy (0-100)");
            Console.WriteLine("  --combo <combo>     Specify max combo");
            Console.WriteLine("  --misses <count>    Specify miss count");
            Console.WriteLine("  --gradual           Show gradual calculation");
            Console.WriteLine();
            Console.WriteLine("Available modes for convert:");
            Console.WriteLine("  osu, taiko, catch, mania");
        }
        
        static void CalculatePP(string beatmapPath, string[] args)
        {
            try
            {
                // Check if file exists
                if (!File.Exists(beatmapPath))
                {
                    Console.WriteLine($"Error: Beatmap file not found at {beatmapPath}");
                    return;
                }
                
                // Parse options
                Mods mods = Mods.None;
                double accuracy = 100.0;
                int? combo = null;
                int misses = 0;
                bool gradual = false;
                
                for (int i = 2; i < args.Length; i++)
                {
                    string option = args[i].ToLower();
                    
                    if (option == "--mods" && i + 1 < args.Length)
                    {
                        mods = ParseMods(args[++i]);
                    }
                    else if (option == "--acc" && i + 1 < args.Length)
                    {
                        if (double.TryParse(args[++i], out double acc))
                            accuracy = acc;
                    }
                    else if (option == "--combo" && i + 1 < args.Length)
                    {
                        if (int.TryParse(args[++i], out int c))
                            combo = c;
                    }
                    else if (option == "--misses" && i + 1 < args.Length)
                    {
                        if (int.TryParse(args[++i], out int m))
                            misses = m;
                    }
                    else if (option == "--gradual")
                    {
                        gradual = true;
                    }
                }
                
                // Load beatmap
                Console.WriteLine($"Loading beatmap: {beatmapPath}");
                var beatmap = Beatmap.FromPath(beatmapPath);
                
                Console.WriteLine($"Beatmap loaded: {beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]");
                Console.WriteLine($"Mode: {beatmap.Mode}");
                Console.WriteLine();
                
                // Calculate difficulty attributes
                Console.WriteLine($"Calculating difficulty with mods: {mods}");
                var difficultyCalculator = new Difficulty().Mods(mods);
                var diffAttrs = difficultyCalculator.Calculate(beatmap);
                
                Console.WriteLine($"Stars: {diffAttrs.Stars:F2}");
                Console.WriteLine();
                
                // Calculate performance attributes
                Console.WriteLine($"Calculating performance with:");
                Console.WriteLine($"  Accuracy: {accuracy:F2}%");
                Console.WriteLine($"  Misses: {misses}");
                Console.WriteLine($"  Combo: {(combo.HasValue ? combo.Value.ToString() : "Max")}");
                Console.WriteLine();
                
                var performanceCalculator = new Performance(diffAttrs)
                    .Mods(mods)
                    .Accuracy(accuracy)
                    .Misses(misses);
                
                if (combo.HasValue)
                    performanceCalculator.Combo(combo.Value);
                
                var perfAttrs = performanceCalculator.Calculate();
                var maxPerfAttrs = new Performance(diffAttrs).Mods(mods).Calculate();
                
                Console.WriteLine($"PP: {perfAttrs.Pp:F2} / {maxPerfAttrs.Pp:F2} PP");
                
                if (gradual && beatmap.CountHitObjects > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Gradual calculation:");
                    
                    var gradualPerf = difficultyCalculator.GradualPerformance(beatmap);
                    var state = ScoreState.FromAccuracy(beatmap, accuracy, misses);
                    
                    // Show PP at 25%, 50%, 75% and 100% of the way through the map
                    int total = beatmap.CountHitObjects;
                    
                    for (int i = 1; i <= 4; i++)
                    {
                        int index = (int)Math.Round(total * i / 4.0) - 1;
                        index = Math.Max(0, Math.Min(total - 1, index));
                        
                        var attrs = gradualPerf.AtIndex(index, state);
                        
                        if (attrs != null)
                        {
                            Console.WriteLine($"  {i * 25}%: {attrs.Pp:F2} PP");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        static void ConvertMap(string beatmapPath, string modeStr)
        {
            try
            {
                // Check if file exists
                if (!File.Exists(beatmapPath))
                {
                    Console.WriteLine($"Error: Beatmap file not found at {beatmapPath}");
                    return;
                }
                
                // Parse mode
                GameMode mode;
                
                switch (modeStr.ToLower())
                {
                    case "osu":
                        mode = GameMode.Osu;
                        break;
                    case "taiko":
                        mode = GameMode.Taiko;
                        break;
                    case "catch":
                    case "ctb":
                        mode = GameMode.Catch;
                        break;
                    case "mania":
                        mode = GameMode.Mania;
                        break;
                    default:
                        Console.WriteLine($"Error: Unknown mode: {modeStr}");
                        return;
                }
                
                // Load beatmap
                Console.WriteLine($"Loading beatmap: {beatmapPath}");
                var beatmap = Beatmap.FromPath(beatmapPath);
                
                Console.WriteLine($"Beatmap loaded: {beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]");
                Console.WriteLine($"Original mode: {beatmap.Mode}");
                Console.WriteLine();
                
                // Convert beatmap
                if (beatmap.Mode == mode)
                {
                    Console.WriteLine($"Beatmap is already in {mode} mode.");
                    return;
                }
                
                Console.WriteLine($"Converting to {mode} mode...");
                var converted = Difficulty.Convert(beatmap, mode);
                
                // Calculate difficulty for the converted beatmap
                var diffAttrs = new Difficulty().Calculate(converted);
                
                Console.WriteLine($"Converted stars: {diffAttrs.Stars:F2}");
                
                // Get output path
                string dir = Path.GetDirectoryName(beatmapPath) ?? ".";
                string filename = Path.GetFileNameWithoutExtension(beatmapPath);
                string ext = Path.GetExtension(beatmapPath);
                string outputPath = Path.Combine(dir, $"{filename}.{mode}{ext}");
                
                Console.WriteLine($"Converted beatmap would be saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        static void RunExamples()
        {
            Console.WriteLine("Running example 1: Basic rosu-pp API usage");
            Console.WriteLine("----------------------------------------");
            try
            {
                CoreExample.BasicRosuPpExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Example error: {ex.Message}");
            }
            Console.WriteLine();
            
            Console.WriteLine("Running example 2: Gradual calculation");
            Console.WriteLine("------------------------------------");
            try
            {
                CoreExample.GradualCalculationExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Example error: {ex.Message}");
            }
            Console.WriteLine();
            
            Console.WriteLine("Running example 3: Advanced features");
            Console.WriteLine("----------------------------------");
            try
            {
                CoreExample.AdvancedFeaturesExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Example error: {ex.Message}");
            }
            Console.WriteLine();
            
            Console.WriteLine("Running example 4: Accuracy calculation");
            Console.WriteLine("-------------------------------------");
            try
            {
                CoreExample.AccuracyCalculationExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Example error: {ex.Message}");
            }
            Console.WriteLine();
            
            Console.WriteLine("Running example 5: Score state usage");
            Console.WriteLine("----------------------------------");
            try
            {
                CoreExample.ScoreStateExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Example error: {ex.Message}");
            }
            Console.WriteLine();
            
            Console.WriteLine("Running example 6: Calculator patterns");
            Console.WriteLine("------------------------------------");
            try
            {
                CoreExample.CalculatorPatternsExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Example error: {ex.Message}");
            }
        }
        
        static Mods ParseMods(string modsStr)
        {
            if (string.IsNullOrWhiteSpace(modsStr))
                return Mods.None;
            
            Mods result = Mods.None;
            
            // Check for common mod combinations
            modsStr = modsStr.ToUpper();
            
            for (int i = 0; i < modsStr.Length; i += 2)
            {
                if (i + 1 >= modsStr.Length)
                    break;
                
                string mod = modsStr.Substring(i, 2);
                
                switch (mod)
                {
                    case "NF": result |= Mods.NoFail; break;
                    case "EZ": result |= Mods.Easy; break;
                    case "TD": result |= Mods.TouchDevice; break;
                    case "HD": result |= Mods.Hidden; break;
                    case "HR": result |= Mods.HardRock; break;
                    case "SD": result |= Mods.SuddenDeath; break;
                    case "DT": result |= Mods.DoubleTime; break;
                    case "RX": result |= Mods.Relax; break;
                    case "HT": result |= Mods.HalfTime; break;
                    case "NC": result |= Mods.NightcoreMod; break;
                    case "FL": result |= Mods.Flashlight; break;
                    case "SO": result |= Mods.SpunOut; break;
                    case "PF": result |= Mods.Perfect; break;
                }
            }
            
            return result;
        }
    }
}