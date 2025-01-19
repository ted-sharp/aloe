using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

public static class Env
{
    public static string GetEnvironmentString()
    {
        var env = new StringBuilder();

        env.AppendLine("-- OS ------");
        var osArch = RuntimeInformation.OSArchitecture;
        var osBit = Environment.Is64BitOperatingSystem ? "64bit" : "32bit";
        env.AppendLine($"OS: {RuntimeInformation.OSDescription} ({osArch}: {osBit})");
        env.AppendLine($"MachineName: {Environment.MachineName}");
        env.AppendLine($"UserDomainName: {Environment.UserDomainName}");
        env.AppendLine($"UserName: {Environment.UserName}");
        env.AppendLine();

        env.AppendLine("-- NIC ------");
        try
        {
            foreach (var adp in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adp.OperationalStatus != OperationalStatus.Up)
                {
                    // 無効
                    continue;
                }

                var props = adp.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.ToString() == IPAddress.Loopback.ToString())
                    {
                        continue;
                    }

                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    var bytes = adp.GetPhysicalAddress().GetAddressBytes();
                    var macAddr = String.Join(":", bytes.Select(b => b.ToString("X2")));
                    env.AppendLine($"NIC: {addr.Address} ({macAddr})");
                }
            }
        }
        catch (Exception ex)
        {
            env.AppendLine(ex.Message);
        }
        env.AppendLine();

        env.AppendLine("-- Proc ------");
        env.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        var procBit = Environment.Is64BitProcess ? "64bit" : "32bit";
        env.AppendLine($"ProcessId: {Environment.ProcessId} ({procBit})");
        env.AppendLine($"ProcessPath: {Environment.ProcessPath}");
        env.AppendLine();

        env.AppendLine("-- Asm ------");
        var asm = Assembly.GetEntryAssembly();
        if (asm is not null)
        {
            var asmName = asm.GetName();
            env.AppendLine($"AsmName: {asmName.Name} {asmName.Version}");

            var asmTitle = Attribute.GetCustomAttribute(asm,
                typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;

            env.AppendLine($"AsmTitle: {asmTitle?.Title}");
            env.AppendLine($"AsmLocation: {asm.Location}");
        }
        env.AppendLine($"CommandLine: {Environment.CommandLine}");
        env.AppendLine($"CurrentDirectory: {Environment.CurrentDirectory}");

        env.AppendLine($"CurrentManagedThreadId: {Environment.CurrentManagedThreadId}");
        env.AppendLine();

        return env.ToString();
    }
}
