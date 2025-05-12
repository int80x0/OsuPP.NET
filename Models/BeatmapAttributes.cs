namespace OsuPP.NET.Models
{
    /// <summary>
    /// Base class for all beatmap attributes.
    /// </summary>
    public abstract class BeatmapAttributes
    {
        /// <summary>
        /// The star rating of the beatmap.
        /// </summary>
        public float Stars { get; internal set; }
        
        /// <summary>
        /// Maximum combo possible on the beatmap.
        /// </summary>
        public int MaxCombo { get; internal set; }
    }
}