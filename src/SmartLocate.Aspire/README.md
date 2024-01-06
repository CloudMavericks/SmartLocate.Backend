# SmartLocate.Aspire

This directory contains the .NET Aspire projects that is used to orchestrate the SmartLocate application locally. It contains the following projects:

- **SmartLocate.AppHost**: This project is the entry point for the application. It also invokes the necessary executables along with the projects to run the microservices and the API gateway. 
- **SmartLocate.ServiceDefaults**: This project contains the Aspire-specific extension methods to provide Service Discovery, Logging, OpenTelemetry, and Health Checks to the microservices. 

## Orchestrating the SmartLocate Application

This project orchestrates the SmartLocate application locally. It orchestrates the following items:
- SmartLocate.API (API Gateway)
- SmartLocate.Identity (Identity Microservice)
- SmartLocate.Admin (Admin Users Microservice)
- SmartLocate.BusDrivers (Bus Drivers Microservice)
- SmartLocate.Students (Students Microservice)
- SmartLocate.Buses (Buses Microservice)
- SmartLocate.BusRoutes (Bus Routes Microservice)
- SmartLocate.Infrastructure (Infrastructure Microservice - Email and other services)
- A Dapr Sidecar for each microservice
- A Redis container for the Dapr PubSub component

## Running the Application

First, make sure that you have initialized the Dapr locally by running the following command:

```bash
dapr init
```

This will initialize the Dapr locally and create the necessary components, particularly the Redis container by default, which is used for the PubSub component.

Then, run the following command to start the application from the root directory of the repository:

```bash
dotnet run --project src\SmartLocate.Aspire\SmartLocate.AppHost\SmartLocate.AppHost.csproj
```

The application starts with .NET Aspire Dashboard available at `http://localhost:15216/`.