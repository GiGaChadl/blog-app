    using System.Text;
using BlogApp.Application;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Infrastructure;
using BlogApp.Infrastructure.Identity;
using BlogApp.Infrastructure.Persistence;
using BlogApp.Infrastructure.Persistence.Seed;
using BlogApp.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ----- Cấu hình JWT -----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Thiếu section \"Jwt\" trong appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1) // mặc định .NET là 5 phút dung sai, siết lại cho chặt
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ----- Đăng ký Application + Infrastructure (theo đúng Clean Architecture: Api không tự
// biết chi tiết bên trong, chỉ gọi 2 extension method này) -----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddHostedService<BlogApp.Api.BackgroundJobs.SystemMaintenanceBackgroundService>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed admin đầu tiên nếu bảng admin_users đang trống (xem AdminSeeder.cs — chỉ chạy 1 lần,
// tự kiểm tra trước khi tạo, KHÔNG tạo trùng mỗi lần khởi động app).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await AdminSeeder.SeedAsync(db, passwordHasher, app.Configuration, logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseCors("AllowFrontend");

// Thứ tự BẮT BUỘC: Authentication trước Authorization (xác định "anh là ai" trước khi
// hỏi "anh có quyền làm việc này không").
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
