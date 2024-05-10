using elFinder.NetCore.Drivers.FileSystem;
using elFinder.NetCore;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/quan-ly-tep-tin")]
    [Route("admin/quan-ly-tep-tin/{action}")]
    public class FileManagerController : Controller
    {
        readonly IWebHostEnvironment _env;
        public FileManagerController(IWebHostEnvironment env) => _env = env;

        [Route("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            var result = await connector.ProcessAsync(Request);
            return result;
        }

        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            string pathroot = "files";
            var driver = new FileSystemDriver();
            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);
            string rootDirectory = Path.Combine(_env.WebRootPath, pathroot);
            string url = $"{uri.Scheme}://{uri.Authority}/{pathroot}/";
            string urlthumb = $"{uri.Scheme}://{uri.Authority}/admin/quan-ly-tep-tin/thumb/"; // Sửa đổi route ở đây
            var root = new RootVolume(rootDirectory, url, urlthumb)
            {
                IsReadOnly = false,
                IsLocked = false,
                Alias = "files",
                ThumbnailSize = 100,
            };
            driver.AddRoot(root);
            return new Connector(driver)
            {
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}
