using System.IO;
using Microsoft.AspNetCore.Mvc;
using Abp.Auditing;
using Abp.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.QrCode;
using Tt.HttpClient.Weixin;
using TtWork.HttpClient.Weixin;
using TtWork.Lib.Redis;
using TtWork.Project.Controllers;

namespace TtWork.Project.Web.Host.Controllers {
    public class HomeController : AbpControllerBase {
        private readonly ILogger<HomeController> _logger;
        private readonly IWeixinApi _weixinApi;
        private readonly IConfiguration _configuration;
        private readonly IRedisClient _redisClient;

        public HomeController(
            ILogger<HomeController> logger,
            IWeixinApi weixinApi,
            IConfiguration configuration,
            IRedisClient redisClient) {
            _logger = logger;
            _weixinApi = weixinApi;
            _configuration = configuration;
            _redisClient = redisClient;
        }

        public ActionResult Index() {
            return Redirect($"/index.html");
        }

        [DisableAuditing]
        [DontWrapResult]
        public string health() {
            _logger.LogInformation("health check");
            _logger.LogWarning("health check");
            return "ok";
        }

        [HttpGet]
        [DontWrapResult]
        public FileResult Qr(string str) {
            using var ms = new MemoryStream();
            // QRCodeHelper.GetQRCode(str, ms);
            // ms.Seek(0, SeekOrigin.Begin);
            return File(GetQrCode(str), "image/Png");
        }

        public byte[] GetQrCode(string text, int width = 200, int height = 200, int margin = 1,
            bool pureBarcode = true) {
            var content = "My IO";

            //创建生成器
            using (var generator = new QRCodeGenerator()) {
                // 设置错误校正能力（ECC）级别
                var qr = generator.CreateQrCode(content, ECCLevel.H);

                // 创建一个Canvas
                var info = new SKImageInfo(512, 512);
                using (var surface = SKSurface.Create(info)) {
                    var canvas = surface.Canvas;

                    // 渲染二维码到Canvas
                    canvas.Render(qr, info.Width, info.Height);

                    // 输出到文件
                    using (var image = surface.Snapshot())
                        // 将图片编码为字节数组  
                    using (var memoryStream = new MemoryStream()) {
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100)) {
                            data.SaveTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
        }
    }
}