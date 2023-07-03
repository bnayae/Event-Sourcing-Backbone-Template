[![NuGet](https://img.shields.io/nuget/v/Skeleton.svg)](https://www.nuget.org/packages/Skeleton/) 

## Get started 

### Docker Compose

Set-up the environment with docker-compose 

```bash
cd ./dockers/compose
```

```bash
docker compose up -d
```

Stop the environment

```bash

docker compose down
```

#### Jaeger 

For Jaeger (tracing) [Browse to](http://localhost:16686/search):
```bash
http://localhost:16686/search
```

#### Grafana 

For Grafana (metrics) [Browse to](http://localhost:3000/)::
```bash
http://localhost:3000

# Credentials sets in the `compose.yaml` file
# Defaults are:
#   user admin
#   password: grafana

# Goto the Event Sourcing dashboard
```

### CLI

Minimal:

```bash
dotnet new evtsrc -uri event-demo --consumer-group main-consumer -n MyCompany.Events -e MyEvent
```

With s3 storage:  

```bash
dotnet new evtsrc -uri event-demo -s3 --aws-profile AWS_PROFILE --aws-profile-region us-east-1 --s3-bucket event-sourcing-demo --consumer-group main-consumer -n MyCompany.Events -e MyEvent
```

Only Consumer:  

```bash
dotnet new evtsrc --no-producer -uri event-demo --consumer-group main-consumer -n MyCompany.Events.Consumer -e MyEvent
```  

Only Producer:  

```bash
dotnet new evtsrc --no-consumer -uri event-demo -n MyCompany.Events.Producer -e MyEvent
```  

With GitHub Workflow (CI):  

```bash
dotnet new evtsrc -uri event-demo --consumer-group main-consumer --github-ci --git-email ci-mail@gmail.com -n MyCompany.Events -e MyEvent
```  
## Tune Telemetry

- Show Redis Instrumentation

  > Set the `EVENT_SOURCE_WITH_REDIS_TRACE` environment variable to `true`

- Hide S3 Instrumentation (when using S3 provider)

## Open Telemetry

Read more:

- [Manual Instrumentation](https://opentelemetry.io/docs/instrumentation/net/getting-started/#manual-instrumentation)
- [Manual Metrics](https://opentelemetry.io/docs/instrumentation/net/getting-started/#manual-metrics)
