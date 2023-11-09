using Microsoft.AspNetCore.Mvc;

namespace GoogleCalenderApi;

public class GoogleCalenderViewDto
{

    [FromQuery]
        public string calenderId {get; set;}
    [FromQuery]
        public string userToken { get; set;}
    [FromQuery]
        public DateTime? startDate { get; set;}
    [FromQuery]
        public DateTime? endDate { get; set;}
    [FromQuery]
        public string? searchQuery { get; set;}
    [FromQuery]
        public string? pageSize { get; set;}
    [FromQuery]
        public string? pageToken { get; set;}
 

}
