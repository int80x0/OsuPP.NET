using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using OsuPP.NET.Models.Enums;

namespace OsuPP.NET.Models
{
    /// <summary>
    /// Represents a decoded .osu file with all properties needed for difficulty and performance calculations.
    /// </summary>
    public class Beatmap
    {
        #region Properties

        /// <summary>
        /// The beatmap's game mode.
        /// </summary>
        public GameMode Mode { get; private set; }
        
        /// <summary>
        /// The beatmap's version/difficulty name.
        /// </summary>
        public string Version { get; private set; } = string.Empty;
        
        /// <summary>
        /// The beatmap's MD5 hash.
        /// </summary>
        public string Md5 { get; private set; } = string.Empty;
        
        /// <summary>
        /// Title of the song.
        /// </summary>
        public string Title { get; private set; } = string.Empty;
        
        /// <summary>
        /// Artist of the song.
        /// </summary>
        public string Artist { get; private set; } = string.Empty;
        
        /// <summary>
        /// Creator of the beatmap.
        /// </summary>
        public string Creator { get; private set; } = string.Empty;
        
        /// <summary>
        /// Approach rate.
        /// </summary>
        public float ApproachRate { get; private set; }
        
        /// <summary>
        /// Circle size / Key count for mania.
        /// </summary>
        public float CircleSize { get; private set; }
        
        /// <summary>
        /// Health drain rate.
        /// </summary>
        public float HpDrainRate { get; private set; }
        
        /// <summary>
        /// Overall difficulty.
        /// </summary>
        public float OverallDifficulty { get; private set; }
        
        /// <summary>
        /// Slider multiplier.
        /// </summary>
        public double SliderMultiplier { get; private set; }
        
        /// <summary>
        /// Slider tick rate.
        /// </summary>
        public double SliderTickRate { get; private set; }
        
        /// <summary>
        /// Stack leniency.
        /// </summary>
        public float StackLeniency { get; private set; } = 0.7f;
        
        /// <summary>
        /// Whether the beatmap is a converted beatmap.
        /// </summary>
        public bool IsConvert { get; private set; }
        
        /// <summary>
        /// Audio filename.
        /// </summary>
        public string AudioFilename { get; private set; } = string.Empty;
        
        /// <summary>
        /// Beatmap ID.
        /// </summary>
        public int BeatmapId { get; private set; }
        
        /// <summary>
        /// Beatmap set ID.
        /// </summary>
        public int BeatmapSetId { get; private set; }
        
        /// <summary>
        /// The original mode before conversion, if applicable.
        /// </summary>
        public GameMode? OriginalMode { get; private set; }

        /// <summary>
        /// All hit objects.
        /// </summary>
        internal List<HitObject> HitObjects { get; } = new List<HitObject>();
        
        /// <summary>
        /// All timing points.
        /// </summary>
        internal List<TimingPoint> TimingPoints { get; } = new List<TimingPoint>();

        /// <summary>
        /// The number of hit objects.
        /// </summary>
        public int CountHitObjects => HitObjects.Count;
        
        /// <summary>
        /// The number of circles.
        /// </summary>
        public int CountCircles { get; private set; }
        
        /// <summary>
        /// The number of sliders.
        /// </summary>
        public int CountSliders { get; private set; }
        
