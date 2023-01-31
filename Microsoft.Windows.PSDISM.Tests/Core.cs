using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Windows.PSDISM.Tests
{
    [TestClass]
    public class Core
    {
        // A valid .wim file to test
        private const string IMAGE_TEST_FILEPATH = @"..\..\data\Winre-2.wim";
        // A valid .msu or .cab file to test adding a package to the .wim file
        // Ensure the package matches the build and arch of the .wim
        private const string IMAGE_TEST_PACKAGEPATH = @"..\..\data\abc.msu";
        // A folder to unpack the .wim file
        private const string IMAGE_TEST_MOUNTPATH = @"..\..\data\mount";
        // Set this to the output of 'Edition' from Dism /Get-ImageInfo /ImageFile:<image_file_path>
        // This is to ensure the struct populated properly
        private const string IMAGE_TEST_EDITIONID = "WindowsPE";

        [TestMethod]
        public void CanInit()
        {
            // So far, 0 seems to be the only successful one (as opposed to MSI return codes, for example)
            if(Win32.DISM.IsSuccessful(
                Win32.DISM.DismInitialize(Win32.DISM.DismLogLevel.DismLogErrors, null, null))){
                Assert.IsTrue(Win32.DISM.IsSuccessful(Win32.DISM.DismShutdown()));
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CanGetImageInfoByPath()
        {
            bool success = false;
            Win32.DISM.DismInitialize(Win32.DISM.DismLogLevel.DismLogErrors, null, null);

            IntPtr info;
            uint count;
            Win32.DISM.DismGetImageInfo(IMAGE_TEST_FILEPATH, out info, out count);
            if(count > 0)
            {
                Win32.DISM.DismImageInfo[] pkgInfo;
                Win32.DISM.MarshalUnmananagedArray2Struct<Win32.DISM.DismImageInfo>(info, (int)count, out pkgInfo);
                success = pkgInfo[0].EditionId == IMAGE_TEST_EDITIONID;
            }

            Win32.DISM.DismShutdown();
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void CanMount()
        {
            bool success = false;
            Win32.DISM.DismInitialize(Win32.DISM.DismLogLevel.DismLogErrors, null, null);

            if(Win32.DISM.IsSuccessful(
                Win32.DISM.DismMountImage(IMAGE_TEST_FILEPATH, IMAGE_TEST_MOUNTPATH, 1, null,
                    Win32.DISM.DismImageIdentifier.DismImageIndex, Win32.DISM.Constants.DISM_MOUNT_READONLY,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
                )
            {
                success = Win32.DISM.IsSuccessful(
                    Win32.DISM.DismUnmountImage(IMAGE_TEST_MOUNTPATH, Win32.DISM.Constants.DISM_DISCARD_IMAGE,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
            }

            Win32.DISM.DismShutdown();
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void CanGetImageInfoByMount()
        {
            bool success = false;

            Win32.DISM.DismInitialize(Win32.DISM.DismLogLevel.DismLogErrors, null, null);
            int mounted = Win32.DISM.DismMountImage(IMAGE_TEST_FILEPATH, IMAGE_TEST_MOUNTPATH, 1, null,
                Win32.DISM.DismImageIdentifier.DismImageIndex, Win32.DISM.Constants.DISM_MOUNT_READONLY,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (Win32.DISM.IsSuccessful(mounted))
            {
                uint session;
                int opened = Win32.DISM.DismOpenSession(IMAGE_TEST_MOUNTPATH, null, null, out session);
                if (Win32.DISM.IsSuccessful(opened))
                {
                    IntPtr info;
                    uint count;
                    Win32.DISM.DismGetMountedImageInfo(out info, out count);

                    if(count > 0)
                    {
                        Win32.DISM.DismMountedImageInfo[] pkgInfo;
                        Win32.DISM.MarshalUnmananagedArray2Struct<Win32.DISM.DismMountedImageInfo>(info, (int)count, out pkgInfo);
                        success = pkgInfo[0].MountStatus == Win32.DISM.DismMountStatus.DismMountStatusOk;
                    }

                    Win32.DISM.DismCloseSession(session);
                }


                Win32.DISM.DismUnmountImage(IMAGE_TEST_MOUNTPATH, Win32.DISM.Constants.DISM_DISCARD_IMAGE,
                        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            Win32.DISM.DismShutdown();
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void CanAddPackage()
        {
            bool success = false;

            Win32.DISM.DismInitialize(Win32.DISM.DismLogLevel.DismLogErrors, null, null);
            Win32.DISM.DismMountImage(IMAGE_TEST_FILEPATH, IMAGE_TEST_MOUNTPATH, 1, null,
                Win32.DISM.DismImageIdentifier.DismImageIndex, Win32.DISM.Constants.DISM_MOUNT_READONLY,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            uint session;
            Win32.DISM.DismOpenSession(IMAGE_TEST_MOUNTPATH, null, null, out session);

            int added = Win32.DISM.DismAddPackage(session, IMAGE_TEST_PACKAGEPATH, false, true,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            success = Win32.DISM.IsSuccessful(added);

            Win32.DISM.DismCloseSession(session);
            Win32.DISM.DismUnmountImage(IMAGE_TEST_MOUNTPATH, Win32.DISM.Constants.DISM_DISCARD_IMAGE,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            Win32.DISM.DismShutdown();
            Assert.IsTrue(success);
        }
    }
}
