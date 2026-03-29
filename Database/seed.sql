-- DEFAULT DATA FOR TESTING
-- Includes SurgeryTypes, Nurses and Patients

-- SURGERY TYPES
-- =========================
INSERT INTO SurgeryTypes (surgeryType_name)
VALUES 
('Rhinoplasty'),
('Liposuction'),
('Breast Augmentation'),
('Abdominoplasty'),
('Facelift');

-- DEFAULT ADMIN
--------------------------------
-- Email: admin@nurselink.com
-- Password: admin123

-- DEFAULT NURSES (PWD=Nurse123!)
--------------------------------
INSERT INTO Nurses (nurse_name, nurse_surname, nurse_email, nurse_password)
VALUES
('Ana', 'Lopez', 'anna.lopez@nurselink.com', '$2a$11$eLp44jFrQJbgllGY6yayH.l/kKFL8U.HOwqDJs5k51J40KhyngsLu'),
('Maria', 'Garcia', 'maria.garcia@nurselink.com', '$2a$11$eLp44jFrQJbgllGY6yayH.l/kKFL8U.HOwqDJs5k51J40KhyngsLu'),
('Laura', 'Fernandez', 'laura.fernandez@nurselink.com', '$2a$11$eLp44jFrQJbgllGY6yayH.l/kKFL8U.HOwqDJs5k51J40KhyngsLu');

-- DEFAULT PATIENTS (PWD=Patient123!)
--------------------------------
INSERT INTO Patients (
    patient_name,
    patient_surname,
    patient_email,
    patient_password,
    patient_birthdate,
    patient_phone, 
    patient_password
)
VALUES
('Rosa', 'Segura', 'rosasegura@gmail.com', '1234', '1990-05-10', '600111222','$2a$11$0wwJxJq0UkIapvvNwaNmXOaXi9k6xTUNgXRHKqgHzHwc5tbWFZQD2'),
('Carla', 'Perez', 'carlos.perez@gmail.com', '1234', '1985-03-20', '600333444','$2a$11$0wwJxJq0UkIapvvNwaNmXOaXi9k6xTUNgXRHKqgHzHwc5tbWFZQD2'),
('Lucia', 'Martinez', 'lucia.martinez@gmail.com', '1234', '1995-07-15', '600555666','$2a$11$0wwJxJq0UkIapvvNwaNmXOaXi9k6xTUNgXRHKqgHzHwc5tbWFZQD2');