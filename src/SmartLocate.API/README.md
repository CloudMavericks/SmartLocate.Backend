# SmartLocate.API

This project is the API gateway for the SmartLocate application that uses Ocelot to route requests to the appropriate microservices.

It contains a bunch of middleware for redirecting requests to the appropriate microservices with appropriate authentication and authorization as well as providing a Swagger UI for the API. 
The Important part of this API is the `ocelot.json` file which contains the routing information for the API. 
By default, the API is configured to run on port 7000 using HTTP or port 8000 using HTTPS.

## Running the API

Use the following command to run the API:

```bash
dotnet run
```
The API will be available at `http://localhost:7000` or `https://localhost:8000` depending on the launch profile used. The Swagger UI will be available at `/swagger/index.html`.
> **Note:** The API endpoints will not work unless the microservices are running. Besides, the API doesn't support HTTPS Redirection yet. It is also recommended to run the API through the [SmartLocate.Aspire/SmartLocate.AppHost](../SmartLocate.Aspire/README.md) project as it will run the API along with the microservices and the Dapr sidecars.

