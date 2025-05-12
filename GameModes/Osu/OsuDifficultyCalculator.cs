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
        }
        
        public void SetCS(float cs)
        {
            _cs = cs;
        }
        
        public void SetHP(float hp)
        {
            _hp = hp;
        }
        
        public void SetOD(float od)
        {
            _od = od;
        }
        
        public DifficultyAttributes Calculate()
        {
            if (_beatmap == null)
                throw new InvalidOperationException("Beatmap not set");
            
            // This is a simplified implementation of the osu!standard difficulty calculation
            // In a real implementation, this would be much more complex, replicating osu!lazer's algorithm
            // The actual calculation would involve:
            // - Processing hit objects to build strain arrays
            // - Calculating aim, speed and flashlight strains
            // - Computing various slider-related factors
            // - Accounting for modifiers like HR, HD, DT, etc.
            
            // For now, we'll just create a dummy implementation
            
            // Calculate preempt (AR) and hit window (OD) in milliseconds
            float preempt = MathUtils.ApproachRateToPreemptTime(_ar);
            float hitWindow = MathUtils.OverallDifficultyToHitWindow(_od);
            
            // Calculate aim and speed strains
            // This would normally involve a complex algorithm processing all hit objects
            // For this dummy implementation, we'll use placeholder values
            float aimStrain = 3.5f * (_cs / 5.0f) * (float)_clockRate;
            float speedStrain = 2.8f * (_od / 10.0f) * (float)_clockRate;
            
            // Apply mod multipliers
            if (_mods.HasFlag(Mods.Hidden))
            {
                aimStrain *= 1.1f;
            }
            
            if (_mods.HasFlag(Mods.Flashlight))
            {
                aimStrain *= 1.3f;
            }
            
            if (_mods.HasFlag(Mods.HardRock))
            {
                aimStrain *= 1.1f;
                speedStrain *= 1.1f;
            }
            
            // Calculate slider factor (simplified)
            float sliderFactor = 0.15f;
            
            // Calculate total star rating
            float starRating = (float)Math.Pow(Math.Pow(aimStrain, 1.1) + Math.Pow(speedStrain, 1.1), 1.0 / 1.1);
            
            // Calculate max combo (simplified)
            int maxCombo = _beatmap.CountHitObjects;
            
            // Create and return the attributes
            return new OsuDifficultyAttributes
            {
                Mode = GameMode.Osu,
                Mods = _mods,
                Stars = starRating,
                AimStrain = aimStrain,
                SpeedStrain = speedStrain,
                FlashlightStrain = _mods.HasFlag(Mods.Flashlight) ? aimStrain * 0.4f : 0f,
                SliderFactor = sliderFactor,
                ApproachRate = _ar,
                OverallDifficulty = _od,
                ClockRate = _clockRate,
                PreemptTime = preempt,
                HitWindowGreat = hitWindow,
                MaxCombo = maxCombo,
                SpeedNoteCount = maxCombo
            };
        }
    }
}