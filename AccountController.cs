using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Customer360Clean.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        // إذا كان مسجل دخول بالفعل، يروح للوحة التحكم
        if (HttpContext.Session.GetString("IsLoggedIn") == "true")
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // اسم المستخدم: admin   كلمة المرور: 123
        if (username == "admin" && password == "123")
        {
            HttpContext.Session.SetString("IsLoggedIn", "true");
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