        /// <summary>
        /// The number of spinners.
        /// </summary>
        public int CountSpinners { get; private set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Constructor to create an empty beatmap.
        /// </summary>
        public Beatmap() { }
        
        /// <summary>
        /// Constructor to create a beatmap with specific properties.
        /// </summary>
        private Beatmap(
            GameMode mode,
            string version,
            string md5,
            float approachRate,
            float circleSize,
            float hpDrainRate,
            float overallDifficulty,
            double sliderMultiplier,
            double sliderTickRate)
        {
            Mode = mode;
            Version = version;
            Md5 = md5;
            ApproachRate = approachRate;
            CircleSize = circleSize;
            HpDrainRate = hpDrainRate;
            OverallDifficulty = overallDifficulty;
            SliderMultiplier = sliderMultiplier;
            SliderTickRate = sliderTickRate;
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Parses a .osu file from the given path.
        /// </summary>
        /// <param name="path">Path to the .osu file</param>
        /// <returns>A parsed beatmap</returns>
        public static Beatmap FromPath(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Beatmap file not found at {path}");
            
            string content = File.ReadAllText(path);
            return FromContent(content);
        }
        
        /// <summary>
        /// Parses a .osu file from the given content.
        /// </summary>
        /// <param name="content">String content of a .osu file</param>
        /// <returns>A parsed beatmap</returns>
        public static Beatmap FromContent(string content)
        {
            var beatmap = new Beatmap();
            beatmap.Parse(content);
            return beatmap;
        }

        /// <summary>
        /// Parses a .osu file from the given byte array.
        /// </summary>
        /// <param name="bytes">Byte array containing the .osu file content</param>
        /// <returns>A parsed beatmap</returns>
        public static Beatmap FromBytes(byte[] bytes)
        {
            string content = Encoding.UTF8.GetString(bytes);
            return FromContent(content);
        }

        /// <summary>
        /// Asynchronously parses a .osu file from the given path.
        /// </summary>
        /// <param name="path">Path to the .osu file</param>
        /// <returns>A task that returns a parsed beatmap</returns>
        public static async Task<Beatmap> FromPathAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Beatmap file not found at {path}");
            
            string content = await File.ReadAllTextAsync(path);
            return FromContent(content);
        }
        
        /// <summary>
        /// Asynchronously parses a .osu file from the given byte array.
        /// </summary>
        /// <param name="bytes">Byte array containing the .osu file content</param>
        /// <returns>A task that returns a parsed beatmap</returns>
        public static async Task<Beatmap> FromBytesAsync(byte[] bytes)
        {
            return await Task.Run(() => FromBytes(bytes));
        }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Creates a copy of this beatmap.
        /// </summary>
        /// <returns>A new beatmap with the same properties</returns>
        public Beatmap Clone()
        {
            var clone = new Beatmap
            {
                Mode = Mode,
                Version = Version,
                Md5 = Md5,
                Title = Title,
                Artist = Artist,
                Creator = Creator,
                ApproachRate = ApproachRate,
                CircleSize = CircleSize,
                HpDrainRate = HpDrainRate,
                OverallDifficulty = OverallDifficulty,
                SliderMultiplier = SliderMultiplier,
                SliderTickRate = SliderTickRate,
                StackLeniency = StackLeniency,
                IsConvert = IsConvert,
                AudioFilename = AudioFilename,
                BeatmapId = BeatmapId,
                BeatmapSetId = BeatmapSetId,
                OriginalMode = OriginalMode,
                CountCircles = CountCircles,
                CountSliders = CountSliders,
                CountSpinners = CountSpinners
            };
            
            // Clone hit objects and timing points
            foreach (var hitObject in HitObjects)
            {
                clone.HitObjects.Add(hitObject.Clone());
            }
            
            foreach (var timingPoint in TimingPoints)
            {
                clone.TimingPoints.Add(timingPoint.Clone());
            }
            
            return clone;
        }
        
        /// <summary>
        /// Converts the beatmap to a different game mode.
        /// </summary>
        /// <param name="mode">The target game mode</param>
        /// <returns>A new beatmap converted to the specified mode</returns>
        public Beatmap Convert(GameMode mode)
        {
            if (Mode == mode)
                return Clone();
            
            var converted = Clone();
            converted.Mode = mode;
            converted.IsConvert = true;
            converted.OriginalMode = Mode;
            
            // Perform mode-specific conversions
            switch (mode)
            {
                case GameMode.Taiko:
                    ConvertToTaiko(converted);
                    break;
                case GameMode.Catch:
                    ConvertToCatch(converted);
                    break;
                case GameMode.Mania:
                    ConvertToMania(converted);
                    break;
            }
            
            return converted;
        }
        
        private void Parse(string content)
        {
            // Calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                Md5 = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            
            // Split content into sections
            var sections = ParseSections(content);
            
            // Parse each section
            if (sections.TryGetValue("General", out var generalLines))
                ParseGeneralSection(generalLines);
            
            if (sections.TryGetValue("Metadata", out var metadataLines))
                ParseMetadataSection(metadataLines);
            
            if (sections.TryGetValue("Difficulty", out var difficultyLines))
                ParseDifficultySection(difficultyLines);
            
            if (sections.TryGetValue("TimingPoints", out var timingPointsLines))
                ParseTimingPointsSection(timingPointsLines);
            
            if (sections.TryGetValue("HitObjects", out var hitObjectsLines))
                ParseHitObjectsSection(hitObjectsLines);
            
            CountObjects();
        }
        
        private Dictionary<string, List<string>> ParseSections(string content)
        {
            var sections = new Dictionary<string, List<string>>();
            string currentSection = "";
            var currentLines = new List<string>();
            
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    
                    // Skip comments and empty lines
                    if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                        continue;
                    
                    // Check for section header
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // Save previous section
                        if (!string.IsNullOrEmpty(currentSection))
                        {
                            sections[currentSection] = currentLines;
                        }
                        
                        // Start new section
                        currentSection = line.Substring(1, line.Length - 2);
                        currentLines = new List<string>();
                    }
                    else if (!string.IsNullOrEmpty(currentSection))
                    {
                        currentLines.Add(line);
                    }
                }
            }
            
