using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VHEmpAPI.Models;
using VHEmpAPI.Models.Repository;
using VHEmpAPI.Interfaces;
using VHEmpAPI;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VHEmpAPI.DbCommon;

var builder = WebApplication.CreateBuilder(args);

//var firebaseCredentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirebaseCredentials", "exampledemo-7c519-firebase-adminsdk-e29z6-5e5f41cc02.json");

//FirebaseApp firebaseApp = null;

//try
//{
//    firebaseApp = FirebaseApp.Create(new AppOptions
//    {
//        Credential = GoogleCredential.FromFile(firebaseCredentialPath)
//    });
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"FirebaseApp creation error: {ex.Message}");
//    throw; // Handle or rethrow the exception as appropriate
//}

//builder.Services.AddSingleton<FirebaseService>();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = false;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = false,
        ValidateIssuer = false,
        //ValidateLifetime = true,
        ValidateAudience = false,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});

builder.Services.AddSingleton<IJwtAuth>(new Auth(builder.Configuration["JWT:Key"], builder.Configuration));

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
builder.Configuration.GetConnectionString("VHMobileDBConnection")
));

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddControllers();
builder.Services.AddSingleton<FirebaseService>();
builder.Services.AddHostedService<NotificationService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        //In = ParameterLocation.Header,
        //Name = "Authorization",
        //Type = SecuritySchemeType.ApiKey

        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    //options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IDBMethods, DBMethods>();

builder.Services.AddScoped<IEmpLoginRepository, EmpLoginRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
