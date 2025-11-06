public class UserQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Id";
    public bool SortDescending { get; set; } = false;
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
}
