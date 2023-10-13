using MediatR;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Event;

public class CommandSetStream<TDto>
    : CommandSet<TDto>,
        IStreamRequest<Command<TDto>>,
        ICommandSet<TDto> where TDto : DTO
{
    protected CommandSetStream() : base() { }

    protected CommandSetStream(CommandMode commandMode) : base(commandMode) { }

    protected CommandSetStream(
        CommandMode commandMode,
        EventPublishMode publishPattern,
        Command<TDto>[] DtoCommands
    ) : base(commandMode, publishPattern, DtoCommands) { }
}
