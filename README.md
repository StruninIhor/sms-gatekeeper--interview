# SMS Gatekeeper

A comprehensive SMS rate limiting and monitoring system that validates incoming messages against configurable rate limits. This project demonstrates a microservices architecture with real-time monitoring capabilities.

## Project Overview

SMS Gatekeeper is designed to protect SMS infrastructure by enforcing rate limits on both account and phone number levels. The system tracks message history, provides real-time metrics, and offers a user-friendly dashboard for monitoring and analysis.

## Architecture

The project follows a microservices architecture with the following components:

### Backend (.NET Core)

Located in `src/backend/SmsGatekeeper/`:

- **SmsGatekeeper**: The main API service that handles SMS rate limiting logic
  - Validates incoming SMS requests against configurable rate limits
  - Uses Redis for distributed rate limiting with sliding window algorithm
  - Exposes REST endpoints for SMS validation

- **SmsDataApi**: API service for historical data and metrics
  - Provides endpoints for querying message history with filtering and pagination
  - Hosts SignalR hub for real-time metrics updates

- **SmsGatekeeper.Domain**: Shared domain models and database context
  - Contains entity models, database context and migrations

- **HistoryWorker**: Background services
  - Processes and stores SMS events for historical tracking

### Frontend (Angular)

Located in `src/frontend/`:

- **Real-time Dashboard**: Displays current SMS processing metrics
  - Uses SignalR for real-time updates from the backend
  - Shows current message processing rate

- **Message History**: Provides searchable history of SMS requests
  - Supports filtering by phone number, account ID, and date range
  - Includes pagination with configurable page size
  - Persists user preferences in localStorage

## Technical Features

- **Rate Limiting**: Implements a sliding window rate limiter using Redis
- **Real-time Updates**: Uses SignalR for pushing metrics to the frontend
- **Responsive UI**: Built with Bootstrap
- **Fallback Mechanism**: Frontend gracefully degrades to mock data if backend is unavailable (for debugging / frontend development purposes)
- **Validation**: Client and server-side validation for phone numbers (format: 1NPANXXXXX - according to E.164 standard)
- **Persistence**: Uses SQL Server for historical data and Redis for rate limiting and events / subscriptions.

## Development Setup

### Backend Requirements
- .NET 8.0 SDK
- SQL Server (or LocalDB)
- Redis

### Frontend Requirements
- Node.js (v18+)
- npm

### Running the Application
1. Start Redis server
   ```
   docker run -p 6379:6379 redis
   ```
2. Start SQL Server
3. Run the backend services:
4. Run the frontend:
   ```
   cd src/frontend
   npm install
   ng serve --open
   ```

## API Endpoints

### SmsGatekeeper API
- `POST /sms/cansendsms`: Validates if a message can be sent based on rate limits

### SmsDataApi
- `GET /api/history`: Retrieves message history with filtering and pagination
- `/metrics`: SignalR hub for real-time metrics updates

## Configuration

Rate limits and other settings can be configured in the respective `appsettings.json` (or `appsettings.{EnvironmentName}.json`) files:
- Account-level rate limit
- Phone number rate limit
- Database connection string
- Redis connection string
