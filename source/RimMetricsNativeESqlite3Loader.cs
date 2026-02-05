using System;
using System.IO;
using System.Runtime.InteropServices;
using Verse;

namespace RimMetrics
{
    public static class RimMetricsNativeESqlite3Loader
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryW(string lpFileName);

        private const int RTLD_NOW = 2;

        // Linux
        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_linux(string fileName, int flags);

        // macOS
        [DllImport("libSystem.B.dylib", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_osx(string fileName, int flags);

        public static void Preload()
        {
            string modRoot = FindModRoot();
            string nativePath = GetNativePath(modRoot);

            if (!File.Exists(nativePath))
                throw new FileNotFoundException("[RimMetrics] Missing native e_sqlite3 library", nativePath);

            IntPtr h;

            if (IsWindows())
            {
                h = LoadLibraryW(nativePath);
                if (h == IntPtr.Zero)
                    throw new Exception("[RimMetrics] LoadLibrary failed. Win32Error=" + Marshal.GetLastWin32Error());
            }
            else if (IsMac())
            {
                h = dlopen_osx(nativePath, RTLD_NOW);
                if (h == IntPtr.Zero)
                    throw new Exception("[RimMetrics] dlopen failed on macOS for " + nativePath);
            }
            else
            {
                h = dlopen_linux(nativePath, RTLD_NOW);
                if (h == IntPtr.Zero)
                    throw new Exception("[RimMetrics] dlopen failed on Linux for " + nativePath);
            }

            Logger.Message("Preloaded native SQLite OK: " + nativePath);
        }

        private static string GetNativePath(string modRoot)
        {
            if (IsWindows())
                return Path.Combine(modRoot, "1.6", "Base", "Native", "win-x64", "e_sqlite3.dll");

            if (IsMac())
            {
                // Best-effort Apple Silicon detection
                bool isArm64 = (IntPtr.Size == 8) && Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")?.IndexOf("ARM", StringComparison.OrdinalIgnoreCase) >= 0;
                return isArm64
                    ? Path.Combine(modRoot, "1.6", "Base", "Native", "osx-arm64", "libe_sqlite3.dylib")
                    : Path.Combine(modRoot, "1.6", "Base", "Native", "osx-x64", "libe_sqlite3.dylib");
            }

            return Path.Combine(modRoot, "1.6", "Base", "Native", "linux-x64", "libe_sqlite3.so");
        }

        private static bool IsWindows()
        {
            var p = Environment.OSVersion.Platform;
            return p == PlatformID.Win32NT || p == PlatformID.Win32Windows || p == PlatformID.Win32S || p == PlatformID.WinCE;
        }

        private static bool IsMac()
        {
            // Mono heuristic
            return Directory.Exists("/Applications") && Directory.Exists("/System");
        }

        private static string FindModRoot()
        {
            const string packageId = "sk.rimmetrics";

            foreach (var m in LoadedModManager.RunningModsListForReading)
                if (string.Equals(m.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
                    return m.RootDir;

            // fallback: name match
            foreach (var m in LoadedModManager.RunningModsListForReading)
                if (m.Name != null && m.Name.IndexOf("RimMetrics", StringComparison.OrdinalIgnoreCase) >= 0)
                    return m.RootDir;

            throw new Exception("[RimMetrics] Could not locate mod root (set packageId).");
        }
    }
}
