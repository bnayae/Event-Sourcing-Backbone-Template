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

## Open Telemetry

Read more:

- [Manual Instrumentation](https://opentelemetry.io/docs/instrumentation/net/getting-started/#manual-instrumentation)
- [Manual Metrics](https://opentelemetry.io/docs/instrumentation/net/getting-started/#manual-metrics)
