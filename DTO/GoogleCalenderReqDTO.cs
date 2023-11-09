namespace GoogleCalenderApi;

public class GoogleCalenderReqDTO
{
    public string Summary {get;set;}

    public string Description { get; set; }
 

    public DateTime StartTime { get; set; }


    public DateTime EndTime { get; set; }

    public string refreshToken {get;set;}

    public string CalendarId { get;set; }
  
 
}
