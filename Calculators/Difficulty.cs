using System;
using OsuPP.NET.GameModes.Osu;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;
using OsuPP.NET.Utils;

namespace OsuPP.NET.Calculators
{
    /// <summary>
    /// Calculator for difficulty attributes.
    /// </summary>
    public class Difficulty
    {
        private Mods _mods;
        private double _clockRate = 1.0;
        private float? _ar;
        private bool _arWithMods;
        private float? _cs;
        private bool _csWithMods;
        private float? _hp;
        private bool _hpWithMods;
        private float? _od;
        private bool _odWithMods;
        private bool _customSpeed;
        private GameMode? _convertToMode;
        private string? _convertToArg;
        
        /// <summary>
        /// Creates a new difficulty calculator with default settings.
        /// </summary>
        public Difficulty()
        {
        }
        
        /// <summary>
        /// Static factory method to create a difficulty calculator from existing attributes.
        /// </summary>
        /// <param name="attributes">The difficulty attributes to use as a base</param>
        /// <returns>A new difficulty calculator with settings from the attributes</returns>
        public static Difficulty StaticDifficulty(DifficultyAttributes attributes)
        {
            var diff = new Difficulty()
                .Mods(attributes.Mods)
                .ClockRate(attributes.ClockRate)
                .ApproachRate(attributes.ApproachRate, true)
                .OverallDifficulty(attributes.OverallDifficulty, true);
            
            return diff;
        }
        
        /// <summary>
        /// Static factory method to convert a beatmap to another game mode.
        /// </summary>
        /// <param name="beatmap">The beatmap to convert</param>
        /// <param name="mode">The target game mode</param>
        /// <param name="arg">Optional conversion argument (e.g., key count for mania)</param>
        /// <returns>The converted beatmap</returns>
        public static Beatmap Convert(Beatmap beatmap, GameMode mode, string? arg = null)
        {
            if (beatmap.Mode == mode)
                return beatmap.Clone();
            
            return beatmap.Convert(mode);
        }
        
        /// <summary>
        /// Sets the mods to be applied for calculation.
        /// </summary>
        /// <param name="mods">The mods to apply</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty Mods(Mods mods)
        {
            _mods = mods;
            return this;
        }
        
        /// <summary>
        /// Sets the clock rate for calculation (affected by DT/HT/custom rate).
        /// </summary>
        /// <param name="clockRate">The clock rate to apply</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty ClockRate(double clockRate)
        {
            if (clockRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(clockRate), "Clock rate must be positive");
            
            _clockRate = clockRate;
            return this;
        }
        
        /// <summary>
        /// Sets whether to use a custom speed (clock rate) instead of deriving it from mods.
        /// </summary>
        /// <param name="enabled">Whether to use custom speed</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty CustomSpeed(bool enabled)
        {
            _customSpeed = enabled;
            return this;
        }
        
        /// <summary>
        /// Sets a custom approach rate value.
        /// </summary>
        /// <param name="ar">The AR value to use</param>
        /// <param name="withMods">Whether the given AR already includes mod effects</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty ApproachRate(float ar, bool withMods = false)
        {
            _ar = ar;
            _arWithMods = withMods;
            return this;
        }
        
        /// <summary>
        /// Sets a custom circle size value.
        /// </summary>
        /// <param name="cs">The CS value to use</param>
        /// <param name="withMods">Whether the given CS already includes mod effects</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty CircleSize(float cs, bool withMods = false)
        {
            _cs = cs;
            _csWithMods = withMods;
            return this;
        }
        
        /// <summary>
        /// Sets a custom HP drain value.
        /// </summary>
        /// <param name="hp">The HP value to use</param>
        /// <param name="withMods">Whether the given HP already includes mod effects</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty DrainRate(float hp, bool withMods = false)
        {
            _hp = hp;
            _hpWithMods = withMods;
            return this;
        }
        
        /// <summary>
        /// Sets a custom overall difficulty value.
        /// </summary>
        /// <param name="od">The OD value to use</param>
        /// <param name="withMods">Whether the given OD already includes mod effects</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty OverallDifficulty(float od, bool withMods = false)
        {
            _od = od;
            _odWithMods = withMods;
            return this;
        }
        
        /// <summary>
        /// Sets the target mode to convert the beatmap to before calculation.
        /// </summary>
        /// <param name="mode">The target game mode</param>
        /// <param name="arg">Optional conversion argument (e.g., key count for mania)</param>
        /// <returns>This calculator for method chaining</returns>
        public Difficulty ConvertTo(GameMode mode, string? arg = null)
        {
            _convertToMode = mode;
            _convertToArg = arg;
            return this;
        }
        
        /// <summary>
        /// Calculates difficulty attributes for the given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to calculate for</param>
        /// <returns>Difficulty attributes based on the beatmap's mode</returns>
        public DifficultyAttributes Calculate(Beatmap beatmap)
        {
            // Convert beatmap if needed
            Beatmap mapToUse = beatmap;
            if (_convertToMode.HasValue && _convertToMode.Value != beatmap.Mode)
            {
                mapToUse = Convert(beatmap, _convertToMode.Value, _convertToArg);
            }
            
            // Calculate actual values including mods
            float ar = CalculateAR(mapToUse);
            float cs = CalculateCS(mapToUse);
            float hp = CalculateHP(mapToUse);
            float od = CalculateOD(mapToUse);
            
            // Calculate clock rate
            double clockRate = _customSpeed ? _clockRate : CalculateClockRate();
            
            // Create the appropriate mode-specific calculator
            var calculator = CreateCalculator(mapToUse.Mode);
            
            // Set parameters
            calculator.SetBeatmap(mapToUse);
            calculator.SetMods(_mods);
            calculator.SetClockRate(clockRate);
            calculator.SetAR(ar);
            calculator.SetCS(cs);
            calculator.SetHP(hp);
            calculator.SetOD(od);
            
            // Run calculation
            return calculator.Calculate();
        }
        
