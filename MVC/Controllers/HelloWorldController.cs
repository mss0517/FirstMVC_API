using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace WebApplication1.Controllers
{
    public class HelloWorldController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public string Welcome(string name, int Id = 1)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, ID: {Id}");
        }
    }
}