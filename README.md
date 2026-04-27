# NurseLink

NurseLink is a web application designed to improve post-operative
patient follow-up, developed as a Final Degree Project (TFG).

The project includes a backend API, a frontend application and the
database scripts required to create and initialize the system.

------------------------------------------------------------------------

## 🏥 Overview

The system enables patients to report their recovery status and
communicate with their assigned nurse, while healthcare professionals
can monitor patient progress and manage follow-up care efficiently.

Administrators can manage the main data of the system, including nurses,
patients, surgery types and patient assignments. Nurses can access their
assigned patients, review their information, communicate with them and
monitor their recovery status.

------------------------------------------------------------------------

## 👥 User Roles

### Administrator

- Manages nurses and patients
- Manages surgery types
- Assigns patients to nurses
- Removes patient assignments when needed
- Oversees system data
- Accesses the administrator dashboard

### Nurse

- Accesses the nurse dashboard
- Monitors assigned patients
- Reviews patient information
- Reviews symptom reports
- Checks patient recovery status
- Communicates with patients

### Patient

- Reports post-operative symptoms
- Tracks recovery progress
- Communicates with the assigned nurse

------------------------------------------------------------------------

## 🧱 System Architecture

The application follows a three-tier architecture:

- Frontend (Presentation Layer) → Vue 3
- Backend (Business Logic Layer) → ASP.NET Core Web API using C#
- Database (Data Layer) → SQL Server

The frontend communicates with the backend through REST API requests.
The backend manages the business logic, authentication, data validation
and database access. The database stores the information related to users,
patients, nurses, surgeries, surgery types, reports, alerts, assignments
and messages.

```text
Frontend (Vue 3)
        |
        | HTTP / REST API
        v
Backend (ASP.NET Core Web API)
        |
        | Database access
        v
Database (SQL Server)
```

------------------------------------------------------------------------

## 🔐 Authentication

Authentication is implemented using JWT (JSON Web Tokens) with
role-based authorization.

The system identifies the user role after login and redirects the user to
the corresponding area of the application.

Available roles:

- Administrator
- Nurse
- Patient

------------------------------------------------------------------------

## 🗄️ Database Setup

To initialize the database, follow these steps:

### 1. Create the database

```sql
USE master;
GO

CREATE DATABASE NurseLinkDB;
GO
```

### 2. Select the database

```sql
USE NurseLinkDB;
GO
```

### 3. Create the database structure

Execute:

```text
Database/Script Database Creation.sql
```

This script creates the required tables, primary keys, foreign keys and
relationships used by the application.

### 4. Insert initial data

Here you have two options:

- Insert predefined base data by executing the script:

```text
Database/Script Database General Data.sql
```

- Use Swagger to insert the base data manually, such as administrators,
  nurses, patients, surgery types, surgeries and assignments.

------------------------------------------------------------------------

## 🚀 Running the Application

### Backend (ASP.NET Core Web API)

1. Open the solution in Visual Studio.
2. Check that the database connection string is correctly configured.
3. Run the API project.
4. Open Swagger to test the API endpoints.

Swagger:

```text
https://localhost:{port}/swagger
```

Replace `{port}` with the port used by Visual Studio when running the API.

------------------------------------------------------------------------

### Frontend (Vue 3)

Open a terminal in the frontend folder:

```bash
cd nurselink-frontend
npm install
npm run dev
```

After running the frontend, open the local URL shown in the terminal.

Example:

```text
http://localhost:5173
```

The exact port may change depending on the local environment.

------------------------------------------------------------------------

## 🧪 API Testing (Swagger)

Swagger is available when the backend API is running. It provides access
to the API endpoints and can be used to test the backend independently
from the frontend.

Using Swagger, the following API areas can be tested:

### Admin

- `POST /api/Admin/create`
- `GET /api/Admin`
- `GET /api/Admin/dashboardKpis`
- `GET /api/Admin/patientsWithAlerts`
- `GET /api/Admin/unassignedPatients`

### Assignments

- `POST /api/Assignments/create`
- `DELETE /api/Assignments/patient/{patientId}`

### Auth

- `POST /api/Auth/login`

