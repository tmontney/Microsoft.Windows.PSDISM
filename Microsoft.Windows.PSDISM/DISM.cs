using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.PSDISM.Win32
{
    public class DISM
    {
        // (800702E4) -2147024156: requires elevation
        // (C0040001) -1073479679: not initialized
        // (80070002) -2147024894: path not found/package not applicable
        // (C1420117) -1052638953: files in use (unmount)

        // Init/Shutdown
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismInitialize(DismLogLevel LogLevel,
            [MarshalAs(UnmanagedType.LPWStr)] string LogFilePath, [MarshalAs(UnmanagedType.LPWStr)] string ScratchDirectory);
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismShutdown();
        // Open/Close
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismOpenSession([MarshalAs(UnmanagedType.LPWStr)] string ImagePath,
            [MarshalAs(UnmanagedType.LPWStr)] string WindowsDirectory,
                [MarshalAs(UnmanagedType.LPWStr)] string SystemDrive,
                    out uint Session);
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismCloseSession(uint Session);
        // Mount/Unmount
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismMountImage([MarshalAs(UnmanagedType.LPWStr)] string ImageFilePath,
            [MarshalAs(UnmanagedType.LPWStr)] string MountPath, uint ImageIndex,
                [MarshalAs(UnmanagedType.LPWStr)] string ImageName, DismImageIdentifier ImageIdentifier,
                    uint Flags, IntPtr CancelEvent, IntPtr Progress, IntPtr UserData);
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismUnmountImage([MarshalAs(UnmanagedType.LPWStr)] string MountPath,
                uint Flags, IntPtr CancelEvent, IntPtr Progress, IntPtr UserData);
        // Get
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismGetImageInfo([MarshalAs(UnmanagedType.LPWStr)] string ImageFilePath,
            out IntPtr ImageInfo, out uint Count);
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismGetPackages(uint Session,
            out IntPtr Package, out uint Count);
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismGetMountedImageInfo(out IntPtr MountedImageInfo,
            out uint Count);
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismGetPackageInfo(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string Identifier,
            DismPackageIdentifier PackageIdentifier, out IntPtr PackageInfo);
        // Add
        [DllImport("DismApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 DismAddPackage(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string PackagePath,
            bool IgnoreCheck, bool PreventPending, IntPtr CancelEvent, IntPtr Progress, IntPtr UserData);

        public enum DismPackageIdentifier
        {
            DismPackageNone = 0,
            DismPackageName = 1,
            DismPackagePath = 2
        };

        public enum DismImageIdentifier
        {
            DismImageIndex = 0,
            DismImageName = 1
        };

        public enum DismLogLevel
        {
            DismLogErrors = 0,
            DismLogErrorsWarnings = 1,
            DismLogErrorsWarningsInfo = 2
        };

        public enum DismImageType
        {
            DismImageTypeUnsupported = -1,
            DismImageTypeWim = 0,
            DismImageTypeVhd = 1
        };

        public enum DismImageBootable
        {
            DismImageBootableYes = 0,
            DismImageBootableNo = 1,
            DismImageBootableUnknown = 2
        };

        public enum DismPackageFeatureState
        {
            DismStateNotPresent = 0,
            DismStateUninstallPending = 1,
            DismStateStaged = 2,
            DismStateRemoved = 3,
            DismStateInstalled = 4,
            DismStateInstallPending = 5,
            DismStateSuperseded = 6,
            DismStatePartiallyInstalled = 7
        };

        public enum DismReleaseType
        {
            DismReleaseTypeCriticalUpdate = 0,
            DismReleaseTypeDriver = 1,
            DismReleaseTypeFeaturePack = 2,
            DismReleaseTypeHotfix = 3,
            DismReleaseTypeSecurityUpdate = 4,
            DismReleaseTypeSoftwareUpdate = 5,
            DismReleaseTypeUpdate = 6,
            DismReleaseTypeUpdateRollup = 7,
            DismReleaseTypeLanguagePack = 8,
            DismReleaseTypeFoundation = 9,
            DismReleaseTypeServicePack = 10,
            DismReleaseTypeProduct = 11,
            DismReleaseTypeLocalPack = 12,
            DismReleaseTypeOther = 13,
            DismReleaseTypeOnDemandPack = 14
        };

        public enum DismMountMode
        {
            DismReadWrite = 0,
            DismReadOnly = 1
        };

        public enum DismMountStatus
        {
            DismMountStatusOk = 0,
            DismMountStatusNeedsRemount = 1,
            DismMountStatusInvalid = 2
        };

        public enum DismRestartType
        {
            DismRestartNo = 0,
            DismRestartPossible = 1,
            DismRestartRequired = 2
        };

        public enum DismFullyOfflineInstallable
        {
            DismFullyOfflineInstallable = 0,
            DismFullyOfflineNotInstallable = 1,
            DismFullyOfflineInstallableUndetermined = 2
        };

        public delegate void DismProgressCallback(uint Current, uint Total, Void UserData);

        // https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/dism/dism-api-constants?view=windows-11#constants
        // https://github.com/Chuyu-Team/DISMSDK/blob/main/dismapi.h
        public struct Constants
        {
            // Mount flags
            public const uint DISM_MOUNT_READWRITE = 0x00000000;
            public const uint DISM_MOUNT_READONLY = 0x00000001;
            public const uint DISM_MOUNT_OPTIMIZE = 0x00000002;
            public const uint DISM_MOUNT_CHECK_INTEGRITY = 0x00000004;
            // Unmount flags
            public const uint DISM_COMMIT_IMAGE = 0x00000000;
            public const uint DISM_DISCARD_IMAGE = 0x00000001;
            // Commit flags
            public const string DISM_COMMIT_GENERATE_INTEGRITY = "DISM_COMMIT_GENERATE_INTEGRITY";
            public const string DISM_COMMIT_APPEND = "DISM_COMMIT_APPEND";
            // Commit + Unmount flags
            // Equivalent to DISM_COMMIT_IMAGE + DISM_COMMIT_GENERATE_INTEGRITY and DISM_COMMIT_APPEND
            public const uint DISM_COMMIT_MASK = 0xffff0000;
            // Reserved storage state flags
            public const uint DISM_RESERVED_STORAGE_DISABLED = 0x00000000;
            public const uint DISM_RESERVED_STORAGE_ENABLED = 0x00000001;
        }

        public struct DismCustomProperty
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Name;
            [MarshalAs(UnmanagedType.LPWStr)] public string Value;
            [MarshalAs(UnmanagedType.LPWStr)] public string Path;
        }

        public struct DismFeature
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string FeatureName;
            public DismPackageFeatureState State;
        }

        public struct DismPackageInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string PackageName;
            public DismPackageFeatureState PackageState;
            public DismReleaseType ReleaseType;
            public SYSTEMTIME InstallTime;
            public bool Applicable;
            [MarshalAs(UnmanagedType.LPWStr)] public string Copyright;
            [MarshalAs(UnmanagedType.LPWStr)] public string Company;
            public SYSTEMTIME CreationTime;
            [MarshalAs(UnmanagedType.LPWStr)] public string DisplayName;
            [MarshalAs(UnmanagedType.LPWStr)] public string Description;
            [MarshalAs(UnmanagedType.LPWStr)] public string InstallClient;
            [MarshalAs(UnmanagedType.LPWStr)] public string InstallPackageName;
            public SYSTEMTIME LastUpdateTime;
            [MarshalAs(UnmanagedType.LPWStr)] public string ProductName;
            [MarshalAs(UnmanagedType.LPWStr)] public string ProductVersion;
            public DismRestartType RestartRequired;
            public DismFullyOfflineInstallable FullyOffline;
            [MarshalAs(UnmanagedType.LPWStr)] public string SupportInformation;
            public DismCustomProperty CustomProperty;
            public uint CustomPropertyCount;
            public DismFeature Feature;
            public uint FeatureCount;
        }

        public struct DismMountedImageInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string MountPath;
            [MarshalAs(UnmanagedType.LPWStr)] public string ImageFilePath;
            public uint ImageIndex;
            public DismMountMode MountMode;
            public DismMountStatus MountStatus;
        }

        public struct DismPackage
        {
            [MarshalAs(UnmanagedType.LPWStr)] string PackageName;
            DismPackageFeatureState PackageState;
            DismReleaseType ReleaseType;
            SYSTEMTIME InstallTime;
        }

        public struct DismLanguage
        {
            [MarshalAs(UnmanagedType.LPWStr)] string Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)] public short Year;
            [MarshalAs(UnmanagedType.U2)] public short Month;
            [MarshalAs(UnmanagedType.U2)] public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)] public short Day;
            [MarshalAs(UnmanagedType.U2)] public short Hour;
            [MarshalAs(UnmanagedType.U2)] public short Minute;
            [MarshalAs(UnmanagedType.U2)] public short Second;
            [MarshalAs(UnmanagedType.U2)] public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        struct DismWimCustomizedInfo
        {
            public uint Size;
            public uint DirectoryCount;
            public uint FileCount;
            public SYSTEMTIME CreatedTime;
            public SYSTEMTIME ModifiedTime;
        }

        [ComVisible(true)]
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct Void
        {
        }

        public struct DismImageInfo
        {
            public DismImageType ImageType;
            public uint ImageIndex;
            [MarshalAs(UnmanagedType.LPWStr)] public string ImageName;
            [MarshalAs(UnmanagedType.LPWStr)] public string ImageDescription;
            public UInt64 ImageSize;
            public uint Architecture;
            [MarshalAs(UnmanagedType.LPWStr)] public string ProductName;
            [MarshalAs(UnmanagedType.LPWStr)] public string EditionId;
            [MarshalAs(UnmanagedType.LPWStr)] public string InstallationType;
            [MarshalAs(UnmanagedType.LPWStr)] public string Hal;
            [MarshalAs(UnmanagedType.LPWStr)] public string ProductType;
            [MarshalAs(UnmanagedType.LPWStr)] public string ProductSuite;
            public uint MajorVersion;
            public uint MinorVersion;
            public uint Build;
            public uint SpBuild;
            public uint SpLevel;
            public DismImageBootable Bootable;
            [MarshalAs(UnmanagedType.LPWStr)] public string SystemRoot;
            public DismLanguage Language;
            public uint LanguageCount;
            public uint DefaultLanguageIndex;
            public Void CustomizedInfo;
        }

        public static void MarshalUnmananagedArray2Struct<T>(IntPtr unmanagedArray, int length, out T[] managedArray)
        {
            var size = Marshal.SizeOf(typeof(T));
            managedArray = new T[length];

            for (int i = 0; i < length; i++)
            {
                IntPtr ins = IntPtr.Add(unmanagedArray, i * size);
                managedArray[i] = Marshal.PtrToStructure<T>(ins);
            }
        }

        public static bool IsSuccessful(int ret)
        {
            return ret == 0;
        }
    }
}
