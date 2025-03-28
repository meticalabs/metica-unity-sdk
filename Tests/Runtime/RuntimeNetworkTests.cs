using Metica.Network;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Metica.RuntimeTests
{
    public class RuntimeNetworkTests
    {
        [Test]
        public async Task RuntimeGetAsyncPasses()
        {
            IHttpService _http = new HttpServiceDotnet(10, 30, 60);
            var response = await _http.GetAsync("https://metica.com", null);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Status == HttpResponse.ResultStatus.Success, "GET request unsuccessful");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        //[UnityTest]
        //public IEnumerator RuntimeMeticaSdkAsyncCallsTestsWithEnumeratorPasses()
        //{
        //    // Use the Assert class to test conditions.
        //    // Use yield to skip a frame.
        //    yield return null;
        //}
    }
}
