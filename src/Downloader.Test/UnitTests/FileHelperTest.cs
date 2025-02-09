﻿using System;
using System.IO;
using Downloader.DummyHttpServer;
using Downloader.Test.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Downloader.Test.UnitTests
{
    [TestClass]
    public class FileHelperTest
    {
        [TestMethod]
        public void CreateFileSpecialPathTest()
        {
            // arrange
            string baseUrl = Path.Combine(DummyFileHelper.TempDirectory, "downloader", "test");
            string filename = Path.Combine(baseUrl, Guid.NewGuid().ToString("N") + ".test");

            // act
            FileHelper.CreateFile(filename).Dispose();

            // assert
            Assert.IsTrue(File.Exists(filename));

            File.Delete(filename);
        }

        [TestMethod]
        public void CreateFileNoPathTest()
        {
            // arrange
            string baseUrl = "  ";
            string filename = Path.Combine(baseUrl, Guid.NewGuid().ToString("N") + DummyFileHelper.TempFilesExtension);

            // act
            var fileStream = FileHelper.CreateFile(filename);

            // assert
            Assert.AreEqual(Stream.Null, fileStream);
        }

        [TestMethod]
        public void GetTempFileSpecialPathTest()
        {
            // arrange
            string baseUrl = Path.Combine(DummyFileHelper.TempDirectory, "downloader", "test");

            // act
            string tempFile = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);

            // assert
            Assert.IsTrue(tempFile.StartsWith(baseUrl));

            File.Delete(tempFile);
        }

        [TestMethod]
        public void GetTempFileNoPathTest()
        {
            // arrange
            string baseUrl = " ";
            string tempFolder = DummyFileHelper.TempDirectory;

            // act
            string tempFile = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);

            // assert
            Assert.IsTrue(tempFile.StartsWith(tempFolder));

            File.Delete(tempFile);
        }

        [TestMethod]
        public void GetTempFileNullPathTest()
        {
            // arrange
            string tempFolder = DummyFileHelper.TempDirectory;

            // act
            string tempFile = FileHelper.GetTempFile(null, string.Empty);

            // assert
            Assert.IsTrue(tempFile.StartsWith(tempFolder));

            File.Delete(tempFile);
        }

        [TestMethod]
        public void GetTempFileSpecialPathNonDuplicationTest()
        {
            // arrange
            string baseUrl = Path.Combine(DummyFileHelper.TempDirectory, "downloader", "test");

            // act
            string tempFile1 = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);
            string tempFile2 = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);

            // assert
            Assert.AreNotEqual(tempFile1, tempFile2);

            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }

        [TestMethod]
        public void GetTempFileNoPathNonDuplicationTest()
        {
            // arrange
            string baseUrl = "     ";

            // act
            string tempFile1 = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);
            string tempFile2 = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);

            // assert
            Assert.AreNotEqual(tempFile1, tempFile2);

            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }

        [TestMethod]
        public void GetTempFileNullPathNonDuplicationTest()
        {
            // act
            string tempFile1 = FileHelper.GetTempFile(null, string.Empty);
            string tempFile2 = FileHelper.GetTempFile(null, string.Empty);

            // assert
            Assert.AreNotEqual(tempFile1, tempFile2);

            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }

        [TestMethod]
        public void GetTempFileSpecialPathCreationTest()
        {
            // arrange
            string baseUrl = Path.Combine(DummyFileHelper.TempDirectory, "downloader", "test");

            // act
            string tempFile = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);

            // assert
            Assert.IsTrue(File.Exists(tempFile));

            File.Delete(tempFile);
        }

        [TestMethod]
        public void GetTempFileNullPathCreationTest()
        {
            // act
            string tempFile = FileHelper.GetTempFile(null, string.Empty);

            // assert
            Assert.IsTrue(File.Exists(tempFile));

            File.Delete(tempFile);
        }

        [TestMethod]
        public void GetTempFileNoPathCreationTest()
        {
            // arrange
            string baseUrl = " ";

            // act
            string tempFile = FileHelper.GetTempFile(baseUrl, DummyFileHelper.TempFilesExtension);

            // assert
            Assert.IsTrue(File.Exists(tempFile));

            File.Delete(tempFile);
        }

        [TestMethod]
        public void GetAvailableFreeSpaceOnDiskTest()
        {
            // arrange
            var mainDriveRoot = Path.GetPathRoot(DummyFileHelper.TempDirectory);
            var mainDrive = new DriveInfo(mainDriveRoot ?? string.Empty);
            var mainDriveAvailableFreeSpace = mainDrive.AvailableFreeSpace - 1024;

            // act
            var availableFreeSpace = FileHelper.GetAvailableFreeSpaceOnDisk(DummyFileHelper.TempDirectory);

            // assert
            Assert.IsTrue(mainDriveAvailableFreeSpace < availableFreeSpace);
        }

        [TestMethod]
        public void GetAvailableFreeSpaceOnDiskWhenUncPathTest()
        {
            // arrange
            var mainDriveRoot = Path.GetPathRoot("\\UNC_Server_1234584456465487981231\\testFolder\\test.test");

            // act
            var availableFreeSpace = FileHelper.GetAvailableFreeSpaceOnDisk(mainDriveRoot);

            // assert
            Assert.AreEqual(availableFreeSpace, 0);
        }

        [TestMethod]
        public void ThrowIfNotEnoughSpaceTest()
        {
            // arrange
            var mainDriveRoot = Path.GetPathRoot(DummyFileHelper.TempDirectory);

            // act
            void ThrowIfNotEnoughSpaceMethod() => FileHelper.ThrowIfNotEnoughSpace(long.MaxValue, mainDriveRoot);

            // assert
            Assert.ThrowsException<IOException>(ThrowIfNotEnoughSpaceMethod);
        }

        [TestMethod]
        public void ThrowIfNotEnoughSpaceWhenIsNullTest()
        {
            // act
            void ThrowIfNotEnoughSpaceMethod() => FileHelper.ThrowIfNotEnoughSpace(long.MaxValue, null);

            // assert
            AssertHelper.DoesNotThrow<IOException>(ThrowIfNotEnoughSpaceMethod);
        }

        [TestMethod]
        public void ThrowIfNotEnoughSpaceWhenPathIsNullTest()
        {
            // arrange
            var mainDriveRoot = Path.GetPathRoot(DummyFileHelper.TempDirectory);

            // act
            void ThrowIfNotEnoughSpaceMethod() => FileHelper.ThrowIfNotEnoughSpace(1, mainDriveRoot, null);

            // assert
            AssertHelper.DoesNotThrow<IOException>(ThrowIfNotEnoughSpaceMethod);
        }
    }
}
