using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace R3.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "GenerateZLogger";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "ZLOG001",
        title: "ZLoggerMessageAttribute annotated declared type must be partial",
        messageFormat: "The ZLoggerMessageAttribute annotated declared type '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NestedNotAllow = new(
        id: "ZLOG002",
        title: "ZLoggerMessageAttribute annotated declared type must not be nested type",
        messageFormat: "The ZLoggerMessageAttribute annotated declared type '{0}' must be not nested type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustBePartial = new(
        id: "ZLOG003",
        title: "ZLoggerMessage method must be partial",
        messageFormat: "The ZLoggerMessage method '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MessageTemplateParseFailed = new(
        id: "ZLOG004",
        title: "MessageTemplate is invalid",
        messageFormat: "MessageTemplate '{0}' is invalid, failed to parse",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustReturnVoid = new(
        id: "ZLOG005",
        title: "Method return type must be void",
        messageFormat: "The ZLoggerMessage method '{0}' return type must be void",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenericNotSupported = new(
        id: "ZLOG006",
        title: "Generic method is not supported",
        messageFormat: "The ZLoggerMessage method '{0}' is generic, define in generic is not supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor LogLevelNotFound = new(
        id: "ZLOG007",
        title: "LogLevel is not found in attribtue or parameter",
        messageFormat: "The ZLoggerMessage method '{0}' has no LogLevel in attribute or parameter",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingLogger = new(
        id: "ZLOG008",
        title: "ILogger is not found in parameters",
        messageFormat: "The ZLoggerMessage method '{0}' has no ILogger in parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TemplateHasNoCorrespondingArgument = new(
        id: "ZLOG009",
        title: "Template parameter must match argument",
        messageFormat: "The ZLoggerMessage method '{0}' template paramter '{1}' has no corresponding argument in method parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ArgumentHasNoCorrespondingTemplate = new(
        id: "ZLOG010",
        title: "Argument must match template parameter",
        messageFormat: "The ZLoggerMessage method '{0}' argument '{1}' has no corresponding parameter in template",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RefKindNotSupported = new(
        id: "ZLOG011",
        title: "Argument must match template parameter",
        messageFormat: "The ZLoggerMessage method '{0}' argument '{1}' has no corresponding parameter in template",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenericTypeNotSupported = new(
        id: "ZLOG012",
        title: "Generic type is not supported",
        messageFormat: "The ZLoggerMessageAttribute annotated declared type '{0}' is generic, define in generic is not supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateEventIdIsNotAllowed = new(
        id: "ZLOG013",
        title: "Duplicate EventId is not allowed",
        messageFormat: "The ZLoggerMessage method '{0}' EventId '{1}' is duplicated",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
