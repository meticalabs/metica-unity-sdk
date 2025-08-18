using Metica.SDK.Storage;
using NUnit.Framework;

[TestFixture]
public class FileStorageTests : StorageTestsBase
{
    [TearDown]
    public void TearDown()
    {
    }

    protected override IStorageStrategy CreateStrategy()
    {
        return new FileStorageStrategy();
    }
}
