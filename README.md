# Real-Time Chat App

A full-stack chat application using Angular frontend + .NET backend. 

## Features
- Real-time messaging between users
- Create and join chat channels
- User authentication and profiles
- Encrypted password storage

## Tech Stack
**Frontend (Angular)**
- WebSocket service for real-time updates
- RxJS for state management
- Component-based architecture
- Unit tests using Jasmine and Karma

**Backend (.NET Core)**
- WebSocket handlers for real-time communication
- Entity Framework with SQLite
- Unit tests with Moq

## Running it locally

**Backend:**
```bash
cd backend
dotnet run --project Server
```

**Frontend:**
```bash
cd frontend  
npm install
ng serve
```

