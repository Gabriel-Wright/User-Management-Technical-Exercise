namespace UserManagement.UI.Exceptions
{

    //Throw this exception for API related errors
    //As opposed to using middle man -  we filter all exceptions from API calls through this
    //Specific Exceptions are still thrown in browser, so we don't lose information for that.
    //But all that is necessasry for UI is the message and status code.
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