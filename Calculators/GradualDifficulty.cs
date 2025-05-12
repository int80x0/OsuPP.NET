using System;
using System.Collections;
using System.Collections.Generic;
using OsuPP.NET.Models;

namespace OsuPP.NET.Calculators
{
    /// <summary>
    /// Provides a way to calculate difficulty attributes gradually for each hit object.
    /// </summary>
    public class GradualDifficulty : IEnumerable<DifficultyAttributes>
    {
        private readonly IDifficultyCalculator _calculator;
        private readonly Beatmap _beatmap;
        private int _currentIndex;
        private int _totalHitObjects;
        private DifficultyAttributes? _currentAttributes;
        
        /// <summary>
        /// Creates a new gradual difficulty calculator.
        /// </summary>
        /// <param name="calculator">The difficulty calculator to use</param>
        /// <param name="beatmap">The beatmap to calculate for</param>
        internal GradualDifficulty(IDifficultyCalculator calculator, Beatmap beatmap)
        {
            _calculator = calculator;
            _beatmap = beatmap;
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
        /// Returns the current difficulty attributes without advancing the index.
        /// </summary>
        /// <returns>The current difficulty attributes or throws if no calculation has been done yet</returns>
        public DifficultyAttributes Current()
        {
            if (_currentAttributes == null)
                throw new InvalidOperationException("No calculation has been done yet");
            
            return _currentAttributes;
        }
        
        /// <summary>
        /// Calculates difficulty attributes for the next hit object.
        /// </summary>
        /// <returns>The difficulty attributes or null if all objects have been processed</returns>
        public DifficultyAttributes? Next()
        {
            if (_currentIndex >= _totalHitObjects)
                return null;
            
            _currentAttributes = CalculateAtIndex(_currentIndex);
            _currentIndex++;
            
            return _currentAttributes;
        }
        
        /// <summary>
        /// Calculates difficulty attributes for the next N hit objects.
        /// </summary>
        /// <param name="count">The number of objects to skip</param>
        /// <returns>The difficulty attributes or null if all objects have been processed</returns>
        public DifficultyAttributes? NextN(int count)
        {
            if (_currentIndex >= _totalHitObjects)
                return null;
            
            _currentIndex = Math.Min(_currentIndex + count, _totalHitObjects);
            _currentAttributes = CalculateAtIndex(_currentIndex - 1);
            
            return _currentAttributes;
        }
        
        /// <summary>
        /// Calculates difficulty attributes at a specific index.
        /// </summary>
        /// <param name="index">The index to calculate at (0-based)</param>
        /// <returns>The difficulty attributes or null if the index is invalid</returns>
        public DifficultyAttributes? AtIndex(int index)
        {
            if (index < 0 || index >= _totalHitObjects)
                return null;
            
            _currentIndex = index + 1;
            _currentAttributes = CalculateAtIndex(index);
            
            return _currentAttributes;
        }
        
        /// <summary>
        /// Enumerates all difficulty attributes for each hit object.
        /// </summary>
        /// <returns>An enumeration of difficulty attributes</returns>
        public IEnumerator<DifficultyAttributes> GetEnumerator()
        {
            Reset();
            
            while (_currentIndex < _totalHitObjects)
            {
                _currentAttributes = CalculateAtIndex(_currentIndex);
                _currentIndex++;
                
                yield return _currentAttributes;
            }
        }
        
        /// <summary>
        /// Enumerates all difficulty attributes for each hit object.
        /// </summary>
        /// <returns>An enumeration of difficulty attributes</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        /// <summary>
        /// Skips to the last hit object and calculates its difficulty attributes.
        /// </summary>
        /// <returns>The difficulty attributes for the last hit object or null if there are no hit objects</returns>
        public DifficultyAttributes? Last()
        {
            if (_totalHitObjects == 0)
                return null;
            
            _currentIndex = _totalHitObjects;
            _currentAttributes = CalculateAtIndex(_totalHitObjects - 1);
            
            return _currentAttributes;
        }
        
        /// <summary>
        /// Resets the state to recalculate from the beginning.
        /// </summary>
        public void Reset()
        {
            _currentIndex = 0;
            _currentAttributes = null;
        }
        
        private DifficultyAttributes CalculateAtIndex(int index)
        {
            // In a real implementation, this would configure the calculator
            // to process only up to the given hit object index
            
            // For this simplified implementation, we're creating a partial beatmap
            // with only the hit objects up to the given index
            var partialBeatmap = CreatePartialBeatmap(_beatmap, index);
            
            // Set the beatmap to the calculator
            _calculator.SetBeatmap(partialBeatmap);
            
            // Calculate and return the difficulty attributes
            return _calculator.Calculate();
        }
        
        private Beatmap CreatePartialBeatmap(Beatmap original, int maxIndex)
        {
            // Create a copy of the beatmap with hit objects up to the max index
            // This is a simplified approach - a real implementation would modify
            // the calculator to respect the max index without copying the beatmap
            
            var partialBeatmap = original.Clone();
            
            // Only include hit objects up to the max index
            // Note: This is a conceptual example - the actual implementation
            // depends on how Beatmap.Clone() and Beatmap.HitObjects are implemented
            
            // The pruning of hit objects would happen here
            
            return partialBeatmap;
        }
    }
}