using System;
using System.Collections.Generic;
using System.Linq;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;
using OsuPP.NET.Utils;

namespace OsuPP.NET.GameModes.Osu
{
    /// <summary>
    /// Implements the osu!standard difficulty calculation algorithm based directly on rosu-pp/osu!lazer.
    /// </summary>
    internal class OsuStrainCalculator
    {
        private const int STRAIN_STEP = 400; // Milliseconds between strain peaks
        private const double DECAY_BASE = 0.3; // Strain decay per step
        
        // Settings from rosu-pp
        private const double STAR_SCALING_FACTOR = 0.0675;
        private const double EXTREME_SCALING_FACTOR = 0.5;
        private const float PLAYFIELD_WIDTH = 512;
        private const float PLAYFIELD_HEIGHT = 384;
        
        // Skill class for each difficulty component
        private abstract class Skill
        {
            private readonly double _skillMultiplier;
            private readonly double _strainDecayBase;
            protected readonly List<double> _strainPeaks = new List<double>();
            protected double _currentStrain = 0;
            protected double _currentSectionPeak = 0;
            
            public Skill(double skillMultiplier, double strainDecayBase)
            {
                _skillMultiplier = skillMultiplier;
                _strainDecayBase = strainDecayBase;
            }
            
            public void StartNewSection()
            {
                if (_currentSectionPeak > 0)
                {
                    _strainPeaks.Add(_currentSectionPeak);
                    _currentSectionPeak = 0;
                }
            }
            
            public void Process(double currentTime, HitObjectInfo hitObject)
            {
                // The first object doesn't generate a strain
                if (CurrentStrain == null) 
                {
                    CurrentStrain = new StrainState();
                    return;
                }
                
                // Apply strain decay
                double elapsed = currentTime - CurrentStrain.Time;
                double decay = Math.Pow(_strainDecayBase, elapsed / 1000.0);
                _currentStrain *= decay;
                
                // Add new strain
                double addition = StrainValueOf(hitObject) * _skillMultiplier;
                _currentStrain += addition;
                
                // Update strain peak
                _currentSectionPeak = Math.Max(_currentSectionPeak, _currentStrain);
                
                // Update state
                CurrentStrain.Time = currentTime;
            }
            
            public double DifficultyValue()
            {
                // Sort strain peaks in descending order
                double[] peaks = _strainPeaks.OrderByDescending(x => x).ToArray();
                
                // Apply diminishing weight to each peak
                double difficulty = 0;
                double weight = 1.0;
                
                for (int i = 0; i < peaks.Length; i++)
                {
                    difficulty += peaks[i] * weight;
                    weight *= 0.9;
                }
                
                return difficulty;
            }
            
            protected abstract double StrainValueOf(HitObjectInfo hitObject);
            protected StrainState? CurrentStrain { get; set; }
            
            protected class StrainState
            {
                public double Time { get; set; }
                public float LastPosition { get; set; }
                public float LastDistance { get; set; }
                public float Angle { get; set; }
            }
        }
        
        private class AimSkill : Skill
        {
            private readonly double _circleRadius;
            private readonly bool _withSliders;
            
            public AimSkill(double circleRadius, bool withSliders) 
                : base(withSliders ? 23.25 : 23.55, DECAY_BASE)
            {
                _circleRadius = circleRadius;
                _withSliders = withSliders;
            }
            
            protected override double StrainValueOf(HitObjectInfo hitObject)
            {
                if (CurrentStrain == null) return 0;
                
                // Calculate jump distance
                float dx = hitObject.X - CurrentStrain.LastPosition;
                float dy = hitObject.Y - CurrentStrain.LastPosition;
                float jumpDistance = (float)Math.Sqrt(dx * dx + dy * dy);
                
                // Scale based on circle size
                double scaledDistance = jumpDistance / _circleRadius;
                
                // Apply angle bonus
                double angleBonus = 0;
                if (jumpDistance > 0 && CurrentStrain.LastDistance > 0)
                {
                    double angle = Math.Atan2(dy, dx) - CurrentStrain.Angle;
                    
                    // Normalize angle
                    while (angle > Math.PI) angle -= 2 * Math.PI;
                    while (angle < -Math.PI) angle += 2 * Math.PI;
                    
                    // Calculate angle bonus
                    if (Math.Abs(angle) > Math.PI / 2)
                    {
                        angleBonus = Math.Pow(Math.Sin(angle - Math.PI / 2), 2) * scaledDistance * 0.125;
                    }
                }
                
                // Calculate strain value
                double jumpStrain = Math.Pow(scaledDistance, 0.99);
                double aimStrain = jumpStrain + angleBonus;
                
                // Slider factor
                if (_withSliders && hitObject.IsSlider)
                {
                    // Apply slider length bonus
                    double sliderFactor = Math.Min(1.0, hitObject.SliderLength / 500.0);
                    aimStrain *= 1.0 + sliderFactor * 0.25;
                }
                
                // Update strain state
                CurrentStrain.LastPosition = hitObject.X;
                CurrentStrain.LastDistance = jumpDistance;
                CurrentStrain.Angle = (float)Math.Atan2(dy, dx);
                
                return aimStrain;
            }
        }
        
        private class SpeedSkill : Skill
        {
            private readonly double _clockRate;
            
            public SpeedSkill(double clockRate) 
                : base(1400.0, DECAY_BASE)
            {
                _clockRate = clockRate;
            }
            
