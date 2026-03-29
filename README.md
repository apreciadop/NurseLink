NurseLink
NurseLink is a web-based healthcare communication platform developed as a Final Degree Project (TFG).

The system allows communication and management between nurses, patients, and administrators through a centralized digital solution.

Project Structure
NurseLink

-- NurseLink.API/ # Backend (ASP.NET Core Web API)

-- nurselink-frontend/ # Frontend (Vue 3)

-- Database/ # SQL scripts (schema + seed)

Technologies Used
Frontend
Vue 3
Vite
JavaScript
CSS
Backend
ASP.NET Core Web API
Entity Framework Core
SQL Server
JWT Authentication
Authentication
The system uses JWT (JSON Web Tokens) for authentication.

Roles:

Administrator
Nurse
Patient
Database Setup
Execute Database/schema.sql
Execute Database/seed.sql
This will create the database structure and insert default users for testing.

Run the Project
Backend
Navigate to: NurseLink.API Run

API available at: https://localhost:7186

Swagger: https://localhost:7186/swagger

Frontend
Navigate to: nurselink-frontend Install dependencies:

npm install
Run: npm run dev

Frontend available at: http://localhost:5173

Current Features
JWT Authentication
Role-based login
Temporary Dashboard per role (Admin, Nurse, Patient)
Author
Alejandro Preciado Pérez
Final Degree Project (TFG)
