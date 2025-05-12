using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OsuPP.NET.Models;

namespace OsuPP.NET.Utils
{
    /// <summary>
    /// Utility class for beatmap-related operations.
    /// </summary>
    public static class MapUtils
    {
        /// <summary>
        /// Calculates the MD5 hash of a beatmap file.
        /// </summary>
        /// <param name="path">Path to the beatmap file</param>
        /// <returns>The MD5 hash as a hexadecimal string</returns>
        public static string CalculateMD5(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Beatmap file not found at {path}");
            
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Calculates the MD5 hash of beatmap content.
        /// </summary>
        /// <param name="content">The beatmap content as a string</param>
        /// <returns>The MD5 hash as a hexadecimal string</returns>
        public static string CalculateMD5FromContent(string content)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(content);
                byte[] hash = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Asynchronously calculates the MD5 hash of a beatmap file.
        /// </summary>
        /// <param name="path">Path to the beatmap file</param>
        /// <returns>A task that returns the MD5 hash as a hexadecimal string</returns>
        public static async Task<string> CalculateMD5Async(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Beatmap file not found at {path}");
            
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                byte[] hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Gets a beatmap file's hash from its file name if it follows the MD5 naming convention.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The hash if the file name is in the MD5 format, null otherwise</returns>
        public static string? GetHashFromFileName(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            
            // Check if the name is a valid MD5 hash (32 hexadecimal characters)
            if (name.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(name, "^[0-9a-fA-F]{32}$"))
            {
                return name.ToLowerInvariant();
            }
            
            return null;
        }
        
        /// <summary>
        /// Calculates maximum combo for a beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap</param>
        /// <returns>The maximum combo possible</returns>
        public static int CalculateMaxCombo(Beatmap beatmap)
        {
            // In a real implementation, this would analyze all hit objects
            // and calculate the maximum combo
            
            // For now, we'll just return the number of hit objects as an approximation
            return beatmap.CountHitObjects;
        }
    }
}