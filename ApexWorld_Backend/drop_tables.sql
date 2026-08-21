USE ApexWorldREPMS;

-- Drop foreign key dependents first
IF OBJECT_ID('REPMS.PaymentRecords', 'U') IS NOT NULL DROP TABLE REPMS.PaymentRecords;
IF OBJECT_ID('REPMS.Bookings', 'U') IS NOT NULL DROP TABLE REPMS.Bookings;
IF OBJECT_ID('REPMS.LoanApplications', 'U') IS NOT NULL DROP TABLE REPMS.LoanApplications;
IF OBJECT_ID('REPMS.Reviews', 'U') IS NOT NULL DROP TABLE REPMS.Reviews;
IF OBJECT_ID('REPMS.WishlistItems', 'U') IS NOT NULL DROP TABLE REPMS.WishlistItems;

-- Drop parents
IF OBJECT_ID('REPMS.Properties', 'U') IS NOT NULL DROP TABLE REPMS.Properties;

-- Drop independent tables
IF OBJECT_ID('REPMS.AuditLogs', 'U') IS NOT NULL DROP TABLE REPMS.AuditLogs;
IF OBJECT_ID('REPMS.Enquiries', 'U') IS NOT NULL DROP TABLE REPMS.Enquiries;
IF OBJECT_ID('REPMS.RevokedTokens', 'U') IS NOT NULL DROP TABLE REPMS.RevokedTokens;
IF OBJECT_ID('REPMS.Users', 'U') IS NOT NULL DROP TABLE REPMS.Users;

-- Also clean up the dbo tables from the faulty migration so they are totally removed
IF OBJECT_ID('dbo.PaymentRecords', 'U') IS NOT NULL DROP TABLE dbo.PaymentRecords;
IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID('dbo.LoanApplications', 'U') IS NOT NULL DROP TABLE dbo.LoanApplications;
IF OBJECT_ID('dbo.Reviews', 'U') IS NOT NULL DROP TABLE dbo.Reviews;
IF OBJECT_ID('dbo.WishlistItems', 'U') IS NOT NULL DROP TABLE dbo.WishlistItems;
IF OBJECT_ID('dbo.Properties', 'U') IS NOT NULL DROP TABLE dbo.Properties;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Enquiries', 'U') IS NOT NULL DROP TABLE dbo.Enquiries;
IF OBJECT_ID('dbo.RevokedTokens', 'U') IS NOT NULL DROP TABLE dbo.RevokedTokens;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
