# achihapi
Web API for HIH, built on ASP.NET Core 1.0.

## Install
To install this Web API to your own server, please follow the steps below.


### Clone or Download
You can clone this [repository] (https://github.com/alvachien/achihapi.git) or download it.


### Tools
Though I using Visual Studio 2015, the project can be processed by any IDE which supports ASP.NET Core.


### appsettings.json 
The appsettings.json has been removed because it defines the connection strings to the database. This file is manadatory to make the API works. 

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

# Author
Alva Chien


# Licence
MIT