            // Save last section
            if (!string.IsNullOrEmpty(currentSection))
            {
                sections[currentSection] = currentLines;
            }
            
            return sections;
        }
        
        private void ParseGeneralSection(List<string> lines)
        {
            foreach (var line in lines)
            {
                if (!line.Contains(":"))
                    continue;
                
                var parts = line.Split(':', 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                switch (key)
                {
                    case "AudioFilename":
                        AudioFilename = value;
                        break;
                    case "Mode":
                        if (int.TryParse(value, out int mode) && Enum.IsDefined(typeof(GameMode), mode))
                            Mode = (GameMode)mode;
                        break;
                    // Parse other general properties
                }
            }
        }
        
        private void ParseMetadataSection(List<string> lines)
        {
            foreach (var line in lines)
            {
                if (!line.Contains(":"))
                    continue;
                
                var parts = line.Split(':', 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                switch (key)
                {
                    case "Title":
                        Title = value;
                        break;
                    case "Artist":
                        Artist = value;
                        break;
                    case "Creator":
                        Creator = value;
                        break;
                    case "Version":
                        Version = value;
                        break;
                    case "BeatmapID":
                        if (int.TryParse(value, out int beatmapId))
                            BeatmapId = beatmapId;
                        break;
                    case "BeatmapSetID":
                        if (int.TryParse(value, out int beatmapSetId))
                            BeatmapSetId = beatmapSetId;
                        break;
                    // Parse other metadata properties
                }
            }
        }
        
        private void ParseDifficultySection(List<string> lines)
        {
            foreach (var line in lines)
            {
                if (!line.Contains(":"))
                    continue;
                
                var parts = line.Split(':', 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                switch (key)
                {
                    case "HPDrainRate":
                        if (float.TryParse(value, out float hp))
                            HpDrainRate = hp;
                        break;
                    case "CircleSize":
                        if (float.TryParse(value, out float cs))
                            CircleSize = cs;
                        break;
                    case "OverallDifficulty":
                        if (float.TryParse(value, out float od))
                            OverallDifficulty = od;
                        break;
                    case "ApproachRate":
                        if (float.TryParse(value, out float ar))
                            ApproachRate = ar;
                        break;
                    case "SliderMultiplier":
                        if (double.TryParse(value, out double sm))
                            SliderMultiplier = sm;
                        break;
                    case "SliderTickRate":
                        if (double.TryParse(value, out double str))
                            SliderTickRate = str;
                        break;
                    // Parse other difficulty properties
                }
            }
            
            // If AR is not set, use OD as default value (old maps)
            if (ApproachRate == 0)
                ApproachRate = OverallDifficulty;
        }
        
        private void ParseTimingPointsSection(List<string> lines)
        {
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 2)
                    continue;
                
                // Parse timing point
                var timingPoint = new TimingPoint();
                
                if (double.TryParse(parts[0], out double time))
                    timingPoint.Time = time;
                
                if (double.TryParse(parts[1], out double beatLength))
                    timingPoint.BeatLength = beatLength;
                
                if (parts.Length > 2 && int.TryParse(parts[2], out int meter))
                    timingPoint.Meter = meter;
                
                if (parts.Length > 6 && int.TryParse(parts[6], out int type))
                    timingPoint.Uninherited = (type & 1) == 1;
                
                TimingPoints.Add(timingPoint);
            }
        }
        
        private void ParseHitObjectsSection(List<string> lines)
        {
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 4)
                    continue;
                
                // Parse hit object
                var hitObject = new HitObject();
                
                if (int.TryParse(parts[0], out int x))
                    hitObject.X = x;
                
                if (int.TryParse(parts[1], out int y))
                    hitObject.Y = y;
                
                if (double.TryParse(parts[2], out double time))
                    hitObject.StartTime = time;
                
                if (int.TryParse(parts[3], out int type))
                {
                    hitObject.Type = type;
                    
                    if ((type & 1) != 0)
                        hitObject.ObjectType = HitObjectType.Circle;
                    else if ((type & 2) != 0)
                        hitObject.ObjectType = HitObjectType.Slider;
                    else if ((type & 8) != 0)
                        hitObject.ObjectType = HitObjectType.Spinner;
                    else if ((type & 128) != 0)
                        hitObject.ObjectType = HitObjectType.Hold;
                }
                
                // Parse specific object type data
                switch (hitObject.ObjectType)
                {
                    case HitObjectType.Slider:
                        if (parts.Length > 5)
                        {
                            // Parse slider data
                            var curveData = parts[5].Split('|');
                            if (curveData.Length > 0)
                                hitObject.CurveType = curveData[0];
                            
                            // Parse curve points
                            for (int i = 1; i < curveData.Length; i++)
                            {
                                var pointParts = curveData[i].Split(':');
                                if (pointParts.Length == 2 &&
                                    int.TryParse(pointParts[0], out int px) &&
                                    int.TryParse(pointParts[1], out int py))
                                {
                                    hitObject.CurvePoints.Add((px, py));
                                }
                            }
                            
                            // Parse repeat count, length, etc.
                            if (parts.Length > 6 && int.TryParse(parts[6], out int slides))
                                hitObject.Slides = slides;
                            
                            if (parts.Length > 7 && double.TryParse(parts[7], out double length))
                                hitObject.Length = length;
                        }
                        break;
                    
                    case HitObjectType.Spinner:
                        if (parts.Length > 5 && double.TryParse(parts[5], out double spinnerEndTime))
                            hitObject.EndTime = spinnerEndTime;
                        break;
                    
                    case HitObjectType.Hold:
                        if (parts.Length > 5)
                        {
                            var endTimeParts = parts[5].Split(':');
                            if (endTimeParts.Length > 0 && double.TryParse(endTimeParts[0], out double holdEndTime))
                                hitObject.EndTime = holdEndTime;
                        }
                        break;
                }
                
                HitObjects.Add(hitObject);
            }
            
            // Sort hit objects by start time
            HitObjects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        }
        
        private void CountObjects()
        {
            CountCircles = 0;
            CountSliders = 0;
            CountSpinners = 0;
            
            foreach (var hitObject in HitObjects)
            {
                switch (hitObject.ObjectType)
                {
                    case HitObjectType.Circle:
                        CountCircles++;
                        break;
                    case HitObjectType.Slider:
                        CountSliders++;
                        break;
                    case HitObjectType.Spinner:
                        CountSpinners++;
                        break;
                }
            }
        }
        
        private void ConvertToTaiko(Beatmap converted)
        {
            // Implement Taiko conversion
        }
        
        private void ConvertToCatch(Beatmap converted)
        {
            // Implement Catch conversion
        }
        
        private void ConvertToMania(Beatmap converted)
        {
            // Implement Mania conversion
        }
        
        #endregion
        
        #region Hit Objects and Timing Points
        
        /// <summary>
        /// A hit object (circle, slider, spinner, hold) in the beatmap.
        /// </summary>
        internal class HitObject
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double StartTime { get; set; }
            public double EndTime { get; set; }
            public int Type { get; set; }
            public HitObjectType ObjectType { get; set; }
            public string CurveType { get; set; } = "";
            public List<(int, int)> CurvePoints { get; } = new List<(int, int)>();
            public int Slides { get; set; } = 1;
            public double Length { get; set; }
            
            public HitObject Clone()
            {
                var clone = new HitObject
                {
                    X = X,
                    Y = Y,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Type = Type,
                    ObjectType = ObjectType,
                    CurveType = CurveType,
                    Slides = Slides,
                    Length = Length
                };
                
                foreach (var point in CurvePoints)
                {
                    clone.CurvePoints.Add(point);
                }
                
                return clone;
            }
        }
        
        /// <summary>
        /// A timing point in the beatmap.
        /// </summary>
        internal class TimingPoint
        {
            public double Time { get; set; }
            public double BeatLength { get; set; }
            public int Meter { get; set; } = 4;
            public bool Uninherited { get; set; }
            
            public TimingPoint Clone()
            {
                return new TimingPoint
                {
                    Time = Time,
                    BeatLength = BeatLength,
                    Meter = Meter,
                    Uninherited = Uninherited
                };
            }
        }
        
        /// <summary>
        /// Types of hit objects.
        /// </summary>
        internal enum HitObjectType
        {
            Circle,
            Slider,
            Spinner,
            Hold
        }
        
        #endregion
    }
}