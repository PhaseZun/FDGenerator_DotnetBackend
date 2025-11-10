using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// 1️⃣ Add CORS
// ------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5205") // MVC frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ------------------------------
// 2️⃣ Logging setup
// ------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ------------------------------
// 3️⃣ Add controllers and services
// ------------------------------
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuthApi.Services.AuthService>();
builder.Services.AddSingleton<AuthApi.Services.MinioService>();
builder.Services.AddSingleton<AuthApi.Services.PdfService>();

// ------------------------------
// 4️⃣ Add JWT Authentication
// ------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 🔐 Configure token validation
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // checks for token expiry
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // (Optional) Add header if token expired
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

// ------------------------------
// 5️⃣ Build app
// ------------------------------
var app = builder.Build();

// ------------------------------
// 6️⃣ Middleware pipeline
// ------------------------------
app.UseCors();
app.UseRouting();
app.UseHttpsRedirection();

// 👇 Authentication must come BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
