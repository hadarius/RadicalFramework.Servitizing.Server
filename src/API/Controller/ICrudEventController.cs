using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Controller;

using DTO;

public interface ICrudEventController<TKey, TEntity, TDto> where TDto : DTO
{
    Task<IActionResult> Count();
    Task<IActionResult> Delete([FromBody] TDto[] dtos);
    Task<IActionResult> Delete([FromRoute] TKey key, [FromBody] TDto dto);
    Task<IActionResult> Get();
    Task<IActionResult> Get(int offset, int limit);
    Task<IActionResult> Get(TKey key);
    Task<IActionResult> Patch([FromBody] TDto[] dtos);
    Task<IActionResult> Patch([FromRoute] TKey key, [FromBody] TDto dto);
    Task<IActionResult> Post(int offset, int limit, QueryDTO query);
    Task<IActionResult> Post([FromBody] TDto[] dtos);
    Task<IActionResult> Post([FromRoute] TKey key, [FromBody] TDto dto);
    Task<IActionResult> Put([FromBody] TDto[] dtos);
    Task<IActionResult> Put([FromRoute] TKey key, [FromBody] TDto dto);
}