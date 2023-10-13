using FluentValidation;
using Radical.Servitizing.Data.Store;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Command.Validator;

using Instant.Proxies;
using Entity;

public abstract class CommandSetValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICommandSet
{
    protected static readonly string[] SupportedLanguages;

    protected readonly IServicer _servicer;

    static CommandSetValidatorBase()
    {
        SupportedLanguages = CultureInfo
            .GetCultures(CultureTypes.NeutralCultures)
            .Select(c => c.TwoLetterISOLanguageName)
            .Distinct()
            .ToArray();
    }

    public CommandSetValidatorBase(IServicer servicer)
    {
        _servicer = servicer;
    }

    protected virtual void ValidateLimit(int min, int max)
    {
        RuleFor(a => a)
            .Must(a => a.Commands.Count() >= min)
            .WithMessage($"Items count below minimum quantity")
            .Must(a => a.Commands.Count() <= max)
            .WithMessage($"Items count above maximum quantity");
    }

    protected void ValidateRequired(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(propertyName))
                            .NotEmpty()
                            .WithMessage(a => $"{propertyName} is required!")
                );
        }
    }

    protected void ValidateLanguage(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(propertyName))
                            .Must(SupportedLanguages.Contains)
                            .WithMessage("Agreement language must conform to ISO 639-1.")
                );
        }
    }

    protected void ValidateEqual(object item, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(propertyName))
                            .Equal(item)
                            .WithMessage($"{propertyName} is equal: {item}")
                );
        }
    }

    protected void ValidateNotEqual(object item, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(propertyName))
                            .NotEqual(item)
                            .WithMessage($"{propertyName} is not equal: {item}")
                );
        }
    }

    protected void ValidateLength(int min, int max, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(propertyName).ToString())
                            .MinimumLength(min)
                            .WithMessage($"{propertyName} minimum text length: {max} characters")
                            .MaximumLength(max)
                            .WithMessage($"{propertyName} maximum text length: {max} characters")
                );
        }
    }

    protected void ValidateEnum(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(propertyName))
                            .IsInEnum()
                            .WithMessage($"Incorrect {propertyName} number")
                );
        }
    }

    protected void ValidateEmail(params string[] emailPropertyNames)
    {
        foreach (string emailPropertyName in emailPropertyNames)
        {
            RuleForEach(a => a.Commands)
                .ChildRules(
                    c =>
                        c.RuleFor(r => r.Data.ValueOf(emailPropertyName).ToString())
                            .EmailAddress()
                            .WithMessage($"Invalid {emailPropertyName} address.")
                );
        }
    }

    protected void ValidateExist<TStore, TEntity>(
        LogicOperand operand,
        params string[] propertyNames
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        var _repository = _servicer.Use<TStore, TEntity>();

        RuleForEach(a => a.Commands)
            .ChildRules(
                c =>
                    c.RuleFor(r => r.Data)
                        .MustAsync(
                            async (cmd, cancel) =>
                            {
                                return await _repository.Exist(
                                    buildPredicate<TEntity>(
                                        (IInnerProxy)cmd,
                                        operand,
                                        propertyNames
                                    )
                                );
                            }
                        )
                        .WithMessage($"{typeof(TEntity).Name} already exists")
            );
    }

    protected void ValidateNotExist<TStore, TEntity>(
        LogicOperand operand,
        params string[] propertyNames
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        var _repository = _servicer.Use<TStore, TEntity>();

        RuleForEach(a => a.Commands)
            .ChildRules(
                c =>
                    c.RuleFor(r => r.Data)
                        .MustAsync(
                            async (cmd, cancel) =>
                            {
                                return await _repository.NotExist(
                                    buildPredicate<TEntity>(
                                        (IInnerProxy)cmd,
                                        operand,
                                        propertyNames
                                    )
                                );
                            }
                        )
                        .WithMessage($"{typeof(TEntity).Name} does not exists")
            );
    }

    private Expression<Func<TEntity, bool>> buildPredicate<TEntity>(
        IInnerProxy dataInput,
        LogicOperand operand,
        params string[] propertyNames
    ) where TEntity : IInnerProxy
    {
        Expression<Func<TEntity, bool>> predicate =
            operand == LogicOperand.And ? predicate = e => true : predicate = e => false;
        foreach (var item in propertyNames)
        {
            predicate =
                operand == LogicOperand.And
                    ? predicate.And(e => e[item] == dataInput[item])
                    : predicate.Or(e => e[item] == dataInput[item]);
        }
        return predicate;
    }
}
