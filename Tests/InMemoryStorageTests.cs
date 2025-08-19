using Metica.SDK.Storage;
using NUnit.Framework;

[TestFixture]
public class InMemoryStorageTests : StorageTestsBase
{
    [TearDown]
    public void TearDown()
    {
    }

    protected override IStorageStrategy CreateStrategy()
        => new InMemoryStorageStrategy();
}
