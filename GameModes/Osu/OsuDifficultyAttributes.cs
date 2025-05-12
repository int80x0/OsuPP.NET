using OsuPP.NET.Models;

namespace OsuPP.NET.GameModes.Osu
{
    /// <summary>
    /// Difficulty attributes specific to osu!standard.
    /// </summary>
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        /// <summary>
        /// The aim difficulty component.
        /// </summary>
        public float AimStrain { get; internal set; }
        
        /// <summary>
        /// The speed difficulty component.
        /// </summary>
        public float SpeedStrain { get; internal set; }
        
        /// <summary>
        /// The flashlight difficulty component.
        /// </summary>
        public float FlashlightStrain { get; internal set; }
        
        /// <summary>
        /// The slider factor component.
        /// </summary>
        public float SliderFactor { get; internal set; }
        
        /// <summary>
        /// The approach rate (preempt time) in milliseconds after mods.
        /// </summary>
        public float PreemptTime { get; internal set; }
        
        /// <summary>
        /// The overall difficulty (hit window) in milliseconds after mods.
        /// </summary>
        public float HitWindowGreat { get; internal set; }
        
        /// <summary>
        /// The speed note count.
        /// </summary>
        public int SpeedNoteCount { get; internal set; }
    }
}