using api.Repository;
using api.Service;
using Google.Cloud.Firestore;

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
