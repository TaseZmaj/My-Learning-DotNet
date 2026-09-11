namespace Domain.Dto;

public class ExternalConsultationsDto
{
    public List<ExternalConsultationDto> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
}

public class ExternalConsultationDto
{
    public string ExternalId { get; set; }
    public string RoomName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public string Status { get; set; }
}


//{
//   "items": [
//     {
//       "externalId": "string",
//       "roomName": "string",
//       "startTime": "2026-05-20T17:14:51.722Z",
//       "endTime": "2026-05-20T17:14:51.722Z",
//       "lastModifiedUtc": "2026-05-20T17:14:51.722Z",
//       "status": "string"
//     }
//   ],
//   "page": 0,
//   "pageSize": 0,
//   "totalCount": 0,
//   "totalPages": 0,
//   "hasNextPage": true
// }