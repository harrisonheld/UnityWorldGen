using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace WorldGenerator
{
    /// <summary>
    /// A collection of helper methods that don't fit anywhere else.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Will hash any amount of any object. Order matters.
        /// In addition, the hash is STABLE - meaning it will give consistent results even if the application is restarted.
        /// (This is not true of the built-in GetHashCode method.)
        /// </summary>
        public static int MultiHash(params object[] values)
        {
            // get the raw bytes of the objects
            using MemoryStream stream = new();
            BinaryFormatter formatter = new();
            foreach (object obj in values)
            {
                formatter.Serialize(stream, obj);
            }
            byte[] dataBytes = stream.ToArray();

            // sha256 babey
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(dataBytes);
            // Convert the first 4 bytes of the hash to an integer
            int hashValue = BitConverter.ToInt32(hashBytes, 0);
            return hashValue;
        }
    }
}