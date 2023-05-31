using Microsoft.AspNetCore.Mvc;

namespace lab10.Controllers;

// // [Route("api/[controller]")]
// [Route("api")]
public class AppController : Controller {
    private Connector _connector;
    
    
    public AppController(Connector connector) {
        _connector = connector;
    }
    
    [HttpGet]
    [Route("/")]
    public IActionResult Index()
    {
        return View("Index.cshtml");
    }
    
    [HttpGet]
    [Route("/list/")]
    public IActionResult List()
    {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");
        ViewBag.Login = login;
        ViewBag.Password = password;
        
        return View("PageList.cshtml");
    }
    
    [HttpGet]
    [Route("/panel/")]
    public IActionResult Panel() {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");

        if (login != null && password != null)
        {
            ViewBag.Login = login;
            ViewBag.Password = password;
            return View("Panel.cshtml");
        }

        return Redirect("/login/");
    }
    
    [HttpGet]
    [Route("/login/")]
    public IActionResult Login()
    {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");

        if (login != null && password != null) {
            return Redirect("/panel/");
        }
        return View("Login.cshtml");
    }
    
    [HttpPost] 
    [Route("/login/")]
    public IActionResult HandleLoginForm(IFormCollection form)
    {
        string login = form["login"].ToString();
        string password = form["password"].ToString();
        
        if (_connector.ValidateUser(login, password))
        {
            ViewBag.Login = login;
            ViewBag.Password = password;
            HttpContext.Session.SetString("login", login);
            HttpContext.Session.SetString("haslo", password);
            return Redirect("/panel/");
        }
        
        ViewBag.ErrorMessage = "Niepoprawny login lub hasło";
        return View("Login.cshtml");
    }

    [HttpPost] 
    [Route("/logout/")]
    public IActionResult HandleLogoutForm(IFormCollection form)
    {
        
        HttpContext.Session.Remove("login");
        HttpContext.Session.Remove("haslo");
        
        return Redirect("/panel/");
    }
    
    [HttpGet]
    [Route("/critical/")]
    public IActionResult CriticalAsset()
    {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");
        
        if (login == null || password == null) {
            return Redirect("/login/");
        }
        
        return View("Critical.cshtml");
    }
    
    [HttpGet]
    [Route("/data/")]
    public IActionResult Data()
    {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");
        
        if (login == null || password == null) {
            return Redirect("/login/");
        }
        
        ViewBag.Data = _connector.getData() ?? new List<(int,string)>();
        
        return View("Data.cshtml");
    }
    
    [HttpPost] 
    [Route("/data/add")]
    public IActionResult HandleAddDataForm(IFormCollection form)
    {
        
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");
        
        if (login == null || password == null) {
            return Redirect("/login/");
        }
        
        string new_data = form["new_data"].ToString();

        _connector.AddData(new_data);
        
        return Redirect("/data/");
    }
    
    [HttpPost] 
    [Route("/data/delete")]
    public IActionResult HandleDeleteDataForm(IFormCollection form)
    {
        
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");
        
        if (login == null || password == null) {
            return Redirect("/login/");
        }
        
        int id = Int32.Parse(form["id"].ToString());

        _connector.RemoveData(id);
        
        return Redirect("/data/");
    }
    
}