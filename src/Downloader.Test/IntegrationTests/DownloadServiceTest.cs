using Downloader.DummyHttpServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Downloader.Test.IntegrationTests
{
    [TestClass]
    public class DownloadServiceTest : DownloadService
    {
        [TestMethod]
        public void CancelAsyncTest()
        {
            // arrange
            AsyncCompletedEventArgs eventArgs = null;
            string address = DummyFileHelper.GetFileUrl(DummyFileHelper.FileSize16Kb);
            Options = new DownloadConfiguration {
                BufferBlockSize = 1024,
                ChunkCount = 8,
                ParallelDownload = true,
                MaxTryAgainOnFailover = 100,
                OnTheFlyDownload = true
            };
            DownloadStarted += (s, e) => CancelAsync();
            DownloadFileCompleted += (s, e) => eventArgs = e;

            // act
            DownloadFileTaskAsync(address).Wait();

            // assert
            Assert.IsTrue(IsCancelled);
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(eventArgs.Cancelled);
            Assert.AreEqual(typeof(TaskCanceledException), eventArgs.Error.GetType());

            Clear();
        }

        [TestMethod]
        public void CompletesWithErrorWhenBadUrlTest()
        {
            // arrange
            Exception onCompletionException = null;
            string address = "https://nofile";
            FileInfo file = new FileInfo(Path.GetTempFileName());
            Options = new DownloadConfiguration {
                BufferBlockSize = 1024,
                ChunkCount = 8,
                ParallelDownload = true,
                MaxTryAgainOnFailover = 0,
                OnTheFlyDownload = true
            };
            DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e) {
                onCompletionException = e.Error;
            };

            // act
            DownloadFileTaskAsync(address, file.FullName).Wait();

            // assert
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onCompletionException);
            Assert.AreEqual(typeof(WebException), onCompletionException.GetType());

            Clear();
            file.Delete();
        }

        [TestMethod]
        public void ClearTest()
        {
            // arrange
            CancelAsync();

            // act
            Clear();

            // assert
            Assert.IsFalse(IsCancelled);
        }

        [TestMethod]
        public void TestPackageSituationAfterDispose()
        {
            // arrange
            var sampleDataLength = 1024;
            Package.TotalFileSize = sampleDataLength * 64;
            Package.Chunks = new[] { new Chunk(0, Package.TotalFileSize) };
            Package.Chunks[0].Storage = new MemoryStorage();
            Package.Chunks[0].Storage.WriteAsync(DummyData.GenerateRandomBytes(sampleDataLength), 0, sampleDataLength);
            Package.Chunks[0].SetValidPosition();

            // act
            Dispose();

            // assert
            Assert.IsNotNull(Package.Chunks);
            Assert.AreEqual(sampleDataLength, Package.ReceivedBytesSize);
            Assert.AreEqual(sampleDataLength * 64, Package.TotalFileSize);

            Package.Clear();
        }

        [TestMethod]
        public void TestPackageChunksDataAfterDispose()
        {
            // arrange
            var dummyData = DummyData.GenerateOrderedBytes(1024);
            Package.Chunks = new ChunkHub(Options).ChunkFile(1024 * 64, 64);
            foreach (var chunk in Package.Chunks)
            {
                chunk.Storage.WriteAsync(dummyData, 0, 1024).Wait();
            }

            // act
            Dispose();

            // assert
            Assert.IsNotNull(Package.Chunks);
            foreach (var chunk in Package.Chunks)
            {
                Assert.IsTrue(dummyData.AreEqual(chunk.Storage.OpenRead()));
            }

            Package.Clear();
        }

        [TestMethod]
        public void CancelPerformanceTest()
        {
            // arrange
            AsyncCompletedEventArgs eventArgs = null;
            var watch = new Stopwatch();
            string address = DummyFileHelper.GetFileUrl(DummyFileHelper.FileSize16Kb);
            Options = new DownloadConfiguration {
                BufferBlockSize = 1024,
                ChunkCount = 8,
                ParallelDownload = true,
                MaxTryAgainOnFailover = 100,
                OnTheFlyDownload = true
            };
            DownloadStarted += (s, e) => {
                watch.Start();
                CancelAsync();
            };
            DownloadFileCompleted += (s, e) => eventArgs = e;

            // act
            DownloadFileTaskAsync(address).Wait();
            watch.Stop();

            // assert
            Assert.IsTrue(eventArgs?.Cancelled);
            Assert.IsTrue(watch.ElapsedMilliseconds < 1000);

            Clear();
        }

        [TestMethod]
        public void ResumePerformanceTest()
        {
            // arrange
            AsyncCompletedEventArgs eventArgs = null;
            var watch = new Stopwatch();
            var isCancelled = false;
            string address = DummyFileHelper.GetFileUrl(DummyFileHelper.FileSize16Kb);
            Options = new DownloadConfiguration {
                BufferBlockSize = 1024,
                ChunkCount = 8,
                ParallelDownload = true,
                MaxTryAgainOnFailover = 100,
                OnTheFlyDownload = true
            };
            DownloadStarted += (s, e) => { 
                if (isCancelled == false) 
                    CancelAsync(); 
                isCancelled=true; 
            };
            DownloadFileCompleted += (s, e) => eventArgs = e;
            DownloadProgressChanged += (s, e) => watch.Stop();

            // act
            DownloadFileTaskAsync(address).Wait();
            watch.Start();
            DownloadFileTaskAsync(Package).Wait();

            // assert
            Assert.IsFalse(eventArgs?.Cancelled);
            Assert.IsTrue(watch.ElapsedMilliseconds < 1000);

            Clear();
        }
    }
}