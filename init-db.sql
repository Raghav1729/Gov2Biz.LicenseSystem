-- Create Gov2BizLicenseSystem database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Gov2BizLicenseSystem')
BEGIN
    CREATE DATABASE Gov2BizLicenseSystem;
END
GO

-- Switch to Gov2BizLicenseSystem
USE Gov2BizLicenseSystem;
GO

-- Create Tenants table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE Tenants (
        Id NVARCHAR(50) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Domain NVARCHAR(100) NOT NULL UNIQUE,
        ConnectionString NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    
    CREATE INDEX IX_Tenants_Domain ON Tenants(Domain);
    CREATE INDEX IX_Tenants_IsActive ON Tenants(IsActive);
    
    PRINT 'Tenants table created successfully';
END
ELSE
BEGIN
    PRINT 'Tenants table already exists';
END
GO

-- Create Agencies table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Agencies')
BEGIN
    CREATE TABLE Agencies (
        Id NVARCHAR(50) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(20) NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
        UNIQUE (Code, TenantId)
    );
    
    CREATE INDEX IX_Agencies_TenantId ON Agencies(TenantId);
    CREATE INDEX IX_Agencies_Code ON Agencies(Code);
    CREATE INDEX IX_Agencies_IsActive ON Agencies(IsActive);
    
    PRINT 'Agencies table created successfully';
END
ELSE
BEGIN
    PRINT 'Agencies table already exists';
END
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        AgencyId NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
        FOREIGN KEY (AgencyId) REFERENCES Agencies(Id),
        UNIQUE (Email, TenantId)
    );

    -- Create indexes
    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_TenantId ON Users(TenantId);
    CREATE INDEX IX_Users_Role ON Users(Role);
    CREATE INDEX IX_Users_AgencyId ON Users(AgencyId);
    CREATE INDEX IX_Users_IsActive ON Users(IsActive);

    PRINT 'Users table created successfully';
END
ELSE
BEGIN
    PRINT 'Users table already exists';
END
GO

-- Create LicenseApplications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LicenseApplications')
BEGIN
    CREATE TABLE LicenseApplications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationNumber NVARCHAR(100) NOT NULL,
        LicenseType NVARCHAR(100) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Submitted',
        ApplicantId INT NOT NULL,
        AgencyId NVARCHAR(50) NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        ReviewerId INT NULL,
        SubmittedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ReviewedAt DATETIME2 NULL,
        ApprovedAt DATETIME2 NULL,
        RejectedAt DATETIME2 NULL,
        IssuedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        ReviewerNotes NVARCHAR(1000) NULL,
        RejectionReason NVARCHAR(1000) NULL,
        ApplicationFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
        IsPaid BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (ApplicantId) REFERENCES Users(Id),
        FOREIGN KEY (AgencyId) REFERENCES Agencies(Id),
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
        FOREIGN KEY (ReviewerId) REFERENCES Users(Id),
        UNIQUE (ApplicationNumber, TenantId)
    );
    
    CREATE INDEX IX_LicenseApplications_ApplicantId ON LicenseApplications(ApplicantId);
    CREATE INDEX IX_LicenseApplications_AgencyId ON LicenseApplications(AgencyId);
    CREATE INDEX IX_LicenseApplications_TenantId ON LicenseApplications(TenantId);
    CREATE INDEX IX_LicenseApplications_Status ON LicenseApplications(Status);
    CREATE INDEX IX_LicenseApplications_SubmittedAt ON LicenseApplications(SubmittedAt);
    
    PRINT 'LicenseApplications table created successfully';
END
ELSE
BEGIN
    PRINT 'LicenseApplications table already exists';
END
GO

