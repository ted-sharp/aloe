using System.Security.Cryptography.X509Certificates;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class CertificateChecker : IChecker
{
    public string SectionKey => "cert";

    public string SectionTitle => "Certificates (LocalMachine\\My)";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var warningDays = profile.Cert.WarningDays;
        var now = DateTime.Now;

        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        try
        {
            store.Open(OpenFlags.ReadOnly);
        }
        catch (Exception ex)
        {
            writer.WriteLine($"  Failed to open certificate store: {ex.Message}");
            writer.WriteLine("  (Run as Administrator to access LocalMachine certificates)");
            return Task.CompletedTask;
        }

        if (store.Certificates.Count == 0)
        {
            writer.WriteLine("  No certificates found");
            return Task.CompletedTask;
        }

        foreach (var cert in store.Certificates)
        {
            var subject = cert.GetNameInfo(X509NameType.SimpleName, false);
            var expiry = cert.NotAfter;
            var daysLeft = (expiry - now).Days;

            string status;
            if (daysLeft < 0)
            {
                status = "[EXPIRED]";
            }
            else if (daysLeft <= warningDays)
            {
                status = $"[WARNING: expires in {daysLeft} days]";
            }
            else
            {
                status = "[OK]";
            }

            writer.WriteLine($"  {subject}");
            writer.WriteLine($"    Expires: {expiry:yyyy-MM-dd}  {status}");
        }

        return Task.CompletedTask;
    }
}
