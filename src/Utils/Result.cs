using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Muzonia.Utils;

public class UnwrapException<E>(E err) : Exception 
{
    E Err => err;
}

[StructLayout(LayoutKind.Auto)]
public readonly struct Res<T,E>
{
    private readonly T val;
    private readonly E err;
    private readonly bool isval;

    private Res(T val, E err, bool isval)
    {
        this.val = val;
        this.err = err;
        this.isval = isval;
    }
    public Res(T ok) : this(ok, default!, true) { }
    public Res(E err) : this(default!,err,false) { }
    public static Res<T, E> Ok(T ok)
        => new(ok, default!, true);
    public T Val => isval ? val : throw new UnwrapException<E>(err);
    public E Err => err;
    public bool IsVal => isval;
    public bool IsErr => !isval;
    public T? ValOrDefault => val;

    public static implicit operator Res<T, E>(T val) =>
        new(val);

    public static implicit operator Res<T, E>(E err) =>
        new(err);

    public void Deconstruct(out bool HasVal, out T? Val, out E? Err)
    {
        HasVal = isval;
        if (isval)
        {
            Err = default;
            Val = val;
        }
        else
        {
            Val = default;
            Err = err;
        }
    }

    public U Match<U>(Func<T, U> valFunc, Func<E, U> errFunc)
        => isval ? valFunc(val) : errFunc(err);

    public Res<T,E> Switch(Action<T>? valFunc, Action<E>? errFunc)
    {
        if (isval)
        {
            valFunc?.Invoke(val);
        }
        else
        {
            errFunc?.Invoke(err);
        }
        return this;
    }

    public Res<T, E> Map(Func<T, T> func)
        => isval ? new(func(val)) : this;
}