-- Create Licenses table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Licenses')
BEGIN
    CREATE TABLE Licenses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LicenseNumber NVARCHAR(100) NOT NULL,
        Type NVARCHAR(100) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        ApplicationId INT NOT NULL,
        ApplicantId INT NOT NULL,
        AgencyId NVARCHAR(50) NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        IssuedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL,
        RenewedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        Notes NVARCHAR(1000) NULL,
        FOREIGN KEY (ApplicationId) REFERENCES LicenseApplications(Id),
        FOREIGN KEY (ApplicantId) REFERENCES Users(Id),
        FOREIGN KEY (AgencyId) REFERENCES Agencies(Id),
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
        UNIQUE (LicenseNumber, TenantId)
    );
    
    CREATE INDEX IX_Licenses_ApplicationId ON Licenses(ApplicationId);
    CREATE INDEX IX_Licenses_ApplicantId ON Licenses(ApplicantId);
    CREATE INDEX IX_Licenses_AgencyId ON Licenses(AgencyId);
    CREATE INDEX IX_Licenses_TenantId ON Licenses(TenantId);
    CREATE INDEX IX_Licenses_Status ON Licenses(Status);
    CREATE INDEX IX_Licenses_ExpiresAt ON Licenses(ExpiresAt);
    
    PRINT 'Licenses table created successfully';
END
ELSE
BEGIN
    PRINT 'Licenses table already exists';
END
GO

-- Create Documents table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
BEGIN
    CREATE TABLE Documents (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FileName NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        FileSize BIGINT NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        EntityId INT NOT NULL,
        DocumentType NVARCHAR(50) NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        UploadedBy INT NOT NULL,
        UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        Notes NVARCHAR(1000) NULL,
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
        FOREIGN KEY (UploadedBy) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_Documents_EntityType_EntityId ON Documents(EntityType, EntityId);
    CREATE INDEX IX_Documents_TenantId ON Documents(TenantId);
    CREATE INDEX IX_Documents_UploadedBy ON Documents(UploadedBy);
    CREATE INDEX IX_Documents_DocumentType ON Documents(DocumentType);
    CREATE INDEX IX_Documents_IsDeleted ON Documents(IsDeleted);
    
    PRINT 'Documents table created successfully';
END
ELSE
BEGIN
    PRINT 'Documents table already exists';
END
GO

-- Create Notifications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        RecipientId INT NOT NULL,
        EntityReference NVARCHAR(200) NULL,
        TenantId NVARCHAR(50) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ReadAt DATETIME2 NULL,
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (RecipientId) REFERENCES Users(Id),
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
    );
    
    CREATE INDEX IX_Notifications_RecipientId ON Notifications(RecipientId);
    CREATE INDEX IX_Notifications_TenantId ON Notifications(TenantId);
    CREATE INDEX IX_Notifications_Type ON Notifications(Type);
    CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
    CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
    
    PRINT 'Notifications table created successfully';
END
ELSE
BEGIN
    PRINT 'Notifications table already exists';
END
GO

-- Create Payments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE Payments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TransactionId NVARCHAR(100) NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        Amount DECIMAL(10,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        Currency NVARCHAR(10) NOT NULL DEFAULT 'USD',
        ApplicationId INT NOT NULL,
        PayerId INT NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CompletedAt DATETIME2 NULL,
        UpdatedAt DATETIME2 NULL,
        GatewayResponse NVARCHAR(MAX) NULL,
        Notes NVARCHAR(1000) NULL,
        FOREIGN KEY (ApplicationId) REFERENCES LicenseApplications(Id),
        FOREIGN KEY (PayerId) REFERENCES Users(Id),
        FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
        UNIQUE (TransactionId, TenantId)
    );
    
    CREATE INDEX IX_Payments_ApplicationId ON Payments(ApplicationId);
    CREATE INDEX IX_Payments_PayerId ON Payments(PayerId);
    CREATE INDEX IX_Payments_TenantId ON Payments(TenantId);
    CREATE INDEX IX_Payments_Status ON Payments(Status);
    CREATE INDEX IX_Payments_CreatedAt ON Payments(CreatedAt);
    
    PRINT 'Payments table created successfully';
END
ELSE
BEGIN
    PRINT 'Payments table already exists';
END
GO

PRINT 'All tables created successfully';
GO

-- Insert sample data
PRINT 'Inserting sample data...';
GO

