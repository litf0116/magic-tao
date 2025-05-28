using Microsoft.AspNetCore.Antiforgery;
using TtWork.Project.Controllers;

namespace TtWork.Project.Web.Host.Controllers
{
    public class AntiForgeryController : AbpControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
