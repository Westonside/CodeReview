namespace UserService.Model.Dto.Pagination;

public struct PaginationResDto<T>
{
    public T List { get; set; }

    public PaginationMeta Meta { get; set; }
}

public struct PaginationMeta
{
    public long Total     { get; set; }
    public int  PageNum   { get; set; }
    public int  PageSize  { get; set; }
    public int  PageTotal { get; set; }
}
