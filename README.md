# achihapi
Web API for HIH, built on ASP.NET Core 1.0.

## appsettings.json 
The appsettings.json has been removed. This file is manadatory to make the API works. The file defines the connection strings to the database.

An example file look like following:

{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=XXX;Initial Catalog=XXX;Persist Security Info=True;User ID=XXX;Password=XXX;",
    "DebugConnection": "Server=XXX;Database=XXX;Integrated Security=SSPI;MultipleActiveResultSets=true"
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}

