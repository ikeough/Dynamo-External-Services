using DropboxService;
using NUnit.Framework;

namespace DropboxServiceTests
{
    [TestFixture]
    public class DropboxServiceTests
    {
        [Test]
        public void Test()
        {
            var dropBoxService = new DropBoxService
            {
                Authenticate = () => { Assert.Pass("You have faked authentication."); }
            };
            dropBoxService.Login();
        }
    }
}
