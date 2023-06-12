## Get started 

### Docker

Run the following dockers.

- Redis

```bash
docker run -p 6379:6379 -it --rm --name redis-Json redislabs/rejson:latest
```

```bash
docker run --name jaeger-otel  --rm -it -e COLLECTOR_OTLP_ENABLED=true -p 16686:16686 -p 4317:4317 -p 4318:4318  jaegertracing/all-in-one:latest
```

Browse to:
```bash
http://localhost:16686/search
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
