using System.Reflection;
using Castle.Components.DictionaryAdapter.Xml;
using CookieCrumble;
using CookieCrumble.Formatters;
using CookieCrumble.HotChocolate;
using TUnit.Core.Exceptions;

namespace Muzonia.Api.IntegrationTest;

public class CookieCrumbleFramework : ITestFramework
{
    public bool IsValidTestMethod(MemberInfo? method)
    {
        return method?.GetCustomAttributes(typeof(TestAttribute)).Any()
            ?? false;
    }

    public void ThrowTestException(string message)
    {
        throw new TUnitException(message);
    }
}

public sealed class CookieCrumbleModule : SnapshotModule
{
    protected override ITestFramework? TryCreateTestFramework()
    {
        return new CookieCrumbleFramework();
    }
}
