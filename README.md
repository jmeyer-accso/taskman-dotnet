# Taskman implementation in .NET

Sample ASP.NET MVC application

## Build

```bash
dotnet build
```

## Test

```bash
dotnet test
```

## Run Application

Start the database container

```bash
docker run --rm -d -p 5432:5432 --name taskman-db -e POSTGRES_PASSWORD='t@skm@n123' -e POSTGRES_USER=taskman -e POSTGRES_DB=taskman postgres:latest
```

Run the application

```bash
dotnet run --project Taskman.App
```
