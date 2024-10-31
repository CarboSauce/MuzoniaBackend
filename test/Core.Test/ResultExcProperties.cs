namespace Muzonia.Core.Test;

public class ResultExcProperties
{
    [Fact]
    public void BasicPropertiesShouldWork_ForValueCase()
    {
        var res = new Res<int>(1);

        Assert.True(res.IsVal);
        Assert.False(res.IsErr);
        Assert.Equal(1, res.Val);
        Assert.Equal(1, res.ValOrDefault);
        Assert.Null(res.Err);
        res.ThrowErr();
    }

    [Fact]
    public void BasicPropertiesShouldWork_ForErrorCase()
    {
        var res = new Res<int>(new NullReferenceException());

        Assert.False(res.IsVal);
        Assert.True(res.IsErr);
        Assert.True(res.Err is NullReferenceException);
        Assert.True(res.Err is not null);
        Assert.Equal(default, res.ValOrDefault);
        var ex = Assert.Throws<NullReferenceException>(() =>
        {
            _ = res.Val;
        });
        Assert.True(ex is not null);
    }
}
