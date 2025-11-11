
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
builder.Services.AddSingleton<AuthApi.Services.PdfService>();

// ------------------------------
// 6️⃣ Build app
// ------------------------------
var app = builder.Build();
// ------------------------------
// 7️⃣ Middleware pipeline
// ------------------------------
app.UseCors();
app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
