using System;
using System.Linq;
using OsuPP.NET.GameModes.Osu;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Calculators
{
    /// <summary>
    /// Calculator for performance attributes.
    /// </summary>
    public class Performance
    {
        private DifficultyAttributes? _difficultyAttributes;
        private Mods _mods;
        private int? _combo;
        private int? _misses;
        private double? _accuracy;
        private int? _n300;
        private int? _n100;
        private int? _n50;
        private int? _nGeki;
        private int? _nKatu;
        private int? _osuSliderEndHits;
        private int? _osuLargeTickHits;
        private int? _osuSmallTickHits;
        private int? _passedObjects;
        private long? _score;
        private HitResultPriority _priority = HitResultPriority.BestCase;
        private ScoreState? _scoreState;
        private bool _isLazerScore = true;  // Default to lazer score calculation
        
        /// <summary>
        /// Creates a new performance calculator.
        /// </summary>
        public Performance()
        {
        }
        
        /// <summary>
        /// Creates a new performance calculator using the given difficulty attributes.
        /// </summary>
        /// <param name="difficultyAttributes">The difficulty attributes to use</param>
        public Performance(DifficultyAttributes difficultyAttributes)
        {
            _difficultyAttributes = difficultyAttributes;
            _mods = difficultyAttributes.Mods;
        }
        
        /// <summary>
        /// Creates a copy of this performance calculator.
        /// </summary>
        /// <returns>A new performance calculator with the same settings</returns>
        public Performance Clone()
        {
            return new Performance
            {
                _difficultyAttributes = _difficultyAttributes,
                _mods = _mods,
                _combo = _combo,
                _misses = _misses,
                _accuracy = _accuracy,
                _n300 = _n300,
                _n100 = _n100,
                _n50 = _n50,
                _nGeki = _nGeki,
                _nKatu = _nKatu,
                _osuSliderEndHits = _osuSliderEndHits,
                _osuLargeTickHits = _osuLargeTickHits,
                _osuSmallTickHits = _osuSmallTickHits,
                _passedObjects = _passedObjects,
                _score = _score,
                _priority = _priority,
                _scoreState = _scoreState?.Clone(),
                _isLazerScore = _isLazerScore
            };
        }
        
        /// <summary>
        /// Sets the difficulty attributes to use for the calculation.
        /// </summary>
        /// <param name="difficultyAttributes">The difficulty attributes to use</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance UseAttributes(DifficultyAttributes difficultyAttributes)
        {
            _difficultyAttributes = difficultyAttributes;
            return this;
        }
        
        /// <summary>
        /// Sets the mods to be applied for calculation.
        /// </summary>
        /// <param name="mods">The mods to apply</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance Mods(Mods mods)
        {
            _mods = mods;
            return this;
        }
        
        /// <summary>
        /// Sets the combo to use for the calculation.
        /// </summary>
        /// <param name="combo">The combo value</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance Combo(int combo)
        {
            _combo = Math.Max(0, combo);
            return this;
        }
        
        /// <summary>
        /// Sets the number of misses to use for the calculation.
        /// </summary>
        /// <param name="misses">The number of misses</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance Misses(int misses)
        {
            _misses = Math.Max(0, misses);
            return this;
        }
        
        /// <summary>
        /// Sets the accuracy to use for the calculation.
        /// </summary>
        /// <param name="accuracy">The accuracy as a percentage (0-100)</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance Accuracy(double accuracy)
        {
            _accuracy = Math.Clamp(accuracy, 0.0, 100.0);
            return this;
        }
        
        /// <summary>
        /// Sets the number of 300s to use for the calculation.
        /// </summary>
        /// <param name="n300">The number of 300s</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance N300(int n300)
        {
            _n300 = Math.Max(0, n300);
            return this;
        }
        
        /// <summary>
        /// Sets the number of 100s to use for the calculation.
        /// </summary>
        /// <param name="n100">The number of 100s</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance N100(int n100)
        {
            _n100 = Math.Max(0, n100);
            return this;
        }
        
        /// <summary>
        /// Sets the number of 50s to use for the calculation.
        /// </summary>
        /// <param name="n50">The number of 50s</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance N50(int n50)
        {
            _n50 = Math.Max(0, n50);
            return this;
        }
        
        /// <summary>
        /// Sets the number of Gekis (osu!mania 320s) to use for the calculation.
        /// </summary>
        /// <param name="nGeki">The number of Gekis</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance NGeki(int nGeki)
        {
            _nGeki = Math.Max(0, nGeki);
            return this;
        }
        
        /// <summary>
        /// Sets the number of Katus (osu!mania 200s, osu!ctb small missed drops) to use for the calculation.
        /// </summary>
        /// <param name="nKatu">The number of Katus</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance NKatu(int nKatu)
        {
            _nKatu = Math.Max(0, nKatu);
            return this;
        }
        
        /// <summary>
        /// Sets the number of slider end hits for osu!standard.
        /// </summary>
        /// <param name="sliderEndHits">The number of slider end hits</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance SliderEndHits(int sliderEndHits)
        {
            _osuSliderEndHits = Math.Max(0, sliderEndHits);
            return this;
        }
        
        /// <summary>
        /// Sets the number of large tick hits for osu!standard.
        /// </summary>
        /// <param name="largeTickHits">The number of large tick hits</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance LargeTickHits(int largeTickHits)
        {
            _osuLargeTickHits = Math.Max(0, largeTickHits);
            return this;
        }
        
        /// <summary>
        /// Sets the number of small tick hits for osu!standard.
        /// </summary>
        /// <param name="smallTickHits">The number of small tick hits</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance SmallTickHits(int smallTickHits)
        {
            _osuSmallTickHits = Math.Max(0, smallTickHits);
            return this;
        }
        
        /// <summary>
        /// Sets the score for osu!mania.
        /// </summary>
        /// <param name="score">The score value</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance Score(long score)
        {
            _score = Math.Max(0, score);
            return this;
        }
        
        /// <summary>
        /// Sets the number of passed objects for partial plays like fails.
        /// </summary>
        /// <param name="passedObjects">The number of passed objects</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance PassedObjects(int passedObjects)
        {
            _passedObjects = Math.Max(0, passedObjects);
            return this;
        }
        
        /// <summary>
        /// Sets the priority for hit results when accuracy is specified.
        /// </summary>
        /// <param name="priority">The priority to use</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance Priority(HitResultPriority priority)
        {
            _priority = priority;
            return this;
        }
        
        /// <summary>
        /// Sets whether to use osu!lazer or stable score calculation.
        /// </summary>
        /// <param name="isLazer">True for lazer, false for stable</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance IsLazerScore(bool isLazer)
        {
            _isLazerScore = isLazer;
            return this;
        }
        
        /// <summary>
        /// Sets the score state to use for calculation.
        /// </summary>
        /// <param name="scoreState">The score state to use</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance ScoreState(ScoreState scoreState)
        {
            _scoreState = scoreState;
            return this;
        }
        
        /// <summary>
        /// Sets all parameters from a score state.
        /// </summary>
        /// <param name="state">The score state to use</param>
        /// <returns>This calculator for method chaining</returns>
        public Performance UseScoreState(ScoreState state)
        {
            _combo = state.MaxCombo;
            _misses = state.Misses;
            _n300 = state.N300;
            _n100 = state.N100;
            _n50 = state.N50;
            _nGeki = state.NGeki;
            _nKatu = state.NKatu;
            _osuSliderEndHits = state.SliderEndHits;
            _osuLargeTickHits = state.OsuLargeTickHits;
            _osuSmallTickHits = state.OsuSmallTickHits;
            
            // Store original state for possible later use
            _scoreState = state.Clone();
            
            return this;
        }
        
        /// <summary>
        /// Creates a new instance for continued calculation from the current state.
        /// </summary>
        /// <returns>A new performance calculator with the same settings</returns>
        public Performance CreateNew()
        {
            return Clone();
        }
        
        /// <summary>
        /// Calculates performance attributes.
        /// </summary>
        /// <returns>Performance attributes based on the beatmap's mode</returns>
        public PerformanceAttributes Calculate()
        {
            if (_difficultyAttributes == null)
                throw new InvalidOperationException("Difficulty attributes not set");
            
            // Create the appropriate mode-specific calculator
            var calculator = CreateCalculator(_difficultyAttributes.Mode);
            
            // Set parameters
            calculator.SetDifficultyAttributes(_difficultyAttributes);
            calculator.SetMods(_mods);
            calculator.SetIsLazerScore(_isLazerScore);
            
            if (_scoreState != null)
            {
                calculator.SetScoreState(_scoreState);
            }
            else
            {
                // Process individual parameters if ScoreState wasn't provided
                if (_combo.HasValue)
                    calculator.SetCombo(_combo.Value);
                
                if (_misses.HasValue)
                    calculator.SetMisses(_misses.Value);
                
                if (_accuracy.HasValue)
                    calculator.SetAccuracy(_accuracy.Value);
                
                if (_n300.HasValue)
                    calculator.SetN300(_n300.Value);
                
                if (_n100.HasValue)
                    calculator.SetN100(_n100.Value);
                
                if (_n50.HasValue)
                    calculator.SetN50(_n50.Value);
                
                if (_nGeki.HasValue)
                    calculator.SetNGeki(_nGeki.Value);
                
                if (_nKatu.HasValue)
                    calculator.SetNKatu(_nKatu.Value);
                
                if (_osuSliderEndHits.HasValue)
                    calculator.SetSliderEndHits(_osuSliderEndHits.Value);
                
                if (_osuLargeTickHits.HasValue)
                    calculator.SetLargeTickHits(_osuLargeTickHits.Value);
                
                if (_osuSmallTickHits.HasValue)
                    calculator.SetSmallTickHits(_osuSmallTickHits.Value);
                
                if (_score.HasValue)
                    calculator.SetScore(_score.Value);
                
                if (_passedObjects.HasValue)
                    calculator.SetPassedObjects(_passedObjects.Value);
                
                calculator.SetPriority(_priority);
            }
            
            // Run calculation
            return calculator.Calculate();
        }
        
        private IPerformanceCalculator CreateCalculator(GameMode mode)
        {
            return mode switch
            {
                GameMode.Osu => new OsuPerformanceCalculator(),
              //  GameMode.Taiko => new TaikoPerformanceCalculator(),
              //  GameMode.Catch => new CatchPerformanceCalculator(),
              //  GameMode.Mania => new ManiaPerformanceCalculator(),
                _ => throw new ArgumentException($"Unknown game mode: {mode}")
            };
        }
    }
    
    /// <summary>
    /// Interface for performance calculators.
    /// </summary>
    internal interface IPerformanceCalculator
    {
        void SetDifficultyAttributes(DifficultyAttributes attributes);
        void SetMods(Mods mods);
        void SetCombo(int combo);
        void SetMisses(int misses);
        void SetAccuracy(double accuracy);
        void SetN300(int n300);
        void SetN100(int n100);
        void SetN50(int n50);
        void SetNGeki(int nGeki);
        void SetNKatu(int nKatu);
        void SetSliderEndHits(int sliderEndHits);
        void SetLargeTickHits(int largeTickHits);
        void SetSmallTickHits(int smallTickHits);
        void SetScore(long score);
        void SetPassedObjects(int passedObjects);
        void SetPriority(HitResultPriority priority);
        void SetIsLazerScore(bool isLazerScore);
        void SetScoreState(ScoreState? scoreState);
        PerformanceAttributes Calculate();
    }
}