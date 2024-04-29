using Metica.Unity;
using NUnit.Framework;

abstract class Utils
{
    public const string TestUserId = "testUser";
    public const string TestApp = "testApp";
    public const string TestKey = "testKey";
    public const string testOfferId = "testOffer";
    public const string testBundleId = "testBundle";
    public const string testVariantId = "testVariant";
    public const string testPlacementId = "testPlacement";

    public static void InitSdk()
    {
        MeticaAPI.Initialise(TestUserId, TestApp, TestKey, result => { Assert.That(result.Result); });
    }
}