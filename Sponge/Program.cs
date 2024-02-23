using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Sponge;
using Sponge.Common;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//builder.Services.AddScoped<IClaimsTransformation, RolesClaimsTransformation>();
builder.Services.AddTransient<Email>();


builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
    //options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));

});

builder.Services.AddDbContext<SPONGE_Context>(
    //options => options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=zwqmyad0001;Initial Catalog=SPONGE_QA_APP;Persist Security Info=True;User ID=SPONGE_QA_APP;Password=Lw#Bbt/1sPBG;TrustServerCertificate=True")));
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionStrings")));

builder.Services.AddRazorPages();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMiddleware<ErrorHandlingMiddleware>();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
//app.Use(async (context, next) =>
//{
//    if (!context.User.Identity.IsAuthenticated)
//    {
//        // This triggers Windows Authentication.
//        var result = await context.AuthenticateAsync(NegotiateDefaults.AuthenticationScheme);

//        if (result?.Principal != null)
//        {
//            // Assign the authenticated ClaimsPrincipal to the context.
//            context.User = result.Principal;
//        }
//        else
//        {
//            // Authentication failed: You may log the failure details if needed.
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//            logger.LogError("Windows Authentication failed.");
//            // You can handle the response accordingly, for example:
//            // context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//            // return; // End the pipeline here if authentication is critical for further processing.
//        }
//    }
//    var isAuthenticated = context.User.Identity.IsAuthenticated;
//    var claimsIdentity = context.User.Identity as ClaimsIdentity;

//    if (!claimsIdentity.IsAuthenticated)
//    {
//        await context.AuthenticateAsync(NegotiateDefaults.AuthenticationScheme);
//        isAuthenticated = claimsIdentity.IsAuthenticated;  // Re-check after manually triggering authentication
//    }

//    if (isAuthenticated)
//    {
//        // Assuming the authentication provided the user's name
//        var dbContext = context.RequestServices.GetRequiredService<SPONGE_Context>();
//        var identityName = context.User.Identity.Name;
//        var userNameSplit = identityName.Split(new[] { "\\" }, StringSplitOptions.None);
//        var userNamePart = userNameSplit[1];

//        // Fetch the roles from the database.
//        var userRoles = await (from user in dbContext.SPG_USERS
//                               join userFunc in dbContext.SPG_USERS_FUNCTION on user.USER_ID equals userFunc.USER_ID
//                               join role in dbContext.SPG_ROLE on userFunc.ROLE_ID equals role.ROLE_ID
//                               where user.USER_ID == userNamePart
//                               select role.ROLE_NAME).Distinct().ToListAsync();

//        // Add the roles as claims.
//        //foreach (var roleName in userRoles)
//        //{

//        //        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));

//        //}
//        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
//    }

//    // Proceed to the next middleware
//    await next.Invoke();

//    var claims = context.User.Claims.ToList();
//});
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    // other endpoint configurations
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
