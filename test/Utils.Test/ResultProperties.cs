using Muzonia.Utils;

namespace Utils.Test;

public class ResultProperties
{
    [Fact]
    public void BasicPropertiesShouldWork_ForValueCase()
    {
        var res = new Res<int, string>(1);

        Assert.True(res.IsVal);
        Assert.False(res.IsErr);
        Assert.Equal(1, res.Val);
        Assert.Equal(1, res.ValOrDefault);
        Assert.Null(res.ErrOrDefault);
        var ex = Assert.Throws<NoErrorException<int>>(() =>
        {
            _ = res.Err;
        });
        Assert.Equal(1, ex.Val);
        Assert.Equal(1, ex.Inner);
    }

    [Fact]
    public void BasicPropertiesShouldWork_ForErrorCase()
    {
        var res = new Res<int, string>("0");

        Assert.False(res.IsVal);
        Assert.True(res.IsErr);
        Assert.Equal("0", res.Err);
        Assert.Equal("0", res.ErrOrDefault);
        Assert.Equal(default, res.ValOrDefault);
        var ex = Assert.Throws<NoValueException<string>>(() =>
        {
            _ = res.Val;
        });
        Assert.Equal("0", ex.Err);
        Assert.Equal("0", ex.Inner);
    }

    [Fact]
    public void BasicPropertiesShouldWork_ForValueCase_WithSameTandE()
    {
        var res = Res<int, int>.Value(1);

        Assert.True(res.IsVal);
        Assert.False(res.IsErr);
        Assert.Equal(1, res.Val);
        Assert.Equal(1, res.ValOrDefault);
        Assert.Equal(default, res.ErrOrDefault);
        var ex = Assert.Throws<NoErrorException<int>>(() =>
        {
            _ = res.Err;
        });
        Assert.Equal(1, ex.Val);
        Assert.Equal(1, ex.Inner);
    }

    [Fact]
    public void BasicPropertiesShouldWork_ForErrorCase_WithSameTandE()
    {
        var res = Res<int, int>.Error(0);

        Assert.False(res.IsVal);
        Assert.True(res.IsErr);
        Assert.Equal(0, res.Err);
        Assert.Equal(0, res.ErrOrDefault);
        Assert.Equal(default, res.ValOrDefault);
        var ex = Assert.Throws<NoValueException<int>>(() =>
        {
            _ = res.Val;
        });
        Assert.Equal(0, ex.Err);
        Assert.Equal(0, ex.Inner);
    }
}
