var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5205") // Your MVC frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuthApi.Services.AuthService>();
builder.Services.AddSingleton<AuthApi.Services.MinioService>();

// **Register PdfService**
builder.Services.AddSingleton<AuthApi.Services.PdfService>();

var app = builder.Build();

// Use CORS
app.UseCors();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