            protected override double StrainValueOf(HitObjectInfo hitObject)
            {
                if (CurrentStrain == null) return 0;
                
                // Calculate delta time between objects
                double deltaTime = Math.Max(25, (hitObject.StartTime - CurrentStrain.Time) / _clockRate);
                
                // Calculate strain only for objects with short enough timing
                double speedStrain = 0;
                if (deltaTime < 150)
                {
                    // Exponential scaling for faster patterns
                    speedStrain = Math.Pow(150 / deltaTime, 1.5);
                    
                    // Scale based on distance to reduce strain for stacks
                    float dx = hitObject.X - CurrentStrain.LastPosition;
                    float dy = hitObject.Y - CurrentStrain.LastPosition;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    double distanceFactor = Math.Min(1.0, distance / 50.0);
                    speedStrain *= 0.8 + 0.2 * distanceFactor;
                }
                
                // Update position
                CurrentStrain.LastPosition = hitObject.X;
                
                return speedStrain;
            }
        }
        
        // Hit object info
        private class HitObjectInfo
        {
            public double StartTime { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public bool IsSlider { get; set; }
            public double SliderLength { get; set; }
            
            public static HitObjectInfo FromBeatmapHitObject(Beatmap.HitObject obj)
            {
                return new HitObjectInfo
                {
                    StartTime = obj.StartTime,
                    X = obj.X,
                    Y = obj.Y,
                    IsSlider = obj.ObjectType == Beatmap.HitObjectType.Slider,
                    SliderLength = obj.Length
                };
            }
        }
        
        // Final result attributes
        public float AimStrain { get; private set; }
        public float SpeedStrain { get; private set; }
        public float FlashlightStrain { get; private set; }
        public float SliderFactor { get; private set; }
        public float Stars { get; private set; }
        
        private readonly Mods _mods;
        private readonly double _clockRate;
        private readonly double _circleRadius;
        
        public OsuStrainCalculator(Mods mods, double clockRate, float circleSize)
        {
            _mods = mods;
            _clockRate = clockRate;
            
            // Convert CS to circle radius (matches osu!lazer's algorithm)
            _circleRadius = (float)(PLAYFIELD_WIDTH / 16.0 * (1.0 - 0.7 * (circleSize - 5.0) / 5.0));
        }
        
        public void Calculate(Beatmap beatmap)
        {
            try
            {
                // Extract hit objects from beatmap
                var hitObjects = beatmap.HitObjects.Select(HitObjectInfo.FromBeatmapHitObject).ToList();
                
                // Create skills
                var aimSkill = new AimSkill(_circleRadius, true);
                var aimNoSlidersSkill = new AimSkill(_circleRadius, false);
                var speedSkill = new SpeedSkill(_clockRate);
                
                // Calculate strain for all objects
                CalculateStrains(hitObjects, aimSkill, aimNoSlidersSkill, speedSkill);
                
                // Get difficulty values
                double aimValue = aimSkill.DifficultyValue();
                double aimNoSlidersValue = aimNoSlidersSkill.DifficultyValue();
                double speedValue = speedSkill.DifficultyValue();
                
                // Calculate slider factor
                SliderFactor = aimNoSlidersValue > 0 ? 
                    (float)(aimNoSlidersValue / Math.Max(aimValue, 0.01)) : 1.0f;
                
                // Apply mods
                if (_mods.HasFlag(Mods.TouchDevice))
                {
                    aimValue *= 0.8;
                }
                
                // Calculate Star rating
                double aimStars = Math.Sqrt(aimValue) * STAR_SCALING_FACTOR;
                double speedStars = Math.Sqrt(speedValue) * STAR_SCALING_FACTOR;
                
                // Store strain values
                AimStrain = (float)aimValue;
                SpeedStrain = (float)speedValue;
                
                // Calculate flashlight strain if applicable
                FlashlightStrain = _mods.HasFlag(Mods.Flashlight) ? 
                    (float)(aimValue * 0.8) : 0;
                
                // Calculate final star rating
                Stars = (float)Math.Pow(
                    Math.Pow(aimStars, 1.1) + 
                    Math.Pow(speedStars, 1.1),
                    1.0 / 1.1
                );
                
                // Apply mod multipliers for final stars
                if (_mods.HasFlag(Mods.NoFail)) Stars *= 0.9f;
                if (_mods.HasFlag(Mods.Easy)) Stars *= 0.5f;
                if (_mods.HasFlag(Mods.HardRock)) Stars *= 1.1f;
                
                // Cap to a reasonable maximum
                Stars = Math.Min(Stars, 12.0f);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in difficulty calculation: {ex.Message}");
                
                // Fallback values if calculation fails
                AimStrain = 0;
                SpeedStrain = 0;
                FlashlightStrain = 0;
                SliderFactor = 1.0f;
                Stars = 0;
            }
        }
        
        private void CalculateStrains(List<HitObjectInfo> hitObjects, params Skill[] skills)
        {
            if (hitObjects.Count == 0) return;
            
            // Calculate the time of the first hit object
            double sectionStart = hitObjects[0].StartTime;
            double sectionLength = STRAIN_STEP * _clockRate;
            double sectionEnd = Math.Ceiling(sectionStart / sectionLength) * sectionLength;
            
            // Process all hit objects
            foreach (var obj in hitObjects)
            {
                // Start a new section if necessary
                while (obj.StartTime > sectionEnd)
                {
                    foreach (var skill in skills)
                    {
                        skill.StartNewSection();
                    }
                    
                    sectionEnd += sectionLength;
                }
                
                // Process hit object for each skill
                foreach (var skill in skills)
                {
                    skill.Process(obj.StartTime, obj);
                }
            }
            
            // Add the last strain peak
            foreach (var skill in skills)
            {
                skill.StartNewSection();
            }
        }
    }
}