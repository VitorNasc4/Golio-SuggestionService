using Azure.Identity;
using SuggestionService.Consumers;
using SuggestionService.Repository;
using SuggestionService.Repository.Interfaces;
using SuggestionService.Service;
using SuggestionService.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var appConfigString = Environment.GetEnvironmentVariable("APP_CONFIG");
if (string.IsNullOrEmpty(appConfigString))
{
  appConfigString = builder.Configuration["ConnectionStrings:AppConfig"];
}
builder.Configuration.AddAzureAppConfiguration(options =>
{
  options.Connect(appConfigString)
      .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISuggestionRepository, SuggestionRepository>();
builder.Services.AddScoped<IMessageBusService, MessageBusService>();
builder.Services.AddScoped<ISuggestionService, SuggestionService.Service.SuggestionService>();

builder.Services.AddHostedService<NewSuggestionConsumer>();
builder.Services.AddHostedService<SuggestionVoteConsumer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();


app.Run();

