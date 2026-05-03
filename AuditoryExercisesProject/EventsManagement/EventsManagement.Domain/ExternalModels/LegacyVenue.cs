namespace EventsManagement.Domain.ExternalModels;

//CREATE TABLE dbo.Venues (
//     VenueId       INT IDENTITY(1,1) PRIMARY KEY,
//     Name          NVARCHAR(200) NOT NULL,
//     Address       NVARCHAR(500),
//     City          NVARCHAR(100),
//     Country       NVARCHAR(100),
//     ZipCode       NVARCHAR(20),
//     TotalCapacity INT NOT NULL DEFAULT 0,
//     IsActive      BIT NOT NULL DEFAULT 1,
//     LastModified  DATETIME2 NOT NULL DEFAULT GETUTCDATE()
// );

public class LegacyVenue
{
    public int VenueId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    
    public string Country { get; set; }
    public string ZipCode { get; set; }
    public int TotalCapacity { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime LastModified { get; set; }
}