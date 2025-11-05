public class UserQuery
{
    public string? SearchTerm { get; set; } //Search by fullname
    public bool? IsActive { get; set; } //Filter by active or not
    public int Page { get; set; } = 1; //pagination: page number
    public int PageSize { get; set; } = 10; //pagination: items per page
    public string SortBy { get; set; } = "Id"; //column to sort by - probably will just use id for now
    public bool SortDescending { get; set; } = false;
}