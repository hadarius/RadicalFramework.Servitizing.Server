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

public class CommandSetValidator<TDto> : CommandSetValidatorBase<CommandSet<TDto>> where TDto : DTO
{
    public CommandSetValidator(IServicer ultimateService) : base(ultimateService) { }

    protected void ValidationScope<T>(CommandMode commandMode, Action validations)
    {
        ValidationScope(typeof(T), commandMode, validations);
    }

    protected void ValidationScope(Type type, CommandMode commandMode, Action validations)
    {
        WhenAsync(
            async (cmd, cancel) =>
                await Task.Run(
                    () =>
                        ((int)cmd.CommandMode & (int)commandMode) > 0
                        && GetType().UnderlyingSystemType.IsAssignableTo(type),
                    cancel
                ),
            validations
        );
    }

    protected void ValidationScope(CommandMode commandMode, Action validations)
    {
        WhenAsync(
            async (cmd, cancel) =>
                await Task.Run(() => ((int)cmd.CommandMode & (int)commandMode) > 0, cancel),
            validations
        );
    }

    protected override void ValidateLimit(int min, int max)
    {
        RuleFor(a => a)
            .Must(a => a.Count >= min)
            .WithMessage($"Items count below minimum quantity")
            .Must(a => a.Count <= max)
            .WithMessage($"Items count above maximum quantity");
    }

    protected void ValidateRequired(params Expression<Func<Command<TDto>, object>>[] members)
    {
        members
            .ForEach(
                member =>
                    RuleForEach(v => v)
                        .ChildRules(
                            a =>
                                a.RuleFor(member)
                                    .NotEmpty()
                                    .WithMessage(
                                        $"property {member.Parameters.LastOrDefault().Name} is required!"
                                    )
                        )
            )
            .ToArray();
    }

    protected void ValidateEmail(params Expression<Func<Command<TDto>, string>>[] members)
    {
        members
            .ForEach(
                member =>
                    RuleForEach(v => v)
                        .ChildRules(
                            a =>
                                a.RuleFor(member)
                                    .EmailAddress()
                                    .WithMessage($"Invalid email address.")
                        )
            )
            .ToArray();
    }

    protected void ValidateLength(
        int min,
        int max,
        params Expression<Func<Command<TDto>, object>>[] members
    )
    {
        members.ForEach(
            (member) =>
            {
                RuleForEach(v => v)
                    .ChildRules(
                        a =>
                            a.RuleFor(member)
                                .Must(
                                    (text) =>
                                    {
                                        if (text == null)
                                            return false;
                                        var length = text.ToString().Length;
                                        return !(length < min) || !(length > max);
                                    }
                                )
                                .WithMessage(
                                    $"text length above range limit min:{min} - max:{max} characters"
                                )
                    );
            }
        );
    }

    protected void ValidateCount(
        int min,
        int max,
        params Expression<Func<Command<TDto>, object>>[] members
    )
    {
        members.ForEach(
            (member) =>
            {
                RuleForEach(v => v)
                    .ChildRules(
                        a =>
                            a.RuleFor(member)
                                .Must(
                                    (collection) =>
                                    {
                                        var count = ((ICollection)collection).Count;
                                        return !(count < min) || !(count > max);
                                    }
                                )
                                .WithMessage(
                                    $"Items count above range limit min:{min} - max:{max} characters"
                                )
                    );
            }
        );
    }

    protected void ValidateEnum(params Expression<Func<Command<TDto>, string>>[] members)
    {
        members
            .ForEach(
                member =>
                    RuleForEach(v => v)
                        .ChildRules(
                            a => a.RuleFor(member).IsInEnum().WithMessage($"Incorrect enum value")
                        )
            )
            .ToArray();
    }

    protected void ValidateNotEqual(
        object item,
        params Expression<Func<Command<TDto>, object>>[] members
    )
    {
        members
            .ForEach(
                member =>
                    RuleForEach(v => v)
                        .ChildRules(
                            a =>
                                a.RuleFor(member)
                                    .NotEqual(item)
                                    .WithMessage($"value not equal: {item}")
                        )
            )
            .ToArray();
    }

    protected void ValidateEqual(
        object item,
        params Expression<Func<Command<TDto>, object>>[] members
    )
    {
        members
            .ForEach(
                member =>
                    RuleForEach(v => v)
                        .ChildRules(
                            a => a.RuleFor(member).Equal(item).WithMessage($"value equal: {item}")
                        )
            )
            .ToArray();
    }

    protected void ValidateLanguage(params Expression<Func<Command<TDto>, object>>[] members)
    {
        members
            .ForEach(
                member =>
                    RuleForEach(v => v)
                        .ChildRules(
                            a =>
                                a.RuleFor(member)
                                    .Must(SupportedLanguages.Contains)
                                    .WithMessage("Agreement language must conform to ISO 639-1.")
                        )
            )
            .ToArray();
    }

    protected void ValidateExist<TStore, TEntity>(
        Func<TDto, Expression<Func<TEntity, bool>>> command,
        string message = null
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        var _repository = _servicer.Use<TStore, TEntity>();
        RuleForEach(e => e)
            .MustAsync(async (cmd, cancel) => await _repository.Exist(command(cmd.Data)))
            .WithMessage(
                $"{typeof(TEntity).Name} does not exists " + message != null
                    ? "with " + message
                    : null
            );
    }

    protected void ValidateNotExist<TStore, TEntity>(
        Func<TDto, Expression<Func<TEntity, bool>>> command,
        string message = null
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        var _repository = _servicer.Use<TStore, TEntity>();
        RuleForEach(e => e)
            .MustAsync(async (cmd, cancel) => await _repository.NotExist(command(cmd.Data)))
            .WithMessage(
                $"{typeof(TEntity).Name} already exists " + message != null
                    ? "with " + message
                    : null
            );
    }
}
