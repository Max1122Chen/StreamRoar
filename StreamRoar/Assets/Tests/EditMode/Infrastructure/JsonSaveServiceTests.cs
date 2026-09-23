using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using StreamRoar.Infrastructure;

namespace StreamRoar.Tests.EditMode.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    public sealed class JsonSaveServiceTests
    {
        string m_TempDirectory;
        JsonSaveService m_Save;

        [SetUp]
        public void SetUp()
        {
            m_TempDirectory = Path.Combine(Path.GetTempPath(), "StreamRoarTests", Guid.NewGuid().ToString("N"));
            m_Save = new JsonSaveService(m_TempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(m_TempDirectory))
                Directory.Delete(m_TempDirectory, true);
        }

        [Test]
        public void SaveThenTryLoad_ReturnsPersistedData()
        {
            var payload = new SampleSaveData { Name = "alpha", Score = 12 };
            m_Save.Save("slot1", payload);

            bool loaded = m_Save.TryLoad("slot1", out SampleSaveData restored);

            Assert.That(loaded, Is.True);
            Assert.That(restored.Name, Is.EqualTo("alpha"));
            Assert.That(restored.Score, Is.EqualTo(12));
            Assert.That(m_Save.Exists("slot1"), Is.True);
        }

        [Test]
        public void TryLoad_MissingFile_ReturnsFalse()
        {
            bool loaded = m_Save.TryLoad("missing", out SampleSaveData _);

            Assert.That(loaded, Is.False);
            Assert.That(m_Save.Exists("missing"), Is.False);
        }

        [Test]
        public void TryLoad_CorruptedJson_Throws()
        {
            Directory.CreateDirectory(m_TempDirectory);
            File.WriteAllText(Path.Combine(m_TempDirectory, "broken.json"), "{ not-json");

            Assert.Throws<JsonReaderException>(() => m_Save.TryLoad("broken", out SampleSaveData _));
        }

        [Test]
        public void Delete_RemovesExistingSave()
        {
            m_Save.Save("slot2", new SampleSaveData { Name = "beta", Score = 1 });
            m_Save.Delete("slot2");

            Assert.That(m_Save.Exists("slot2"), Is.False);
            Assert.That(m_Save.TryLoad("slot2", out SampleSaveData _), Is.False);
        }

        [Test]
        public void Save_InvalidId_Throws()
        {
            Assert.Throws<ArgumentException>(() => m_Save.Save("../hack", new SampleSaveData()));
        }

        sealed class SampleSaveData
        {
            public string Name;
            public int Score;
        }
    }
}
