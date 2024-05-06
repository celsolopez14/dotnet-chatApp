using api.Interfaces;
using api.Repository;
using api.Service;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Get the current directory
string currentDirectory = Directory.GetCurrentDirectory();

string firebase_credentialsPath = Path.Combine(currentDirectory, "firestoredb_credentials.json");
string vertexai_credentialsPath = Path.Combine(currentDirectory, "vertexai_credentials.json");

GoogleCredential firebase_credentials = GoogleCredential.FromFile(firebase_credentialsPath);

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", vertexai_credentialsPath);

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

builder.Services.AddSingleton<FirebaseApp>((s) =>
{
    return FirebaseApp.Create(new AppOptions
    {
        Credential = firebase_credentials
    });
});

builder.Services.AddScoped<FirestoreDb>((s) =>
{
    // Create the FirestoreClient
    FirestoreClient firestoreClient = new FirestoreClientBuilder
    {
        // Configure the Firestore client options as needed
        ChannelCredentials = firebase_credentials.ToChannelCredentials()
    }.Build();

    return FirestoreDb.Create(configuration["Firebase:project_id"], firestoreClient);
});

builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<PredictionServiceClient>((s) =>
{
    return new PredictionServiceClientBuilder
    {
        Endpoint = $"{configuration["VertexAI:location"]}-aiplatform.googleapis.com"
    }.Build();
});

builder.Services.AddScoped<GenerateContentRequest>((s) =>
{
    Content systemInstructions = new Content();
    systemInstructions.Parts.AddRange(new List<Part>{
        new Part{Text = "Keep responses conversational and informal."},
        new Part{Text = "Feel free to include emojis in your responses for added expression."},
        new Part{Text = "Keep responses positive and upbeat in tone."}
    });
    return new GenerateContentRequest
    {
        Model = $"projects/{configuration["VertexAI:project_id"]}/locations/{configuration["VertexAI:location"]}/publishers/{configuration["VertexAI:publisher"]}/models/{configuration["VertexAI:model"]}",
        GenerationConfig = new GenerationConfig
        {
            Temperature = 0.4f,
            TopP = 1,
            TopK = 32,
            MaxOutputTokens = 2048,
        },
        SystemInstruction = systemInstructions
    };
});

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
