using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OsuPP.NET.Calculators;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET
{
    /// <summary>
    /// A simplified API for osu! PP calculations.
    /// </summary>
    public static class OsuPpCalculator
    {
        /// <summary>
        /// Calculates the star rating for a beatmap with the given mods.
        /// </summary>
        /// <param name="beatmapPath">Path to the .osu file</param>
        /// <param name="mods">Mods to apply</param>
        /// <returns>The star rating</returns>
        public static float CalculateStars(string beatmapPath, Mods mods = Mods.None)
        {
            var beatmap = Beatmap.FromPath(beatmapPath);
            var attrs = new Difficulty()
                .Mods(mods)
                .Calculate(beatmap);
            
            return attrs.Stars;
        }
        
        /// <summary>
        /// Calculates the star rating for a beatmap with the given mods.
        /// </summary>
        /// <param name="beatmapContent">Content of the .osu file</param>
        /// <param name="mods">Mods to apply</param>
        /// <returns>The star rating</returns>
        public static float CalculateStarsFromContent(string beatmapContent, Mods mods = Mods.None)
        {
            var beatmap = Beatmap.FromContent(beatmapContent);
            var attrs = new Difficulty()
                .Mods(mods)
                .Calculate(beatmap);
            
            return attrs.Stars;
        }
        
        /// <summary>
        /// Calculates the star rating for a beatmap with the given mods.
        /// </summary>
        /// <param name="beatmapBytes">Byte content of the .osu file</param>
        /// <param name="mods">Mods to apply</param>
        /// <returns>The star rating</returns>
        public static float CalculateStarsFromBytes(byte[] beatmapBytes, Mods mods = Mods.None)
        {
            var beatmap = Beatmap.FromBytes(beatmapBytes);
            var attrs = new Difficulty()
                .Mods(mods)
                .Calculate(beatmap);
            
            return attrs.Stars;
        }
        
        /// <summary>
        /// Calculates the performance points for a score.
        /// </summary>
        /// <param name="beatmapPath">Path to the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracy">Accuracy percentage (0-100)</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="n300">Number of 300s (null for auto-calculated)</param>
        /// <param name="n100">Number of 100s (null for auto-calculated)</param>
        /// <param name="n50">Number of 50s (null for auto-calculated)</param>
        /// <returns>The performance points</returns>
        public static float CalculatePP(
            string beatmapPath,
            Mods mods = Mods.None,
            double accuracy = 100.0,
            int? combo = null,
            int misses = 0,
            int? n300 = null,
            int? n100 = null,
            int? n50 = null)
        {
            var beatmap = Beatmap.FromPath(beatmapPath);
            return CalculatePPWithBeatmap(beatmap, mods, accuracy, combo, misses, n300, n100, n50);
        }
        
        /// <summary>
        /// Calculates the performance points for a score.
        /// </summary>
        /// <param name="beatmapContent">Content of the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracy">Accuracy percentage (0-100)</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="n300">Number of 300s (null for auto-calculated)</param>
        /// <param name="n100">Number of 100s (null for auto-calculated)</param>
        /// <param name="n50">Number of 50s (null for auto-calculated)</param>
        /// <returns>The performance points</returns>
        public static float CalculatePPFromContent(
            string beatmapContent,
            Mods mods = Mods.None,
            double accuracy = 100.0,
            int? combo = null,
            int misses = 0,
            int? n300 = null,
            int? n100 = null,
            int? n50 = null)
        {
            var beatmap = Beatmap.FromContent(beatmapContent);
            return CalculatePPWithBeatmap(beatmap, mods, accuracy, combo, misses, n300, n100, n50);
        }
        
        /// <summary>
        /// Calculates the performance points for a score.
        /// </summary>
        /// <param name="beatmapBytes">Byte content of the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracy">Accuracy percentage (0-100)</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="n300">Number of 300s (null for auto-calculated)</param>
        /// <param name="n100">Number of 100s (null for auto-calculated)</param>
        /// <param name="n50">Number of 50s (null for auto-calculated)</param>
        /// <returns>The performance points</returns>
        public static float CalculatePPFromBytes(
            byte[] beatmapBytes,
            Mods mods = Mods.None,
            double accuracy = 100.0,
            int? combo = null,
            int misses = 0,
            int? n300 = null,
            int? n100 = null,
            int? n50 = null)
        {
            var beatmap = Beatmap.FromBytes(beatmapBytes);
            return CalculatePPWithBeatmap(beatmap, mods, accuracy, combo, misses, n300, n100, n50);
        }
        
        /// <summary>
        /// Calculates PP for various accuracies.
        /// </summary>
        /// <param name="beatmapPath">Path to the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracies">List of accuracies to calculate for</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <returns>Dictionary mapping accuracies to PP values</returns>
        public static Dictionary<double, float> CalculatePPForAccuracies(
            string beatmapPath,
            Mods mods = Mods.None,
            double[]? accuracies = null,
            int misses = 0,
            int? combo = null)
        {
            var beatmap = Beatmap.FromPath(beatmapPath);
            return CalculatePPForAccuraciesWithBeatmap(beatmap, mods, accuracies, misses, combo);
        }
        
        /// <summary>
        /// Calculates PP for various accuracies.
        /// </summary>
        /// <param name="beatmapContent">Content of the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracies">List of accuracies to calculate for</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <returns>Dictionary mapping accuracies to PP values</returns>
        public static Dictionary<double, float> CalculatePPForAccuraciesFromContent(
            string beatmapContent,
            Mods mods = Mods.None,
            double[]? accuracies = null,
            int misses = 0,
            int? combo = null)
        {
            var beatmap = Beatmap.FromContent(beatmapContent);
            return CalculatePPForAccuraciesWithBeatmap(beatmap, mods, accuracies, misses, combo);
        }
        
        /// <summary>
        /// Calculates PP for various accuracies.
        /// </summary>
        /// <param name="beatmapBytes">Byte content of the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracies">List of accuracies to calculate for</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <returns>Dictionary mapping accuracies to PP values</returns>
        public static Dictionary<double, float> CalculatePPForAccuraciesFromBytes(
            byte[] beatmapBytes,
            Mods mods = Mods.None,
            double[]? accuracies = null,
            int misses = 0,
            int? combo = null)
        {
            var beatmap = Beatmap.FromBytes(beatmapBytes);
            return CalculatePPForAccuraciesWithBeatmap(beatmap, mods, accuracies, misses, combo);
        }
        
        /// <summary>
        /// Calculates the detailed performance attributes for a score.
        /// </summary>
        /// <param name="beatmapPath">Path to the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracy">Accuracy percentage (0-100)</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="n300">Number of 300s (null for auto-calculated)</param>
        /// <param name="n100">Number of 100s (null for auto-calculated)</param>
        /// <param name="n50">Number of 50s (null for auto-calculated)</param>
        /// <returns>The performance attributes</returns>
        public static PerformanceAttributes CalculatePerformanceAttributes(
            string beatmapPath,
            Mods mods = Mods.None,
            double accuracy = 100.0,
            int? combo = null,
            int misses = 0,
            int? n300 = null,
            int? n100 = null,
            int? n50 = null)
        {
            var beatmap = Beatmap.FromPath(beatmapPath);
            return CalculatePerformanceAttributesWithBeatmap(beatmap, mods, accuracy, combo, misses, n300, n100, n50);
        }
        
        /// <summary>
        /// Asynchronously calculates the PP for a beatmap.
        /// </summary>
        /// <param name="beatmapPath">Path to the .osu file</param>
        /// <param name="mods">Mods used</param>
        /// <param name="accuracy">Accuracy percentage (0-100)</param>
        /// <param name="combo">Maximum combo (null for full combo)</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="n300">Number of 300s (null for auto-calculated)</param>
        /// <param name="n100">Number of 100s (null for auto-calculated)</param>
        /// <param name="n50">Number of 50s (null for auto-calculated)</param>
        /// <returns>The performance points</returns>
        public static async Task<float> CalculatePPAsync(
            string beatmapPath,
            Mods mods = Mods.None,
            double accuracy = 100.0,
            int? combo = null,
            int misses = 0,
            int? n300 = null,
            int? n100 = null,
            int? n50 = null)
        {
            var beatmap = await Beatmap.FromPathAsync(beatmapPath);
            return CalculatePPWithBeatmap(beatmap, mods, accuracy, combo, misses, n300, n100, n50);
        }
        
        #region Private Helper Methods
        
        private static float CalculatePPWithBeatmap(
            Beatmap beatmap,
            Mods mods,
            double accuracy,
            int? combo,
            int misses,
            int? n300,
            int? n100,
            int? n50)
        {
            // Calculate difficulty attributes
            var diffAttrs = new Difficulty()
                .Mods(mods)
                .Calculate(beatmap);
            
            // Create performance calculator
            var perfCalc = new Performance(diffAttrs)
                .Mods(mods)
                .Accuracy(accuracy)
                .Misses(misses);
            
            // Set combo if provided
            if (combo.HasValue)
            {
                perfCalc.Combo(combo.Value);
            }
            
            // Set hit results if provided
            if (n300.HasValue)
            {
                perfCalc.N300(n300.Value);
            }
            
            if (n100.HasValue)
            {
                perfCalc.N100(n100.Value);
            }
            
            if (n50.HasValue)
            {
                perfCalc.N50(n50.Value);
            }
            
            // Calculate and return PP
            return perfCalc.Calculate().Pp;
        }
        
        private static Dictionary<double, float> CalculatePPForAccuraciesWithBeatmap(
            Beatmap beatmap,
            Mods mods,
            double[]? accuracies,
            int misses,
            int? combo)
        {
            // Default accuracies if not provided
            if (accuracies == null || accuracies.Length == 0)
            {
                accuracies = new[] { 95.0, 98.0, 99.0, 99.5, 100.0 };
            }
            
            // Calculate difficulty attributes
            var diffAttrs = new Difficulty()
                .Mods(mods)
                .Calculate(beatmap);
            
            // Create gradual performance calculator
            var gradualPerf = new Difficulty()
                .Mods(mods)
                .GradualPerformance(beatmap);
            
            // Skip to the end
            var emptyState = new ScoreState { Misses = misses };
            if (combo.HasValue)
            {
                emptyState.MaxCombo = combo.Value;
            }
            
            gradualPerf.Last(emptyState);
            
            // Calculate for each accuracy
            var results = gradualPerf.ForAccuracies(accuracies, misses, combo);
            
            // Convert to simple dictionary
            var simplifiedResults = new Dictionary<double, float>();
            
            foreach (var kvp in results)
            {
                simplifiedResults.Add(kvp.Key, kvp.Value.Pp);
            }
            
            return simplifiedResults;
        }
        
        private static PerformanceAttributes CalculatePerformanceAttributesWithBeatmap(
            Beatmap beatmap,
            Mods mods,
            double accuracy,
            int? combo,
            int misses,
            int? n300,
            int? n100,
            int? n50)
        {
            // Calculate difficulty attributes
            var diffAttrs = new Difficulty()
                .Mods(mods)
                .Calculate(beatmap);
            
            // Create performance calculator
            var perfCalc = new Performance(diffAttrs)
                .Mods(mods)
                .Accuracy(accuracy)
                .Misses(misses);
            
            // Set combo if provided
            if (combo.HasValue)
            {
                perfCalc.Combo(combo.Value);
            }
            
            // Set hit results if provided
            if (n300.HasValue)
            {
                perfCalc.N300(n300.Value);
            }
            
            if (n100.HasValue)
            {
                perfCalc.N100(n100.Value);
            }
            
            if (n50.HasValue)
            {
                perfCalc.N50(n50.Value);
            }
            
            // Calculate and return performance attributes
            return perfCalc.Calculate();
        }
        
        #endregion
    }
}