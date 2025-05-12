using System;
using System.Collections.Generic;
using OsuPP.NET.Calculators;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Examples
{
    /// <summary>
    /// Examples of using the OsuPP.NET library that exactly match rosu-pp API.
    /// </summary>
    public static class CoreExample
    {
        /// <summary>
        /// Basic example matching the primary rosu-pp example.
        /// </summary>
        public static void BasicRosuPpExample()
        {
            // Decode the map
            var map = Beatmap.FromPath("./resources/2785319.osu");
            
            // Calculate difficulty attributes 
            var diffAttrs = new Difficulty()
                .Mods(Mods.Hidden | Mods.HardRock) // HDHR
                .Calculate(map);
            
            var stars = diffAttrs.Stars;
            
            // Calculate performance attributes
            var perfAttrs = new Performance(diffAttrs)
                // To speed up the calculation, we used the previous attributes.
                // Note that this should only be done if the map and all difficulty
                // settings stay the same, otherwise the final attributes will be incorrect!
                .Mods(Mods.Hidden | Mods.HardRock) // HDHR, must be the same as before
                .Combo(789)
                .Accuracy(99.2)
                .Misses(2)
                .Calculate();
            
            var pp = perfAttrs.Pp;
            
            // Again, we re-use the previous attributes for maximum efficiency.
            var maxPp = new Performance(diffAttrs)
                .Mods(Mods.Hidden | Mods.HardRock) // Still the same
                .Calculate()
                .Pp;
            
            Console.WriteLine($"Stars: {stars} | PP: {pp}/{maxPp}");
        }
        
        /// <summary>
        /// Gradual calculation example matching the rosu-pp gradual calculation example.
        /// </summary>
        public static void GradualCalculationExample()
        {
            var map = Beatmap.FromPath("./resources/1028484.osu");
            
            var gradual = new Difficulty()
                .Mods(Mods.HardRock | Mods.DoubleTime) // HRDT
                .ClockRate(1.2)
                .GradualPerformance(map);
            
            var state = new ScoreState(); // empty state, everything is on 0.
            
            // The first 10 hitresults are 300s
            for (int i = 0; i < 10; i++)
            {
                state.N300++;
                state.MaxCombo++;
                var attrs = gradual.Next(state.Clone());
                
                if (attrs != null)
                {
                    Console.WriteLine($"PP: {attrs.Pp}");
                }
            }
            
            // Fast-forward to the end
            // In a real implementation, you'd set the final state like this:
            // state.MaxCombo = ...
            // state.N300 = ...
            // state.NKatu = ...
            // etc.
            
            var finalAttrs = gradual.Last(state.Clone());
            
            if (finalAttrs != null)
            {
                Console.WriteLine($"Final PP: {finalAttrs.Pp}");
            }
        }
        
        /// <summary>
        /// Example of using advanced features like mode conversion and custom parameters.
        /// </summary>
        public static void AdvancedFeaturesExample()
        {
            var map = Beatmap.FromPath("./resources/example.osu");
            
            // Convert a map to a different mode
            var maniaMap = Difficulty.Convert(map, GameMode.Mania, "4K"); // Convert to 4-key mania
            
            // Calculate difficulty with custom parameters
            var diffAttrs = new Difficulty()
                .Mods(Mods.HardRock | Mods.DoubleTime)
                .CustomSpeed(true) // Use custom clock rate instead of deriving from mods
                .ClockRate(1.5)    // Custom clock rate
                .ApproachRate(10)  // Custom AR value
                .OverallDifficulty(8, false) // Custom OD, not yet affected by mods
                .Calculate(map);
            
            Console.WriteLine($"Custom Stars: {diffAttrs.Stars}");
            
            // Calculate difficulty with all possible mod combinations
            var baseDiff = new Difficulty();
            
            // Define different mod combinations
            var modCombinations = new[]
            {
                (name: "NM", mods: Mods.None),
                (name: "HR", mods: Mods.HardRock),
                (name: "DT", mods: Mods.DoubleTime),
                (name: "HD", mods: Mods.Hidden),
                (name: "FL", mods: Mods.Flashlight),
                (name: "HDHR", mods: Mods.Hidden | Mods.HardRock),
                (name: "HDDT", mods: Mods.Hidden | Mods.DoubleTime),
                (name: "HRDT", mods: Mods.HardRock | Mods.DoubleTime),
                (name: "HDFL", mods: Mods.Hidden | Mods.Flashlight),
                (name: "HDHRDT", mods: Mods.Hidden | Mods.HardRock | Mods.DoubleTime),
            };
            
            foreach (var (name, mods) in modCombinations)
            {
                var attrs = baseDiff.Mods(mods).Calculate(map);
                Console.WriteLine($"{name}: {attrs.Stars} stars, {new Performance(attrs).Calculate().Pp} PP");
            }
        }
        
        /// <summary>
        /// Example of calculating accuracy values.
        /// </summary>
        public static void AccuracyCalculationExample()
        {
            var map = Beatmap.FromPath("./resources/example.osu");
            
            // Calculate difficulty
            var diffAttrs = new Difficulty()
                .Mods(Mods.Hidden | Mods.HardRock)
                .Calculate(map);
            
            // Create a list of accuracies to calculate for
            var accuracies = new[] { 95.0, 97.0, 98.0, 99.0, 99.5, 100.0 };
            
            // Using Performance calculator directly
            var basePerf = new Performance(diffAttrs).Mods(Mods.Hidden | Mods.HardRock);
            
            foreach (var acc in accuracies)
            {
                var attrs = basePerf.Clone().Accuracy(acc).Calculate();
                Console.WriteLine($"{acc}%: {attrs.Pp} PP");
            }
            
            // Using GradualPerformance
            var gradual = new Difficulty()
                .Mods(Mods.Hidden | Mods.HardRock)
                .GradualPerformance(map);
            
            // Fast forward to the end
            gradual.Last(new ScoreState());
            
            // Calculate for multiple accuracies at once
            var accResults = gradual.ForAccuracies(accuracies, misses: 0);
            
            foreach (var kvp in accResults)
            {
                Console.WriteLine($"{kvp.Key}%: {kvp.Value.Pp} PP");
            }
        }
        
        /// <summary>
        /// Example of working with score states.
        /// </summary>
        public static void ScoreStateExample()
        {
            var map = Beatmap.FromPath("./resources/example.osu");
            
            // Create a score state directly
            var manualState = new ScoreState
            {
                MaxCombo = 500,
                N300 = 450,
                N100 = 30,
                N50 = 10,
                Misses = 5
            };
            
            // Create a score state from accuracy
            var accuracyState = ScoreState.FromAccuracy(
                map,
                accuracy: 95.0,
                misses: 2,
                priority: HitResultPriority.BestCase
            );
            
            // Create a score state from hit results
            var hitResultsState = ScoreState.FromHitResults(
                n300: 400,
                n100: 50,
                n50: 20,
                misses: 10,
                combo: 300
            );
            
            // Build a score state incrementally
            var incrementalState = new ScoreState();
            incrementalState.AddHitResult(HitResult.Great);
            incrementalState.AddHitResult(HitResult.Great);
            incrementalState.AddHitResult(HitResult.Good);
            incrementalState.AddHitResult(HitResult.Miss);
            incrementalState.AddHitResult(HitResult.Great);
            
            // Calculate performance with different states
            var diffAttrs = new Difficulty().Calculate(map);
            var perfCalc = new Performance(diffAttrs);
            
            Console.WriteLine($"Manual state: {perfCalc.ScoreState(manualState).Calculate().Pp} PP");
            Console.WriteLine($"Accuracy state: {perfCalc.ScoreState(accuracyState).Calculate().Pp} PP");
            Console.WriteLine($"Hit results state: {perfCalc.ScoreState(hitResultsState).Calculate().Pp} PP");
            Console.WriteLine($"Incremental state: {perfCalc.ScoreState(incrementalState).Calculate().Pp} PP");
        }
        
        /// <summary>
        /// Example showing different ways to use the Difficulty and Performance calculators.
        /// </summary>
        public static void CalculatorPatternsExample()
        {
            var map = Beatmap.FromPath("./resources/example.osu");
            
            // Pattern 1: Calculate difficulty then performance
            var diffAttrs = new Difficulty()
                .Mods(Mods.HardRock)
                .Calculate(map);
            
            var perfAttrs = new Performance(diffAttrs)
                .Accuracy(98.5)
                .Calculate();
            
            Console.WriteLine($"Pattern 1: {perfAttrs.Pp} PP");
            
            // Pattern 2: Chain calculations with Performance.CreateNew()
            var chain = new Performance(diffAttrs)
                .Accuracy(95.0)
                .Calculate()
                .Pp;
            
            var maxChain = new Performance(diffAttrs)
                .CreateNew()  // Create a new instance for continued calculation
                .Calculate()
                .Pp;
            
            Console.WriteLine($"Pattern 2: {chain}/{maxChain} PP");
            
            // Pattern 3: Use static factory methods
            var staticDiff = Difficulty.StaticDifficulty(diffAttrs);
            var staticPerfAttrs = staticDiff
                .Calculate(map);
            
            Console.WriteLine($"Pattern 3: {staticPerfAttrs.Stars} stars");
            
            // Pattern 4: Use score state
            var state = ScoreState.FromAccuracy(map, 97.0, 1);
            var stateAttrs = new Performance(diffAttrs)
                .UseScoreState(state)  // Set all parameters from score state
                .Calculate();
            
            Console.WriteLine($"Pattern 4: {stateAttrs.Pp} PP");
        }
    }
}