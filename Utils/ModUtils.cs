using System;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Utils
{
    /// <summary>
    /// Utility class for mod-related operations.
    /// </summary>
    public static class ModUtils
    {
        /// <summary>
        /// Applies mod effects to approach rate.
        /// </summary>
        /// <param name="ar">The base approach rate</param>
        /// <param name="mods">The mods to apply</param>
        /// <param name="clockRate">The clock rate (1.0 = normal, 1.5 = DT, 0.75 = HT)</param>
        /// <returns>The modified approach rate</returns>
        public static float ApplyARMods(float ar, Mods mods, double clockRate)
        {
            // Apply Hard Rock
            if (mods.HasFlag(Mods.HardRock))
            {
                ar = Math.Min(10f, ar * 1.4f);
            }
            
            // Apply Easy
            if (mods.HasFlag(Mods.Easy))
            {
                ar = ar * 0.5f;
            }
            
            // Apply DT/HT effects
            ar = ApplyRateChangeToAR(ar, clockRate);
            
            return ar;
        }
        
        /// <summary>
        /// Applies mod effects to circle size.
        /// </summary>
        /// <param name="cs">The base circle size</param>
        /// <param name="mods">The mods to apply</param>
        /// <returns>The modified circle size</returns>
        public static float ApplyCSMods(float cs, Mods mods)
        {
            // Apply Hard Rock
            if (mods.HasFlag(Mods.HardRock))
            {
                cs = Math.Min(10f, cs * 1.3f);
            }
            
            // Apply Easy
            if (mods.HasFlag(Mods.Easy))
            {
                cs = cs * 0.5f;
            }
            
            return cs;
        }
        
        /// <summary>
        /// Applies mod effects to HP drain rate.
        /// </summary>
        /// <param name="hp">The base HP drain rate</param>
        /// <param name="mods">The mods to apply</param>
        /// <returns>The modified HP drain rate</returns>
        public static float ApplyHPMods(float hp, Mods mods)
        {
            // Apply Hard Rock
            if (mods.HasFlag(Mods.HardRock))
            {
                hp = Math.Min(10f, hp * 1.4f);
            }
            
            // Apply Easy
            if (mods.HasFlag(Mods.Easy))
            {
                hp = hp * 0.5f;
            }
            
            return hp;
        }
        
        /// <summary>
        /// Applies mod effects to overall difficulty.
        /// </summary>
        /// <param name="od">The base overall difficulty</param>
        /// <param name="mods">The mods to apply</param>
        /// <param name="clockRate">The clock rate (1.0 = normal, 1.5 = DT, 0.75 = HT)</param>
        /// <returns>The modified overall difficulty</returns>
        public static float ApplyODMods(float od, Mods mods, double clockRate)
        {
            // Apply Hard Rock
            if (mods.HasFlag(Mods.HardRock))
            {
                od = Math.Min(10f, od * 1.4f);
            }
            
            // Apply Easy
            if (mods.HasFlag(Mods.Easy))
            {
                od = od * 0.5f;
            }
            
            // Apply DT/HT effects
            od = ApplyRateChangeToOD(od, clockRate);
            
            return od;
        }
        
        /// <summary>
        /// Gets the effective clock rate based on mods.
        /// </summary>
        /// <param name="mods">The mods to check</param>
        /// <returns>The clock rate (1.0 = normal, 1.5 = DT, 0.75 = HT)</returns>
        public static double GetClockRate(Mods mods)
        {
            if (mods.HasFlag(Mods.DoubleTime) || mods.HasFlag(Mods.Nightcore))
                return 1.5;
            
            if (mods.HasFlag(Mods.HalfTime))
                return 0.75;
            
            return 1.0;
        }
        
        /// <summary>
        /// Applies rate change to approach rate.
        /// </summary>
        /// <param name="ar">The base approach rate</param>
        /// <param name="clockRate">The clock rate (1.0 = normal, 1.5 = DT, 0.75 = HT)</param>
        /// <returns>The modified approach rate</returns>
        private static float ApplyRateChangeToAR(float ar, double clockRate)
        {
            if (Math.Abs(clockRate - 1.0) < 0.001)
                return ar;
            
            // Convert AR to preempt time in milliseconds
            float preempt = MathUtils.ApproachRateToPreemptTime(ar);
            
            // Apply clock rate to preempt time
            preempt = (float)(preempt / clockRate);
            
            // Convert back to AR
            return MathUtils.PreemptTimeToApproachRate(preempt);
        }
        
        /// <summary>
        /// Applies rate change to overall difficulty.
        /// </summary>
        /// <param name="od">The base overall difficulty</param>
        /// <param name="clockRate">The clock rate (1.0 = normal, 1.5 = DT, 0.75 = HT)</param>
        /// <returns>The modified overall difficulty</returns>
        private static float ApplyRateChangeToOD(float od, double clockRate)
        {
            if (Math.Abs(clockRate - 1.0) < 0.001)
                return od;
            
            // Convert OD to hit window in milliseconds
            float hitWindow = MathUtils.OverallDifficultyToHitWindow(od);
            
            // Apply clock rate to hit window
            hitWindow = (float)(hitWindow / clockRate);
            
            // Convert back to OD
            return MathUtils.HitWindowToOverallDifficulty(hitWindow);
        }
    }
}