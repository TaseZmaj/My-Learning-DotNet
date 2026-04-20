namespace EventsManagement.Domain.ExternalModels;

// CREATE TABLE dbo.Sections (
//     SectionId    INT IDENTITY(1,1) PRIMARY KEY,
//     VenueId      INT NOT NULL REFERENCES dbo.Venues(VenueId),
//     Name         NVARCHAR(100) NOT NULL,
//     Capacity     INT NOT NULL DEFAULT 0,
//     LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE()
// );

public class LegacySection
{
    public int SectionId { get; set; }
    public int VenueId { get; set;}
    public string Name { get; set; }
    public int Capacity { get; set; }
    public DateTime LastModified { get; set; }
}