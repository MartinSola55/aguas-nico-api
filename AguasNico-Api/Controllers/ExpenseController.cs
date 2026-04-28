using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO;
using AguasNico_Api.Models.DTO.Expenses;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AguasNico_Api.Controllers;

[Authorize(Policy = Policies.Admin)]
public class ExpenseController(ExpenseService expenseService) : BaseController
{
    private readonly ExpenseService _expenseService = expenseService;

    [HttpPost]
    public async Task<BaseResponse<GetExpensesResponse>> GetAll([FromBody] GetExpensesRequest rq) => await _expenseService.GetAll(rq);

    [HttpGet]
    public async Task<BaseResponse<GetExpenseResponse>> GetOne([FromQuery] GetExpenseRequest rq) => await _expenseService.GetOne(rq);

    [HttpPost]
    public async Task<BaseResponse<CreateExpenseResponse>> Create([FromBody] CreateExpenseRequest rq) => await _expenseService.Create(rq);

    [HttpPost]
    public async Task<BaseResponse<UpdateExpenseResponse>> Update([FromBody] UpdateExpenseRequest rq) => await _expenseService.Update(rq);

    [HttpPost]
    public async Task<BaseResponse> Delete([FromBody] DeleteExpenseRequest rq) => await _expenseService.Delete(rq);

    [HttpGet]
    public async Task<BaseResponse<SearchExpenseByDateResponse>> SearchByDate([FromQuery] SearchExpenseByDateRequest rq) => await _expenseService.SearchByDate(rq);
}
