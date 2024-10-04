using BlazorCodeBase.Server.Database.DbContext;
using BlazorCodeBase.Server.Database.Interceptor;
using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Server.Model.Command;
using BlazorCodeBase.Server.Model.Common;
using FastEndpoints;
using FastEndpoints.Swagger;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var configuration = builder.Configuration
                            .AddJsonFile(builder.Configuration["SettingFile"]!.ToString(),
                                        false,
                                        reloadOnChange: true)
                            .Build();

builder.Services
        .Configure<Settings>(configuration.GetSection(nameof(Settings)))
        .AddEntityFrameworkSqlite()
        .AddDbContext<SQLiteContext>(options => options.UseSqlite(configuration["Settings:ConnectionString:DbContext"]))
        .AddEndpointsApiExplorer()
        .AddHealthChecks()
        .Services
        .AddIdentityCore<UserInfo>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;

            options.User.RequireUniqueEmail = true;

            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);

            options.SignIn.RequireConfirmedEmail = true;

            options.Tokens.EmailConfirmationTokenProvider = "Email";
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultEmailProvider;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<SQLiteContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<EmailTokenProvider<UserInfo>>("Email")
        .Services
        .Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(5);
        })
        .AddHttpContextAccessor()
        .AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins("https://localhost:7081")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("Content-Disposition");
            });
        })
        .Configure<JsonOptions>(op =>
        {
            op.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                                                        | JsonIgnoreCondition.WhenWritingNull;
        })
        .AddOutputCache(options =>
        {
            options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(10);
        })
            .AddAuthentication(options =>
            {
                options.DefaultScheme = "MultiAuthScheme";
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = configuration["Settings:GoogleAuthen:ClientId"];
                options.ClientSecret = configuration["Settings:GoogleAuthen:ClientSecret"];
                options.CallbackPath = "/api/google/signin-google";

                options.Events.OnRedirectToAuthorizationEndpoint = context =>
                {
                    var uriBuilder = new UriBuilder(context.RedirectUri);
                    var query = QueryHelpers.ParseQuery(uriBuilder.Query);
                    var queryDict = new Dictionary<string, string>(query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));
                    queryDict.TryAdd("prompt", "select_account");
                    uriBuilder.Query = QueryString.Create(queryDict).ToUriComponent();
                    context.Response.Redirect(uriBuilder.ToString());
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Settings:Jwt:Key"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[Constant.ACCESS_TOKEN];
                        return Task.CompletedTask;
                    }
                };
            })
            .AddPolicyScheme("MultiAuthScheme", "MultiAuthScheme", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api/google"))
                    {
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    }
                    else
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }
                };
            })
        .Services
        .AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.AuthenticationMethod, "mfa")
                .Build();
        })
        .AddFastEndpoints()
        .SwaggerDocument(o =>
        {
            o.EnableJWTBearerAuth = false;
            o.DocumentSettings = s =>
            {
                s.Title = "ApiCodeBase";
                s.Version = "v1.0";
            };
        })
        .AddFluentEmail(configuration["Settings:MailSettings:SenderMail"])
        .AddMailKitSender(new()
        {
            User = configuration["Settings:MailSettings:Username"],
            Password = configuration["Settings:MailSettings:Password"],
            Server = configuration["Settings:MailSettings:Host"],
            Port = int.Parse(configuration["Settings:MailSettings:Port"]),
            SocketOptions = bool.Parse(configuration["Settings:MailSettings:IsSecure"]) ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            RequiresAuthentication = bool.Parse(configuration["Settings:MailSettings:IsAuthen"])
        })
        .AddRazorRenderer();
AddSerilog();
AddServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapHealthChecks("/api/pingServer").RequireAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.UseCors("CorsPolicy")
   .UseHttpsRedirection()
   .UseSwaggerUi()
   .UseSwaggerGen()
   .UseAuthentication()
   .UseAuthorization()
   .Use(async (context, next) =>
   {
       using (Serilog.Context.LogContext.PushProperty("UserId", context.User.Identity?.Name ?? string.Empty))
       {
           await next(context);
       }
   })
   .UseExceptionHandler(handler =>
   {
       handler.Run(async httpContext =>
       {
           var exception = httpContext.Features.Get<IExceptionHandlerFeature>();
           var responseText = Newtonsoft.Json.JsonConvert.SerializeObject((exception?.Error?.Message, httpContext.Response.StatusCode),
                                                                           Newtonsoft.Json.Formatting.Indented);
           Log.Error(responseText);
       });
   })
   .UseFastEndpoints(c =>
   {
       c.Endpoints.RoutePrefix = "api";
       c.Endpoints.Configurator = ep =>
       {
           //Middleware was here
           //ep.PostProcessor<LoggingRequest>(Order.Before);
           //ep.PostProcessor<LoggingResponse>(Order.After);
       };
       c.Serializer.RequestDeserializer = async (req, tDto, jCtx, ct) =>
       {
           using var reader = new StreamReader(req.Body);
           var requestBody = await reader.ReadToEndAsync(ct);
           if (req.QueryString.HasValue)
           {
               Log.Information($"{req.QueryString.Value}");
           }
           if (!string.IsNullOrWhiteSpace(requestBody))
           {
               Log.Information(requestBody);
           }
           return Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody, tDto);
       };
   });

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SQLiteContext>();
    db.Database.EnsureCreated();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await roleManager.CreateAsync(new IdentityRole("Guest"));
    await roleManager.CreateAsync(new IdentityRole("User"));
    await roleManager.CreateAsync(new IdentityRole("Admin"));
}

app.MapGet("api/google/login", async (context) =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/api/google/signin-google/" });
}).AllowAnonymous();

app.MapPost("api/User/Logout", async (HttpContext context, IOptionsSnapshot<Settings> settings) =>
{
    context.Response.Cookies.Delete(Constant.ACCESS_TOKEN, settings.Value.Jwt.CookieOpt);

}).AllowAnonymous(); 

app.Run();

void AddSerilog()
{
    string template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{TraceId}] [{UserId}] {Message:lj}{NewLine}{Exception}";
    Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Information()
                    .WriteTo.Async(a =>
                    {
                        a.Console(outputTemplate: template);
                    })
                    .WriteTo.Logger(lc =>
                    {
                        lc.WriteTo.Map("UserId", string.Empty, (name, wt) => wt.Async(a =>
                        {
                            a.File($"{Constant.LOGS}/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}/{name}/.txt",
                                    outputTemplate: template,
                                    rollingInterval: RollingInterval.Day,
                                    fileSizeLimitBytes: 10485760,
                                    rollOnFileSizeLimit: true,
                                    shared: true);
                        }));
                    })
                    .CreateLogger();
    builder.Logging.AddSerilog();
}

void AddServices()
{
    builder.Services.AddScoped<SendMailBuilder>();
    builder.Services.AddScoped<JwtGenerateBuilder>();
    builder.Services.AddScoped<RegisterBuilder>();

    builder.Services.AddScoped<ModifieldInterceptor>();

    builder.Services.AddScoped<RoleManager<IdentityRole>>();
    builder.Services.AddScoped<UserManager<UserInfo>>();
    builder.Services.AddScoped<SignInManager<UserInfo>>();

    builder.Services.AddScoped<UserInfoBuilder>();
}
