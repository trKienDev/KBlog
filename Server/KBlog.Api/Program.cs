var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// THAY THẾ AddOpenApi() bằng các dòng chuẩn của Swashbuckle:
builder.Services.AddEndpointsApiExplorer(); // Cần thiết cho API Explorer và Swagger
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "KBlog API", Version = "v1" });
});

var app = builder.Build();

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