using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using Entity;
using Event;

public abstract class CommandBase : ICommand
{
    private Entity entity;

    public virtual long Id { get; set; }

    public object[] Keys { get; set; }

    [JsonIgnore]
    public virtual Entity Entity
    {
        get => entity;
        set
        {
            entity = value;
            if (Id == 0 && entity.Id != 0)
                Id = entity.Id;
        }
    }

    [JsonIgnore]
    public virtual object Data { get; set; }

    [JsonIgnore]
    public ValidationResult Result { get; set; }

    public string ErrorMessages => Result.ToString();

    public CommandMode CommandMode { get; set; }

    public EventPublishMode PublishMode { get; set; }

    public virtual object Input => Data;

    public virtual object Output => IsValid ? Id : ErrorMessages;

    public bool IsValid => Result.IsValid;

    protected CommandBase()
    {
        Result = new ValidationResult();
    }

    protected CommandBase(CommandMode commandMode, EventPublishMode publishMode) : this()
    {
        CommandMode = commandMode;
        PublishMode = publishMode;
    }

    protected CommandBase(object entryData, CommandMode commandMode, EventPublishMode publishMode)
        : this(commandMode, publishMode)
    {
        Data = entryData;
    }

    protected CommandBase(
        object entryData,
        CommandMode commandMode,
        EventPublishMode publishMode,
        params object[] keys
    ) : this(commandMode, publishMode, keys)
    {
        Data = entryData;
    }

    protected CommandBase(
        CommandMode commandMode,
        EventPublishMode publishMode,
        params object[] keys
    ) : this(commandMode, publishMode)
    {
        Keys = keys;
    }
}
