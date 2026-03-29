# NurseLink

NurseLink is a web application designed to improve post-operative
patient follow-up developed as a Final Degree Project (TFG).

------------------------------------------------------------------------

## 🏥 Overview

The system enables patients to report their recovery status and
communicate with their assigned nurse, while healthcare professionals
can monitor patient progress and manage follow-up care efficiently.

------------------------------------------------------------------------

## 👥 User Roles

### Administrator

-   Manages users (nurses and patients)
-   Oversees system data

### Nurse

-   Monitors assigned patients
-   Reviews symptom reports
-   Communicates with patients

### Patient

-   Reports post-operative symptoms
-   Tracks recovery progress
-   Communicates with assigned nurse

------------------------------------------------------------------------

## 🧱 System Architecture

The application follows a three-tier architecture:

-   Frontend (Presentation Layer) → Vue 3\
-   Backend (Business Logic Layer) → ASP.NET Core (C#) with REST API\
-   Database (Data Layer) → SQL Server

------------------------------------------------------------------------

## 🔐 Authentication

Authentication is implemented using JWT (JSON Web Tokens) with
role-based authorization.

------------------------------------------------------------------------

## 🗄️ Database Setup

To initialize the database, follow these steps:

### 1. Create the database

USE master; GO

CREATE DATABASE NurseLinkDB; GO

### 2. Select the database

USE NurseLinkDB; GO

### 3. Create the database structure

Execute: Database/Script Database Creation.sql

### 4. Insert initial data

Here you have 2 options:
- A predefined base data executing the script: Database/Script Database General Data.sql
- Use Swagger for inserting the base data (administrator, nurses, patients...)

------------------------------------------------------------------------

## 🚀 Running the Application

### Backend (ASP.NET Core)

1.  Open the solution in Visual Studio\
2.  Run the API project

Swagger: https://localhost:{port}/swagger

------------------------------------------------------------------------

### Frontend (Vue 3)

cd nurselink-frontend\
npm install\
npm run dev

------------------------------------------------------------------------

## 🧪 API Testing (Swagger)

Using Swagger you can: 
- Create administrators
- Get all the administrators
- Create nurses
- Get all the nurses
- Create patients
- Get all the patients
- Create surgery types
- Get all the surgery types
- Create surgeries
- Get all the surgeries
- Authenticate in the system using an email and password. The system will detect if you are an Administrator, Nurse or Patient

------------------------------------------------------------------------

## 📁 Project Structure

NurseLink/\
├── Database/\
├── NurseLink.API/\
├── nurselink-frontend/

------------------------------------------------------------------------

## 📦 Technologies Used

-   ASP.NET Core\
-   Vue 3\
-   SQL Server\
-   JWT

------------------------------------------------------------------------

## 👨‍💻 Author

Alejandro Preciado
