using BlazorCodeBase.Server.Database.DbContext;
using BlazorCodeBase.Server.Database.Interceptor;
using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Server.Model.Command;
using BlazorCodeBase.Server.Model.Common;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using BaseResponse = BlazorCodeBase.Shared.BaseResponse;

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
                builder.WithOrigins("http://localhost:3000")
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
        .AddAuthenticationJwtBearer(s => s.SigningKey = configuration["Settings:Jwt:Key"], o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Settings:Jwt:Key"]!)),
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false
            };

            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies[Constant.ACCESS_TOKEN];
                    return Task.CompletedTask;
                }
            };
        })
        .AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("amr", "mfa")
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

app.UseHttpsRedirection();
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
           var response = new BaseResponse()
           {
               Message = exception?.Error?.Message,
               StatusCode = httpContext.Response.StatusCode,
           };
           httpContext.Response.StatusCode = 200;
           httpContext.Response.ContentType = "application/json";
           var responseText = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
           Log.Error(responseText);
           await httpContext.Response.WriteAsync(responseText);
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
       c.Serializer.ResponseSerializer = (rsp, dto, cType, jCtx, ct) =>
       {
           rsp.ContentType = cType;
           string responseText = string.Empty;
           if (rsp.StatusCode > 299)
           {
               rsp.StatusCode = 200;
               responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
               Log.Error(responseText);
           }
           else
           {
               if (dto is not BaseResponse)
               {
                   var response = new BaseResponse()
                   {
                       StatusCode = rsp.StatusCode,
                       Data = dto,
                       Message = "OK"
                   };
                   responseText = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
                   Log.Information(responseText);
               }
               else
               {
                   responseText = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
                   Log.Information(responseText);
               }
           }
           return rsp.WriteAsync(responseText, ct);
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

    builder.Services.AddScoped<ModifieldInterceptor>();

    builder.Services.AddScoped<RoleManager<IdentityRole>>();
    builder.Services.AddScoped<UserManager<UserInfo>>();
    builder.Services.AddScoped<SignInManager<UserInfo>>();

    builder.Services.AddScoped<UserInfoBuilder>();
}