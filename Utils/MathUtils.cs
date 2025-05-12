using System;

namespace OsuPP.NET.Utils
{
    /// <summary>
    /// Utility class for mathematical operations.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Converts approach rate to preempt time in milliseconds.
        /// </summary>
        /// <param name="ar">The approach rate</param>
        /// <returns>The preempt time in milliseconds</returns>
        public static float ApproachRateToPreemptTime(float ar)
        {
            if (ar < 5.0f)
                return 1800f - 120f * ar;
            
            return 1200f - 150f * (ar - 5.0f);
        }
        
        /// <summary>
        /// Converts preempt time in milliseconds to approach rate.
        /// </summary>
        /// <param name="preempt">The preempt time in milliseconds</param>
        /// <returns>The approach rate</returns>
        public static float PreemptTimeToApproachRate(float preempt)
        {
            if (preempt > 1200f)
                return (1800f - preempt) / 120f;
            
            return 5.0f + (1200f - preempt) / 150f;
        }
        
        /// <summary>
        /// Converts overall difficulty to hit window in milliseconds.
        /// </summary>
        /// <param name="od">The overall difficulty</param>
        /// <returns>The hit window (300) in milliseconds</returns>
        public static float OverallDifficultyToHitWindow(float od)
        {
            return 80f - 6f * od;
        }
        
        /// <summary>
        /// Converts hit window in milliseconds to overall difficulty.
        /// </summary>
        /// <param name="hitWindow">The hit window (300) in milliseconds</param>
        /// <returns>The overall difficulty</returns>
        public static float HitWindowToOverallDifficulty(float hitWindow)
        {
            return (80f - hitWindow) / 6f;
        }
        
        /// <summary>
        /// Converts circle size to circle radius.
        /// </summary>
        /// <param name="cs">The circle size</param>
        /// <returns>The radius in osu!pixels</returns>
        public static float CircleSizeToRadius(float cs)
        {
            return 54.4f - 4.48f * cs;
        }
        
        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The clamped value</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            
            if (value > max)
                return max;
            
            return value;
        }
        
        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The clamped value</returns>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            
            if (value > max)
                return max;
            
            return value;
        }
        
        /// <summary>
        /// Linear interpolation between two values.
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>The interpolated value</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp(t, 0f, 1f);
        }
        
        /// <summary>
        /// Linear interpolation between two values.
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>The interpolated value</returns>
        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * Clamp(t, 0.0, 1.0);
        }
    }
}