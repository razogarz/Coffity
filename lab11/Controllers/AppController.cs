using Microsoft.AspNetCore.Mvc;

namespace lab10.Controllers;

// // [Route("api/[controller]")]
// [Route("api")]
public class AppController : Controller {
    private Connector _connector;

    private SortedSet<string> _categories;
    
    private bool _return_with_paths = false;
    
    public AppController(Connector connector) {
        _connector = connector;
        _categories = _connector.GetCategories();
        _return_with_paths = Driver.return_with_paths;
    }
    
    [HttpGet]
    [Route("/")]
    public IActionResult Index(string categories = null, string sort = null) {
        
        HashSet<string> _selected_categories = new HashSet<string>();
        if (categories != null)
        {
            string[] cat_names_array = categories.Split(';');
            foreach (var cat_name in cat_names_array)
            {
                _selected_categories.Add(cat_name);
            }
        }
        List<(string, bool)> cats_list = new List<(string, bool)>();
        foreach (var category in _categories)
        {
            cats_list.Add((category, _selected_categories.Contains(category)));
        }
        ViewBag.cats_list = cats_list;

        if (sort == null)
        {
            sort = "asc";
        }
        ViewBag.sort = sort;

        var coffe_list = _connector.GetCoffe(_selected_categories, sort);
        if (coffe_list != null) {
            ViewBag.coffe_list = coffe_list;
        }

        if (_return_with_paths){
            return View("Index.cshtml");
        }
        else{
            return View();
        }
        
    }
    
    [HttpPost]
    [Route("/filters")]
    public IActionResult HandleCategories(string[] selectedCategories, string sort)
    {
        string cat_names = string.Join(';', selectedCategories);
        
        return Redirect("/?categories=" + cat_names + "&sort=" + sort);
    }
    //
    // [HttpGet]
    // [Route("/list/")]
    // public IActionResult List()
    // {
    //     var login = HttpContext.Session.GetString("login");
    //     var password = HttpContext.Session.GetString("haslo");
    //     ViewBag.Login = login;
    //     ViewBag.Password = password;
    //     
    //     return View("PageList.cshtml");
    // }
    //
    [HttpGet]
    [Route("/login/")]
    public IActionResult Login()
    {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");

        if (login != null && password != null) {
            return Redirect("/panel/");
        }
        
        if (_return_with_paths){
            return View("Login.cshtml");
        }
        else{
            return View();
        }
        
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
        
        // if (_return_with_paths){
        //     return View("Login.cshtml");
        // }
        // else{
        //     return View();
        // }
        return Redirect("/login/");
        
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
    [Route("/panel/")]
    public IActionResult Panel() {
        var login = HttpContext.Session.GetString("login");
        var password = HttpContext.Session.GetString("haslo");

        if (login != null && password != null)
        {
            ViewBag.Login = login;
            ViewBag.Password = password;
            if (_return_with_paths){
                return View("Panel.cshtml");
            }
            else{
                return View();
            }
        }

        return Redirect("/login/");
    }
    
    [HttpGet]
    [Route("/editCoffees/")]
    public IActionResult EditCoffees(string categories = null, string sort = null)
    {
        HashSet<string> _selected_categories = new HashSet<string>();
        if (categories != null)
        {
            string[] cat_names_array = categories.Split(';');
            foreach (var cat_name in cat_names_array)
            {
                _selected_categories.Add(cat_name);
            }
        }
        List<(string, bool)> cats_list = new List<(string, bool)>();
        foreach (var category in _categories)
        {
            cats_list.Add((category, _selected_categories.Contains(category)));
        }
        ViewBag.cats_list = cats_list;

        if (sort == null)
        {
            sort = "asc";
        }
        ViewBag.sort = sort;

        var coffe_list = _connector.GetCoffe(_selected_categories, sort);
        if (coffe_list != null) {
            ViewBag.coffe_list = coffe_list;
        }

        if (_return_with_paths){
            return View("EditCoffees.cshtml");
        }
        else{
            return View();
        }
    }
    
    [HttpPost]
    [Route("/coffeeEditForm/")]
    public IActionResult CoffeeEditForm(IFormCollection form)
    {
        int id = Int32.Parse(form["id"].ToString());

        ViewBag.coffe = _connector.GetCoffeWithRecipeAndCategories(id);

        
        if (_return_with_paths){
            return View("CoffeeEditForm.cshtml");
        }
        else{
            return View();
        }
    }
    
    [HttpPost]
    [Route("/saveCoffeeEdit/")]
    public IActionResult SaveCoffeeEdit(IFormCollection form)
    {
        int id = Int32.Parse(form["id"]);
        string name = form["name"].ToString();
        string image = form["image"].ToString();
        string desc = form["description"].ToString();
        _connector.UpdateCoffe(id, name, image, desc);
        return Redirect("/editCoffees/");
    }
    
    //
    // [HttpGet]
    // [Route("/critical/")]
    // public IActionResult CriticalAsset()
    // {
    //     var login = HttpContext.Session.GetString("login");
    //     var password = HttpContext.Session.GetString("haslo");
    //     
    //     if (login == null || password == null) {
    //         return Redirect("/login/");
    //     }
    //     
    //     return View("Critical.cshtml");
    // }
    //
    // [HttpGet]
    // [Route("/data/")]
    // public IActionResult Data()
    // {
    //     var login = HttpContext.Session.GetString("login");
    //     var password = HttpContext.Session.GetString("haslo");
    //     
    //     if (login == null || password == null) {
    //         return Redirect("/login/");
    //     }
    //     
    //     // ViewBag.Data = _connector.GetCoffe() ?? new List<(int,string)>();
    //     ViewBag.Data = new List<(int,string)>();
    //     
    //     return View("Data.cshtml");
    // }
    //
    // [HttpPost] 
    // [Route("/data/add")]
    // public IActionResult HandleAddDataForm(IFormCollection form)
    // {
    //     
    //     var login = HttpContext.Session.GetString("login");
    //     var password = HttpContext.Session.GetString("haslo");
    //     
    //     if (login == null || password == null) {
    //         return Redirect("/login/");
    //     }
    //     
    //     string new_data = form["new_data"].ToString();
    //
    //     // _connector.AddData(new_data);
    //     
    //     return Redirect("/data/");
    // }
    //
    // [HttpPost] 
    // [Route("/data/delete")]
    // public IActionResult HandleDeleteDataForm(IFormCollection form)
    // {
    //     
    //     var login = HttpContext.Session.GetString("login");
    //     var password = HttpContext.Session.GetString("haslo");
    //     
    //     if (login == null || password == null) {
    //         return Redirect("/login/");
    //     }
    //     
    //     int id = Int32.Parse(form["id"].ToString());
    //
    //     // _connector.RemoveData(id);
    //     
    //     return Redirect("/data/");
    // }
    //
    
    
    [HttpPost]
    [Route("/coffe")]
    public IActionResult Coffe(IFormCollection form) {
        int id = Int32.Parse(form["id"].ToString());

        ViewBag.coffe = _connector.GetCoffeWithRecipeAndCategories(id);
        

        
        if (_return_with_paths){
            return View("Coffe.cshtml");
        }
        else{
            return View();
        }
        
    }
    
    [HttpGet]
    [Route("/category/{cat_name}")]
    public IActionResult HandleCategoryRequest(string cat_name)
    {
        return Redirect("/?cat_names=" + cat_name);
    }
    
    
    
}