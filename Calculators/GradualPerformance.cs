using System;
using System.Collections.Generic;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Calculators
{
    /// <summary>
    /// Provides a way to calculate performance attributes gradually for each hit object.
    /// </summary>
    public class GradualPerformance
    {
        private readonly IDifficultyCalculator _calculator;
        private readonly Beatmap _beatmap;
        private readonly Mods _mods;
        private readonly bool _isLazerScore;
        private int _currentIndex;
        private int _totalHitObjects;
        private DifficultyAttributes? _lastDifficultyAttributes;
        
        /// <summary>
        /// Creates a new gradual performance calculator.
        /// </summary>
        /// <param name="calculator">The difficulty calculator to use</param>
        /// <param name="beatmap">The beatmap to calculate for</param>
        /// <param name="mods">The mods to apply</param>
        /// <param name="isLazerScore">Whether to use lazer score calculation</param>
        internal GradualPerformance(IDifficultyCalculator calculator, Beatmap beatmap, Mods mods, bool isLazerScore = true)
        {
            _calculator = calculator;
            _beatmap = beatmap;
            _mods = mods;
            _isLazerScore = isLazerScore;
            _currentIndex = 0;
            _totalHitObjects = beatmap.CountHitObjects;
        }
        
        /// <summary>
        /// The number of remaining hit objects to process.
        /// </summary>
        public int RemainingCount => _totalHitObjects - _currentIndex;
        
        /// <summary>
        /// The current index being processed.
        /// </summary>
        public int CurrentIndex => _currentIndex;
        
        /// <summary>
        /// The total number of hit objects.
        /// </summary>
        public int TotalHitObjects => _totalHitObjects;
        
        /// <summary>
        /// Calculates performance attributes for the next hit object based on the current score state.
        /// </summary>
        /// <param name="state">The current score state</param>
        /// <returns>The performance attributes or null if all objects have been processed</returns>
        public PerformanceAttributes? Next(ScoreState state)
        {
            if (_currentIndex >= _totalHitObjects)
                return null;
            
            var result = CalculateAtIndex(_currentIndex, state);
            _currentIndex++;
            
            return result;
        }
        
        /// <summary>
        /// Calculates performance attributes for the next N hit objects based on the current score state.
        /// </summary>
        /// <param name="count">The number of objects to skip</param>
        /// <param name="state">The current score state</param>
        /// <returns>The performance attributes or null if all objects have been processed</returns>
        public PerformanceAttributes? NextN(int count, ScoreState state)
        {
            if (_currentIndex >= _totalHitObjects)
                return null;
            
            _currentIndex = Math.Min(_currentIndex + count, _totalHitObjects);
            return CalculateAtIndex(_currentIndex - 1, state);
        }
        
        /// <summary>
        /// Calculates performance attributes at a specific index based on the given score state.
        /// </summary>
        /// <param name="index">The index to calculate at (0-based)</param>
        /// <param name="state">The score state at that index</param>
        /// <returns>The performance attributes or null if the index is invalid</returns>
        public PerformanceAttributes? AtIndex(int index, ScoreState state)
        {
            if (index < 0 || index >= _totalHitObjects)
                return null;
            
            _currentIndex = index + 1;
            return CalculateAtIndex(index, state);
        }
        
        /// <summary>
        /// Skips to the last hit object and calculates its performance attributes.
        /// </summary>
        /// <param name="state">The final score state</param>
        /// <returns>The performance attributes for the last hit object or null if there are no hit objects</returns>
        public PerformanceAttributes? Last(ScoreState state)
        {
            if (_totalHitObjects == 0)
                return null;
            
            _currentIndex = _totalHitObjects;
            return CalculateAtIndex(_totalHitObjects - 1, state);
        }
        
        /// <summary>
        /// Creates a list of performance attributes for each specific accuracy value.
        /// </summary>
        /// <param name="accuracies">The accuracies to calculate for</param>
        /// <param name="misses">The number of misses</param>
        /// <param name="combo">The max combo reached</param>
        /// <returns>Dictionary mapping accuracies to performance attributes</returns>
        public Dictionary<double, PerformanceAttributes> ForAccuracies(IEnumerable<double> accuracies, int misses = 0, int? combo = null)
        {
            var result = new Dictionary<double, PerformanceAttributes>();
            
            // Skip to the end to ensure we calculate for the full map
            _currentIndex = _totalHitObjects;
            
            // Calculate difficulty attributes for the full map
            if (_lastDifficultyAttributes == null)
            {
                _lastDifficultyAttributes = CalculateDifficultyAtIndex(_totalHitObjects - 1);
            }
            
            // Create performance calculator
            var perfCalc = new Performance(_lastDifficultyAttributes)
                .Mods(_mods)
                .Misses(misses)
                .IsLazerScore(_isLazerScore);
            
            if (combo.HasValue)
            {
                perfCalc.Combo(combo.Value);
            }
            
            // Calculate for each accuracy
            foreach (var acc in accuracies)
            {
                var attrs = perfCalc.Clone().Accuracy(acc).Calculate();
                result[acc] = attrs;
            }
            
            return result;
        }
        
        /// <summary>
        /// Resets the state to recalculate from the beginning.
        /// </summary>
        public void Reset()
        {
            _currentIndex = 0;
        }
        
        private PerformanceAttributes CalculateAtIndex(int index, ScoreState state)
        {
            // Calculate difficulty attributes up to the given index
            var diffAttrs = CalculateDifficultyAtIndex(index);
            _lastDifficultyAttributes = diffAttrs;
            
            // Create a performance calculator using those difficulty attributes
            var perfCalc = new Performance(diffAttrs)
                .Mods(_mods)
                .ScoreState(state)
                .IsLazerScore(_isLazerScore);
            
            // Set passed objects to indicate partial play
            if (index < _totalHitObjects - 1)
            {
                perfCalc.PassedObjects(index + 1);
            }
            
            // Calculate and return the performance attributes
            return perfCalc.Calculate();
        }
        
        private DifficultyAttributes CalculateDifficultyAtIndex(int index)
        {
            // Configure the calculator to process only up to the given hit object index
            // This is specific to the implementation of GradualDifficulty
            
            // In a real implementation, we would modify the calculator to only consider
            // objects up to the specified index
            
            // For simplicity, we'll create a GradualDifficulty and calculate up to the index
            var gradualDiff = new GradualDifficulty(_calculator, _beatmap);
            
            for (int i = 0; i <= index; i++)
            {
                if (i == index)
                {
                    return gradualDiff.Current();
                }
                
                gradualDiff.Next();
            }
            
            // This should never happen if index is valid
            throw new InvalidOperationException("Failed to calculate difficulty at index");
        }
    }
}