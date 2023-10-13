using FluentValidation.Results;

namespace Radical.Servitizing.Server.API.Operation.Command;

using Entity;

public interface ICommand : IOperation
{
    long Id { get; set; }

    object[] Keys { get; set; }

    Entity Entity { get; set; }

    object Data { get; set; }

    ValidationResult Result { get; set; }

    bool IsValid { get; }
}
