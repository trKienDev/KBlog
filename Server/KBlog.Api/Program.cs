using KBlog.Application.Contracts.Identity;
using KBlog.Application.Contracts.Persistence;
using KBlog.Application.Services;
using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using KBlog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// I. ĐĂNG KÝ DỊCH VỤ (SERVICE REGISTRATION)
// =================================================================

// 1. Đọc Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Thêm DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Thêm Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
	// Cấu hình mật khẩu (tùy chọn)
	options.Password.RequireDigit = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 4. Thêm và cấu hình Authentication với JWT Bearer
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];
var jwtKey = builder.Configuration["JwtSettings:Key"];

if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(jwtKey))
{
	throw new InvalidOperationException("JWT settings are not configured properly in appsettings.json or user secrets.");
}

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtIssuer,
		ValidAudience = jwtAudience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
});


// 5. Thêm các Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

// 6. Thêm các Services
builder.Services.AddScoped<IAuthService, AuthService>();	

// Cấu hình routing api
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// 6. Thêm Controller và Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "KBlog API", Version = "v1" });
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Please enter a valid token",
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		BearerFormat = "JWT",
		Scheme = "Bearer"
	});
	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			[]
		}
	});
});


var app = builder.Build();

// =================================================================
// II. CẤU HÌNH MIDDLEWARE PIPELINE
// =================================================================

// Seeding Roles & Default Admin User (Chạy trước khi ứng dụng bắt đầu nhận request)
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = services.GetRequiredService<UserManager<User>>();
		string[] roleNames = [ "Admin", "Reader" ];
		foreach (var roleName in roleNames)
		{
			if (!await roleManager.RoleExistsAsync(roleName))
			{
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}
		}
		var adminEmail = "admin@example.com";
		var adminUser = await userManager.FindByEmailAsync(adminEmail);
		if (adminUser == null)
		{
			var newAdminUser = new User { UserName = "admin", Email = adminEmail, EmailConfirmed = true, RegisteredAt = DateTime.UtcNow };
			var result = await userManager.CreateAsync(newAdminUser, "Admin123!");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(newAdminUser, "Admin");
			}
		}
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// !!! THỨ TỰ CỰC KỲ QUAN TRỌNG !!!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
