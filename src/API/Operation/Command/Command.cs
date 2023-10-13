using MediatR;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Event;
using Uniques;

public class Command<TDto> : CommandBase, IRequest<Command<TDto>>, IUnique where TDto : DTO
{
    [JsonIgnore]
    public override TDto Data => base.Data as TDto;

    protected Command() { }

    protected Command(CommandMode commandMode, TDto dataObject)
    {
        CommandMode = commandMode;
        base.Data = dataObject;
    }

    protected Command(CommandMode commandMode, EventPublishMode publishMode, TDto dataObject)
        : base(dataObject, commandMode, publishMode) { }

    protected Command(
        CommandMode commandMode,
        EventPublishMode publishMode,
        TDto dataObject,
        params object[] keys
    ) : base(dataObject, commandMode, publishMode, keys) { }

    protected Command(CommandMode commandMode, EventPublishMode publishMode, params object[] keys)
        : base(commandMode, publishMode, keys) { }

    public byte[] GetBytes()
    {
        return Data.GetBytes();
    }

    public byte[] GetKeyBytes()
    {
        return Data.GetKeyBytes();
    }

    public bool Equals(IUnique other)
    {
        return Data.Equals(other);
    }

    public int CompareTo(IUnique other)
    {
        return Data.CompareTo(other);
    }

    public override long Id
    {
        get => Data.Id;
        set => Data.Id = value;
    }

    public ulong Key
    {
        get => Data.Key;
        set => Data.Key = value;
    }

    public ulong TypeKey
    {
        get => Data.TypeKey;
        set => Data.TypeKey = value;
    }
}
