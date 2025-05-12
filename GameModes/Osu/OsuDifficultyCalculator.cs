using System;
using OsuPP.NET.Calculators;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;
using OsuPP.NET.Utils;

namespace OsuPP.NET.GameModes.Osu
{
    /// <summary>
    /// Calculator for osu!standard difficulty attributes.
    /// </summary>
    internal class OsuDifficultyCalculator : IDifficultyCalculator
    {
        private Beatmap? _beatmap;
        private Mods _mods;
        private double _clockRate = 1.0;
        private float _ar;
        private float _cs;
        private float _hp;
        private float _od;
        private bool _areAttrsSet = false;
        
        public void SetBeatmap(Beatmap beatmap)
        {
            _beatmap = beatmap;
        }
        
        public void SetMods(Mods mods)
        {
            _mods = mods;
        }
        
        public void SetClockRate(double clockRate)
        {
            _clockRate = clockRate;
        }
        
        public void SetAR(float ar)
        {
            _ar = ar;
            _areAttrsSet = true;
        }
        
        public void SetCS(float cs)
        {
            _cs = cs;
            _areAttrsSet = true;
        }
        
        public void SetHP(float hp)
        {
            _hp = hp;
            _areAttrsSet = true;
        }
        
        public void SetOD(float od)
        {
            _od = od;
            _areAttrsSet = true;
        }
        
        public DifficultyAttributes Calculate()
        {
            if (_beatmap == null)
                throw new InvalidOperationException("Beatmap not set");
            
            try
            {
                // Get beatmap attributes from beatmap or use provided values
                if (!_areAttrsSet)
                {
                    _ar = _beatmap.ApproachRate;
                    _cs = _beatmap.CircleSize;
                    _hp = _beatmap.HpDrainRate;
                    _od = _beatmap.OverallDifficulty;
                }
                
                // Apply mods effects on difficulty params
                float csWithMods = ModUtils.ApplyCSMods(_cs, _mods);
                float arWithMods = ModUtils.ApplyARMods(_ar, _mods, _clockRate);
                float hpWithMods = ModUtils.ApplyHPMods(_hp, _mods);
                float odWithMods = ModUtils.ApplyODMods(_od, _mods, _clockRate);
                
                // Calculate MS values
                float hitWindowGreat = MathUtils.OverallDifficultyToHitWindow(odWithMods);
                float preemptTime = MathUtils.ApproachRateToPreemptTime(arWithMods);
                
                // Calculate max combo based on hit objects
                int maxCombo = MapUtils.CalculateMaxCombo(_beatmap);
                
                // Create strain calculator from rosu-pp
                var strainCalculator = new OsuStrainCalculator(_mods, _clockRate, csWithMods);
                strainCalculator.Calculate(_beatmap);
                
                // Get strain values
                float aimStrain = strainCalculator.AimStrain;
                float speedStrain = strainCalculator.SpeedStrain;
                float flashlightStrain = strainCalculator.FlashlightStrain;
                float sliderFactor = strainCalculator.SliderFactor;
                float stars = strainCalculator.Stars;
                
                // Make sure values are not NaN or Infinity
                if (float.IsNaN(stars) || float.IsInfinity(stars))
                {
                    stars = CalculateFallbackStars(_beatmap, csWithMods, arWithMods, odWithMods);
                    aimStrain = stars * 0.6f;
                    speedStrain = stars * 0.4f;
                    flashlightStrain = 0;
                    sliderFactor = 1.0f;
                }
                
                // Create attributes
                return new OsuDifficultyAttributes
                {
                    Mode = GameMode.Osu,
                    Mods = _mods,
                    Stars = stars,
                    AimStrain = aimStrain,
                    SpeedStrain = speedStrain,
                    FlashlightStrain = flashlightStrain,
                    SliderFactor = sliderFactor,
                    ApproachRate = arWithMods,
                    OverallDifficulty = odWithMods,
                    ClockRate = _clockRate,
                    PreemptTime = preemptTime,
                    HitWindowGreat = hitWindowGreat,
                    MaxCombo = maxCombo,
                    SpeedNoteCount = maxCombo
                };
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in difficulty calculation: {ex.Message}");
                
                // Fallback difficulty calculation
                float fallbackStars = CalculateFallbackStars(_beatmap, _cs, _ar, _od);
                
                return new OsuDifficultyAttributes
                {
                    Mode = GameMode.Osu,
                    Mods = _mods,
                    Stars = fallbackStars,
                    AimStrain = fallbackStars * 0.6f,
                    SpeedStrain = fallbackStars * 0.4f,
                    FlashlightStrain = 0f,
                    SliderFactor = 1.0f,
                    ApproachRate = _ar,
                    OverallDifficulty = _od,
                    ClockRate = _clockRate,
                    PreemptTime = MathUtils.ApproachRateToPreemptTime(_ar),
                    HitWindowGreat = MathUtils.OverallDifficultyToHitWindow(_od),
                    MaxCombo = _beatmap.CountHitObjects,
                    SpeedNoteCount = _beatmap.CountHitObjects
                };
            }
        }
        
        private float CalculateFallbackStars(Beatmap beatmap, float cs, float ar, float od)
        {
            // Simple fallback based on beatmap attributes and hit object density
            int objectCount = beatmap.CountHitObjects;
            double approxLength = beatmap.HitObjects.Count > 0 ? 
                (beatmap.HitObjects[beatmap.HitObjects.Count - 1].StartTime - 
                 beatmap.HitObjects[0].StartTime) / 1000.0 : 60.0;
            
            // Object density per second
            double density = objectCount / Math.Max(1.0, approxLength);
            
            // Calculate an approximate star rating based on difficulty settings and density
            double baseRating = (cs + ar + od) / 3.0 * 0.8;
            double densityFactor = Math.Min(1.0, Math.Sqrt(density / 4.0));
            
            return (float)Math.Min(6.0, baseRating + densityFactor * 1.4);
        }
    }
}