using FluentValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Command.Validator;

using Entity;
using Servitizing.Data.Store;
using Instant.Proxies;

public abstract class CommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICommand
{
    protected static readonly string[] SupportedLanguages;

    protected readonly IServicer uservice;

    static CommandValidatorBase()
    {
        SupportedLanguages = CultureInfo
            .GetCultures(CultureTypes.NeutralCultures)
            .Select(c => c.TwoLetterISOLanguageName)
            .Distinct()
            .ToArray();
    }

    public CommandValidatorBase(IServicer ultimateService)
    {
        uservice = ultimateService;
    }

    protected void ValidateRequired(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleFor(a => a.Data.ValueOf(propertyName))
                .NotEmpty()
                .WithMessage(a => $"{propertyName} is required!");
        }
    }

    protected void ValidateLanguage(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleFor(a => a.Data.ValueOf(propertyName))
                .Must(SupportedLanguages.Contains)
                .WithMessage("Language must conform to ISO 639-1.");
        }
    }

    protected void ValidateNotEqual(object item, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleFor(e => e.Data.ValueOf(propertyName))
                .NotEqual(item)
                .WithMessage($"{propertyName} is not equal: {item}");
        }
    }

    protected void ValidateEqual(object item, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleFor(e => e.Data.ValueOf(propertyName))
                .Equal(item)
                .WithMessage($"{propertyName} is equal: {item}");
        }
    }

    protected void ValidateLength(int min, int max, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleFor(a => a.Data.ValueOf(propertyName).ToString())
                .MinimumLength(min)
                .WithMessage($"{propertyName} minimum text length: {max} characters")
                .MaximumLength(max)
                .WithMessage($"{propertyName} maximum text length: {max} characters");
        }
    }

    protected void ValidateEnum(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            RuleFor(e => e.Data.ValueOf(propertyName))
                .IsInEnum()
                .WithMessage($"Incorrect {propertyName} number");
        }
    }

    protected void ValidateEmail(params string[] emailPropertyNames)
    {
        foreach (string emailPropertyName in emailPropertyNames)
        {
            RuleFor(a => a.Data.ValueOf(emailPropertyName).ToString())
                .EmailAddress()
                .When(a => !string.IsNullOrEmpty(a.Data.ValueOf(emailPropertyName).ToString()))
                .WithMessage($"Invalid {emailPropertyName} address.");
        }
    }

    protected void ValidateExist<TStore, TEntity>(
        LogicOperand operand,
        params string[] propertyNames
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        RuleFor(e => e)
            .MustAsync(
                async (cmd, cancel) =>
                {
                    return await uservice
                        .Use<TStore, TEntity>()
                        .Exist(
                            buildPredicate<TEntity>((IInnerProxy)cmd.Data, operand, propertyNames)
                        );
                }
            )
            .WithMessage($"{typeof(TEntity).Name} already exists");
    }

    protected void ValidateNotExist<TStore, TEntity>(
        LogicOperand operand,
        params string[] propertyNames
    )
        where TEntity : Entity
        where TStore : IDatabaseStore
    {
        RuleFor(e => e)
            .MustAsync(
                async (cmd, cancel) =>
                {
                    return await uservice
                        .Use<TStore, TEntity>()
                        .NotExist(
                            buildPredicate<TEntity>((IInnerProxy)cmd.Data, operand, propertyNames)
                        );
                }
            )
            .WithMessage($"{typeof(TEntity).Name} does not exists");
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
