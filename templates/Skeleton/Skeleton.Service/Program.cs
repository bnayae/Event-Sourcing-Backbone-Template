#if (s3)
using Amazon.S3;
#endif
using Skeleton;
using Skeleton.Abstractions;
using Skeleton.Controllers;
using Microsoft.OpenApi.Models;
using EventSourcing.Backbone;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ###############  EVENT SOURCING CONFIGURATION STARTS ############################

var services = builder.Services;

#if (s3 && s3Credentials == "profile")
// inject AWS credentials according to the profile definition at appsettings.json
// Remember to set it right!
// see: https://medium.com/r/?url=https%3A%2F%2Fcodewithmukesh.com%2Fblog%2Faws-credentials-for-dotnet-applications%2F
services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
services.AddAWSService<IAmazonS3>();
#endif

#if (EnableTelemetry)
builder.AddOpenTelemetryEventSourcing();
// Tune telemetry level
services.AddSingleton<TelemetryLevel>(LogLevel.Information);
// services.AddSingleton(new TelemetryLevel { Metric = LogLevel.Information, Trace = LogLevel.Debug });
#endif

services.AddEventSourceRedisConnection();
#if (EnableProducer && s3 )
builder.AddKeyedProductCycleProducer(ProductCycleConstants.URI, ProductCycleConstants.S3_BUCKET);
#endif
#if (EnableProducer && !s3 )
builder.AddKeyedProductCycleProducer(ProductCycleConstants.URI);
#endif
#if (EnableConsumer && s3)
builder.AddKeyedConsumer(ProductCycleConstants.URI, ProductCycleConstants.S3_BUCKET);
#endif
#if (EnableConsumer && !s3)
builder.AddKeyedConsumer(ProductCycleConstants.URI);
#endif

#if (EnableConsumer)
services.AddHostedService<ConsumerJob>();
#endif


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
            Description = 
            """
            Check the ReadMe for setting up developer environment.
            cd ./dockers/compose
            docker compose up -d
            Jaeger:  http://localhost:16686/search
            Grafana: http://localhost:3000
            """,
        });
    }); 

var app = builder.Build();

#if (EnableTelemetry && prometheus)
app.UseOpenTelemetryPrometheusScrapingEndpoint();
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetService<ILogger<Program>>();
List<string> switches = new();
#if (EnableProducer)
switches.Add("Producer");
#endif
#if (EnableConsumer)
switches.Add("Consumer");
#endif
#if (s3)
switches.Add("S3 Storage [Bucket:{CHANGE_THE_BUCKET}, Profile:{AWS_PROFILE}, Region:{AWS_PROFILE_REGION}]");
#endif
logger?.LogInformation("Service Configuration Event Sourcing `{event-bundle}` on URI: `{URI}`, Features: [{features}]", "ProductCycle", ProductCycleConstants.URI, string.Join(", ", switches));
    
app.Run();
