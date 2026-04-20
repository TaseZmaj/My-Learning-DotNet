namespace EventsManagement.Domain.ExternalModels;

// CREATE TABLE dbo.Seats (
//     SeatId       INT IDENTITY(1,1) PRIMARY KEY,
//     SectionId    INT NOT NULL REFERENCES dbo.Sections(SectionId),
//     Row          NVARCHAR(10) NOT NULL,
//     Number       INT NOT NULL,
//     Label        NVARCHAR(50),
//     IsAccessible BIT NOT NULL DEFAULT 0,
//     LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE()
// );

public class LegacySeat
{
    public int SeatId { get; set; }
    public int SectionId { get; set; }
    public int Row { get; set; }
    public int Number { get; set; }
    public string Label { get; set; } 
    public bool IsAccessible { get; set; }
    public DateTime LastModified { get; set; }
}