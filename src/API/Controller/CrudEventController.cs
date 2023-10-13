using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Controller;

using DTO;
using Entity;
using Event;
using Operation.Command;
using Operation.Query;
using Servitizing.Data.Store;

[Route($"{StoreRoutes.Constant.CrudEventStore}/Events")]
public abstract class CrudEventController<TKey, TStore, TEntity, TDto> : ControllerBase
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected Func<TKey, Func<TDto, object>> _keysetter = k => e => e.SetId(k);
    protected Func<TKey, Expression<Func<TEntity, bool>>> _keymatcher = k =>
        e => k.Equals(e.Id);
    protected Func<TDto, Expression<Func<TEntity, bool>>> _predicate;
    protected readonly IServicer _servicer;
    protected readonly EventPublishMode _publishMode;

    protected CrudEventController(IServicer servicer)
    {
        _servicer = servicer;
    }

    protected CrudEventController(
        IServicer servicer,
        Func<TKey, Expression<Func<TEntity, bool>>> keymatcher
    )
    {
        _servicer = servicer;
        _keymatcher = keymatcher;
    }

    [HttpGet]
    public virtual async Task<IActionResult> Get()
    {
        return Ok(
            await _servicer.Send(new Get<TStore, TEntity, TDto>(0, 0)).ConfigureAwait(true)
        );
    }

    [HttpGet("{key}")]
    public virtual async Task<IActionResult> Get(TKey key)
    {
        Task<TDto> query =
            _keymatcher == null
                ? _servicer.Send(new Find<TStore, TEntity, TDto>(key))
                : _servicer.Send(new Find<TStore, TEntity, TDto>(_keymatcher(key)));

        return Ok(await query.ConfigureAwait(false));
    }

    [HttpGet("{offset}/{limit}")]
    public virtual async Task<IActionResult> Get(int offset, int limit)
    {
        return Ok(
            await _servicer
                .Send(new Get<TStore, TEntity, TDto>(offset, limit))
                .ConfigureAwait(true)
        );
    }

    [HttpPost]
    public virtual async Task<IActionResult> Post([FromBody] TDto[] dtos)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _servicer
            .Send(new CreateSet<TStore, TEntity, TDto>(_publishMode, dtos))
            .ConfigureAwait(false);

        object[] response = result
            .ForEach(c => (isValid = c.IsValid) ? c.Id as object : c.ErrorMessages)
            .ToArray();
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }

    [HttpPost("query/{offset}/{limit}")]
    public virtual async Task<IActionResult> Post(int offset, int limit, QueryDTO query)
    {
        query.Filter.ForEach(
            (fi) =>
                fi.Value = JsonSerializer.Deserialize(
                    ((JsonElement)fi.Value).GetRawText(),
                    Type.GetType($"System.{fi.Type}", null, null, false, true)
                )
        );

        return Ok(
            await _servicer
                .Send(
                    new Filter<TStore, TEntity, TDto>(
                        offset,
                        limit,
                        new EntityFilterExpression<TEntity>(query.Filter).Create(),
                        new EntitySortExpression<TEntity>(query.Sort)
                    )
                )
                .ConfigureAwait(false)
        );
    }

    [HttpPost("{key}")]
    public virtual async Task<IActionResult> Post(TKey key, [FromBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _keysetter(key).Invoke(dto);

        var result = await _servicer
            .Send(new CreateSet<TStore, TEntity, TDto>(_publishMode, new[] { dto }))
            .ConfigureAwait(false);

        var response = result
            .ForEach(c => (isValid = c.IsValid) ? c.Id as object : c.ErrorMessages)
            .ToArray();
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }

    [HttpPatch]
    public virtual async Task<IActionResult> Patch([FromBody] TDto[] dtos)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        CommandSet<TDto> result = await _servicer
            .Send(new ChangeSet<TStore, TEntity, TDto>(EventPublishMode.PropagateCommand, dtos))
            .ConfigureAwait(false);

        object[] response = result
            .ForEach(c => (isValid = c.IsValid) ? c.Id as object : c.ErrorMessages)
            .ToArray();
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }

    [HttpPatch("{key}")]
    public virtual async Task<IActionResult> Patch(TKey key, [FromBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Command<TDto> result = await _servicer
            .Send(new Change<TStore, TEntity, TDto>(EventPublishMode.PropagateCommand, dto, key))
            .ConfigureAwait(false);

        object response = result.IsValid ? result.Id as object : result.ErrorMessages;
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }

    [HttpPut]
    public virtual async Task<IActionResult> Put([FromBody] TDto[] dtos)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _servicer.Send(new UpdateSet<TStore, TEntity, TDto>
                                                                    (_publishMode, dtos, _predicate))
                                                                                .ConfigureAwait(false);

        var response = result.ForEach(c => (isValid = c.IsValid) ? c.Id as object : c.ErrorMessages)
            .ToArray();
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }

    [HttpPut("{key}")]
    public virtual async Task<IActionResult> Put(TKey key, [FromBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _keysetter(key).Invoke(dto);

        var result = await _servicer.Send(new UpdateSet<TStore, TEntity, TDto>
                                                    (_publishMode, new[] { dto }, _predicate))
                                                        .ConfigureAwait(false);

        var response = result.ForEach(c => (isValid = c.IsValid)
                                              ? c.Id as object
                                              : c.ErrorMessages).ToArray();
        return !isValid
               ? UnprocessableEntity(response)
               : Ok(response);
    }

    [HttpDelete]
    public virtual async Task<IActionResult> Delete([FromBody] TDto[] dtos)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        CommandSet<TDto> result = await _servicer
            .Send(new DeleteSet<TStore, TEntity, TDto>(EventPublishMode.PropagateCommand, dtos))
            .ConfigureAwait(false);

        object[] response = result
            .ForEach(c => (isValid = c.IsValid) ? c.Id as object : c.ErrorMessages)
            .ToArray();
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }

    [HttpDelete("{key}")]
    public virtual async Task<IActionResult> Delete(TKey key, [FromBody] TDto dto)
    {
        bool isValid = false;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _keysetter(key).Invoke(dto);

        var result = await _servicer
            .Send(
                new DeleteSet<TStore, TEntity, TDto>(
                    EventPublishMode.PropagateCommand,
                    new[] { dto }
                )
            )
            .ConfigureAwait(false);

        var response = result
            .ForEach(c => (isValid = c.IsValid) ? c.Id as object : c.ErrorMessages)
            .ToArray();
        return !isValid ? UnprocessableEntity(response) : Ok(response);
    }


}
