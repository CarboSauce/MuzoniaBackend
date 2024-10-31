namespace Muzonia.Core.Test;

public class ResultMethods
{
    [Fact]
    public void MatchingShouldWork_OnValueCase()
    {
        var res = new Res<int, string>(1);

        int val = res switch
        {
            (var a, null) => a ?? 0,
            (_, var a) => int.Parse(a ?? "0"),
        };
    }
}
