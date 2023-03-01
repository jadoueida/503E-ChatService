using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChatService.Configuration;
using ChatService.Storage;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(x =>
{
    var configuration = builder.Configuration.GetSection("BlobStorage").Get<BlobStorageConfiguration>();
    return new BlobServiceClient(configuration.ConnectionString);
});

builder.Services.AddSingleton<IUserStore, CosmosUserStorage>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobStorageConfiguration>(builder.Configuration.GetSection("BlobStorage"));

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