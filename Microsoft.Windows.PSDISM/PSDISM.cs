using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Windows.PSDISM.Win32;

namespace Microsoft.Windows.PSDISM
{
    [Cmdlet((VerbsCommon.Open), "PSDismSession")]
    public class OpenPSDismSessionCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string ImagePath;
        [Parameter(Mandatory = false)]
        public string WindowsDirectory = null;
        [Parameter(Mandatory = false)]
        public string SystemDrive = null;

        protected override void BeginProcessing()
        {
            ThrowableErrors.IfInvalidDirectory(this, ImagePath, false);
            ThrowableErrors.IfInvalidDirectory(this, WindowsDirectory, true);
            ThrowableErrors.IfInvalidDirectory(this, SystemDrive, true);
        }

        protected override void ProcessRecord()
        {
            uint session;
            int ret = DISM.DismOpenSession(ImagePath, WindowsDirectory, SystemDrive, out session);

            ThrowableErrors.IfDismError(this, ret);

            WriteObject(session);
        }
    }

    [Cmdlet((VerbsCommon.Close), "PSDismSession")]
    public class ClosePSDismSessionCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateRange(0, int.MaxValue)]
        public int Session;

        protected override void ProcessRecord()
        {
            int ret = DISM.DismCloseSession((uint)Session);
            ThrowableErrors.IfDismError(this, ret);
        }
    }

    [Cmdlet((VerbsData.Initialize), "PSDism")]
    public class InitializePSDismCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public DISM.DismLogLevel LogLevel;
        [Parameter(Mandatory = false)]
        public string LogFilePath;
        [Parameter(Mandatory = false)]
        public string ScratchDirectory;

        protected override void BeginProcessing()
        {
            // 'LogFilePath' creates the file (if not null) if doesn't exist?
            ThrowableErrors.IfInvalidDirectory(this, ScratchDirectory, true);
        }

        protected override void ProcessRecord()
        {
            int ret = DISM.DismInitialize(LogLevel, LogFilePath, ScratchDirectory);
            ThrowableErrors.IfDismError(this, ret);
        }
    }

    // Since 'Dispose' is not an approved verb
    [Cmdlet((VerbsCommon.Remove), "PSDism")]
    public class RemovePSDismCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
            int ret = DISM.DismShutdown();
            ThrowableErrors.IfDismError(this, ret);
        }
    }

    [Cmdlet((VerbsCommon.Get), "PSDismPackages")]
    public class GetPSDismPackagesCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateRange(0, int.MaxValue)]
        public int Session;

        protected override void ProcessRecord()
        {
            IntPtr info;
            uint count;
            DISM.DismPackage[] pkgInfo;

            int ret = DISM.DismGetPackages((uint)Session, out info, out count);
            ThrowableErrors.IfDismError(this, ret);

            DISM.MarshalUnmananagedArray2Struct<DISM.DismPackage>(info, (int)count, out pkgInfo);
            WriteObject(pkgInfo);
        }
    }

    [Cmdlet((VerbsCommon.Add), "PSDismPackage")]
    public class AddPSDismPackageCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateRange(0, int.MaxValue)]
        public int Session;
        [Parameter(Mandatory = true)]
        public string PackagePath;
        [Parameter(Mandatory = true)]
        public bool IgnoreCheck;
        [Parameter(Mandatory = true)]
        public bool PreventPending;
        [Parameter(Mandatory = false)]
        public IntPtr CancelEvent = IntPtr.Zero;
        [Parameter(Mandatory = false)]
        public IntPtr Progress = IntPtr.Zero;
        [Parameter(Mandatory = false)]
        public IntPtr UserData = IntPtr.Zero;

        protected override void BeginProcessing()
        {
            ThrowableErrors.IfInvalidPackagePath(this, PackagePath);
        }

        protected override void ProcessRecord()
        {
            int ret = DISM.DismAddPackage((uint)Session, PackagePath, IgnoreCheck,
                PreventPending, CancelEvent, Progress, UserData);
            ThrowableErrors.IfDismError(this, ret);
        }
    }

    [Cmdlet((VerbsCommon.Get), "PSDismMountedImageInfo")]
    public class GetPSDismMountedImageInfoCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
            IntPtr info;
            uint count;
            DISM.DismMountedImageInfo[] pkgInfo;

            int ret = DISM.DismGetMountedImageInfo(out info, out count);
            ThrowableErrors.IfDismError(this, ret);

            DISM.MarshalUnmananagedArray2Struct<DISM.DismMountedImageInfo>(info, (int)count, out pkgInfo);
            WriteObject(pkgInfo);
        }
    }

    [Cmdlet((VerbsCommon.Get), "PSDismImageInfo")]
    public class GetPSDismImageInfoCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string ImagePath;

        protected override void BeginProcessing()
        {
            ThrowableErrors.IfInvalidFile(this, ImagePath, false, new string[] {".wim", ".vhd"});
        }

        protected override void ProcessRecord()
        {
            IntPtr info;
            uint count;
            DISM.DismMountedImageInfo[] pkgInfo;

            int ret = DISM.DismGetImageInfo(ImagePath, out info, out count);
            ThrowableErrors.IfDismError(this, ret);

            DISM.MarshalUnmananagedArray2Struct<DISM.DismMountedImageInfo>(info, (int)count, out pkgInfo);
            WriteObject(pkgInfo);
        }
    }

    public static class ThrowableErrors
    {
        public static void IfInvalidFile(Cmdlet instance, string FilePath, bool Optional,
            string[] ValidExtensions = null)
        {
            if (Optional & (String.IsNullOrEmpty(FilePath) || String.IsNullOrWhiteSpace(FilePath))) { return; }

            System.IO.FileInfo fi = new System.IO.FileInfo(FilePath);
            if (!fi.Exists)
            {
                instance.ThrowTerminatingError(new ErrorRecord(new System.IO.FileNotFoundException(),
                    "FileNotFound", ErrorCategory.ObjectNotFound, "FilePath"));
            }
            else if(!(fi.Length > 0))
            {
                instance.ThrowTerminatingError(new ErrorRecord(new System.IO.InvalidDataException(),
                    "EmptyFile", ErrorCategory.InvalidData, "FilePath"));
            }
            else if(null != ValidExtensions && !ValidExtensions.Any(x => x.ToLower() == fi.Extension))
            {
                instance.ThrowTerminatingError(new ErrorRecord(new System.IO.InvalidDataException(),
                    "InvalidFileExtension", ErrorCategory.InvalidType, "FilePath"));
            }
        }

        public static void IfInvalidDirectory(Cmdlet instance, string DirectoryPath, bool Optional)
        {
            if(Optional & (String.IsNullOrEmpty(DirectoryPath) || String.IsNullOrWhiteSpace(DirectoryPath))) { return; }

            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                instance.ThrowTerminatingError(new ErrorRecord(new System.IO.DirectoryNotFoundException(),
                    "DirectoryNotFound", ErrorCategory.ObjectNotFound, "DirectoryPath"));
            }
        }

        // Per the docs...
        // A relative or absolute path to the .cab or .msu file being added, or a folder containing the expanded files of a single .cab file.
        public static void IfInvalidPackagePath(Cmdlet instance, string PackagePath)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(PackagePath);
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(PackagePath);

            bool validAsFile = fi.Exists && fi.Length <= 0 && (fi.Extension.ToLower() == ".cab" | fi.Extension.ToLower() == ".msu");
            bool validAsDirectory = di.Exists && di.GetFiles().Length > 0;

            if (!validAsFile & !validAsDirectory)
            {
                instance.ThrowTerminatingError(new ErrorRecord(new System.IO.InvalidDataException(),
                    "InvalidPackage", ErrorCategory.ObjectNotFound, "PackagePath"));
            }
        }

        public static void IfDismError(Cmdlet instance, int Value)
        {
            // Translate errors?
            if (DISM.IsSuccessful(Value))
            {
                instance.ThrowTerminatingError(new ErrorRecord(new System.Exception(),
                    "DismException", ErrorCategory.NotSpecified, Value));
            }
        }
    }
}