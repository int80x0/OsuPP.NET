namespace OsuPP.NET.Models.Enums
{
    /// <summary>
    /// Represents the priority of hit results when applying accuracy.
    /// </summary>
    public enum HitResultPriority
    {
        /// <summary>
        /// Prioritize the best hit results.
        /// </summary>
        BestCase,
        
        /// <summary>
        /// Prioritize the worst hit results.
        /// </summary>
        WorstCase
    }
}