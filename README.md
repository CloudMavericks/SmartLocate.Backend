# SmartLocate.Backend

This project contains the backend for the SmartLocate application and uses the microservice architecture.

It is built primarily using ASP.NET Core 8, MongoDB and Dapr.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker
- Dapr CLI
- MongoDB
- `aspire` Workload

### First Steps

1. Clone this repository.
2. Ensure that you have Dapr initialized in your machine and if not, run the following command in your terminal:
    ```bash
    dapr init
    ```
3. Ensure that you have MongoDB running in your machine and if not, run the following command in your terminal:
    ```bash
    docker run -d -p 27017:27017 mongo
    ```
   By default, the application uses the same database (`http://localhost:27017/SmartLocate`) for all the microservices. If you want to use different databases for each microservice, you can change the connection string in the `appsettings.json` file of each microservice.
4. To run the microservices, you need to run the `aspire` workload. To do so, run the following command in your terminal:
    ```bash
    dotnet workload install aspire
    ```   
5. Open the `SmartLocate.sln` solution file in Visual Studio or JetBrains Rider to open the solution. 
6. Build and run the [SmartLocate.Aspire/SmartLocate.AppHost](src/SmartLocate.Aspire/README.md) project to run all the microservices. For more information, refer to the [README.md](src/SmartLocate.Aspire/README.md) file of the project.

## Orchestrating the Microservices

The microservices, API Gateway and the Dapr sidecars are orchestrated using the [SmartLocate.Aspire/SmartLocate.AppHost](src/SmartLocate.Aspire/README.md) project using .NET Aspire.

For more information, refer to the [README.md](src/SmartLocate.Aspire/README.md) file of the project.

## API Gateway

The API Gateway is built using the [Ocelot](https://ocelot.readthedocs.io/en/latest/) library and runs at `http://localhost:7000`.

For more information, refer to the [README.md](src/SmartLocate.API/README.md) file of the project.

## Microservices

The backend is built using the microservice architecture. The following are the microservices that are used in the application:

- [SmartLocate.Buses](src/SmartLocate.Buses/README.md) - `http://localhost:7001`
- [SmartLocate.BusRoutes](src/SmartLocate.BusRoutes/README.md) - `http://localhost:7002`
- [SmartLocate.Students](src/SmartLocate.Students/README.md) - `http://localhost:7003`
- [SmartLocate.Identity](src/SmartLocate.Identity/README.md) - `http://localhost:7004`
- [SmartLocate.BusDrivers](src/SmartLocate.BusDrivers/README.md) - `http://localhost:7005`
- [SmartLocate.Admin](src/SmartLocate.Admin/README.md) - `http://localhost:7006`
- [SmartLocate.Infrastructure](src/SmartLocate.Infrastructure/README.md) - `http://localhost:7050`