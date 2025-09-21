# AnchorzUp URL Shortener

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git (for cloning the repository)

## Setup & Run

### 1. Download from GitHub

```bash
# Navigate to Desktop or desired folder
cd Desktop

# Clone the repository (replace with the GitHub link you receive)
git clone https://github.com/dafinaak/Url-Shortener.git
cd Url-Shortener/Url-Shortener

# Alternative: Download as ZIP from GitHub
# Click "Code" -> "Download ZIP" -> Extract to Desktop
```

### 2. Verify Prerequisites

```bash
# Check if you have .NET 8 SDK installed
dotnet --version


# If not installed, download from:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Build the Project

```bash
dotnet build
```

### 5. Run the Application

```bash
dotnet run
```

### 6. Access the Application

After running `dotnet run`, you will see:
```
Now listening on: http://localhost:5240
```

Open your web browser and navigate to:
- Main Application: `http://localhost:5240`
- Swagger API: `http://localhost:5240/swagger`


## What Happens Automatically

The application will automatically:
- Create the SQLite database (`urlshortener.db`)
- Set up the database schema and tables
- Serve the frontend interface
- Configure all API endpoints

## Stop the Application

To stop the application, press `Ctrl+C` in the terminal.

## Troubleshooting

### Common Issues

1. Port Already in Use: 
   - Stop the application with Ctrl+C before starting again
   - Or change the port in `Properties/launchSettings.json`

2. Build Errors:
   - Run `dotnet restore` to restore NuGet packages
   - Ensure .NET 8 SDK is properly installed
   - Try: `dotnet clean` then `dotnet build`

3. Database Issues: 
   - Delete `urlshortener.db` and restart the application
   - The database will be recreated automatically

4. QR Code Not Generating:
   - Ensure all dependencies are restored: `dotnet restore`
  

