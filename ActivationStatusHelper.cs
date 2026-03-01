using System;

namespace DreamsLive_Solutions_PresenterApp1
{
    public static class ActivationStatusHelper
    {
        public static string GetActivationStatusString(bool forTitleBar = false)
        {
            var licenseManager = new SecureLicenseManager();
            var usageManager = new UsageManager();
            var licenseData = licenseManager.GetLicenseData();

            if (licenseData != null)
            {
                if (licenseData.IsActivated)
                {
                    int? daysUntilExpiry = licenseManager.GetDaysUntilExpiry();
                    int remainingUses = usageManager.GetRemainingUses();

                    if (daysUntilExpiry.HasValue && daysUntilExpiry.Value >= 0 && daysUntilExpiry.Value <= 7)
                    {
                        return forTitleBar ? $"(Expires in {daysUntilExpiry.Value} days)" : $"Your license will expire in {daysUntilExpiry.Value} days.";
                    }
                    else if (usageManager.IsLicenseNearingExpiry())
                    {
                        return forTitleBar ? $"({remainingUses} uses remaining)" : $"You have only {remainingUses} uses remaining.";
                    }
                    else if (licenseData.ExpiryDate.HasValue && licenseData.ExpiryDate.Value.Date < DateTime.MaxValue.Date)
                    {
                        return $"(Activated - Expires {licenseData.ExpiryDate.Value:yyyy-MM-dd})";
                    }
                    else if (licenseData.Uses.HasValue)
                    {
                        int maxUses = usageManager.GetMaxUses();
                        return $"(Activated - {remainingUses}/{maxUses} uses remaining)";
                    }
                    else
                    {
                        return "(Activated - Permanent)";
                    }
                }
                else
                {
                    // Handle non-activated (e.g., expired) licenses
                    return "(License Expired)";
                }
            }
            else
            {
                // Handle trial logic
                if (usageManager.IsInGracePeriod())
                {
                    TimeSpan remainingGracePeriod = usageManager.GetGracePeriodTimeRemaining();
                    return $"(Grace Period: {remainingGracePeriod.Minutes}m {remainingGracePeriod.Seconds}s remaining)";
                }
                else if (usageManager.HasExceededUsageLimit())
                {
                    return "(Trial Expired)";
                }
                else
                {
                    int remainingUses = usageManager.GetRemainingUses();
                    return $"(Trial: {remainingUses} uses remaining)";
                }
            }
        }
    }
}
