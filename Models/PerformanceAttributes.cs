namespace OsuPP.NET.Models
{
    /// <summary>
    /// Base class for all performance attributes.
    /// </summary>
    public abstract class PerformanceAttributes : BeatmapAttributes
    {
        /// <summary>
        /// The performance points value.
        /// </summary>
        public float Pp { get; internal set; }
        
        /// <summary>
        /// The difficulty attributes used to calculate these performance attributes.
        /// </summary>
        public DifficultyAttributes Difficulty { get; internal set; } = null!;
        
        /// <summary>
        /// Gets the performance points value.
        /// </summary>
        /// <returns>The PP value</returns>
        public float PP() => Pp;
    }
}