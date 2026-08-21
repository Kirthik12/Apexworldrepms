USE ApexWorldREPMS;

-- Add categories if missing
IF NOT EXISTS (SELECT 1 FROM [REPMS].[PropertyCategories] WHERE Name = 'Villa')
BEGIN
    INSERT INTO [REPMS].[PropertyCategories] ([Name], [Description], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES ('Villa', 'Premium standalone houses', GETUTCDATE(), GETUTCDATE(), 0),
           ('Apartment', 'High-rise residential', GETUTCDATE(), GETUTCDATE(), 0),
           ('Commercial', 'Office and retail spaces', GETUTCDATE(), GETUTCDATE(), 0);
END

-- Assign category to properties
UPDATE [REPMS].[Properties] SET CategoryId = (SELECT TOP 1 Id FROM [REPMS].[PropertyCategories] WHERE Name = 'Villa') WHERE CategoryId = 0 OR CategoryId IS NULL;

-- Create some Bookings
DECLARE @buyerId INT = (SELECT TOP 1 Id FROM [REPMS].[Users]);
DECLARE @propId INT = (SELECT TOP 1 Id FROM [REPMS].[Properties]);

IF @buyerId IS NOT NULL AND @propId IS NOT NULL
BEGIN
    INSERT INTO [REPMS].[Bookings] 
    ([PropertyId], [BuyerId], [FirstName], [LastName], [Phone], [Email], [AadharNumber], [PanNumber], [Address], [Status], [BookingAmount], [RemainingAmount], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    (@propId, @buyerId, 'Test', 'User', '1234567890', 'test@example.com', '123412341234', 'ABCDE1234F', 'Chennai', 'Pending', 500000, 4500000, GETUTCDATE(), GETUTCDATE(), 0),
    (@propId, @buyerId, 'Test2', 'User2', '1234567890', 'test2@example.com', '123412341234', 'ABCDE1234F', 'Chennai', 'Confirmed', 500000, 4500000, DATEADD(day, -5, GETUTCDATE()), GETUTCDATE(), 0),
    (@propId, @buyerId, 'Test3', 'User3', '1234567890', 'test3@example.com', '123412341234', 'ABCDE1234F', 'Chennai', 'Completed', 5000000, 0, DATEADD(month, -1, GETUTCDATE()), GETUTCDATE(), 0);

    -- Get a booking id
    DECLARE @b1 INT = (SELECT TOP 1 Id FROM [REPMS].[Bookings] WHERE Status = 'Confirmed');
    DECLARE @b2 INT = (SELECT TOP 1 Id FROM [REPMS].[Bookings] WHERE Status = 'Completed');

    -- Insert Payments
    INSERT INTO [REPMS].[Payments]
    ([BookingId], [PropertyId], [BuyerId], [Amount], [PaymentMethod], [TransactionId], [Status], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    (@b1, @propId, @buyerId, 500000, 'UPI', 'TXN123456', 'Completed', DATEADD(day, -5, GETUTCDATE()), GETUTCDATE(), 0),
    (@b2, @propId, @buyerId, 5000000, 'Bank Transfer', 'TXN987654', 'Completed', DATEADD(month, -1, GETUTCDATE()), GETUTCDATE(), 0),
    (@b1, @propId, @buyerId, 100000, 'UPI', 'TXN555555', 'Completed', DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 0),
    (@b2, @propId, @buyerId, 50000, 'Credit Card', 'TXN444444', 'Completed', DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 0);
END
