# Metica SDK Tests

**Intro note**  
We currently only test the Storage System but we have long term plans to test as much as possible via unit testing.

## Setup

The only project-level action needed is to add the package to the project's *testables*. This is done manually in *Packages/manifest.json* where the following must be added

```
{
    ...
    
    "testables": [
    "com.metica.unity"
    ]
```

## Adding tests

Only use **NUnit** tests, which are identified by Unity's Test Runner by the `[Test]` attribute. Avoid `[UnityTest]` because they can complicate the integration with GitHub's (or other) CI/CD (in the future).
Depending on the supported `NUnit.Framework` version, `async/await` can be used in test method signatures. For better compatibility with Unity Test Runner (including Unity 2022) we use a `Task` wrapper. Example:

```
    [Test(Description = "Checks if the `Delete` method successfully deletes an entry.")]
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
```
