using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Models
{
    /// <summary>
    /// Base class for all difficulty attributes.
    /// </summary>
    public abstract class DifficultyAttributes : BeatmapAttributes
    {
        /// <summary>
        /// The game mode for these attributes.
        /// </summary>
        public GameMode Mode { get; internal set; }
        
        /// <summary>
        /// Applied mods for these attributes.
        /// </summary>
        public Mods Mods { get; internal set; }
        
        /// <summary>
        /// The approach rate value after applying mods.
        /// </summary>
        public float ApproachRate { get; internal set; }
        
        /// <summary>
        /// The overall difficulty value after applying mods.
        /// </summary>
        public float OverallDifficulty { get; internal set; }
        
        /// <summary>
        /// Clock rate after applying mods (e.g., 1.5 for DT, 0.75 for HT).
        /// </summary>
        public double ClockRate { get; internal set; } = 1.0;
    }
}