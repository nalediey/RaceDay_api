Welcome to my api project
# RaceDay API

RaceDay API is the ASP.NET Core MVC web application for the RaceDay event platform. It provides a browser-based interface for account registration and sign-in, race-event pages, and organiser and participant areas. The MVC app communicates with the RaceDay API over HTTP.

## Features

- Register and sign in through the RaceDay API.
- Store the API-issued JWT in an HTTP-only cookie and attach it to API requests.
- Redirect signed-in users to organiser or participant areas according to their role.
- View event pages and organiser event listings.
- Create events with details and an optional banner image.
- Browse participant pages for events, enrolments, results, and profile information.


## Project layout

Controllers/   MVC controllers and API-facing actions
Models/        View models and API data-transfer models
Services/      Shared HTTP client support, including JWT forwarding
Views/         Razor views and empty views 

