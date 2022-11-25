namespace UserService.Model.Dto.Pagination;

public static class PaginationResMaker
{
    public static PaginationResDto<T> Make<T>(T list, long total, int pageNum, int pageSize) =>
        new PaginationResDto<T>
        {
            List = list,
            Meta = new PaginationMeta
            {
                Total     = total,
                PageNum   = pageNum,
                PageSize  = pageSize,
                PageTotal = Convert.ToInt32(Math.Ceiling((double)total / pageSize))
            }
        };
}
