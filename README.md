## Overview

This project is an ASP.NET Core Web API for managing foreign exchange rates.

It supports CRUD operations and integrates with an external API to fetch exchange rates when they are not available locally.

The application implements caching logic to avoid unnecessary external API calls.

## Features

* CRUD operations for exchange rates
* External API integration for real-time rates
* Caching mechanism to reduce unnecessary API calls
* Logging for key operations
* Unit tests with ~90% code coverage

## Architecture

The solution follows a layered architecture:

* **API Layer**: Handles HTTP requests (Controllers)
* **Service Layer**: Contains business logic
* **Repository Layer**: Manages data storage (in-memory)
* **Provider Layer**: Handles external API integration

Dependency Injection is used throughout the application.

## How to Run

1. Clone the repository
2. Navigate to the project folder
3. Run the application:

```bash
dotnet run
```

4. Open Swagger:
   The application will display the correct URL in the console output (e.g., https://localhost:5001/swagger)


## External API

This project uses https://open.er-api.com for retrieving exchange rates.

This API was chosen because it does not require authentication and allows the application to run without additional configuration.

The provider is abstracted, so it can be easily replaced with another API if needed.

## Bid and Ask Values

The external API provides mid-market exchange rates only.
For this reason, both Bid and Ask values are set to the same value.

In a real-world scenario, these values would differ to represent the market spread.


## Logging

The application uses built-in ASP.NET Core logging.

Key events such as cache hits, cache misses, stale data refresh, and external API failures are logged to help trace application behaviour and diagnose issues.

## Limitations & Possible Improvements

* Uses in-memory storage (data is lost when the application stops)
* No persistence layer (e.g., SQL database)
* No concurrency control for simultaneous updates
* No message queue/event system implemented
* Basic validation for inputs
* No retry mechanism for external API failures

Possible improvements:

* Add database persistence (e.g., SQL Server)
* Implement caching strategies (e.g., Redis)
* Introduce event-driven architecture for updates
* Add retry policies for external API calls
* Improve validation and error handling

## Testing

Unit tests were implemented using NUnit and Moq.

The tests cover key scenarios:

* Cache miss (external API call)
* Cache hit
* Stale data refresh
* Same currency handling
* External API failure fallback

Code coverage is approximately 90%.

## Notes

This project focuses on demonstrating clean architecture, external API integration, caching strategies, and unit testing rather than production-level completeness.