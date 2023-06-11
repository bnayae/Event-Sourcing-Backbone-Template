# Creating a custom .NET CLI template

- [.NET SDK](https://github.com/dotnet/sdk/)
- [ASP.NET Core Template](https://github.com/dotnet/aspnetcore/blob/main/src/ProjectTemplates/Web.ProjectTemplates/content/WebApi-CSharp/.template.config/template.json)
- [Video Tutorial](https://www.google.com/search?q=Custom+templates+for+dotnet+new&oq=Custom+templates+for+dotnet+new&aqs=chrome..69i57j69i60.581j0j4&sourceid=chrome&ie=UTF-8#fpstate=ive&vld=cid:a6dbe0e2,vid:rdWZo5PD9Ek)*
- [Custom templates for dotnet new](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates)
- [Tutorial: Create an item template](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-item-template)
- [WiKi](https://github.com/dotnet/templating/wiki)
- [Samples](https://github.com/dotnet/dotnet-template-samples)

## NuGet

- [Install as NuGet](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates#install-a-template-package)

```bash
# dotnet build --force
dotnet pack -c Release --force -o .
dotnet new install . 
dotnet new evtsrc -h
dotnet new evtsrc -n {name-of-the-project}
dotnet new evtsrc -minimal -n {name-of-the-project}
# dotnet new uninstall {groupIdentity}
dotnet new uninstall .
```

## Command Samples

```bash
dotnet new evtsrc -uri event-demo --concumer-group main-consumer -n MyCompany.Events -eb MyEvent

# With s3 storage
dotnet new evtsrc -uri event-demo --s3 --aws-profile AUTH_PROFILE --s3-bucket event-sourcing-demo --concumer-group main-consumer -n MyCompany.Events -eb MyEvent

# Only consumer
dotnet new evtsrc --no-producer -uri event-demo --concumer-group main-consumer -n MyCompany.Events -eb MyEvent

# Only Producer
dotnet new evtsrc --no-consumer -uri event-demo --concumer-group main-consumer -n MyCompany.Events -eb MyEvent
```
