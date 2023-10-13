using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Controller;

using Data.Service;
using DTO;
using Entity;
using Operation.Command;
using Operation.Query;
using Radical.Uniques;
using Repository.Client.Linked;
using Servitizing.Data.Store;
using Servitizing.Event;

//[IgnoreApi]
[LinkedResult]
[OpenDataService]
public class OpenDataController<TKey, TEntry, TReport, TEntity, TDto>
    : ODataController, IOpenEventController<TKey, TEntity, TDto> where TDto : DTO
    where TEntity : Entity
    where TEntry : IDatabaseStore
    where TReport : IDatabaseStore
{
    protected Func<TKey, Func<TDto, object>> _keysetter = k => e => e.SetId(k);
    protected Func<TKey, Expression<Func<TEntity, bool>>> _keymatcher = k => e => k.Equals(e.Id);
    protected Func<TDto, Expression<Func<TEntity, bool>>> _predicate;
    protected readonly IServicer _servicer;
    protected readonly EventPublishMode _publishMode;

    protected OpenDataController() { }

    protected OpenDataController(
        IServicer servicer,
        EventPublishMode publishMode = EventPublishMode.PropagateCommand
    ) : this(servicer, null, k => e => e.SetId(k), k => e => k.Equals(e.Id), publishMode) { }

    protected OpenDataController(
        IServicer servicer,
        Func<TDto, Expression<Func<TEntity, bool>>> predicate,
        Func<TKey, Func<TDto, object>> keysetter,
        Func<TKey, Expression<Func<TEntity, bool>>> keymatcher,
        EventPublishMode publishMode = EventPublishMode.PropagateCommand
    )
    {
        _keymatcher = keymatcher;
        _keysetter = keysetter;
        _servicer = servicer;
        _publishMode = publishMode;
    }

    [HttpGet]
    [EnableQuery]
    public virtual IQueryable<TDto> Get()
    {
        return _servicer.Send(new GetQuery<TReport, TEntity, TDto>()).Result;
    }

    [HttpGet]
    [EnableQuery]
    public virtual async Task<UniqueOne<TDto>> Get([FromODataUri] TKey key)
    {
        return new UniqueOne<TDto>(await _servicer.Send(new FindQuery<TReport, TEntity, TDto>(_keymatcher(key))));
    }

    [HttpPost]
    public virtual async Task<IActionResult> Post([FromODataBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _servicer.Send(new CreateSet<TEntry, TEntity, TDto>
                                                (_publishMode, new[] { dto }))
                                                    .ConfigureAwait(false);

        var response = result.ForEach(c => (isValid = c.IsValid)
                                              ? c.Id as object
                                              : c.ErrorMessages).ToArray();
        return !isValid
               ? UnprocessableEntity(response)
               : Created(response);
    }

    [HttpPatch]
    public virtual async Task<IActionResult> Patch([FromODataUri] TKey key, [FromODataBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid) return BadRequest(ModelState);

        _keysetter(key).Invoke(dto);

        var result = await _servicer.Send(new ChangeSet<TEntry, TEntity, TDto>
                                              (_publishMode, new[] { dto }, _predicate))
                                                 .ConfigureAwait(false);

        var response = result.ForEach(c => (isValid = c.IsValid)
                                              ? c.Id as object
                                              : c.ErrorMessages).ToArray();
        return !isValid
               ? UnprocessableEntity(response)
               : Updated(response);
    }

    [HttpPut]
    public virtual async Task<IActionResult> Put([FromODataUri] TKey key, [FromODataBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _keysetter(key).Invoke(dto);

        var result = await _servicer.Send(new UpdateSet<TEntry, TEntity, TDto>
                                                    (_publishMode, new[] { dto }, _predicate))
                                                        .ConfigureAwait(false);

        var response = result.ForEach(c => (isValid = c.IsValid)
                                              ? c.Id as object
                                              : c.ErrorMessages).ToArray();
        return !isValid
               ? UnprocessableEntity(response)
               : Updated(response);
    }

    [HttpDelete]
    public virtual async Task<IActionResult> Delete([FromODataUri] TKey key)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _servicer.Send(new DeleteSet<TEntry, TEntity, TDto>
                                                             (_publishMode, key))
                                                                    .ConfigureAwait(false);

        var response = result.ForEach(c => (isValid = c.IsValid)
                                               ? c.Id as object
                                               : c.ErrorMessages).ToArray();
        return !isValid
               ? UnprocessableEntity(response)
               : Ok(response);
    }
}
