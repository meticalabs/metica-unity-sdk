using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

using Metica.Core;
using Metica.Storage;

public class MockLogger : ILog
{
    public LogLevel CurrentLogLevel { get; set; } = LogLevel.Off;
    public void LogDebug(Func<string> messageSupplier) { }
    public void LogError(Func<string> messageSupplier, Exception error = null) { }
    public void LogInfo(Func<string> messageSupplier) { }
    public void LogWarning(Func<string> messageSupplier) { }
}

public abstract class StorageTestsBase
{
    protected abstract IStorageStrategy CreateStrategy();

    protected StorageManager StorageManager;

    /// <summary>
    /// A dummy test object.
    /// </summary>
    public record TestObject
    {
        public int IntField { get; set; }
        public string StringField { get; set; }
        public Dictionary<string, string> DictionaryField { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        Registry.Register<ILog>(new MockLogger());
        StorageManager = new StorageManager(CreateStrategy());
    }

    [Test(Description = "Check if `SaveAsync` and `LoadAsync` save and load a dummy object.")]
    /// Workaround for Unity Test Runner compatibility (in Unity 2022)
    public void SaveAndLoad_WorksCorrectly()
    {
        Task.Run(async () =>
        {
            await SaveAndLoad_WorksCorrectly_Task();
        }).GetAwaiter().GetResult();
    }

    private async Task SaveAndLoad_WorksCorrectly_Task()
    {
        var obj = new TestObject
        {
            IntField = 42,
            StringField = "hello",
            DictionaryField = new() { { "a", "A" }, { "b", "B" } }
        };

        await StorageManager.SaveAsync("test.json", obj);
        var loaded = await StorageManager.LoadAsync<TestObject>("test.json");

        Assert.That(StorageManager.Exists("test.json"), Is.True);
        Assert.That(loaded.IntField, Is.EqualTo(42));
        Assert.That(loaded.StringField, Is.EqualTo("hello"));
        Assert.That(loaded.DictionaryField["a"], Is.EqualTo("A"));
    }

    [Test(Description = "Checks if the `Delete` method successfully deletes an entry.")]
    /// Workaround for Unity Test Runner compatibility (in Unity 2022)
    public void Delete_RemovesObject()
    {
        Task.Run(async () =>
        {
            await Delete_RemovesObject_Task();
        }).GetAwaiter().GetResult();
    }

    public async Task Delete_RemovesObject_Task()
    {
        var obj = new TestObject { IntField = 99 };
        await StorageManager.SaveAsync("delete_me.json", obj);

        StorageManager.Delete("delete_me.json");

        Assert.That(StorageManager.Exists("delete_me.json"), Is.False);
    }
}
