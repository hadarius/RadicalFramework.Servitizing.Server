namespace Radical.Servitizing.Server.API.Operation.Command.Validator;

using DTO;

public class CommandSetStreamValidator<TDto> : CommandSetValidator<TDto> where TDto : DTO
{
    public CommandSetStreamValidator(IServicer servicer) : base(servicer) { }
}