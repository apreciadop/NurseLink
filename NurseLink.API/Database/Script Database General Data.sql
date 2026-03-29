-- SURGERY TYPES
-- =========================
INSERT INTO SurgeryTypes (surgeryType_name)
VALUES 
('Rhinoplasty'),
('Liposuction'),
('Breast Augmentation'),
('Abdominoplasty'),
('Facelift');

-- DEFAULT NURSES
-- =========================
INSERT INTO Nurses (nurse_name, nurse_surname, nurse_email, nurse_password)
VALUES
('Ana', 'Lopez', 'anna.lopez@nurselink.com', '1234'),
('Maria', 'Garcia', 'maria.garcia@nurselink.com', '1234'),
('Laura', 'Fernandez', 'laura.fernandez@nurselink.com', '1234');

-- DEFAULT PATIENTS
-- =========================
INSERT INTO Patients (
    patient_name,
    patient_surname,
    patient_email,
    patient_password,
    patient_birthdate,
    patient_phone
)
VALUES
('Rosa', 'Segura', 'rosasegura@gmail.com', '1234', '1990-05-10', '600111222'),
('Carla', 'Perez', 'carlos.perez@gmail.com', '1234', '1985-03-20', '600333444'),
('Lucia', 'Martinez', 'lucia.martinez@gmail.com', '1234', '1995-07-15', '600555666');