using api.Interfaces;
using api.Repository;
using api.Service;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Get the current directory
string currentDirectory = Directory.GetCurrentDirectory();

string credentialsPath = Path.Combine(currentDirectory, "serviceAccountKey.json");

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

// Configure user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(currentDirectory)
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
    .Build();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
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
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{configuration["Firebase:project_id"]}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{configuration["Firebase:project_id"]}",
            ValidateAudience = true,
            ValidAudience = configuration["Firebase:project_id"],
            ValidateLifetime = true
        };
    });

builder.Services.AddScoped<FirebaseAuthClient>((sp) =>
new FirebaseAuthClient(new FirebaseAuthConfig
{
    ApiKey = configuration["Firebase:web_api_key"],
    AuthDomain = $"{configuration["Firebase:project_id"]}.firebaseapp.com",
    Providers = new FirebaseAuthProvider[]
    {
        new EmailProvider(),
        new GoogleProvider()
    }
}));

builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IChatRepository>((s) => new ChatRepository(
FirestoreDb.Create(configuration["Firebase:project_id"])
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseWebSockets();
app.Map("/ws/chat/{chatSessionId}", WebSocketService.HandleWebSocket);

app.MapControllers();
app.Run();
