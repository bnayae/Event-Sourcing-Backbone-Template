#if (s3)
using Amazon.S3;
#endif
using Skeleton;
using Skeleton.Abstractions;
using Skeleton.Controllers;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ###############  EVENT SOURCING CONFIGURATION STARTS ############################

var services = builder.Services;

#if (s3)
// inject AWS credentials according to the profile definition at appsettings.json
// Remember to set it right!
// see: https://medium.com/r/?url=https%3A%2F%2Fcodewithmukesh.com%2Fblog%2Faws-credentials-for-dotnet-applications%2F
services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
services.AddAWSService<IAmazonS3>();
#endif

IWebHostEnvironment environment = builder.Environment;
string env = environment.EnvironmentName;

#if (EnableTelemetry)
services.AddOpenTelemetryForEventSourcing(environment);
#endif

services.AddEventSourceRedisConnection();
#if (EnableProducer && s3 )
services.AddKeyedProductCycleProducer(ProductCycleConstants.URI, ProductCycleConstants.S3_BUCKET, env);
#endif
#if (EnableProducer && !s3 )
services.AddKeyedProductCycleProducer(ProductCycleConstants.URI, env);
#endif
#if (EnableConsumer && s3)
services.AddKeyedConsumer(ProductCycleConstants.URI, ProductCycleConstants.S3_BUCKET, env);
#endif
#if (EnableConsumer && !s3)
services.AddKeyedConsumer(ProductCycleConstants.URI, env);
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
logger.LogInformation("Service Configuration Event Sourcing `{event-bundle}` on URI: `{URI}`, Features: [{features}]", "ProductCycle", ProductCycleConstants.URI, string.Join(", ", switches));
    
app.Run();
