# gotowebinar-synchronizer
An unofficial .NET-based synchronization tool for importing and exporting registrants, attendees, and webinar metadata using the GoToWebinar API. This project is not affiliated with or endorsed by GoTo or LogMeIn Inc.
# GoToWebinar Synchronizer
An unofficial .NET-based tool to synchronize webinar data from the [GoToWebinar](https://www.goto.com/webinar) platform. This application allows you to automate the import/export of registrants, attendees, and webinar metadata using the GoToWebinar REST API.

## Features

- Download webinar lists and metadata
- Register external participants for upcoming webinars
- Export attendee participation data
- Avoid duplicate registrations and repeated processing
- Modular service-oriented architecture with support for logging and configuration

## Technologies

- .NET 9 Console Application
- Serilog for logging
- HttpClientFactory for API calls
- Clean architecture (Handlers, Services, DTOs, Models)
- Configuration via appsettings.json and environment variables

## How It Works

This tool uses your GoToWebinar API credentials to:
1. Authenticate and retrieve an access token
2. Load upcoming/past webinars for a configurable time window
3. Read lead/attendee data from local sources or external systems
4. Register new leads or download attendee participation details
5. Persist processed items to avoid reprocessing

## Disclaimer

This software is provided "as is", without warranty of any kind.  
Use it at your own risk. The maintainers and contributors are not responsible for any damage or data loss resulting from its use.



