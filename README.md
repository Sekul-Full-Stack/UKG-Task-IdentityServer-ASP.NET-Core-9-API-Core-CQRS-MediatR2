# Identity Server & People Management API
A secure and extensible **ASP.NET Core 9 API Identity Server** and **People Management API** built using modern .NET 9 practices. This solution leverages **CQRS**, **MediatR**, and **SQL Server**, and is designed **without relying on Microsoft Identity or external authentication providers**.
---

## HR Admin Credentials (for initial access)

- **Email**: `sekul7@gmail.com`  
- **Password**: `Qwerty1!@%`

---

## Key Technologies

- **Backend**: ASP.NET Core 9, CQRS, MediatR, Dapper, FluentValidation  
- **Authentication**: JWT Bearer Tokens  
- **Testing**: Moq, xUnit  
- **Password Security**: Secure password hashing  
- **Frontend**: React JS with Styled Components

---

## Architecture Overview

### Identity Server

- **Independent from Microsoft Identity**
- **No external authentication providers**
- **Custom implementation of user and role management**
- **JWT authentication with claims-based authorization**

#### Layers

##### Service Layer – `IdentityServer.Infrastructure`
- `UserManager`: CRUD for users
- `RoleManager`: CRUD for roles
- `TokenService`: Generates JWT tokens with claims
- `PasswordHasher`: Handles password hashing and verification

##### Repositories – `Dapper` with SQL Server
- `UserRepository`: Direct SQL access for users
- `RoleRepository`: Direct SQL access for roles

#### Result Handling
- All API responses return a generic wrapper: `IdentityResult<T>`

---

## People Management API – `PeopleManagementAPI`

- **Protected via JWT tokens**
- **CORS configured for** `http://localhost:3000`
- Uses a custom `UserHttpClient` to communicate with Identity Server


![Alt Text](images/1.JPG)
![Alt Text](images/2.JPG)
![Alt Text](images/3.JPG)
![Alt Text](images/4.JPG)
![Alt Text](images/5.JPG)
![Alt Text](images/6.JPG)
![Alt Text](images/7.JPG)
 


## Getting Started

### Prerequisites
- SQL Server
- .NET 9 SDK
- Node.js (for React client)

### Database Setup

Run the following SQL script **before** starting the backend:

```sql
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'IdentityServerDB')
BEGIN
    CREATE DATABASE IdentityServerDB;
END
GO

USE IdentityServerDB;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(255) NOT NULL UNIQUE,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        PhoneNumber NVARCHAR(256),
        DateCreated DATETIME DEFAULT GETDATE()
    );

    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        DateCreated DATETIME DEFAULT GETDATE()
    );

    CREATE TABLE UserRoles (
        UserRoleId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
        CONSTRAINT UC_UserRole UNIQUE (UserId, RoleId)
    );

    CREATE TABLE Claims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        Type NVARCHAR(255) NOT NULL,
        Value NVARCHAR(500) NOT NULL,
        FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
        CONSTRAINT UC_Claim UNIQUE (RoleId, Type)
    );

    INSERT INTO Users (Username, Email, PasswordHash, PhoneNumber)
    VALUES (
        'Sekul',
        'sekul7@gmail.com',
        'KZtJqHCbyLkkeeXMYALGtw==;r0mvP0GBsM7wQGVO2V7NG+8SQa0otXZ7gR9S3IbL5EI=',
        '+1234567890'
    );

    COMMIT TRAN


ENDPOINTS:

HR ADMIN
POST
http://localhost:5151/api/people/signup
Request Body:
{
    "UserName": "Sekul5411", 
    "Email": "sekul5114@gmail.com",  
    "Password": "Qwerty1!@%",  
    "PhoneNumber": "0878230345" 
}
+ Bearer Token  

AllowAnonymous
POST
http://localhost:5151/api/people/SignIn 
Request Body:
{
    "Email": "sekul7@gmail.com",  
    "Password": "Qwerty1!@%" 
}
+ Bearer Token  
HR ADMIN user: sekul7@gmail.com  pass: Qwerty1!@%

HR ADMIN
DELETE
http://localhost:5151/api/people/delete-user/18
+ Bearer Token  

MANAGER,HR ADMIN
PATCH
http://localhost:5151/api/people/update-user/54
+ Bearer Token  

HR ADMIN
POST
http://localhost:5151/api/people/create-role
Request Body:
{
    "Name": "Test 2 for create role async",
    "Description": "Description 2 for Test for create role async" 
}
+ Bearer Token  

HR ADMIN
PATCH
http://localhost:5151/api/people/update-role/2
Request Body:
{ 
    "Description": "MANAGER DESCRIPTION TEXT" 
}
+ Bearer Token

HR ADMIN
DELETE
http://localhost:5151/api/people/delete-role/6
+ Bearer Token

HR ADMIN
POST
http://localhost:5151/api/people/admin/assign-role
Request Body:
{
    "UserId": 31,
    "RoleId": 3
}
+ Bearer Token  

MANAGER,HR ADMIN
GET
http://localhost:5151/api/people/all-users
+ Bearer Token 

HR ADMIN
GET
http://localhost:5151/api/people/admin/all-roles
+ Bearer Token 

HR ADMIN
POST
http://localhost:5151/api/people/admin/reset-password
Request Body:
{ 
    "Id": 57 ,
    "NewPassword": "Qwerty1!@%" 
}
+ Bearer Token  

EMPLOYEE,MANAGER,HR ADMIN
GET
http://localhost:5151/api/people/me/info/31
+ Bearer Token 

EMPLOYEE,MANAGER,HR ADMIN
POST
http://localhost:5151/api/people/me/reset-password
Request Body:
{ 
    "Id": 57 ,
    "NewPassword": "Qwerty1!@%" 
}
+ Bearer Token  


