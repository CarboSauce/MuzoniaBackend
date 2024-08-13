using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Muzonia.Utils;

namespace Utils.Test;

public class ResultMethods
{
    // Todo
    [Fact]
    public void MatchingShouldWork_OnValueCase()
    {
        Assert.True(false);
        var res = new Res<int, string>(1);

        int val = res switch
        {
            (var a, null) => a ?? 0,
            (_, var a) => int.Parse(a ?? "0"),
        };
    }
}
