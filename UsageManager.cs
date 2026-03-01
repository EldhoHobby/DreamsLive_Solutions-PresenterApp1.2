using System;
using System.IO;
using System.Security.Cryptography;

namespace DreamsLive_Solutions_PresenterApp1
{
    public class UsageManager
    {
        private const string UsageFileName = "usage.dat";
        private const int DefaultMaxUses = 100;
        private static readonly byte[] Entropy = { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static readonly TimeSpan GracePeriodDuration = TimeSpan.FromMinutes(5);

        private string GetUsageFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DreamsLive_Solutions_PresenterApp1");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, UsageFileName);
        }

        private (int current, int max, long gracePeriodStartTicks) GetUsageData()
        {
            string filePath = GetUsageFilePath();
            if (!File.Exists(filePath))
            {
                return (0, DefaultMaxUses, -1);
            }

            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);
                using (var ms = new MemoryStream(decryptedData))
                using (var reader = new BinaryReader(ms))
                {
                    int current = reader.ReadInt32();
                    int max = reader.ReadInt32();
                    long graceStart = reader.BaseStream.Position < reader.BaseStream.Length ? reader.ReadInt64() : -1;
                    return (current, max, graceStart);
                }
            }
            catch
            {
                return (0, DefaultMaxUses, -1);
            }
        }

        private void SaveUsageData(int current, int max, long gracePeriodStartTicks)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(current);
                writer.Write(max);
                writer.Write(gracePeriodStartTicks);
                byte[] data = ms.ToArray();
                byte[] encryptedData = ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(GetUsageFilePath(), encryptedData);
            }
        }

        public int GetCurrentUses() => GetUsageData().current;
        public int GetMaxUses() => GetUsageData().max;

        public void IncrementUsageCount()
        {
            var (current, max, graceStart) = GetUsageData();
            int newCurrent = current + 1;

            if (newCurrent >= max && graceStart == -1)
            {
                graceStart = DateTime.UtcNow.Ticks;
            }

            SaveUsageData(newCurrent, max, graceStart);
        }

        public void SetUsageLimit(int newMaxUses)
        {
            SaveUsageData(0, newMaxUses, -1);
        }

        public void StartGracePeriodSession()
        {
            var (current, max, _) = GetUsageData();
            // A grace period session can only be started if the trial is over.
            if (current >= max)
            {
                SaveUsageData(current, max, DateTime.UtcNow.Ticks);
            }
        }

        public bool HasExceededUsageLimit()
        {
            var (current, max, _) = GetUsageData();
            return current >= max;
        }

        public bool IsInGracePeriod()
        {
            if (!HasExceededUsageLimit()) return false;
            var (_, _, graceStartTicks) = GetUsageData();
            if (graceStartTicks == -1) return false;

            var graceStartTime = new DateTime(graceStartTicks);
            return DateTime.UtcNow < graceStartTime + GracePeriodDuration;
        }

        public TimeSpan GetGracePeriodTimeRemaining()
        {
            if (!HasExceededUsageLimit()) return TimeSpan.Zero;
            var (_, _, graceStartTicks) = GetUsageData();
            if (graceStartTicks == -1) return TimeSpan.Zero;

            var graceStartTime = new DateTime(graceStartTicks);
            var endTime = graceStartTime + GracePeriodDuration;
            var remaining = endTime - DateTime.UtcNow;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public bool IsLicenseExpired()
        {
            return HasExceededUsageLimit() && !IsInGracePeriod();
        }

        public int GetRemainingUses()
        {
            var (current, max, _) = GetUsageData();
            // Don't go below zero.
            return Math.Max(0, max - current);
        }

        public bool IsLicenseNearingExpiry()
        {
            // A permanent license is represented by a very large number and should not trigger this.
            return GetMaxUses() < int.MaxValue && GetRemainingUses() <= 5;
        }
    }
}
