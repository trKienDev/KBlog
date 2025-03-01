﻿using KBlog.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KBlog.Services.Interfaces;
using KBlog.Services.Implementations;
using KBlog.Data.Repository.Interfaces;
using KBlog.Data.Repository.Implementations;
using KBlog.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using KBlog.Services;

var builder = WebApplication.CreateBuilder(args);

// Config Serilog (logging)
builder.Host.UseSerilog((context, config) =>
{
	config.WriteTo.Console();
});

// Config CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigins", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
		      .AllowAnyMethod()
		      .AllowAnyHeader()
		      .AllowCredentials();
	});
});

// Config Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "defaul-secret-key")),
			ValidateIssuer = false,
			ValidateAudience = false,
			RequireExpirationTime = true,
			ValidateLifetime = true
		};

		// Thêm sự kiện để log các hành động
		options.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = context =>
			{
				Console.WriteLine($"Authentication failed: {context.Exception.Message}");
				return Task.CompletedTask;
			},
			OnTokenValidated = context =>
			{
				var userName = context.Principal?.Identity?.Name;
				if(!string.IsNullOrEmpty( userName ) ) {
					Console.WriteLine($"Token validated for user: {userName}" );
				} else {
					Console.WriteLine("Token validated, but user name is not found");
				}
				return Task.CompletedTask;
			}
		};
	});

// Config API Versioning
builder.Services.AddApiVersioning(options =>
{
	options.ReportApiVersions = true;
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
});

// Config Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { 
		Title = "My API", 
		Version = "v1" 
	});
});

builder.Services.AddHttpContextAccessor();

// DbContext
builder.Services.AddDbContext<KBlogDbContext>(
		options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWebSocketService, WebSocketService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR()
	.AddHubOptions<EmailVerificationHub>(options => {
		options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
	}
);

builder.Services.AddSingleton<IUserIdProvider, EmailAsUserIdProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

// Khai báo ContentTypeProvider để trình duyệt nhận diện .yaml
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".yaml"] = "application/x-yaml";

// Đảm bảo file YAML được phục vụ
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(
	Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
	RequestPath = "",
	ContentTypeProvider = provider
});

app.UseRouting();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.RoutePrefix = "swagger";
		c.SwaggerEndpoint("/openapi.yaml", "My API v1 (YAML)");
	});
}

app.UseWebSockets();
app.Use(async (context, next) =>
{
	if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
	{
		var webSocketService = context.RequestServices.GetRequiredService<IWebSocketService>();
		await webSocketService.HandleWebSocket(context);
	}
	else
	{
		await next();
	}
});

// CORS
app.UseCors("AllowSpecificOrigins");
//Middleware Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.UseRouting();
app.MapHub<EmailVerificationHub>("/emailVerificationHub");

app.Run();
