using FluentValidation.Results;
using System.Collections.Generic;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;

public interface ICommandSet<TDto> : ICommandSet where TDto : DTO
{
    public new IEnumerable<Command<TDto>> Commands { get; }
}

public interface ICommandSet : IOperation
{
    public IEnumerable<ICommand> Commands { get; }

    public ValidationResult Result { get; set; }
}
