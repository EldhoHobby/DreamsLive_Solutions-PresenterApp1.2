using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace DreamsLive_Solutions_PresenterApp1
{
    public static class MachineIdentifier
    {
        public static string GetMachineId()
        {
            try
            {
                string cpuId = GetWmiProperty("Win32_Processor", "ProcessorId");
                string motherboardId = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
                string combinedId = $"CPU:{cpuId}-MB:{motherboardId}";

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedId));
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception)
            {
                // Fallback for environments where WMI is not available
                return "unsupported";
            }
        }

        private static string GetWmiProperty(string wmiClass, string propertyName)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj[propertyName]?.ToString() ?? "N/A";
            }
            return "N/A";
        }
    }
}
