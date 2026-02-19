using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.Repositrories;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using E_Commerce.Repositrories.UnitOfWork;
using E_Commerce.Services.Interfaces;
using E_Commerce.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Dapper;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce App", Version = "v1" });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});


// SQL Server configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

}).AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],

            IssuerSigningKey =
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(
                        builder.Configuration["JWT:SigningKey"]
                    )
                ),

            NameClaimType = ClaimTypes.NameIdentifier
        };
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWorkClass>();
builder.Services.AddScoped<IOrderService, OrderService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
    });

    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");

    app.MapGet("/", () => Results.Redirect("/swagger"));
}


// Basic reachability test
app.MapGet("/ping", () => Results.Ok("Pong! App is running."))
   .AllowAnonymous();


// DB connectivity test endpoint

app.MapGet("/db-test", async (IConfiguration configuration) =>
{
    var connectionString =
        configuration.GetConnectionString("DefaultConnection");

    try
    {
        using var connection = new System.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync();

        return Results.Ok(new
        {
            Success = true,
            Message = "Connected to SQL Server Database successfully!"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Connection failed: {ex.Message}");
    }
})
.AllowAnonymous();


// Apply migrations + seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("Applying migrations to the remote database...");

        await context.Database.MigrateAsync();

        Console.WriteLine("Migrations applied successfully. Seeding data...");

        await DbInitializer.SeedAsync(services);

        Console.WriteLine("Database initialization complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"FATAL DATABASE ERROR: {ex.Message}");
        Console.WriteLine($"STACK TRACE: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            Console.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
            Console.WriteLine($"INNER STACK: {ex.InnerException.StackTrace}");
        }

        Console.WriteLine("--------------------------------------------------");

        var logger =
            services.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex,
            "An error occurred while migrating or seeding the database.");
    }
}


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
