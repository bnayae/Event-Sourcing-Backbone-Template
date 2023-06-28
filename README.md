## Get started 

### CLI

Install the template:

```bash
dotnet new install Event-Sourcing.Backbone.Templates
```

Create new:

```bash
dotnet new evtsrc -h
dotnet new evtsrc -n {name-of-the-project}
```

Uninstall the template:

```bash
dotnet new uninstall Event-Sourcing.Backbone.Templates
```

#### Samples

Basic setting (Redis only):

```bash
dotnet new evtsrc -uri event-demo --consumer-group main-consumer -n MyCompany.Events -e MyEvent
```

With s3 storage:  

```bash
dotnet new evtsrc -uri event-demo-s3 -s3 --aws-profile AWS_PROFILE --aws-profile-region us-east-1 --s3-bucket event-sourcing-demo --consumer-group main-consumer -n MyCompany.Events.S3Storage -e MyEvent
```

Only Consumer:  

```bash
dotnet new evtsrc --no-producer -uri event-demo-split --consumer-group main-consumer -n MyCompany.Events.Consumer -e MyEvent
```

Only Producer:  

```bash
dotnet new evtsrc --no-consumer -uri event-demo-split -n MyCompany.Events.Producer -e MyEvent
```  

With GitHub Workflow (CI):  

```bash
dotnet new evtsrc -uri event-demo-ci --consumer-group main-consumer --github-ci --git-email ci-mail@gmail.com -n MyCompany.Events.CI -e MyEvent
```  