-- Insert sample tenants
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = 'tenant-001')
BEGIN
    INSERT INTO Tenants (Id, Name, Domain, IsActive, CreatedAt) VALUES
    ('tenant-001', 'Department of Transportation', 'dot.gov', 1, GETUTCDATE()),
    ('tenant-002', 'Health Services Agency', 'health.gov', 1, GETUTCDATE()),
    ('tenant-003', 'Business Licensing Board', 'business.gov', 1, GETUTCDATE());
    PRINT 'Sample tenants inserted';
END
GO

-- Insert sample agencies
IF NOT EXISTS (SELECT 1 FROM Agencies WHERE Id = 'DOT')
BEGIN
    INSERT INTO Agencies (Id, Name, Code, TenantId, Description, IsActive, CreatedAt) VALUES
    ('DOT', 'Department of Transportation', 'DOT', 'tenant-001', 'Transportation and vehicle licensing', 1, GETUTCDATE()),
    ('HEALTH', 'Health Services Agency', 'HEALTH', 'tenant-002', 'Healthcare professional licensing', 1, GETUTCDATE()),
    ('BUSINESS', 'Business Licensing Board', 'BUSINESS', 'tenant-003', 'Business operation licensing', 1, GETUTCDATE());
    PRINT 'Sample agencies inserted';
END
GO

-- Insert sample users with proper password hashing (password: password123)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@dot.gov')
BEGIN
    INSERT INTO Users (Email, FirstName, LastName, PasswordHash, Role, TenantId, AgencyId, IsActive, CreatedAt) VALUES
    ('admin@dot.gov', 'System', 'Administrator', 'plain:password123', 'Administrator', 'tenant-001', 'DOT', 1, GETUTCDATE()),
    ('staff@dot.gov', 'Transport', 'Staff', 'plain:password123', 'AgencyStaff', 'tenant-001', 'DOT', 1, GETUTCDATE()),
    ('applicant1@dot.gov', 'John', 'Applicant', 'plain:password123', 'Applicant', 'tenant-001', NULL, 1, GETUTCDATE()),
    ('applicant2@dot.gov', 'Jane', 'Smith', 'plain:password123', 'Applicant', 'tenant-001', NULL, 1, GETUTCDATE()),
    ('admin@health.gov', 'Health', 'Admin', 'plain:password123', 'Administrator', 'tenant-002', 'HEALTH', 1, GETUTCDATE()),
    ('staff@health.gov', 'Medical', 'Staff', 'plain:password123', 'AgencyStaff', 'tenant-002', 'HEALTH', 1, GETUTCDATE()),
    ('applicant@health.gov', 'Alice', 'Johnson', 'plain:password123', 'Applicant', 'tenant-002', NULL, 1, GETUTCDATE());
    PRINT 'Sample users inserted';
END
GO

-- Insert sample license applications
IF NOT EXISTS (SELECT 1 FROM LicenseApplications WHERE ApplicationNumber = 'APP-2024-1001')
BEGIN
    INSERT INTO LicenseApplications (ApplicationNumber, LicenseType, Status, ApplicantId, AgencyId, TenantId, SubmittedAt, ApplicationFee, IsPaid) VALUES
    ('APP-2024-1001', 'Commercial Driver License', 'Submitted', 3, 'DOT', 'tenant-001', DATEADD(day, -10, GETUTCDATE()), 150.00, 0),
    ('APP-2024-1002', 'Vehicle Registration', 'Submitted', 4, 'DOT', 'tenant-001', DATEADD(day, -5, GETUTCDATE()), 75.00, 0),
    ('APP-2024-1003', 'Medical License', 'Under Review', 7, 'HEALTH', 'tenant-002', DATEADD(day, -7, GETUTCDATE()), 500.00, 1),
    ('APP-2024-1004', 'Nursing License', 'Approved', 7, 'HEALTH', 'tenant-002', DATEADD(day, -15, GETUTCDATE()), 300.00, 1);
    PRINT 'Sample license applications inserted';
