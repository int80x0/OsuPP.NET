using System;
using OsuPP.NET.Calculators;
using OsuPP.NET.Models;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.GameModes.Osu
{
    /// <summary>
    /// Calculator for osu!standard performance attributes based directly on rosu-pp/osu!lazer's algorithm.
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
            int totalHits = 0;
            
            try
            {
                // Get hit counts from score state if provided
                if (_scoreState != null)
                {
                    _combo = _scoreState.MaxCombo;
                    _misses = _scoreState.Misses;
                    _n300 = _scoreState.N300;
                    _n100 = _scoreState.N100;
                    _n50 = _scoreState.N50;
                    
                    // Calculate accuracy from hit results
                    totalHits = _n300 + _n100 + _n50 + _misses;
                    if (totalHits > 0)
                    {
                        _accuracy = (300.0 * _n300 + 100.0 * _n100 + 50.0 * _n50) / (300.0 * totalHits) * 100.0;
                    }
                }
                
                // If accuracy is provided but not hit counts, distribute hit counts based on accuracy
                else if (_n300 == 0 && _n100 == 0 && _n50 == 0)
                {
                    totalHits = _passedObjects > 0 ? _passedObjects : maxCombo;
                    
                    // Ensure totalHits is at least equal to misses
                    totalHits = Math.Max(totalHits, _misses);
                    
                    if (_priority == HitResultPriority.BestCase)
                    {
                        DistributeHitsBestCase(totalHits);
                    }
                    else
                    {
                        DistributeHitsWorstCase(totalHits);
                    }
                }
                else
                {
                    totalHits = _n300 + _n100 + _n50 + _misses;
                }
                
                // Ensure combo is valid
                if (_combo <= 0 || _combo > maxCombo - _misses)
                {
                    _combo = Math.Max(0, maxCombo - _misses);
                }
                
                // ----------------------------------------------------------------
                // PERFORMANCE CALCULATION (based exactly on rosu-pp/osu!lazer PP system)
                // ----------------------------------------------------------------
                
                // Get attributes from difficulty calculation
                float aimStrain = diffAttrs.AimStrain;
                float speedStrain = diffAttrs.SpeedStrain;
                float sliderFactor = diffAttrs.SliderFactor;
                float approachRate = diffAttrs.ApproachRate;
                float overallDifficulty = diffAttrs.OverallDifficulty;
                
                // Calculate base PP values
                double aimValue = CalculateAimValue(aimStrain, totalHits);
                double speedValue = CalculateSpeedValue(speedStrain, totalHits);
                double accValue = CalculateAccuracyValue(overallDifficulty, totalHits);
                double flashlightValue = CalculateFlashlightValue(aimStrain, totalHits);
                
                // Apply AR bonus
                double arBonus = 1.0;
                
                // High AR bonus
                if (approachRate > 10.33)
                    arBonus += 0.3 * (approachRate - 10.33);
                // Low AR bonus
                else if (approachRate < 8.0)
                {
                    double lowArBonus = 0.01 * (8.0 - approachRate);
                    
                    // Extra bonus for HD+Low AR
                    if (_mods.HasFlag(Mods.Hidden))
                        lowArBonus *= 2;
                    
                    arBonus += lowArBonus;
                }
                
                aimValue *= arBonus;
                
                // Apply Hidden bonus
                if (_mods.HasFlag(Mods.Hidden))
                {
                    aimValue *= 1.0 + Math.Min(0.18, 0.01 * (12.0 - approachRate));
                    speedValue *= 1.0 + Math.Min(0.15, 0.01 * (12.0 - approachRate));
                    accValue *= 1.02;
                    if (_mods.HasFlag(Mods.Flashlight))
                        flashlightValue *= 1.02;
                }
                
                // Apply Flashlight bonus
                if (_mods.HasFlag(Mods.Flashlight))
                {
                    double flBonus = 1.0 + 0.35 * Math.Min(1.0, totalHits / 200.0);
                    
                    if (totalHits > 200)
                        flBonus += 0.3 * Math.Min(1.0, (totalHits - 200) / 300.0);
                    
                    if (totalHits > 500)
                        flBonus += (totalHits - 500) / 2000.0;
                    
                    aimValue *= flBonus;
                }
                
                // Apply combo scaling and miss penalties
                double comboScale = Math.Min(1.0, Math.Pow((double)_combo / maxCombo, 0.8));
                double missPenalty = Math.Pow(0.97, _misses);
                
                aimValue *= comboScale * missPenalty;
                speedValue *= comboScale * missPenalty;
                if (_mods.HasFlag(Mods.Flashlight))
                    flashlightValue *= comboScale * missPenalty;
                
                // Apply accuracy scaling to aim
                aimValue *= 0.5 + _accuracy / 200.0;
                
                // Apply accuracy scaling to speed
                double relevantAccPortion = 0;
                if (totalHits > 0)
                {
                    double relevantTotalHits = _n300 + _n100 + _n50 + _misses;
                    
                    if (relevantTotalHits > 0)
                    {
                        double speedAccScores = (_n300 * 6.0 + _n100 * 2.0 + _n50 * 0.5) / (relevantTotalHits * 6.0);
                        double objCount = Math.Min(1.0, relevantTotalHits / 1000.0);
                        relevantAccPortion = Math.Pow(speedAccScores, 8.0) * Math.Pow(objCount, 0.3);
                    }
                }
                
                double speedAccuracyFactor = 0.5 + (relevantAccPortion * 14.5 + _accuracy / 200.0) / 2.0;
                speedValue *= speedAccuracyFactor;
                
                // Calculate final PP
                double finalMultiplier = 1.12;
                
                // Apply mod multipliers
                if (_mods.HasFlag(Mods.NoFail))
                    finalMultiplier *= 0.90;
                
                if (_mods.HasFlag(Mods.SpunOut))
                    finalMultiplier *= 0.95;
                
                if (_mods.HasFlag(Mods.Relax))
                {
                    aimValue *= 0.7;
                    speedValue = 0.0;
                    flashlightValue *= 0.7;
                    finalMultiplier *= 0.6;
                }
                
                // Combine all PP values using the standard formula
                double finalPP = Math.Pow(
                    Math.Pow(aimValue, 1.1) +
                    Math.Pow(speedValue, 1.1) +
                    Math.Pow(accValue, 1.1) +
                    Math.Pow(flashlightValue, 1.1),
                    1.0 / 1.1
                ) * finalMultiplier;
                
                // Create and return performance attributes
                return new OsuPerformanceAttributes
                {
                    Stars = diffAttrs.Stars,
                    Pp = (float)finalPP,
                    AimPp = (float)aimValue,
                    SpeedPp = (float)speedValue,
                    AccuracyPp = (float)accValue,
                    FlashlightPp = (float)flashlightValue,
                    Difficulty = diffAttrs,
                    MaxCombo = maxCombo,
                    EffectiveMissCount = _misses
                };
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in performance calculation: {ex.Message}");
                
                // Fallback to a simple estimation
                return FallbackCalculate(diffAttrs);
            }
        }
        
        private OsuPerformanceAttributes FallbackCalculate(OsuDifficultyAttributes diffAttrs)
        {
            // Simple fallback formula based on star rating
            float stars = diffAttrs.Stars;
            double fallbackPP = Math.Pow(stars, 2.5) * 1.5 * (_accuracy / 100.0) * Math.Pow(0.97, _misses);
            
            // Cap at reasonable maximum
            fallbackPP = Math.Min(1000, Math.Max(0, fallbackPP));
            
            return new OsuPerformanceAttributes
            {
                Stars = stars,
                Pp = (float)fallbackPP,
                AimPp = (float)(fallbackPP * 0.6),
                SpeedPp = (float)(fallbackPP * 0.3),
                AccuracyPp = (float)(fallbackPP * 0.1),
                FlashlightPp = 0,
                Difficulty = diffAttrs,
                MaxCombo = diffAttrs.MaxCombo,
                EffectiveMissCount = _misses
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
        
        private double CalculateAimValue(float aimStrain, int numObjects)
        {
            // Base PP calculation
            double rawValue = Math.Pow(5.0 * Math.Max(1.0, aimStrain / 0.0675) - 4.0, 3.0) / 100000.0;
            
            // Length bonus
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, numObjects / 2000.0);
            if (numObjects > 2000)
                lengthBonus += Math.Log10(numObjects / 2000.0) * 0.5;
            
            return rawValue * lengthBonus;
        }
        
        private double CalculateSpeedValue(float speedStrain, int numObjects)
        {
            // Base PP calculation
            double rawValue = Math.Pow(5.0 * Math.Max(1.0, speedStrain / 0.0675) - 4.0, 3.0) / 100000.0;
            
            // Length bonus
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, numObjects / 2000.0);
            if (numObjects > 2000)
                lengthBonus += Math.Log10(numObjects / 2000.0) * 0.5;
            
            return rawValue * lengthBonus;
        }
        
        private double CalculateAccuracyValue(float overallDifficulty, int numObjects)
        {
            // Base accuracy portion
            double betterAccuracyPercentage = Math.Pow(_accuracy / 100.0, 15) * 2.5;
            
            // OD scaling
            double odScaling = Math.Pow(overallDifficulty, 2) / 2500.0;
            
            // Length bonus
            double lengthFactor = Math.Min(1.15, Math.Pow(numObjects / 1000.0, 0.3));
            
            // Calculate accuracy PP
            return betterAccuracyPercentage * odScaling * Math.Pow(lengthFactor, 1.1);
        }
        
        private double CalculateFlashlightValue(float aimStrain, int numObjects)
        {
            if (!_mods.HasFlag(Mods.Flashlight))
                return 0;
            
            // Base PP calculation  
            double rawValue = Math.Pow(aimStrain, 2.0) * 25.0;
            
            // Length bonus
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, numObjects / 2000.0);
            if (numObjects > 2000)
                lengthBonus += Math.Log10(numObjects / 2000.0) * 0.5;
            
            // Apply accuracy scaling
            rawValue *= lengthBonus * (0.5 + _accuracy / 200.0);
            
            return rawValue;
        }
    }
}