        /// <summary>
        /// Creates a gradual difficulty calculator for the given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to calculate for</param>
        /// <returns>A gradual difficulty calculator</returns>
        public GradualDifficulty GradualDifficulty(Beatmap beatmap)
        {
            // Convert beatmap if needed
            Beatmap mapToUse = beatmap;
            if (_convertToMode.HasValue && _convertToMode.Value != beatmap.Mode)
            {
                mapToUse = Convert(beatmap, _convertToMode.Value, _convertToArg);
            }
            
            // Calculate actual values including mods
            float ar = CalculateAR(mapToUse);
            float cs = CalculateCS(mapToUse);
            float hp = CalculateHP(mapToUse);
            float od = CalculateOD(mapToUse);
            
            // Calculate clock rate
            double clockRate = _customSpeed ? _clockRate : CalculateClockRate();
            
            // Create the appropriate mode-specific calculator
            var calculator = CreateCalculator(mapToUse.Mode);
            
            // Set parameters
            calculator.SetBeatmap(mapToUse);
            calculator.SetMods(_mods);
            calculator.SetClockRate(clockRate);
            calculator.SetAR(ar);
            calculator.SetCS(cs);
            calculator.SetHP(hp);
            calculator.SetOD(od);
            
            // Create gradual calculator
            return new GradualDifficulty(calculator, mapToUse);
        }
        
        /// <summary>
        /// Creates a gradual performance calculator for the given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to calculate for</param>
        /// <param name="isLazerScore">Whether to use osu!lazer score calculation</param>
        /// <returns>A gradual performance calculator</returns>
        public GradualPerformance GradualPerformance(Beatmap beatmap, bool isLazerScore = true)
        {
            // Convert beatmap if needed
            Beatmap mapToUse = beatmap;
            if (_convertToMode.HasValue && _convertToMode.Value != beatmap.Mode)
            {
                mapToUse = Convert(beatmap, _convertToMode.Value, _convertToArg);
            }
            
            // Calculate actual values including mods
            float ar = CalculateAR(mapToUse);
            float cs = CalculateCS(mapToUse);
            float hp = CalculateHP(mapToUse);
            float od = CalculateOD(mapToUse);
            
            // Calculate clock rate
            double clockRate = _customSpeed ? _clockRate : CalculateClockRate();
            
            // Create the appropriate mode-specific calculator
            var calculator = CreateCalculator(mapToUse.Mode);
            
            // Set parameters
            calculator.SetBeatmap(mapToUse);
            calculator.SetMods(_mods);
            calculator.SetClockRate(clockRate);
            calculator.SetAR(ar);
            calculator.SetCS(cs);
            calculator.SetHP(hp);
            calculator.SetOD(od);
            
            // Create gradual performance calculator
            return new GradualPerformance(calculator, mapToUse, _mods, isLazerScore);
        }
        
        #region Helper Methods
        
        private float CalculateAR(Beatmap beatmap)
        {
            float ar = _ar ?? beatmap.ApproachRate;
            
            if (!_arWithMods)
            {
                ar = ModUtils.ApplyARMods(ar, _mods, _clockRate);
            }
            
            return ar;
        }
        
        private float CalculateCS(Beatmap beatmap)
        {
            float cs = _cs ?? beatmap.CircleSize;
            
            if (!_csWithMods)
            {
                cs = ModUtils.ApplyCSMods(cs, _mods);
            }
            
            return cs;
        }
        
        private float CalculateHP(Beatmap beatmap)
        {
            float hp = _hp ?? beatmap.HpDrainRate;
            
            if (!_hpWithMods)
            {
                hp = ModUtils.ApplyHPMods(hp, _mods);
            }
            
            return hp;
        }
        
        private float CalculateOD(Beatmap beatmap)
        {
            float od = _od ?? beatmap.OverallDifficulty;
            
            if (!_odWithMods)
            {
                od = ModUtils.ApplyODMods(od, _mods, _clockRate);
            }
            
            return od;
        }
        
        private double CalculateClockRate()
        {
            return ModUtils.GetClockRate(_mods);
        }
        
        private IDifficultyCalculator CreateCalculator(GameMode mode)
        {
            return mode switch
            {
                GameMode.Osu => new OsuDifficultyCalculator(),
             //   GameMode.Taiko => new TaikoDifficultyCalculator(),
             //   GameMode.Catch => new CatchDifficultyCalculator(),
             //   GameMode.Mania => new ManiaDifficultyCalculator(),
                _ => throw new ArgumentException($"Unknown game mode: {mode}")
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for difficulty calculators.
    /// </summary>
    internal interface IDifficultyCalculator
    {
        void SetBeatmap(Beatmap beatmap);
        void SetMods(Mods mods);
        void SetClockRate(double clockRate);
        void SetAR(float ar);
        void SetCS(float cs);
        void SetHP(float hp);
        void SetOD(float od);
        DifficultyAttributes Calculate();
    }
}