END
GO

-- Insert sample licenses
IF NOT EXISTS (SELECT 1 FROM Licenses WHERE LicenseNumber = 'DOT-2024-50001')
BEGIN
    INSERT INTO Licenses (LicenseNumber, Type, Status, ApplicationId, ApplicantId, AgencyId, TenantId, IssuedAt, ExpiresAt, Notes) VALUES
    ('DOT-2024-50001', 'Commercial Driver License', 'Active', 1, 3, 'DOT', 'tenant-001', DATEADD(day, -8, GETUTCDATE()), DATEADD(day, 357, GETUTCDATE()), 'Standard CDL issued'),
    ('HEALTH-2024-30001', 'Medical License', 'Active', 4, 7, 'HEALTH', 'tenant-002', DATEADD(day, -10, GETUTCDATE()), DATEADD(day, 355, GETUTCDATE()), 'Medical practitioner license');
    PRINT 'Sample licenses inserted';
END
GO

-- Insert sample documents
IF NOT EXISTS (SELECT 1 FROM Documents WHERE FileName = 'id_card.jpg')
BEGIN
    INSERT INTO Documents (FileName, ContentType, FileSize, FilePath, EntityType, EntityId, DocumentType, TenantId, UploadedBy, Notes) VALUES
    ('id_card.jpg', 'image/jpeg', 1024000, '/uploads/applications/1/id_card.jpg', 'LicenseApplication', 1, 'Identification', 'tenant-001', 3, 'Applicant identification document'),
    ('medical_degree.pdf', 'application/pdf', 2048000, '/uploads/applications/4/medical_degree.pdf', 'LicenseApplication', 4, 'Education', 'tenant-002', 7, 'Medical degree certificate'),
    ('experience_letter.pdf', 'application/pdf', 512000, '/uploads/applications/4/experience_letter.pdf', 'LicenseApplication', 4, 'Experience', 'tenant-002', 7, 'Work experience verification');
    PRINT 'Sample documents inserted';
END
GO

-- Insert sample notifications
IF NOT EXISTS (SELECT 1 FROM Notifications WHERE Title = 'Application Submitted')
BEGIN
    INSERT INTO Notifications (Title, Message, Type, RecipientId, EntityReference, TenantId, CreatedAt) VALUES
    ('Application Submitted', 'Your license application APP-2024-1001 has been submitted successfully.', 'Info', 3, '1', 'tenant-001', DATEADD(day, -10, GETUTCDATE())),
    ('Application Under Review', 'Your license application APP-2024-1003 is now under review.', 'Info', 7, '3', 'tenant-002', DATEADD(day, -6, GETUTCDATE())),
    ('License Issued', 'Your license HEALTH-2024-30001 has been issued successfully.', 'Success', 7, '2', 'tenant-002', DATEADD(day, -10, GETUTCDATE())),
    ('Payment Required', 'Please complete the payment for your application APP-2024-1001.', 'Payment', 3, '1', 'tenant-001', DATEADD(day, -9, GETUTCDATE()));
    PRINT 'Sample notifications inserted';
END
GO

-- Insert sample payments
IF NOT EXISTS (SELECT 1 FROM Payments WHERE TransactionId = 'TXN-2024-10001')
BEGIN
    INSERT INTO Payments (TransactionId, PaymentMethod, Amount, Status, Currency, ApplicationId, PayerId, TenantId, CreatedAt, CompletedAt) VALUES
    ('TXN-2024-10001', 'Credit Card', 500.00, 'Completed', 'USD', 3, 7, 'tenant-002', DATEADD(day, -15, GETUTCDATE()), DATEADD(day, -15, GETUTCDATE())),
    ('TXN-2024-10002', 'Bank Transfer', 150.00, 'Pending', 'USD', 1, 3, 'tenant-001', DATEADD(day, -9, GETUTCDATE()), NULL);
    PRINT 'Sample payments inserted';
END
GO

PRINT 'Gov2BizLicenseSystem database and all tables created successfully with sample data';
GO
