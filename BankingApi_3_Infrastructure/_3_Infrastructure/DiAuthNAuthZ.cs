using BankingApi._3_Infrastructure._3_Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
namespace BankingApi._3_Infrastructure;

public static class DiAuthNAuthZ {
   
   public static IServiceCollection AddAuthNAuthZ(
      this IServiceCollection services,
      IConfiguration config
   ) {
      services.AddOptions<AuthOptions>()
         .Bind(config.GetSection("AuthServer")) 
         .Validate(o => !string.IsNullOrWhiteSpace(o.Authority), "AuthServer:Authority is required.")
         .ValidateOnStart();

      var auth = config.GetSection("AuthServer").Get<AuthOptions>()
         ?? throw new InvalidOperationException("Missing configuration section 'AuthServer'.");

      Console.WriteLine($"JWT Bearer Authority: {auth.Authority}");
      Console.WriteLine($"JWT Bearer Audience: {auth.Audience}");
      Console.WriteLine($"JWT Bearer ValidateAudience: {auth.ValidateAudience}");
      Console.WriteLine($"JWT Bearer RequireHttpsMetadata: {auth.RequireHttpsMetadata}");
      Console.WriteLine($"JWT Bearer ClockSkewSeconds: {auth.ClockSkewSeconds}");

      //--- AuthN JWT Bearer --------------------------------------------------------------------
      services
         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(opt => {
            opt.Authority = auth.Authority;
            opt.RequireHttpsMetadata = auth.RequireHttpsMetadata;

            if (!string.IsNullOrWhiteSpace(auth.Audience))
               opt.Audience = auth.Audience;

            opt.TokenValidationParameters = new TokenValidationParameters {
               ValidateAudience = auth.ValidateAudience,
               ClockSkew = TimeSpan.FromSeconds(auth.ClockSkewSeconds)
            };

            // optional aber sehr hilfreich:
            opt.Events = new JwtBearerEvents {
               OnAuthenticationFailed = ctx => {
                  var log = ctx.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");
                  log.LogError(ctx.Exception, "JWT auth failed");
                  return Task.CompletedTask;
               },
               OnChallenge = ctx => {
                  var log = ctx.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");
                  log.LogWarning("JWT challenge: error={Error}, desc={Desc}",
                     ctx.Error, ctx.ErrorDescription);
                  return Task.CompletedTask;
               }
            };
         });
      
      
      //--- AuthZ -------------------------------------------------------------------------------
      services.AddAuthorization(options => {
         // Role-based coarse authorization (framework-friendly)
         options.AddPolicy("CustomersOnly", p => p.RequireRole("Customer"));
         options.AddPolicy("EmployeesOnly", p => p.RequireRole("Employee"));
         options.AddPolicy("CustomersOrEmployees", p => p.RequireRole("Customer", "Employee"));
      });


      return services;
   }
}