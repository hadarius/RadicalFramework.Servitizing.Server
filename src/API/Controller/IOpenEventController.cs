using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using System.Linq;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Controller;

using Uniques;
using DTO;

public interface IOpenEventController<TKey, TEntity, TDto> where TDto : DTO
{
    Task<IActionResult> Delete([FromODataUri] TKey key);
    IQueryable<TDto> Get();
    Task<UniqueOne<TDto>> Get([FromODataUri] TKey key);
    Task<IActionResult> Patch([FromODataUri] TKey key, [FromODataBody] TDto dto);
    Task<IActionResult> Post([FromODataBody] TDto dto);
    Task<IActionResult> Put([FromODataUri] TKey key, [FromODataBody] TDto dto);
}