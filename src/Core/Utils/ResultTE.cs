using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Muzonia.Core;

public interface IResultException
{
    public object? Inner { get; }
}

public sealed class NoValueException<E>(E err) : Exception, IResultException
{
    public object? Inner => Err;
    public E Err => err;
}

public sealed class NoErrorException<T>(T val) : Exception, IResultException
{
    public object? Inner => Val;
    public T Val => val;
}

[StructLayout(LayoutKind.Auto)]
public readonly struct Res<T, E>
{
    internal readonly T val = default!;
    internal readonly E err = default!;
    internal readonly bool isval = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Res(T val, E err, bool isval)
    {
        this.val = val;
        this.err = err;
        this.isval = isval;
    }

    public Res(T ok)
    {
        val = ok;
        isval = true;
        Unsafe.SkipInit(out err);
    }

    public Res(E err)
    {
        this.err = err;
        isval = false;
        Unsafe.SkipInit(out val);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<T, E> Value(T value) => new(value, default!, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<T, E> Error(E error) => new(default!, error, false);

    public T Val => isval ? val : throw new NoValueException<E>(err);
    public E Err => !isval ? err : throw new NoErrorException<T>(val);
    public bool IsVal => isval;
    public bool IsErr => !isval;
    public T? ValOrDefault => val;
    public E? ErrOrDefault => err;

    public static implicit operator Res<T, E>(T val) => Value(val);

    public static implicit operator Res<T, E>(E err) => Error(err);

    public void Deconstruct(out bool HasVal, out T? val, out E? err)
    {
        HasVal = isval;
        if (isval)
        {
            err = default;
            val = this.val;
        }
        else
        {
            val = default;
            err = this.err;
        }
    }

    public bool TryGet(out T? Val)
    {
        if (isval)
        {
            Val = val;
            return true;
        }
        Val = default;
        return false;
    }

    public T Or(T value) => isval ? val : value;

    public T OrInvoke(Func<T> func) => isval ? val : func();

    public Task<T> OrInvokeAsync(Func<Task<T>> func) =>
        isval ? Task.FromResult(val) : func();

    // Sync combinators

    public U Match<U>(Func<T, U> valFunc, Func<E, U> errFunc) =>
        isval ? valFunc(val) : errFunc(err);

    public Res<T, E> Inspect(Action<T>? valFunc, Action<E>? errFunc)
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

    public Res<U, E> Map<U>(Func<T, U> valFunc) =>
        isval ? Res<U, E>.Value(valFunc(val)) : Res<U, E>.Error(err);

    public Res<T, E> Map(Func<T, T> valFunc) =>
        isval ? Value(valFunc(val)) : this;

    public Res<U, E> Bind<U>(Func<T, Res<U, E>> valFunc) =>
        isval ? valFunc(val) : Res<U, E>.Error(err);

    public Res<T, E> Bind(Func<T, Res<T, E>> valFunc) =>
        isval ? valFunc(val) : this;

    public Res<T, TErr> MapErr<TErr>(Func<E, TErr> errFunc) =>
        isval ? Res<T, TErr>.Value(val) : Res<T, TErr>.Error(errFunc(err));

    public Res<T, E> MapErr(Func<E, E> errFunc) =>
        isval ? this : Error(errFunc(err));

    public Res<T, TErr> BindErr<TErr>(Func<E, Res<T, TErr>> errFunc) =>
        isval ? Res<T, TErr>.Value(val) : errFunc(err);

    public Res<T, E> BindErr(Func<E, Res<T, E>> errFunc) =>
        isval ? this : errFunc(err);

    // Async combinators

    // Not async for performance (await in callee)
    public Task<U> AsyncMatch<U>(
        Func<T, Task<U>> valFunc,
        Func<E, Task<U>> errFunc
    ) => isval ? valFunc(val) : errFunc(err);

    public async Task<Res<T, E>> AsyncInspect(
        Func<T, Task>? valFunc,
        Func<E, Task>? errFunc
    )
    {
        if (isval)
        {
            if (valFunc is not null)
                await valFunc.Invoke(val);
        }
        else
        {
            if (errFunc is not null)
                await errFunc.Invoke(err);
        }
        return this;
    }

    public async Task<Res<U, E>> AsyncMap<U>(Func<T, Task<U>> valFunc) =>
        isval ? Res<U, E>.Value(await valFunc(val)) : Res<U, E>.Error(err);

    public async Task<Res<T, E>> AsyncMap(Func<T, Task<T>> valFunc) =>
        isval ? Value(await valFunc(val)) : this;

    public async Task<Res<U, E>> AsyncBind<U>(
        Func<T, Task<Res<U, E>>> valFunc
    ) => isval ? await valFunc(val) : Res<U, E>.Error(err);

    public async Task<Res<T, E>> AsyncBind(Func<T, Task<Res<T, E>>> valFunc) =>
        isval ? await valFunc(val) : this;

    public async Task<Res<T, TErr>> AsyncMapErr<TErr>(
        Func<E, Task<TErr>> errFunc
    ) =>
        isval
            ? Res<T, TErr>.Value(val)
            : Res<T, TErr>.Error(await errFunc(err));

    public async Task<Res<T, E>> AsyncMapErr(Func<E, Task<E>> errFunc) =>
        isval ? this : Error(await errFunc(err));

    public async Task<Res<T, TErr>> AsyncBindErr<TErr>(
        Func<E, Task<Res<T, TErr>>> errFunc
    ) => isval ? Res<T, TErr>.Value(val) : await errFunc(err);

    public async Task<Res<T, E>> AsyncBindErr(
        Func<E, Task<Res<T, E>>> errFunc
    ) => isval ? this : await errFunc(err);
}

public static class XDeconstructResultTErrExt
{
    public static void Deconstruct<T, E>(
        this Res<T, E> res,
        out T? val,
        out E? err
    )
        where T : class
        where E : class
    {
        if (res.isval)
        {
            err = default;
            val = res.val;
        }
        else
        {
            val = default;
            err = res.err;
        }
    }

    public static void Deconstruct<T, E>(
        this Res<T, E> res,
        out T? val,
        out E? err
    )
        where T : struct
        where E : class
    {
        if (res.isval)
        {
            err = default;
            val = res.val;
        }
        else
        {
            val = default;
            err = res.err;
        }
    }

    public static void Deconstruct<T, E>(
        this Res<T, E> res,
        out T? val,
        out E? err
    )
        where T : class
        where E : struct
    {
        if (res.isval)
        {
            err = default;
            val = res.val;
        }
        else
        {
            val = default;
            err = res.err;
        }
    }

    public static void Deconstruct<T, E>(
        this Res<T, E> res,
        out T? val,
        out E? err
    )
        where T : struct
        where E : struct
    {
        if (res.isval)
        {
            err = default;
            val = res.val;
        }
        else
        {
            val = default;
            err = res.err;
        }
    }
}
