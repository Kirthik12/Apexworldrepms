USE ApexWorldREPMS;

-- Add categories if missing
IF NOT EXISTS (SELECT 1 FROM [REPMS].[PropertyCategories] WHERE Name = 'Villa')
BEGIN
    INSERT INTO [REPMS].[PropertyCategories] ([Name], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES ('Villa', GETUTCDATE(), GETUTCDATE(), 0),
           ('Apartment', GETUTCDATE(), GETUTCDATE(), 0),
           ('Commercial', GETUTCDATE(), GETUTCDATE(), 0);
END

-- Assign category to properties
UPDATE [REPMS].[Properties] SET CategoryId = (SELECT TOP 1 Id FROM [REPMS].[PropertyCategories] WHERE Name = 'Villa') WHERE CategoryId = 0 OR CategoryId IS NULL;

-- Create some Bookings
DECLARE @buyerId INT = (SELECT TOP 1 Id FROM [REPMS].[Users]);
DECLARE @propId INT = (SELECT TOP 1 Id FROM [REPMS].[Properties]);

IF @buyerId IS NOT NULL AND @propId IS NOT NULL
BEGIN
    INSERT INTO [REPMS].[Bookings] 
    ([PropertyId], [BuyerId], [FirstName], [LastName], [PhoneNumber], [Email], [PermanentAddress], [Status], [CreatedAt], [UpdatedAt], [IsDeleted], [IsVisited])
    VALUES
    (@propId, @buyerId, 'Test', 'User', '1234567890', 'test@example.com', 'Chennai', 'Pending', GETUTCDATE(), GETUTCDATE(), 0, 0),
    (@propId, @buyerId, 'Test2', 'User2', '1234567890', 'test2@example.com', 'Chennai', 'Approved', DATEADD(day, -5, GETUTCDATE()), GETUTCDATE(), 0, 0),
    (@propId, @buyerId, 'Test3', 'User3', '1234567890', 'test3@example.com', 'Chennai', 'Paid', DATEADD(month, -1, GETUTCDATE()), GETUTCDATE(), 0, 0);

    -- Get a booking id
    DECLARE @b1 INT = (SELECT TOP 1 Id FROM [REPMS].[Bookings] WHERE Status = 'Approved');
    DECLARE @b2 INT = (SELECT TOP 1 Id FROM [REPMS].[Bookings] WHERE Status = 'Paid');

    -- Insert Payments
    INSERT INTO [REPMS].[Payments]
    ([BookingId], [PropertyId], [BuyerId], [Amount], [PaymentMethod], [TransactionId], [Status], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    (@b1, @propId, @buyerId, 500000, 'UPI', 'TXN123456', 'Completed', DATEADD(day, -5, GETUTCDATE()), GETUTCDATE(), 0),
    (@b2, @propId, @buyerId, 5000000, 'Bank Transfer', 'TXN987654', 'Completed', DATEADD(month, -1, GETUTCDATE()), GETUTCDATE(), 0),
    (@b1, @propId, @buyerId, 100000, 'UPI', 'TXN555555', 'Completed', DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 0),
    (@b2, @propId, @buyerId, 50000, 'Credit Card', 'TXN444444', 'Completed', DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 0);
END
