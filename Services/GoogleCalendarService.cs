using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using GoogleCalenderApi;
using System.Text;
using Google;
using System.Globalization;

public class GoogleCalendarService : IGoogleCalendarService
{

    public IConfiguration _configuration {get;}
    private readonly HttpClient _httpClient;


    public GoogleCalendarService(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public string GetAuthCode()
    {
        try
        {

            string clientID = _configuration["GoogleCalendarAPI:ClientId"];
            string scopeURL1 = "https://accounts.google.com/o/oauth2/auth?redirect_uri={0}&prompt={1}&response_type={2}&client_id={3}&scope={4}&access_type={5}";  
            string prompt = "consent";
            string response_type = "code";
            string scope = "https://www.googleapis.com/auth/calendar";
            string access_type = "offline";

            var redirectURL = "https://localhost:7138/api/User/auth/callback";
            string redirect_uri_encode = Method.urlEncodeForGoogle(redirectURL);

            var mainURL = string.Format(scopeURL1, redirect_uri_encode, prompt, response_type, clientID, scope, access_type);

            return mainURL;
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }

    public async Task<GoogleTokenResponse> GetTokens(string code)
    {

        var clientId = _configuration["GoogleCalendarAPI:ClientId"];

        string clientSecret = _configuration["GoogleCalendarAPI:ClientSecret"];

        var redirectURL = "https://localhost:7138/api/User/auth/callback";

        var tokenEndpoint = "https://accounts.google.com/o/oauth2/token";

        var content = new StringContent($"code={code}&redirect_uri={Uri.EscapeDataString(redirectURL)}&client_id={clientId}&client_secret={clientSecret}&grant_type=authorization_code", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(tokenEndpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);
            return tokenResponse;
        }
        else
        {
            // Handle the error case when authentication fails
            throw new Exception($"Failed to authenticate: {responseContent}");
        }
    }

    public string AddToGoogleCalendar(GoogleCalenderReqDTO googleCalendarReqDTO)
    {
        try
        {

            var token = new TokenResponse
            {
                RefreshToken = googleCalendarReqDTO.refreshToken
            };

            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                  new GoogleAuthorizationCodeFlow.Initializer
                  {
                      ClientSecrets = new ClientSecrets
                      {
                          ClientId = _configuration["GoogleCalendarAPI:ClientId"],
                          ClientSecret = _configuration["GoogleCalendarAPI:ClientSecret"],
                      }

                  }), "user", token, _configuration["GoogleCalendarAPI:ProjectId"]) ;

            if (credentials == null)
            {
                return "Failed to obtain Google API credentials.";
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
            });

            Event newEvent = new Event()
            {
                Summary = googleCalendarReqDTO.Summary,
                Description = googleCalendarReqDTO.Description,

                Start = new EventDateTime()
                {
                    DateTime = googleCalendarReqDTO.StartTime,
                },

                End = new EventDateTime()
                {
                    DateTime = googleCalendarReqDTO.EndTime,
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = new EventReminder[] {
                        new EventReminder() {
                            Method = "email", Minutes = 30
                        },

                        new EventReminder() {
                            Method = "popup", Minutes = 15
                        },

                        new EventReminder() {
                            Method = "popup", Minutes = 1
                         },
                    }
                }
            };

            EventsResource.InsertRequest insertRequest = service.Events.Insert(newEvent, googleCalendarReqDTO.CalendarId);
            Event createdEvent = insertRequest.Execute();
            return createdEvent.Id;
        }
        catch (GoogleApiException ex)
        {
            // Handle Google API errors
            return "Google API error: " + ex.Message;
        }
        catch (Exception e)
        {
            return "An error occurred: " + e.Message.ToString()+"  "+ e.GetType();
        }
    }

    public GoogleCalendarViewResponseDto ViewGoogleCalendarEvents(GoogleCalenderViewDto googleCalenderViewDto)
    {
        try
        {
            var token = new TokenResponse
            {
                RefreshToken = googleCalenderViewDto.userToken
            };

            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _configuration["GoogleCalendarAPI:ClientId"],
                        ClientSecret = _configuration["GoogleCalendarAPI:ClientSecret"],
                    }
                }), "user", token);

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
            });

            EventsResource.ListRequest request = service.Events.List(googleCalenderViewDto.calenderId);

            // Apply filters if provided
            if (googleCalenderViewDto.startDate.HasValue)
                request.TimeMin = googleCalenderViewDto.startDate.Value;

            if (googleCalenderViewDto.endDate.HasValue)
                request.TimeMax = googleCalenderViewDto.endDate.Value;

            if (!string.IsNullOrEmpty(googleCalenderViewDto.searchQuery))
                request.Q = googleCalenderViewDto.searchQuery;

            // Apply pagination parameters
            if (!string.IsNullOrEmpty(googleCalenderViewDto.pageSize))
                request.MaxResults = int.Parse(googleCalenderViewDto.pageSize);

            if (!string.IsNullOrEmpty(googleCalenderViewDto.pageToken))
                request.PageToken =googleCalenderViewDto.pageToken;

            // Execute the request to get a list of events
            Events events = request.Execute();

            var eventList = events.Items.ToList();

            // Create the response object
            var response = new GoogleCalendarViewResponseDto
            {
                Events = eventList,
                NextPageToken = events.NextPageToken
            };

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new GoogleCalendarViewResponseDto { Events = new List<Event>(), NextPageToken = null };
        }
    }

    public bool DeleteGoogleCalendarEvent(string calenderId,string eventId, string userToken)
    {
        try
        {
            var token = new TokenResponse
            {
                RefreshToken = userToken
            };

            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _configuration["GoogleCalendarAPI:ClientId"],
                        ClientSecret = _configuration["GoogleCalendarAPI:ClientSecret"],
                    }
                }), "user", token);

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
            });

            var deleteEventRequest = service.Events.Delete(calenderId, eventId); 
            deleteEventRequest.Execute();

            return true; 
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false; 
        }
    }









}