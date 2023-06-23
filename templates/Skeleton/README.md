[![NuGet](https://img.shields.io/nuget/v/Skeleton.svg)](https://www.nuget.org/packages/Skeleton/) 

## Get started 


### Docker Compose

Set-up the environment with docker-compose 

```bash
cd .dockers/compose
```

```bash
docker compose up -d
```

Stop the environment

```bash

docker compose down
```

For Jaeger (tracing) Browse to:
```bash
http://localhost:16686/search
```

For Grafana (metrics) Browse to:
```bash
http://localhost:3000

# Credentials sets in the `compose.yaml` file
# defaults are:
#   user admin
#   password: grafana

```

### CLI

Minimal:

```bash
dotnet new evtsrc -uri event-demo --consumer-group main-consumer -n MyCompany.Events -eb MyEvent
```

With s3 storage:  

```bash
dotnet new evtsrc -uri event-demo -s3 --aws-profile AWS_PROFILE --aws-profile-region us-east-1 --s3-bucket event-sourcing-demo --consumer-group main-consumer -n MyCompany.Events -eb MyEvent
```

Only Consumer:  

```bash
dotnet new evtsrc --no-producer -uri event-demo --consumer-group main-consumer -n MyCompany.Events.Consumer -eb MyEvent
```  

Only Producer:  

```bash
dotnet new evtsrc --no-consumer -uri event-demo -n MyCompany.Events.Producer -eb MyEvent
```  

With GitHub Workflow (CI):  

```bash
dotnet new evtsrc -uri event-demo --consumer-group main-consumer --github-ci --git-email ci-mail@gmail.com -n MyCompany.Events -eb MyEvent
```  

## Open Telemetry Snippets

Enrich / Telemetry events

```cs
var activity = Activity.Current;
activity?.SetTag("app.user.id", request.UserId);
activity?.AddEvent(new("Fetch cart"));
```

### Ceating new custom trace / meter 

Make sure to add the `ServiceName` to the `OpenTelemetryExtensions` registration

```cs 
public static class DiagnosticsConfig
{
    public const string ServiceName = "MyService";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);

    public static Meter Meter = new(ServiceName);
    public static Counter<long> RequestCounter =
        Meter.CreateCounter<long>("app.request_counter");
}
```

Use it 

- [Manual Instrumentation](https://opentelemetry.io/docs/instrumentation/net/getting-started/#manual-instrumentation)
- [Manual Metrics](https://opentelemetry.io/docs/instrumentation/net/getting-started/#manual-metrics)

```cs
public IActionResult Index()
{
    // Track work inside of the request
    using var activity = DiagnosticsConfig.ActivitySource.StartActivity("SayHello");
    activity?.SetTag("foo", 1);
    activity?.SetTag("bar", "Hello, World!");
    activity?.SetTag("baz", new int[] { 1, 2, 3 });

    DiagnosticsConfig.RequestCounter
        .WithTag("Action", nameof(Index))
        .WithTag("Controller", nameof(HomeController))
        .Add(1);
    
    return View();
}
```

*/