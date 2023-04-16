namespace VideoStore.IdentityService.Responses
{
    public class UserLoginResponse
    {
        public UserLoginResponse(string accessToken)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; }
    }
}
