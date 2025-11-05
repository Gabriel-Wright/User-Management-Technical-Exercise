namespace UserManagement.UI.Exceptions
{

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