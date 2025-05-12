using System;
using OsuPP.NET.Calculators;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.GameModes.Osu
{
    /// <summary>
    /// Calculator for osu!standard performance attributes.
    /// </summary>
    internal class OsuPerformanceCalculator : IPerformanceCalculator
    {
        private DifficultyAttributes? _difficultyAttributes;
        private Mods _mods;
        private int _combo;
        private int _misses;
        private double _accuracy = 100.0;
        private int _n300;
        private int _n100;
        private int _n50;
        private int _nGeki;
        private int _nKatu;
        private int _sliderEndHits;
        private int _largeTickHits;
        private int _smallTickHits;
        private long _score;
        private int _passedObjects;
        private bool _isLazerScore = true;
        private HitResultPriority _priority = HitResultPriority.BestCase;
        private ScoreState? _scoreState;
        
        public void SetDifficultyAttributes(DifficultyAttributes attributes)
        {
            if (attributes is not OsuDifficultyAttributes)
                throw new ArgumentException("Attributes must be OsuDifficultyAttributes");
            
            _difficultyAttributes = attributes;
        }
        
        public void SetMods(Mods mods)
        {
            _mods = mods;
        }
        
        public void SetCombo(int combo)
        {
            _combo = combo;
        }
        
        public void SetMisses(int misses)
        {
            _misses = misses;
        }
        
        public void SetAccuracy(double accuracy)
        {
            _accuracy = accuracy;
        }
        
        public void SetN300(int n300)
        {
            _n300 = n300;
        }
        
        public void SetN100(int n100)
        {
            _n100 = n100;
        }
        
        public void SetN50(int n50)
        {
            _n50 = n50;
        }
        
        public void SetNGeki(int nGeki)
        {
            _nGeki = nGeki;
        }
        
        public void SetNKatu(int nKatu)
        {
            _nKatu = nKatu;
        }
        
        public void SetSliderEndHits(int sliderEndHits)
        {
            _sliderEndHits = sliderEndHits;
        }
        
        public void SetLargeTickHits(int largeTickHits)
        {
            _largeTickHits = largeTickHits;
        }
        
        public void SetSmallTickHits(int smallTickHits)
        {
            _smallTickHits = smallTickHits;
        }
        
        public void SetScore(long score)
        {
            _score = score;
        }
        
        public void SetPassedObjects(int passedObjects)
        {
            _passedObjects = passedObjects;
        }
        
        public void SetIsLazerScore(bool isLazerScore)
        {
            _isLazerScore = isLazerScore;
        }
        
        public void SetPriority(HitResultPriority priority)
        {
            _priority = priority;
        }
        
        public void SetScoreState(ScoreState? scoreState)
        {
            _scoreState = scoreState;
        }
        
        public PerformanceAttributes Calculate()
        {
            if (_difficultyAttributes == null)
                throw new InvalidOperationException("Difficulty attributes not set");
            
            var diffAttrs = (OsuDifficultyAttributes)_difficultyAttributes;
            int maxCombo = diffAttrs.MaxCombo;
            
            // Get hit counts from score state if provided
            if (_scoreState != null)
            {
                _combo = _scoreState.MaxCombo;
                _misses = _scoreState.Misses;
                _n300 = _scoreState.N300;
                _n100 = _scoreState.N100;
                _n50 = _scoreState.N50;
                _nGeki = _scoreState.NGeki;
                _nKatu = _scoreState.NKatu;
                _sliderEndHits = _scoreState.SliderEndHits;
                _largeTickHits = _scoreState.OsuLargeTickHits;
                _smallTickHits = _scoreState.OsuSmallTickHits;
                
                // Calculate accuracy from hit results
                int totalHits = _n300 + _n100 + _n50 + _misses;
                if (totalHits > 0)
                {
                    _accuracy = (300.0 * _n300 + 100.0 * _n100 + 50.0 * _n50) / (300.0 * totalHits) * 100.0;
                }
            }
            
            // If accuracy is provided but not hit counts, distribute hit counts based on accuracy
            else if (_accuracy < 100.0 && _n300 == 0 && _n100 == 0 && _n50 == 0)
            {
                int totalHits = _passedObjects > 0 ? _passedObjects : maxCombo; // Use passed objects if specified
                
                if (_priority == HitResultPriority.BestCase)
                {
                    // Distribute using best case (prioritize 300s, then 100s, then 50s)
                    DistributeHitsBestCase(totalHits);
                }
                else
                {
                    // Distribute using worst case (prioritize 50s, then 100s, then 300s)
                    DistributeHitsWorstCase(totalHits);
                }
            }
            
            // Ensure combo is valid
            _combo = Math.Min(_combo, maxCombo);
            
            // Consider passed objects if specified
            int effectiveObjectCount = _passedObjects > 0 ? _passedObjects : totalObjects(diffAttrs);
            
            // Implement the actual osu!standard performance calculation
            // based on the latest osu!lazer PP calculation
            
            // Calculate aim PP
            float aimValue = CalculateAimValue(diffAttrs, effectiveObjectCount);
            
            // Calculate speed PP
            float speedValue = CalculateSpeedValue(diffAttrs, effectiveObjectCount);
            
            // Calculate accuracy PP
            float accValue = CalculateAccuracyValue(diffAttrs, effectiveObjectCount);
            
            // Calculate flashlight PP
            float flashlightValue = CalculateFlashlightValue(diffAttrs, effectiveObjectCount);
            
            // Apply combo scaling to aim, speed and flashlight values
            float comboScaling = (float)Math.Min(1.0, Math.Pow(_combo / (float)maxCombo, 0.8));
            
            // Apply miss penalty to aim, speed and flashlight values
            float effectiveMissCount = Math.Max(1.0f, 1 + _misses);
            float missPenalty = 0.97f * (float)Math.Pow(1 - Math.Pow(effectiveMissCount / effectiveObjectCount, 0.775), effectiveMissCount);
            
            aimValue *= comboScaling * missPenalty;
            speedValue *= comboScaling * missPenalty;
            flashlightValue *= comboScaling * missPenalty;
            
            // Calculate total PP
            double finalMultiplier = 1.12;
            
            // Use different multipliers for stable/lazer if needed
            if (!_isLazerScore)
            {
                // Adjust multipliers for stable scores if necessary
            }
            
            // Nerf NoFail mod
            if (_mods.HasFlag(Mods.NoFail))
                finalMultiplier *= 0.90;
            
            // Nerf SpunOut mod
            if (_mods.HasFlag(Mods.SpunOut))
                finalMultiplier *= 0.95;
            
            float totalPp = (float)(Math.Pow(
                Math.Pow(aimValue, 1.1) +
                Math.Pow(speedValue, 1.1) +
                Math.Pow(accValue, 1.1) +
                Math.Pow(flashlightValue, 1.1),
                1.0 / 1.1
            ) * finalMultiplier);
            
            // Create and return performance attributes
            return new OsuPerformanceAttributes
            {
                Stars = diffAttrs.Stars,
                Pp = totalPp,
                AimPp = aimValue,
                SpeedPp = speedValue,
                AccuracyPp = accValue,
                FlashlightPp = flashlightValue,
                Difficulty = diffAttrs,
                MaxCombo = maxCombo,
                EffectiveMissCount = effectiveMissCount
            };
        }
        
        private void DistributeHitsBestCase(int totalHits)
        {
            // Calculate the target total score based on accuracy
            double targetScore = _accuracy * 300.0 * totalHits / 100.0;
            
            // Start with all 300s
            _n300 = totalHits - _misses;
            _n100 = 0;
            _n50 = 0;
            
            // Calculate current score
            double currentScore = 300.0 * _n300;
            
            // If current score is too high, convert some 300s to 100s
            while (currentScore > targetScore && _n300 > 0)
            {
                _n300--;
                _n100++;
                currentScore = 300.0 * _n300 + 100.0 * _n100;
            }
            
            // If current score is still too high, convert some 100s to 50s
            while (currentScore > targetScore && _n100 > 0)
            {
                _n100--;
                _n50++;
                currentScore = 300.0 * _n300 + 100.0 * _n100 + 50.0 * _n50;
            }
        }
        
        private void DistributeHitsWorstCase(int totalHits)
        {
            // Calculate the target total score based on accuracy
            double targetScore = _accuracy * 300.0 * totalHits / 100.0;
            
            // Start with all 50s (worst case)
            _n300 = 0;
            _n100 = 0;
            _n50 = totalHits - _misses;
            
            // Calculate current score
            double currentScore = 50.0 * _n50;
            
            // If current score is too low, convert some 50s to 100s
            while (currentScore < targetScore && _n50 > 0)
            {
                _n50--;
                _n100++;
                currentScore = 50.0 * _n50 + 100.0 * _n100;
            }
            
            // If current score is still too low, convert some 100s to 300s
            while (currentScore < targetScore && _n100 > 0)
            {
                _n100--;
                _n300++;
                currentScore = 50.0 * _n50 + 100.0 * _n100 + 300.0 * _n300;
            }
        }
        
        private float CalculateAimValue(OsuDifficultyAttributes diffAttrs, int objectCount)
        {
            // Implementation of the aim PP calculation based on osu!lazer
            double aimValue = Math.Pow(5.0 * Math.Max(1.0, diffAttrs.AimStrain / 0.0675) - 4.0, 3.0) / 100000.0;
            
            // Length bonus
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, objectCount / 2000.0);
            if (objectCount > 2000)
                lengthBonus += Math.Log10(objectCount / 2000.0) * 0.5;
            
            aimValue *= lengthBonus;
            
            // Apply accuracy scaling to aim
            double accuracyFactor = 0.5 + _accuracy / 200.0;
            aimValue *= accuracyFactor;
            
            // Apply AR bonus
            double arBonus = 1.0;
            if (diffAttrs.ApproachRate > 10.33)
                arBonus += 0.3 * (diffAttrs.ApproachRate - 10.33);
            else if (diffAttrs.ApproachRate < 8.0)
                arBonus += 0.01 * (8.0 - diffAttrs.ApproachRate);
            
            aimValue *= arBonus;
            
            // Apply HD bonus to aim
            if (_mods.HasFlag(Mods.Hidden))
                aimValue *= 1.18;
            
            // Apply FL bonus to aim
            if (_mods.HasFlag(Mods.Flashlight))
                aimValue *= 1.45 * lengthBonus;
            
            return (float)aimValue;
        }
        
        private float CalculateSpeedValue(OsuDifficultyAttributes diffAttrs, int objectCount)
        {
            // Implementation of the speed PP calculation based on osu!lazer
            double speedValue = Math.Pow(5.0 * Math.Max(1.0, diffAttrs.SpeedStrain / 0.0675) - 4.0, 3.0) / 100000.0;
            
            // Length bonus
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, objectCount / 2000.0);
            if (objectCount > 2000)
                lengthBonus += Math.Log10(objectCount / 2000.0) * 0.5;
            
            speedValue *= lengthBonus;
            
            // Calculate accuracy for speed (based on hit results)
            double sixtyPercentAccuracyWeight = 0;
            int totalHits = _n300 + _n100 + _n50 + _misses;
            
            if (totalHits > 0)
            {
                double acc = (_n300 * 6.0 + _n100 * 2.0 + _n50 * 1.0) / (totalHits * 6.0);
                sixtyPercentAccuracyWeight = Math.Pow(acc, 8) * Math.Pow(objectCount / 1500.0, 0.3);
            }
            
            double accuracyValue = Math.Max(0, 14.2 * sixtyPercentAccuracyWeight - 12.2);
            
            // Apply accuracy scaling to speed
            double accuracyFactor = 0.5 + accuracyValue + _accuracy / 400.0;
            speedValue *= accuracyFactor;
            
            // Apply HD bonus to speed
            if (_mods.HasFlag(Mods.Hidden))
                speedValue *= 1.18;
            
            return (float)speedValue;
        }
        
        private float CalculateAccuracyValue(OsuDifficultyAttributes diffAttrs, int objectCount)
        {
            // Implementation of the accuracy PP calculation based on osu!lazer
            double betterAccuracy = Math.Pow(_accuracy / 100.0, 15) * 2.5;
            
            // OD scaling
            double odScaling = Math.Pow(diffAttrs.OverallDifficulty, 2) / 2500.0;
            
            double accValue = betterAccuracy * odScaling * Math.Pow(Math.Min(1.15, Math.Pow(objectCount / 1000.0, 0.3)), 1.1);
            
            // Apply HD bonus to accuracy
            if (_mods.HasFlag(Mods.Hidden))
                accValue *= 1.02;
            
            // Apply FL bonus to accuracy
            if (_mods.HasFlag(Mods.Flashlight))
                accValue *= 1.02;
            
            return (float)accValue;
        }
        
        private float CalculateFlashlightValue(OsuDifficultyAttributes diffAttrs, int objectCount)
        {
            if (!_mods.HasFlag(Mods.Flashlight))
                return 0;
            
            // Implementation of the flashlight PP calculation based on osu!lazer
            double flValue = Math.Pow(diffAttrs.FlashlightStrain, 2.0) * 25.0;
            
            // Length bonus
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, objectCount / 2000.0);
            if (objectCount > 2000)
                lengthBonus += Math.Log10(objectCount / 2000.0) * 0.5;
            
            flValue *= lengthBonus;
            
            // Apply accuracy scaling to FL
            double accuracyFactor = 0.5 + _accuracy / 200.0;
            flValue *= accuracyFactor;
            
            return (float)flValue;
        }
        
        private int totalObjects(OsuDifficultyAttributes diffAttrs)
        {
            // In a real implementation, this would return the number of objects in the beatmap
            // For now, we'll just return the max combo as an approximation
            return diffAttrs.MaxCombo;
        }
    }
}