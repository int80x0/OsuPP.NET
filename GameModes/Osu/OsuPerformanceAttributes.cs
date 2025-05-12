using OsuPP.NET.Models;

namespace OsuPP.NET.GameModes.Osu
{
    /// <summary>
    /// Performance attributes specific to osu!standard.
    /// </summary>
    public class OsuPerformanceAttributes : PerformanceAttributes
    {
        /// <summary>
        /// The aim performance component.
        /// </summary>
        public float AimPp { get; internal set; }
        
        /// <summary>
        /// The speed performance component.
        /// </summary>
        public float SpeedPp { get; internal set; }
        
        /// <summary>
        /// The accuracy performance component.
        /// </summary>
        public float AccuracyPp { get; internal set; }
        
        /// <summary>
        /// The flashlight performance component.
        /// </summary>
        public float FlashlightPp { get; internal set; }
        
        /// <summary>
        /// The effective miss count used in the calculation.
        /// </summary>
        public float EffectiveMissCount { get; internal set; }
    }
}