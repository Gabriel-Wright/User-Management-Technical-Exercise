namespace UserManagement.UI.Exceptions
{

    //Throw this exception for API related errors
    //Error message sent shown on front end.
    public class UserApiException : Exception

    {
        public int? StatusCode { get; }
        public UserApiException(string message, int? statusCode = null, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }

}