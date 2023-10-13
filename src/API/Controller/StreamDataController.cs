using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Controller;

using Data.Service;
using DTO;
using Entity;
using Operation.Command;
using Operation.Query;
using Repository.Client.Linked;
using Servitizing.Data.Store;
using Servitizing.Event;

[LinkedResult]
[StreamDataService]
public class StreamDataController<TKey, TEntry, TReport, TEntity, TDto> : IStreamDataController<TDto> where TDto : DTO
    where TEntity : Entity
    where TEntry : IDatabaseStore
    where TReport : IDatabaseStore
{
    protected Func<TKey, Func<TDto, object>> _keysetter = k => e => e.SetId(k);
    protected Func<TKey, Expression<Func<TEntity, bool>>> _keymatcher;
    protected Func<TDto, Expression<Func<TEntity, bool>>> _predicate;
    protected readonly IServicer _servicer;
    protected readonly EventPublishMode _publishMode;

    public StreamDataController() : this(new Servicer(), null, k => e => e.SetId(k), null, EventPublishMode.PropagateCommand) { }

    public StreamDataController(IServicer servicer,
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

    public virtual IAsyncEnumerable<TDto> All()
    {
        return _servicer.CreateStream(new GetAsync<TReport, TEntity, TDto>(0, 0));
    }

    public virtual async Task<int> Count()
    {
        return await Task.Run(() => _servicer.use<TReport, TEntity>().Count());
    }

    public virtual IAsyncEnumerable<TDto> Range(int offset, int limit)
    {
        return _servicer.CreateStream(new GetAsync<TReport, TEntity, TDto>(offset, limit));
    }

    public virtual IAsyncEnumerable<TDto> Query(int offset, int limit, QueryDTO query)
    {
        query.Filter.ForEach(
            (fi) =>
                fi.Value = JsonSerializer.Deserialize(
                    ((JsonElement)fi.Value).GetRawText(),
                    Type.GetType($"System.{fi.Type}", null, null, false, true)
                )
        );

        return
            _servicer
                .CreateStream(
                    new FilterAsync<TReport, TEntity, TDto>(offset, limit,
                        new EntityFilterExpression<TEntity>(query.Filter).Create(),
                        new EntitySortExpression<TEntity>(query.Sort)
                    )
                );
    }

    public virtual IAsyncEnumerable<string> Creates([FromBody] TDto[] dtos)
    {
        var result = _servicer.CreateStream(new CreateSetAsync<TEntry, TEntity, TDto>
                                                    (_publishMode, dtos));

        var response = result.ForEachAsync(c => c.IsValid
                                               ? c.Id.ToString()
                                               : c.ErrorMessages);
        return response;
    }

    public virtual IAsyncEnumerable<string> Changes([FromBody] TDto[] dtos)
    {
        var result = _servicer.CreateStream(new ChangeSetAsync<TEntry, TEntity, TDto>
                                                   (_publishMode, dtos));

        var response = result.ForEachAsync(c => c.IsValid
                                              ? c.Id.ToString()
                                              : c.ErrorMessages);
        return response;
    }

    public virtual IAsyncEnumerable<string> Updates([FromBody] TDto[] dtos)
    {
        var result = _servicer.CreateStream(new UpdateSetAsync<TEntry, TEntity, TDto>
                                                 (_publishMode, dtos));

        var response = result.ForEachAsync(c => c.IsValid
                                             ? c.Id.ToString()
                                             : c.ErrorMessages);
        return response;
    }

    public virtual IAsyncEnumerable<string> Deletes([FromBody] TDto[] dtos)
    {
        var result = _servicer.CreateStream(new DeleteSetAsync<TEntry, TEntity, TDto>
                                                  (_publishMode, dtos));

        var response = result.ForEachAsync(c => c.IsValid
                                             ? c.Id.ToString()
                                             : c.ErrorMessages);
        return response;
    }
}
