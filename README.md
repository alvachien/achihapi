[![Build Status](https://travis-ci.com/alvachien/achihapi.svg?branch=master)](https://travis-ci.com/alvachien/achihapi)

# achihapi
Web API for [HIH](https://github.com/alvachien/achihui.git), built on ASP.NET Core.


## Live example
This Web API was deployed on Microsoft Azure for testing purpose: https://achihapi.azurewebsites.net


## Install
To install this Web API to your own server, please follow the steps below.


### Step 1. Clone or Download
You can clone this [repository](https://github.com/alvachien/achihapi.git) or download it.


### Step 2. Setup your own database.
You need setup your own database (SQL Server based), and run three sqls under folder 'sql':
1. DBSchema_Table.sql
2. DBSchema_View.sql
3. Predliver_Content.sql


### Step 3. Change the appsettings.json by adding the connection string:
The appsettings.json has been removed because it defines the connection strings to the database. This file is manadatory to make the API works. 

An example file look like following:
```javascript
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
```

### Step 4. Deployment

Deploy this Web API to IIS or other HTTP server.

## Development Tools

Though the whole project developed and tested with Visual Studio 2019 Community Version and Visual Studio Code, the project can be processed by other IDE which supports ASP.NET Core.

## Test

Test project ```hihapi.test``` has been added to solution. And the Travis CI has been integrated with this repo.
Test project ```hihapi.test``` contains both Unit Test and Integration Test, you have to run Unit Test and Integration Test separately. The Travis CI also coverages the unit tests part due to the VM setting is not suit for Identity Server setup.

### Unit Tests

To run the unit test via command line:
```powershell
dotnet test --filter DisplayName~hihapi.test.UnitTests
```

### Integration Tests

To run the integration test via command line:
```powershell
dotnet test --filter DisplayName~hihapi.test.integrationtests
```

# Author
**Alva Chien (Hongjun Qian) | 钱红俊**

A programmer, and a certificated Advanced Photographer.  
 
Contact me:

1. Via mail: alvachien@163.com. Or,
2. [Check my flickr](http://www.flickr.com/photos/alvachien). 


# Licence
MIT
