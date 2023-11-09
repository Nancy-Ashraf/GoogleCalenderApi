using Google;
using GoogleCalenderApi;
using Microsoft.AspNetCore.Mvc;
using System.Net;

[Route("api/events")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IGoogleCalendarService _googleCalendarService;

    public EventsController(IGoogleCalendarService googleCalendarService)
    {
        _googleCalendarService = googleCalendarService;
    }

    [HttpPost]
    public IActionResult AddEvent(GoogleCalenderReqDTO eventDetails)
    {
        try
        {
            if (string.IsNullOrEmpty(eventDetails.Summary)|| string.IsNullOrEmpty(eventDetails.Description) || eventDetails.StartTime == default || eventDetails.EndTime == default)
            {
                return BadRequest("Invalid input.fill the required fields.");
            }

            if (eventDetails.StartTime <= DateTime.Now )
            {
                return BadRequest("Events in the past are not allowed.");

            }
            if (eventDetails.StartTime.DayOfWeek == DayOfWeek.Friday ||
                eventDetails.StartTime.DayOfWeek == DayOfWeek.Saturday)
            {
                return BadRequest("Events on Fridays, or on Saturdays are not allowed.");
            }

            
            string eventId = _googleCalendarService.AddToGoogleCalendar(eventDetails);

            return Created($"/api/events/{eventId}", eventId);
            
        }
        catch (GoogleApiException ex)
        {
            return  StatusCode(400, "Google API error: " + ex.Message);
          
        }
        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while creating the event: " + e.Message);
        }
    }

  
    [HttpGet]
    public IActionResult ViewEvents([FromQuery] GoogleCalenderViewDto googleCalenderViewDto)
    {
        try
        {
            var googleCalendarResponse = _googleCalendarService.ViewGoogleCalendarEvents(googleCalenderViewDto);

            if (googleCalendarResponse.Events.Any())
            {
                var returnEvents = googleCalendarResponse.Events.Select(e => new GoogleCalenderViewReturnDto
                {
                    StartDate = e.Start?.DateTime?.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    EndDate = e.End?.DateTime?.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Summary = e.Summary,
                    Description = e.Description
                }).ToList();

                string nextPageToken = googleCalendarResponse.NextPageToken;

                // Check if there is a next page
                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    return Ok(new { Events = returnEvents, NextPageToken = nextPageToken });
                }
                else
                {
                    return Ok(returnEvents);
                }
            }
            else
            {
                return NotFound("No events found.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to retrieve events: " + ex.Message);
        }
    }




    [HttpDelete("{eventId}")]
    public IActionResult DeleteEvent(string calenderId,string eventId, string userToken)
    {
        try
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return BadRequest("Invalid eventId.");
            }

           
            bool isDeleted = _googleCalendarService.DeleteGoogleCalendarEvent(calenderId,eventId, userToken);

            if (isDeleted)
            {
                return NoContent(); 
            }
            else
            {
                return NotFound("Event not found or deletion failed.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to delete the event: " + ex.Message);
        }
    }


}
