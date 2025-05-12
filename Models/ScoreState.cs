using System;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Models
{
    /// <summary>
    /// Represents the state of a score with hit results and combo.
    /// </summary>
    public class ScoreState
    {
        /// <summary>
        /// Maximum combo reached.
        /// </summary>
        public int MaxCombo { get; set; }
        
        /// <summary>
        /// Number of misses.
        /// </summary>
        public int Misses { get; set; }
        
        /// <summary>
        /// Number of 300s (great).
        /// </summary>
        public int N300 { get; set; }
        
        /// <summary>
        /// Number of 100s (good).
        /// </summary>
        public int N100 { get; set; }
        
        /// <summary>
        /// Number of 50s (meh).
        /// </summary>
        public int N50 { get; set; }
        
        /// <summary>
        /// Number of gekis (for osu!).
        /// </summary>
        public int NGeki { get; set; }
        
        /// <summary>
        /// Number of katus (for osu!).
        /// </summary>
        public int NKatu { get; set; }
        
        /// <summary>
        /// Number of large ticks hit (for osu!).
        /// </summary>
        public int OsuLargeTickHits { get; set; }
        
        /// <summary>
        /// Number of small ticks hit (for osu!).
        /// </summary>
        public int OsuSmallTickHits { get; set; }
        
        /// <summary>
        /// Number of slider end hits (for osu!).
        /// </summary>
        public int SliderEndHits { get; set; }
        
        /// <summary>
        /// Total score (for osu!mania).
        /// </summary>
        public long TotalScore { get; set; }
        
        /// <summary>
        /// Creates a new empty score state.
        /// </summary>
        public ScoreState()
        {
        }
        
        /// <summary>
        /// Creates a new score state with the given hit results.
        /// </summary>
        public ScoreState(int maxCombo, int n300, int n100, int n50, int misses)
        {
            MaxCombo = maxCombo;
            N300 = n300;
            N100 = n100;
            N50 = n50;
            Misses = misses;
        }
        
        /// <summary>
        /// Creates a deep copy of this score state.
        /// </summary>
        /// <returns>A new ScoreState instance with the same values</returns>
        public ScoreState Clone()
        {
            return new ScoreState
            {
                MaxCombo = MaxCombo,
                Misses = Misses,
                N300 = N300,
                N100 = N100,
                N50 = N50,
                NGeki = NGeki,
                NKatu = NKatu,
                OsuLargeTickHits = OsuLargeTickHits,
                OsuSmallTickHits = OsuSmallTickHits,
                SliderEndHits = SliderEndHits,
                TotalScore = TotalScore
            };
        }
        
        /// <summary>
        /// Creates a score state from accuracy using the given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to base the score state on</param>
        /// <param name="accuracy">The target accuracy (0-100)</param>
        /// <param name="misses">The number of misses</param>
        /// <param name="priority">The hit result priority</param>
        /// <returns>A new score state with hit results distributed to match the accuracy</returns>
        public static ScoreState FromAccuracy(Beatmap beatmap, double accuracy, int misses = 0, HitResultPriority priority = HitResultPriority.BestCase)
        {
            accuracy = Math.Clamp(accuracy, 0.0, 100.0);
            misses = Math.Max(0, misses);
            
            // The total number of hit objects
            int totalHits = beatmap.CountHitObjects;
            
            // Create the base score state
            var state = new ScoreState
            {
                Misses = misses,
                MaxCombo = Math.Max(0, totalHits - misses) // Simple approximation
            };
            
            // Distribute hit results
            if (priority == HitResultPriority.BestCase)
            {
                DistributeBestCase(state, totalHits, accuracy);
            }
            else
            {
                DistributeWorstCase(state, totalHits, accuracy);
            }
            
            return state;
        }
        
        /// <summary>
        /// Creates a score state from explicit hit results.
        /// </summary>
        /// <param name="n300">Number of 300s</param>
        /// <param name="n100">Number of 100s</param>
        /// <param name="n50">Number of 50s</param>
        /// <param name="misses">Number of misses</param>
        /// <param name="combo">Maximum combo (optional)</param>
        /// <returns>A new score state with the given hit results</returns>
        public static ScoreState FromHitResults(int n300, int n100, int n50, int misses, int? combo = null)
        {
            int totalHits = n300 + n100 + n50 + misses;
            int maxCombo = combo ?? Math.Max(0, totalHits - misses); // Simple approximation
            
            return new ScoreState
            {
                N300 = Math.Max(0, n300),
                N100 = Math.Max(0, n100),
                N50 = Math.Max(0, n50),
                Misses = Math.Max(0, misses),
                MaxCombo = Math.Max(0, maxCombo)
            };
        }
        
        /// <summary>
        /// Adds a hit result to this score state.
        /// </summary>
        /// <param name="result">The hit result to add</param>
        /// <returns>This score state for method chaining</returns>
        public ScoreState AddHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    N300++;
                    MaxCombo++;
                    break;
                case HitResult.Good:
                    N100++;
                    MaxCombo++;
                    break;
                case HitResult.Meh:
                    N50++;
                    MaxCombo++;
                    break;
                case HitResult.Miss:
                    Misses++;
                    // Reset combo on miss
                    break;
                case HitResult.Geki:
                    NGeki++;
                    MaxCombo++;
                    break;
                case HitResult.Katu:
                    NKatu++;
                    MaxCombo++;
                    break;
                case HitResult.SliderEnd:
                    SliderEndHits++;
                    MaxCombo++;
                    break;
                case HitResult.LargeTick:
                    OsuLargeTickHits++;
                    MaxCombo++;
                    break;
                case HitResult.SmallTick:
                    OsuSmallTickHits++;
                    MaxCombo++;
                    break;
            }
            
            return this;
        }
        
        /// <summary>
        /// Calculates the accuracy based on the current hit results.
        /// </summary>
        /// <returns>The accuracy as a percentage (0-100)</returns>
        public double CalculateAccuracy()
        {
            int totalHits = N300 + N100 + N50 + Misses;
            
            if (totalHits == 0)
                return 100.0;
            
            double totalScore = 300.0 * N300 + 100.0 * N100 + 50.0 * N50;
            double maxScore = 300.0 * totalHits;
            
            return (totalScore / maxScore) * 100.0;
        }
        
        private static void DistributeBestCase(ScoreState state, int totalHits, double accuracy)
        {
            // Calculate the target total score based on accuracy
            double targetScore = accuracy * 300.0 * totalHits / 100.0;
            
            // Start with all 300s
            state.N300 = totalHits - state.Misses;
            state.N100 = 0;
            state.N50 = 0;
            
            // Calculate current score
            double currentScore = 300.0 * state.N300;
            
            // If current score is too high, convert some 300s to 100s
            while (currentScore > targetScore && state.N300 > 0)
            {
                state.N300--;
                state.N100++;
                currentScore = 300.0 * state.N300 + 100.0 * state.N100;
            }
            
            // If current score is still too high, convert some 100s to 50s
            while (currentScore > targetScore && state.N100 > 0)
            {
                state.N100--;
                state.N50++;
                currentScore = 300.0 * state.N300 + 100.0 * state.N100 + 50.0 * state.N50;
            }
        }
        
        private static void DistributeWorstCase(ScoreState state, int totalHits, double accuracy)
        {
            // Calculate the target total score based on accuracy
            double targetScore = accuracy * 300.0 * totalHits / 100.0;
            
            // Start with all 50s (worst case)
            state.N300 = 0;
            state.N100 = 0;
            state.N50 = totalHits - state.Misses;
            
            // Calculate current score
            double currentScore = 50.0 * state.N50;
            
            // If current score is too low, convert some 50s to 100s
            while (currentScore < targetScore && state.N50 > 0)
            {
                state.N50--;
                state.N100++;
                currentScore = 50.0 * state.N50 + 100.0 * state.N100;
            }
            
            // If current score is still too low, convert some 100s to 300s
            while (currentScore < targetScore && state.N100 > 0)
            {
                state.N100--;
                state.N300++;
                currentScore = 50.0 * state.N50 + 100.0 * state.N100 + 300.0 * state.N300;
            }
        }
    }
    
    /// <summary>
    /// Represents a single hit result.
    /// </summary>
    public enum HitResult
    {
        /// <summary>
        /// A 300 (Great) hit.
        /// </summary>
        Great,
        
        /// <summary>
        /// A 100 (Good) hit.
        /// </summary>
        Good,
        
        /// <summary>
        /// A 50 (Meh) hit.
        /// </summary>
        Meh,
        
        /// <summary>
        /// A miss.
        /// </summary>
        Miss,
        
        /// <summary>
        /// A Geki (320 in mania).
        /// </summary>
        Geki,
        
        /// <summary>
        /// A Katu (200 in mania, small missed droplet in ctb).
        /// </summary>
        Katu,
        
        /// <summary>
        /// A slider end hit.
        /// </summary>
        SliderEnd,
        
        /// <summary>
        /// A large slider tick hit.
        /// </summary>
        LargeTick,
        
        /// <summary>
        /// A small slider tick hit.
        /// </summary>
        SmallTick
    }
}