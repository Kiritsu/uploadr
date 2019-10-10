# ShareY

[![Build Status](https://dev.azure.com/allanmercou/ShareY/_apis/build/status/Kiritsu.ShareY?branchName=master)](https://dev.azure.com/allanmercou/ShareY/_build/latest?definitionId=2&branchName=master)

ShareY is a simple server-side uploading service made with ASP.NET Core 2.2. 

ShareY contains an API where you can create/remove/block users, which have a token for authentication. It also has an endpoint which is used to upload files to the server.

ShareY can be used as a ShareX custom upload server.

# Routes

- `GET /` returns a view of the main page.
- `GET /{file_name}` returns a file if it exists.
- `GET /api/upload/{file_name}` returns as a `JSON` details of the specified file. Requires `user` authorization.
- `DELETE /api/upload/{file_name}` deletes the specified file. Requires `user` authorization. Only the author can delete its upload.
- `POST /api/upload` uploads the given file in the body of the request. Requires `user` authorization. Redirects to `GET /{file_name}`.
- `PATCH /api/user/{guid}/block?block=false` unblocks a user by it's guid. Requires `admin` authorization.
- `PATCH /api/user/{guid}/block?block=true` blocks a user by it's guid. Requires `admin` authorization. `Admin` accounts cannot be blocked.
- `DELETE /api/user` deletes the authenticated account. Requires `user` authorization.
- `DELETE /api/user/token?reset=true` resets the current user's token and returns the new generated one. Requires `user` authorization.
- `DELETE /api/user/token` revokes the token of the current's user. Requires `user` authorization.
- `PATCH /api/routes/{routeName}?state={boolean}` enables or disables a configurable route. Requires `admin` authorization. Unknown `routeName` returns every available routes.
- `POST /api/user` creates a new user with the given `email` in a json body. Requires no authorization. (see exemple below)
```json
{
  "Email": "your.email@exemple.com"
}
```

# Requirements

In order to use ShareY, you need the following components:
- .NET Core 2.2 (ASP.NET Core)
- PostgreSQL Server

# Configuration

Use the pre-made file `sharey.json` to create your configuration. This file must be in the same directory as the executable. You can also set an environment variable `SHAREY_CONFIGURATION`.
The configuration is made in multiple parts:
- Database: used to make the connection to your PostgreSQL database.
- Routes: used to block endpoints from being used.
- Files: configuration and requirements of uploads (size, type of file, etc.)
- Email: configuration for the SMTP server for sending emails.
- OneTimeToken: configuration for one-time duration tokens for login

## Database setup

Follow these instructions if you need help to setup your PostgreSQL database for ShareY.

- Connect to your PostgreSQL database as a super-user.
- Create a user. In our case, its name will be `sharey`. Replace `your_password` by a strong password.
```sql
CREATE USER sharey WITH CREATEDB PASSWORD 'your_password';
```
- Create a database. In our case, its name will be `sharey`. Don't forget to change the owner name if you set something else than `sharey`.
```sql
CREATE DATABASE sharey WITH OWNER 'sharey';
```
- Disconnect from your PostgreSQL database.

## Database configuration

- `Hostname` is the hostname of your PostgreSQL server. Default is `localhost` if it's ran locally.
- `Port` is the port of your PostgreSQL server. Default is `5432`.
- `Database` is the name of the target PostgreSQL database that you created in `Database Setup`.
- `Username` is the name of the user that you created in `Database Setup`.
- `Password` is the password of the user that you created in `Database Setup`.
- `UseSsl` indicates wether you want secure connection to the database.

## Routes configuration

- `UserRegisterRoute`: when set to true, anyone can use `POST /api/user/create` route.

## Files configuration

- `SizeMin`: defines the minimum size your upload must have to be uploaded to the server.
- `SizeMax`: defines the maximum size your upload must not reach to be uploaded to the server.
- `FileExtensions`: array of strings which are file extensions that are supported by the server.

# Building ShareY

You need `Visual Studio 2019` or the `.NET Core 2.2 SDK` in order to build ShareY.
If you are building from a terminal, use the following command:
```
dotnet publish -c Release -f netcoreapp2.2 -r [RUNTIME]
```
You can replace [RUNTIME] by either `win-x64` or `linux-x64` depending on the target operating system. See Microsoft documentation for .NET Core runtime if you have troubles.

# Auto update script

## Requirements
- Python 3.7 or higher

## Summary

There's an auto-update script made in python which pull the latest build in Azure Pipelines and download the right artifact if your version of ShareY is outdated. It will shutdown your app, replace the files (config included for now!) and restart it. 

You can use this script in a cronjob to automatically update the app.

It has not been tested very well for now.
