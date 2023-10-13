using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Controller;

using DTO;

[ServiceContract]
public interface IStreamEventController<TDto> where TDto : DTO
{
    Task<int> Count();
    IAsyncEnumerable<TDto> All();
    IAsyncEnumerable<TDto> Range(int offset, int limit);
    IAsyncEnumerable<TDto> Query(int offset, int limit, QueryDTO query);
    IAsyncEnumerable<string> Creates(TDto[] dtos);
    IAsyncEnumerable<string> Changes(TDto[] dtos);
    IAsyncEnumerable<string> Updates(TDto[] dtos);
    IAsyncEnumerable<string> Deletes(TDto[] dtos);
}