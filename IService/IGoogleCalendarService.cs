using Google.Apis.Calendar.v3.Data;
using GoogleCalenderApi;

public interface IGoogleCalendarService
{
    string GetAuthCode();

    Task<GoogleTokenResponse> GetTokens(string code);

    string AddToGoogleCalendar(GoogleCalenderReqDTO googleCalendarReqDTO);
    GoogleCalendarViewResponseDto ViewGoogleCalendarEvents(GoogleCalenderViewDto googleCalenderViewDto);
    bool DeleteGoogleCalendarEvent(string calenderId,string eventId, string userToken);
}