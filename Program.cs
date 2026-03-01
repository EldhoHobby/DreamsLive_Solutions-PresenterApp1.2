using System;
using System.Drawing;
using System.Windows.Forms;

namespace DreamsLive_Solutions_PresenterApp1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // The original logic was identical for both activated and non-activated states.
            // This has been simplified into a single execution path.
            HandleApplicationStartup();
        }

        private static void HandleApplicationStartup()
        {
            UsageManager usageManager = new UsageManager();
            SecureLicenseManager licenseManager = new SecureLicenseManager();

            // A license is considered truly active if it's validated AND has uses remaining.
            // HasExceededUsageLimit doubles as our check for remaining uses.
            if (licenseManager.IsActivated() && !usageManager.HasExceededUsageLimit())
            {
                // If the license is usage-limited, we must increment the usage count.
                var licenseData = licenseManager.GetLicenseData();
                if (licenseData != null && licenseData.Uses.HasValue)
                {
                    usageManager.IncrementUsageCount();
                }

                // Show a warning if the license is nearing expiry, but allow the app to run.
                if (licenseManager.IsLicenseNearingExpiry() || usageManager.IsLicenseNearingExpiry())
                {
                    using (ActivationForm activationForm = new ActivationForm())
                    {
                        activationForm.ShowDialog();
                    }
                }

                RunApplication();
                return;
            }

            // If the code reaches here, the user is either in trial, expired, or has an expired license.
            // The logic for trial and expired states can be combined.

            // If there are uses left (i.e., in trial), increment the count.
            if (!usageManager.HasExceededUsageLimit())
            {
                usageManager.IncrementUsageCount();
                RunApplication();
                return;
            }

            // Trial/License is over. Show expiration message and activation form.
            CopyableMessageBox.Show("Your license has expired. Please enter a new key to reactivate.", "License Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            using (ActivationForm activationForm = new ActivationForm())
            {
                // If the user activates, the form returns OK. We exit and let them restart.
                if (activationForm.ShowDialog() == DialogResult.OK)
                {
                    return;
                }
            }

            // User didn't activate. Start a new grace period session and run the application.
            CopyableMessageBox.Show("You are now entering a 5-minute grace period.", "Grace Period Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
            usageManager.StartGracePeriodSession();
            RunApplication();
        }

        private static void RunApplication()
        {
            MainForm mainForm = new MainForm();
            mainForm.Opacity = 0;

            SplashForm splashForm = null;
            var splashThread = new System.Threading.Thread(() =>
            {
                splashForm = new SplashForm();
                Application.Run(splashForm);
            });

            splashThread.SetApartmentState(System.Threading.ApartmentState.STA);
            splashThread.Start();

            var splashTimer = new System.Diagnostics.Stopwatch();
            splashTimer.Start();

            mainForm.Load += (sender, e) =>
            {
                splashTimer.Stop();
                var elapsed = splashTimer.ElapsedMilliseconds;
                if (elapsed < 2000)
                {
                    System.Threading.Thread.Sleep(2000 - (int)elapsed);
                }

                if (splashForm != null)
                {
                    splashForm.Invoke(new Action(() => splashForm.Close()));
                    splashThread.Join();
                }

                mainForm.Invoke(new Action(() => mainForm.Opacity = 1));
            };

            Application.Run(mainForm);
        }
    }
}