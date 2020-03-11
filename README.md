# UploadR

[![Build Status](https://dev.azure.com/allanmercou/uploadr/_apis/build/status/Kiritsu.uploadr?branchName=master)](https://dev.azure.com/allanmercou/uploadr/_build/latest?definitionId=7&branchName=master)

UploadR is a simple server-side uploading service made with ASP.NET Core 3.1.

# Routes

## Anonymous requests
> `POST /api/account` - Creates a new unverified account.

## Authenticated requests (Unverified)
> `POST /api/account/verify` - Verifies an unverified account.

## Authenticated requests (User)
> `DELETE /api/account` - Deletes the current account.
> `PATCH /api/account/reset` - Resets the current user's token.

## Authenticated requests (Admin)
> `PATCH /api/account/{userId}/block` - Blocks a user by its id.
> `PATCH /api/account/{userId}/unblock` - Unblocks a user by its id.
> `PATCH /api/account/{userId}/reset` - Resets a user's token.

# Requirements

In order to use UploadR, you need the following components:
- ASP.NET Core 3.1
- PostgreSQL Server
- Angular 8

# Configuration

Use the pre-made file `uploadr.json` to create your configuration. This file must be in the same directory as the executable. You can also set an environment variable `UPLOADR_PATH`.

## Database setup

Follow these instructions if you need help to setup your PostgreSQL database for UploadR.

- Connect to your PostgreSQL database as a super-user.
- Create a user. In our case, its name will be `uploadr`. Replace `your_password` by a strong password.
```sql
CREATE USER uploadr PASSWORD 'your_password';
```
- Create a database. In our case, its name will be `uploadr`. Don't forget to change the owner name if you used something else than `uploadr`.
```sql
CREATE DATABASE uploadr WITH OWNER 'uploadr';
```

## Database configuration

- `Hostname` is the hostname of your PostgreSQL server. Default is `localhost` if it's ran locally.
- `Port` is the port of your PostgreSQL server. Default is `5432`.
- `Database` is the name of the target PostgreSQL database that you created in `Database Setup`.
- `Username` is the name of the user that you created in `Database Setup`.
- `Password` is the password of the user that you created in `Database Setup`.

# Using UploadR

You need `Visual Studio 2019` or the `.NET Core 3.1 SDK` in order to build UploadR.
If you are building from a terminal, use the following command:
```
dotnet publish -c Release -f netcoreapp3.1 -r [RUNTIME]
```
You can replace [RUNTIME] by either `win-x64` or `linux-x64` depending on the target operating system. See Microsoft documentation for .NET Core runtime if you have troubles.