### Nurses

- `POST /api/Nurses/create`
- `GET /api/Nurses`
- `GET /api/Nurses/nursesDetailed`
- `PUT /api/Nurses/update/{id}`
- `GET /api/Nurses/{id}`
- `GET /api/Nurses/{id}/assignedPatients`

### Patients

- `POST /api/Patients/create`
- `PUT /api/Patients/update/{patientId}`
- `GET /api/Patients`
- `GET /api/Patients/{id}`
- `GET /api/Patients/patientsDetailed`

### Reports

- `GET /api/Reports/patient/{patientId}`
- `GET /api/Reports/{id}`
- `POST /api/Reports/create`
- `PUT /api/Reports/{id}/nurse-observations`

### Surgeries

- `POST /api/Surgeries/create`
- `GET /api/Surgeries`

### Surgery Types

- `POST /api/SurgeryType/create`
- `GET /api/SurgeryType`

Swagger is especially useful for validating that the backend methods work
correctly before testing them from the frontend interface. It also provides
a clear overview of the available controllers, request parameters and
responses returned by the API.

------------------------------------------------------------------------

## 💬 Messages

Message management is planned as part of the communication workflow between
patients and nurses. This functionality is considered in the system design
and will be refined as part of the final version.

------------------------------------------------------------------------

## 📊 Recovery Status

The application includes a first version of patient recovery status
tracking.

The recovery status is calculated from the alerts generated after
evaluating the symptom reports submitted by patients. Each report contains
different recovery indicators, such as pain, fever, bleeding or swelling.

When a report is evaluated, the system checks whether each indicator exceeds
the limits defined for that symptom. If a value exceeds the expected
threshold, an alert is generated for that specific indicator.

For example:

- If the patient reports a high fever, the system generates a fever alert.
- If the patient reports bleeding, the system generates a bleeding alert.
- If several indicators exceed their limits in the same report, several
  alerts can be generated.

The recovery status is then determined according to the number of alerts
associated with the patient:

| Number of alerts | Recovery status |
|---|---|
| 0 | Stable |
| 1-2 | Warning |
| More than 2 | Alert |

This feature helps nurses and administrators identify patients who may
require additional attention. Instead of manually reviewing all the report
values, the system provides a visual status that summarizes the patient's
current recovery situation.

------------------------------------------------------------------------

## 📁 Project Structure

```text
NurseLink/
├── Database/
│   ├── Script Database Creation.sql
│   └── Script Database General Data.sql
│
├── NurseLink.API/
│   ├── Controllers/
│   ├── Database/
│   ├── Domain/
│   │   ├── DTOs/
│   │   └── Entities/
│   └── Program.cs
│
├── nurselink-frontend/
│   ├── public/
│   ├── src/
│   ├── package.json
│   └── vite.config.js
│
└── README.md
```

------------------------------------------------------------------------

## 📦 Technologies Used

- ASP.NET Core Web API
- C#
- Entity Framework Core
- Vue 3
- JavaScript
- CSS
- SQL Server
- JWT Authentication
- Swagger
- Git and GitHub

------------------------------------------------------------------------

## ✅ Current Implementation Status

The current version of NurseLink includes the main core features of the
application:

- Backend API connected to the database
- Frontend connected to the backend API
- User authentication
- Role-based access
- Administrator dashboard
- Nurse dashboard
- Patient management
- Nurse management
- Surgery type management
- Patient surgery information
- Patient assignment and unassignment
- Search functionality
- Message workflow considered in the system design
- Patient recovery status based on alerts
- API testing through Swagger

Some features are still being refined for the final version, especially
the complete report creation workflow and the final alert generation rules
based on report KPIs.

------------------------------------------------------------------------

## 🧑‍💻 Test Users

Test users and passwords are included in the installation manual or can be
created manually using Swagger.

Example roles that should be available for testing:

| Role | Purpose |
|---|---|
| Administrator | Access to management screens and dashboards |
| Nurse | Access to assigned patients and patient follow-up information |
| Patient | Access to symptom reporting and communication features |

------------------------------------------------------------------------

## 👨‍💻 Author

Alejandro Preciado
