USE NurseLinkDB;
GO

-- =========================
-- NURSES
-- =========================
CREATE TABLE Nurses (
    nurse_id INT IDENTITY(1,1) PRIMARY KEY,
    nurse_name NVARCHAR(255) NOT NULL,
    nurse_surname NVARCHAR(255) NOT NULL,
    nurse_email NVARCHAR(255) NOT NULL UNIQUE,
    nurse_password NVARCHAR(255) NOT NULL,
    nurse_active BIT NOT NULL DEFAULT 1,
    nurse_photo NVARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =========================
-- PATIENTS
-- =========================
CREATE TABLE Patients (
    patient_id INT IDENTITY(1,1) PRIMARY KEY,
    patient_name NVARCHAR(255) NOT NULL,
    patient_surname NVARCHAR(255) NOT NULL,
    patient_email NVARCHAR(255) NOT NULL UNIQUE,
    patient_password NVARCHAR(255) NOT NULL,
    patient_birthdate DATE NULL,
    patient_active BIT NOT NULL DEFAULT 1,
    patient_phone NVARCHAR(255) NULL,
    patient_photo NVARCHAR(255) NULL,
    patient_observations NVARCHAR(MAX) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =========================
-- ASSIGNMENTS (1 nurse per patient)
-- =========================
CREATE TABLE Assignments (
    assignment_id INT IDENTITY(1,1) PRIMARY KEY,
    nurse_id INT NOT NULL,
    patient_id INT NOT NULL UNIQUE,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Assignments_Nurses 
        FOREIGN KEY (nurse_id) REFERENCES Nurses(nurse_id),
    CONSTRAINT FK_Assignments_Patients 
        FOREIGN KEY (patient_id) REFERENCES Patients(patient_id)
);
GO

-- =========================
-- SURGERY TYPES
-- =========================
CREATE TABLE SurgeryTypes (
    surgeryType_id INT IDENTITY(1,1) PRIMARY KEY,
    surgeryType_name NVARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =========================
-- SURGERIES
-- =========================
CREATE TABLE Surgeries (
    surgery_id INT IDENTITY(1,1) PRIMARY KEY,
    surgery_date DATE NOT NULL,
    surgeryType_id INT NOT NULL,
    patient_id INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Surgeries_SurgeryTypes 
        FOREIGN KEY (surgeryType_id) REFERENCES SurgeryTypes(surgeryType_id),
    CONSTRAINT FK_Surgeries_Patients 
        FOREIGN KEY (patient_id) REFERENCES Patients(patient_id)
);
GO

-- =========================
-- REPORTS 
-- =========================
CREATE TABLE Reports (
    report_id INT IDENTITY(1,1) PRIMARY KEY,
    patient_id INT NOT NULL,
    report_date DATE NOT NULL,
    report_pain INT NOT NULL,
    report_fever BIT NOT NULL,
    report_bleeding BIT NOT NULL,
    report_swelling BIT NOT NULL,
    report_observations NVARCHAR(MAX) NULL,
    report_alerts INT NOT NULL DEFAULT 0,
    report_status INT NOT NULL, -- 0 Stable, 1 Warning, 2 Alert
    nurse_id INT NULL,
    report_nurse_observations NVARCHAR(MAX) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Reports_Nurses 
        FOREIGN KEY (nurse_id) REFERENCES Nurses(nurse_id),
    CONSTRAINT FK_Reports_Patients 
        FOREIGN KEY (patient_id) REFERENCES Patients(patient_id),
    CONSTRAINT CHK_Reports_Pain 
        CHECK (report_pain BETWEEN 0 AND 10),
    CONSTRAINT CHK_Reports_Status 
        CHECK (report_status IN (0,1,2))
);
GO

-- =========================
-- CONVERSATIONS
-- =========================
CREATE TABLE Conversations (
    conversation_id INT IDENTITY(1,1) PRIMARY KEY,
    nurse_id INT NOT NULL,
    patient_id INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Conversations_Nurses 
        FOREIGN KEY (nurse_id) REFERENCES Nurses(nurse_id),
    CONSTRAINT FK_Conversations_Patients 
        FOREIGN KEY (patient_id) REFERENCES Patients(patient_id)
);
GO

-- =========================
-- MESSAGES
-- =========================
CREATE TABLE Messages (
    message_id INT IDENTITY(1,1) PRIMARY KEY,
    conversation_id INT NOT NULL,
    message_date DATETIME NOT NULL DEFAULT GETDATE(),
    message_read BIT NOT NULL DEFAULT 0,
    message_text NVARCHAR(MAX) NOT NULL,
    message_sender INT NOT NULL, -- 0 nurse / 1 patient
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Messages_Conversations 
        FOREIGN KEY (conversation_id) REFERENCES Conversations(conversation_id),
    CONSTRAINT CHK_Messages_Sender 
        CHECK (message_sender IN (0,1))
);
GO
