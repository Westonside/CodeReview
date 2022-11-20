using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserService.Contract;

namespace UserService.Model.Dto.Pagination;

public class PaginationReqDto
{
    [FromQuery(Name = "_pageNum")]
    [Range(PaginationConst.MinPageNum, PaginationConst.MaxPageNum)]
    public int PageNum { get; set; } = PaginationConst.DefaultPageNum;

    [FromQuery(Name = "_pageSize")]
    [Range(PaginationConst.MinPageSize, PaginationConst.MaxPageSize)]
    public int PageSize { get; set; } = PaginationConst.DefaultPageSize;
}
