using System.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChatService.Configuration;
using ChatService.Storage;
using Azure.Storage.Blobs;
using ChatService.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//Add Services for Container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

//Added Configurations
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobStorageSettings>(builder.Configuration.GetSection("BlobStorage"));

//Add services

builder.Services.AddSingleton<IUserStore,CosmosUserStorage>();
builder.Services.AddSingleton<IMessageStore,CosmosMessageStore>();
builder.Services.AddSingleton<IImageStore, BlobImageStorage>();
builder.Services.AddSingleton<IConversationStore, CosmosConversationStore>();
builder.Services.AddSingleton<IConversationParticipantStore, CosmosConversationParticipantStore>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddSingleton(sp =>
{
    var cosmosOptions = sp.GetRequiredService<IOptions<CosmosSettings>>();
    return new CosmosClient(cosmosOptions.Value.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var blobOptions = sp.GetRequiredService<IOptions<BlobStorageSettings>>();
    return new BlobServiceClient(blobOptions.Value.ConnectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();


app.Run();