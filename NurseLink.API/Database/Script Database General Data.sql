
USE NurseLinkDB;
GO

----------------------- ADMINISTRATOR -----------------------
-- USER ADMIN. PWD: Admin123*
INSERT INTO Users (user_name, user_surname, user_email, user_password, user_active, user_phone, user_role, user_photo, created_at) VALUES 
  ('Alejandro', 'Administrator', 'administrator@nurselink.com', '$2a$11$k0Q5LBoe6aqEP/BrgFyuAOgUeNDVfC9/t2ZNQWk40BfxwCiGmhdZ6', 1, '', 0, '', GETDATE())

-- CAPTURE ID OF THE USER
DECLARE @UserIdAdmin INT = SCOPE_IDENTITY();

-- ADMIN
INSERT INTO Administrators (user_id) VALUES (@UserIdAdmin);


----------------------- NURSE -----------------------
-- USER NURSE. PWD: Maria123*
INSERT INTO Users (user_name, user_surname, user_email, user_password, user_active, user_phone, user_role, user_photo, created_at) VALUES 
  ('Maria', 'Martinez', 'nurse@nurselink.com', '$2a$11$xS7h6LqkdAIYaSgIudq7b.Nu5kt3zNInqd4ks5Xscus0qMEhcAd7u', 1, '971454554', 1, '', GETDATE())

-- CAPTURE ID OF THE USER
DECLARE @UserIdNurse INT = SCOPE_IDENTITY();

-- NURSE
INSERT INTO Nurses (user_id) VALUES (@UserIdNurse);


----------------------- PATIENT -----------------------
-- USER PATIENT. PWD: Silvia123*
INSERT INTO Users (user_name, user_surname, user_email, user_password, user_active, user_phone, user_role, user_photo, created_at) VALUES 
  ('Silvia', 'Perez', 'patient@nurselink.com', '$2a$11$EkM0zfrIXe5NaLepL9.Tx.MzwGhR5Y0.ZDVwGiLjY58yx/wAAQwOe', 1, '971452784', 2, '', GETDATE())

-- CAPTURE ID OF THE USER
DECLARE @UserIdPatient INT = SCOPE_IDENTITY();

-- PATIENT
INSERT INTO Patients (user_id, patient_observations, created_at) VALUES (@UserIdPatient, 'Post-operative recovery in progress', GETDATE());

DECLARE @IdPatient INT = SCOPE_IDENTITY();

----------------------- SURGERYTYPES Y SURGERY -----------------------
-- SURGERY TYPE
INSERT INTO SurgeryTypes (surgeryType_name, created_at) VALUES ('Rhinoplasty', GETDATE());

-- CAPTURE ID
DECLARE @SurgeryTypeId INT = SCOPE_IDENTITY();

-- SURGERY
INSERT INTO Surgeries (patient_id, surgeryType_id, surgery_date, surgery_notes) VALUES (@IdPatient, @SurgeryTypeId, GETDATE(), 'Initial surgery');

