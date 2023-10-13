using FluentValidation.Results;
using System.Collections.Generic;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;

public interface ICommandSetStream<TDto> : ICommandSet where TDto : DTO
{
    public new IAsyncEnumerable<Command<TDto>> Commands { get; }
}
