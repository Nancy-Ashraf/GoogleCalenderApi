using Google.Apis.Calendar.v3.Data;

namespace GoogleCalenderApi;

public class GoogleCalendarViewResponseDto
{
    public List<Event> Events { get; set; }
    public string NextPageToken { get; set; }
}