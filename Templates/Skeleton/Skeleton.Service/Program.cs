using Amazon.S3;

using Skeleton;
using Skeleton.Abstractions;
using Skeleton.Controllers;

using EventSourcing.Backbone;

using Microsoft.OpenApi.Models;

using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ###############  EVENT SOURCING CONFIGURATION STARTS ############################

var services = builder.Services;

// inject AWS credentials according to the profile definition at appsettings.json
// Remember to set it right!
// see: https://medium.com/r/?url=https%3A%2F%2Fcodewithmukesh.com%2Fblog%2Faws-credentials-for-dotnet-applications%2F
services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
services.AddAWSService<IAmazonS3>();



IWebHostEnvironment environment = builder.Environment;
string env = environment.EnvironmentName;

services.AddOpenTelemetryForEventSourcing(environment);

services.AddEventSourceRedisConnection();
services.AddKeyedProductCycleProducer(ProductCycleConstants.URI, ProductCycleConstants.S3_BUCKET, env);
services.AddKeyedConsumer(ProductCycleConstants.URI, ProductCycleConstants.S3_BUCKET, env);

services.AddHostedService<ConsumerJob>();


// ###############  EVENT SOURCING CONFIGURATION ENDS ############################

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    opt =>
    {
        opt.SupportNonNullableReferenceTypes();
        opt.IgnoreObsoleteProperties();
        opt.IgnoreObsoleteActions();
        opt.DescribeAllParametersInCamelCase();

        opt.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Environment setup",
            Description = @"<p><b>Use the following docker in order to setup the environment</b></p>
<p>docker run -p 6379:6379 -it --rm --name redis-Json redislabs/rejson:latest</p>
<p>docker run --name jaeger-otel  --rm -it -e COLLECTOR_OTLP_ENABLED=true -p 16686:16686 -p 4317:4317 -p 4318:4318  jaegertracing/all-in-one:latest</p>
",
        });
    }); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
