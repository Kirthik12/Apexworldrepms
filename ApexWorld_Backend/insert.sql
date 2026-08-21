USE ApexWorldREPMS;
DECLARE @i INT = 1;
DECLARE @price DECIMAL(18,2) = 5500000;
WHILE @i <= 10
BEGIN
    DECLARE @title NVARCHAR(200) = 'Premium Villa ' + CAST(@i AS NVARCHAR(10)) + ' in Besant Nagar';
    DECLARE @desc NVARCHAR(MAX) = 'Beautiful, spacious villa located in the heart of Besant Nagar with top-tier amenities.';
    DECLARE @address NVARCHAR(500) = 'Besant Nagar, Chennai';
    DECLARE @carpetArea INT = 1000 + (@i * 150);
    DECLARE @bedrooms INT = CASE WHEN @i <= 3 THEN 2 WHEN @i <= 7 THEN 3 ELSE 4 END;
    DECLARE @bathrooms INT = @bedrooms - 1;
    DECLARE @areaSize INT = @carpetArea + 200;
    DECLARE @maintenance DECIMAL(18,2) = 2000 + (@i * 500);
    DECLARE @carParking INT = CASE WHEN @i <= 5 THEN 1 ELSE 2 END;
    
    INSERT INTO [REPMS].[Properties] 
    ([Title], [Description], [Price], [Address], [CarpetArea], [Facing], [ProjectName], [Bedrooms], [Bathrooms], [AreaSize], [Furnishing], [TotalFloors], [Maintenance], [CarParking], [CategoryId], [IsAvailable], [Status], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES 
    (@title, @desc, @price, @address, @carpetArea, 'East', 'Apex Signature', @bedrooms, @bathrooms, @areaSize, 'Semi-Furnished', 2, @maintenance, @carParking, 1, 1, 'Approved', GETUTCDATE(), GETUTCDATE(), 0);
    
    SET @price = @price + 1000000;
    SET @i = @i + 1;
END
