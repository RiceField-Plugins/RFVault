using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Rocket.Core.Logging;

namespace RFVault.Utils
{
    public static class ByteArrayExtensions
    {
        public static byte[] Serialize<T>(this T m)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, m);
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] ByteArrayExtensions Serialize: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
                return Array.Empty<byte>();
            }
        }

        public static T Deserialize<T>(this byte[] byteArray)
        {
            try
            {
                using (var ms = new MemoryStream(byteArray))
                {
                    return (T) new BinaryFormatter().Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] ByteArrayExtensions Deserialize: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
                return default;
            }
        }
    }
}