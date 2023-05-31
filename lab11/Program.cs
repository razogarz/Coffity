using lab10;
using lab10.Controllers;
using Microsoft.Data.Sqlite;


class Driver {

    public static void Main(string[] args) {
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        //Dodanie obsługo sesji
        builder.Services.AddDistributedMemoryCache();
        
        // Register the implementation as a transient service
        builder.Services.AddTransient<Connector>();
        
        Connector connector = new Connector();
        connector.InitBD();


        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true; //plik cookie jest niedostępny przez skrypt po stronie klienta
            options.Cookie.IsEssential = true; //pliki cookie sesji będą zapisywane dzięki czemu sesje będzie mogła być śledzona podczas nawigacji lub przeładowania strony
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{action=Index}/");

        // app.MapGet("", context =>
        // {
        //     context.Response.Redirect("");
        //     return Task.CompletedTask;
        // });

        // app.Use(async (ctx, next) =>
        // {
        //     await next();
        //
        //     if(ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
        //     {
        //         //Re-execute the request so the user gets the error page
        //         string originalPath = ctx.Request.Path.Value;
        //         ctx.Items["originalPath"] = originalPath;
        //         ctx.Request.Path = "/";
        //         await next();
        //     }
        // });

        app.Run();
    }

}