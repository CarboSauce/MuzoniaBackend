namespace Muzonia.Api.Common;

public record struct ReplaceInput<T>(bool Replace, T Value);
