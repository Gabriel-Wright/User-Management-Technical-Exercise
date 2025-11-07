namespace UserManagement.Services.Domain;


public class UserAuditQuery
{
    public string? SearchTerm { get; set; } //Search by fullname
    public AuditAction? Action { get; set; } //Filter by action of query i.e. created, edited, deleted
    public int Page { get; set; } = 1; //pagination: page number
    public int PageSize { get; set; } = 10; //pagination: items per page
    public string SortBy { get; set; } = ""; //column to sort by - probably will just use id for now
    public bool SortDescending { get; set; } = false;
}
