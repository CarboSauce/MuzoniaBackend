namespace Muzonia.Utils;

public readonly struct Res<T,E>
{
    private readonly T val;
    private readonly E err;
    private readonly bool isval;

    public Res(T val)
    {
        this.val = val;
        err = default!;
        isval = true;
    }

    public Res(E err)
    {
        this.err = err;
        val = default!;
        isval = false;
    }
    public T Val => val;
    public E Err => err;
    public bool IsVal => isval;
    public bool IsErr => !isval;

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
}
