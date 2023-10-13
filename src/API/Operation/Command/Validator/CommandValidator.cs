using FluentValidation;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Command.Validator;

using DTO;
using Entity;
using Servitizing.Data.Store;

public class CommandValidator<TDto> : CommandValidatorBase<Command<TDto>> where TDto : DTO
{
    public CommandValidator(IServicer ultimateService) : base(ultimateService) { }

    protected void ValidationScope<T>(CommandMode commandMode, Action actions)
    {
        ValidationScope(typeof(T), commandMode, actions);
    }

    protected void ValidationScope(Type type, CommandMode commandMode, Action actions)
    {
        WhenAsync(
            (cmd, cancel) =>
                Task.Run(
                    () =>
                    {
                        return ((int)cmd.CommandMode & (int)commandMode) > 0
                            && GetType().UnderlyingSystemType.IsAssignableTo(type);
                    },
                    cancel
                ),
            actions
        );
    }

    protected void ValidationScope(CommandMode commandMode, Action actions)
    {
        WhenAsync(
            (cmd, cancel) =>
                Task.Run(
                    () =>
                    {
                        return ((int)cmd.CommandMode & (int)commandMode) > 0;
                    },
                    cancel
                ),
            actions
        );
    }

    protected void ValidateRequired(params Expression<Func<Command<TDto>, object>>[] members)
    {
        foreach (var member in members)
        {
            RuleFor(member)
                .NotEmpty()
                .WithMessage($"{member.Parameters.FirstOrDefault().Name} is required!");
        }
    }

    protected void ValidateEmail(params Expression<Func<Command<TDto>, string>>[] members)
    {
        foreach (var member in members)
        {
            RuleFor(member).EmailAddress().WithMessage($"Invalid email address.");
        }
    }

    protected void ValidateLength(
        int min,
        int max,
        params Expression<Func<Command<TDto>, string>>[] members
    )
    {
        foreach (var member in members)
        {
            RuleFor(member)
                .MinimumLength(min)
                .WithMessage($"minimum text length: {max} characters")
                .MaximumLength(max)
                .WithMessage($"maximum text length: {max} characters");
        }
    }

    protected void ValidateCount(
        int min,
        int max,
        params Expression<Func<Command<TDto>, ICollection>>[] members
    )
    {
        foreach (var member in members)
        {
            RuleFor(member)
                .Must(a => a.Count >= min)
                .WithMessage($"Items count below minimum quantity")
                .Must(a => a.Count <= max)
                .WithMessage($"Items count above maximum quantity");
        }
    }

    protected void ValidateEnum(params Expression<Func<Command<TDto>, string>>[] members)
    {
        foreach (var member in members)
        {
            RuleFor(member).IsInEnum().WithMessage($"Incorrect enum value");
        }
    }

    protected void ValidateNotEqual(
        object item,
        params Expression<Func<Command<TDto>, object>>[] members
    )
    {
        foreach (var member in members)
        {
            RuleFor(member).NotEqual(item).WithMessage($"value not equal: {item}");
        }
    }

    protected void ValidateEqual(
        object item,
        params Expression<Func<Command<TDto>, object>>[] members
    )
    {
        foreach (var member in members)
        {
            RuleFor(member).Equal(item).WithMessage($"value equal: {item}");
        }
    }

    protected void ValidateLanguage(params Expression<Func<Command<TDto>, object>>[] members)
    {
        foreach (var member in members)
        {
            RuleFor(member)
                .Must(SupportedLanguages.Contains)
                .WithMessage("Agreement language must conform to ISO 639-1.");
        }
    }

    protected void ValidateExist<TStore, TEntity>(
        Func<TDto, Expression<Func<TEntity, bool>>> command
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        var _repository = uservice.Use<TStore, TEntity>();

        RuleFor(e => e.Data)
            .MustAsync(
                async (cmd, cancel) =>
                {
                    return await _repository.Exist(command.Invoke((TDto)cmd));
                }
            )
            .WithMessage($"{typeof(TEntity).Name} does not exists");
    }

    protected void ValidateNotExist<TStore, TEntity>(
        Func<TDto, Expression<Func<TEntity, bool>>> command
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        var _reposiotry = uservice.Use<TStore, TEntity>();

        RuleFor(e => e.Data)
            .MustAsync(
                async (cmd, cancel) =>
                {
                    return await _reposiotry.NotExist(command.Invoke((TDto)cmd));
                }
            )
            .WithMessage($"{typeof(TEntity).Name} already exists");
    }
}
