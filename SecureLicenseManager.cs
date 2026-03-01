using System;
using System.IO;
using System.Text;
using System.Globalization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;

namespace DreamsLive_Solutions_PresenterApp1
{
    public enum LicenseStatus
    {
        Valid,
        InvalidKey,
        Expired,
        WrongMachineId
    }

    public class SecureLicenseManager
    {
        private const string LicenseFileName = "license.key";
        private const string PublicKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAj+mw/TSfSl3GZzjjOIBV
PeTxmoYljG+VXueSgYagt83P5KtOnb0cifHhf3m+EQQgt+Eobq761RAxz19KLmwj
6WnxUEcH4leUmk8lVdJng1eKvOtOZ8d3BqMVcv4avWjaKMdELGO/jFws4BWKQZ+I
fzJv1PzppRjmuh5zJsT2vCOlpDpMJ1epSRdzpLLoapGZcbVSSJ8/O6TpkvlJtxnV
Mylg7u5kMucejGXzHi+g7sgex+8tiJ++En9ak5o5H/DzPtMEDV1SlEXToJDio72o
0fdXo4KDDTq3AWFW7WvFIpN/dQA1e/fSHqM4jYvrLFz9Vof7AWAt8Qzflt4wVsYo
VwIDAQAB
-----END PUBLIC KEY-----";

        private string GetLicenseFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DreamsLive_Solutions_PresenterApp1");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, LicenseFileName);
        }

        public bool IsActivated()
        {
            string licenseFile = GetLicenseFilePath();
            if (!File.Exists(licenseFile)) return false;

            string licenseKey = File.ReadAllText(licenseFile);
            var (status, _) = ValidateLicenseKey(licenseKey);
            return status == LicenseStatus.Valid;
        }

        public LicenseStatus ActivateLicense(string licenseKey)
        {
            var (status, licenseData) = ValidateLicenseKey(licenseKey);
            if (status != LicenseStatus.Valid)
            {
                return status;
            }

            File.WriteAllText(GetLicenseFilePath(), licenseKey);

            UsageManager usageManager = new UsageManager();
            if (licenseData.Uses.HasValue)
            {
                usageManager.SetUsageLimit(licenseData.Uses.Value);
            }
            else
            {
                // Permanent license, set a very high usage limit
                usageManager.SetUsageLimit(int.MaxValue);
            }

            return LicenseStatus.Valid;
        }

        public LicenseData GetLicenseData()
        {
            string licenseFile = GetLicenseFilePath();
            if (!File.Exists(licenseFile)) return null;

            string licenseKey = File.ReadAllText(licenseFile);
            var (status, licenseData) = ValidateLicenseKey(licenseKey);

            // We need to return data for expired licenses so the status helper can report it.
            if (status == LicenseStatus.Valid || status == LicenseStatus.Expired)
            {
                if (licenseData != null)
                {
                    licenseData.IsActivated = (status == LicenseStatus.Valid);
                }
                return licenseData;
            }
            return null;
        }

        public int? GetDaysUntilExpiry()
        {
            var licenseData = GetLicenseData();
            if (licenseData != null && licenseData.ExpiryDate.HasValue)
            {
                TimeSpan remaining = licenseData.ExpiryDate.Value.Date - DateTime.UtcNow.Date;
                return (int)Math.Ceiling(remaining.TotalDays);
            }
            return null;
        }

        public bool IsLicenseNearingExpiry()
        {
            var days = GetDaysUntilExpiry();
            return days.HasValue && days.Value >= 0 && days.Value <= 7;
        }

        private LicenseData ParseLicenseData(string data)
        {
            var licenseData = new LicenseData();
            using (var reader = new StringReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("MachineID:"))
                        licenseData.MachineId = line.Substring("MachineID:".Length).Trim();
                    else if (line.StartsWith("ExpiryDate:"))
                        licenseData.ExpiryDate = DateTime.ParseExact(line.Substring("ExpiryDate:".Length).Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    else if (line.StartsWith("Uses:"))
                        licenseData.Uses = int.Parse(line.Substring("Uses:".Length).Trim());
                }
            }
            return licenseData;
        }

        private (LicenseStatus, LicenseData) ValidateLicenseKey(string licenseKey)
        {
            try
            {
                string[] parts = licenseKey.Split(new[] { "---SIGNATURE---" }, StringSplitOptions.None);
                if (parts.Length != 2) return (LicenseStatus.InvalidKey, null);

                byte[] dataBytes = Convert.FromBase64String(parts[0]);
                string data = Encoding.UTF8.GetString(dataBytes);
                byte[] signature = Convert.FromBase64String(parts[1]);

                // Use BouncyCastle to parse the PEM public key
                PemReader pemReader = new PemReader(new StringReader(PublicKey));
                AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pemReader.ReadObject();

                // Create a signer and verify the data
                ISigner signer = SignerUtilities.GetSigner("SHA256withRSA");
                signer.Init(false, publicKey);
                signer.BlockUpdate(dataBytes, 0, dataBytes.Length);

                if (!signer.VerifySignature(signature))
                    return (LicenseStatus.InvalidKey, null);

                var licenseData = ParseLicenseData(data);
                if (licenseData.MachineId != MachineIdentifier.GetMachineId()) return (LicenseStatus.WrongMachineId, licenseData);

                if (licenseData.ExpiryDate.HasValue && licenseData.ExpiryDate.Value < DateTime.UtcNow.Date) return (LicenseStatus.Expired, licenseData);

                licenseData.IsActivated = true;
                return (LicenseStatus.Valid, licenseData);
            }
            catch { return (LicenseStatus.InvalidKey, null); }
        }
    }

    public class LicenseData
    {
        public string MachineId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? Uses { get; set; }
        public bool IsActivated { get; internal set; }
    }
}
