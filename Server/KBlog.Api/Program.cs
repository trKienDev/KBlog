using KBlog.Domain.Entities;
using KBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký ApplicationDbContext với DI Container
builder.Services.AddDbContext<ApplicationDbContext>(options =>	options.UseSqlServer(connectionString));

// 3. Đăng ký các dịch vụ của ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole>(options => {
	// Cấu hình mật khẩu
	options.Password.RequireDigit = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllers();
// THAY THẾ AddOpenApi() bằng các dòng chuẩn của Swashbuckle:
builder.Services.AddEndpointsApiExplorer(); // Cần thiết cho API Explorer và Swagger
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "KBlog API", Version = "v1" });
});

// Thêm dòng này để đọc user secrets trong môi trường development
if (builder.Environment.IsDevelopment())
{
	builder.Configuration.AddUserSecrets<Program>();
}

var app = builder.Build();

// Seeding Roles
using (var scope = app.Services.CreateScope()) {
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	string[] roles = { "Admin", "Reader" };
	foreach(var role in roles) {
		if(!await roleManager.RoleExistsAsync(role)) {
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	// THAY THẾ MapOpenApi() bằng các dòng chuẩn của Swashbuckle:
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "KBlog API V1");
		// Để Swagger UI ở trang gốc, thêm dòng sau:
		// c.RoutePrefix = string.Empty; // Nếu muốn Swagger ở ngay trang chủ (ví dụ: https://localhost:7222/)
	});
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();