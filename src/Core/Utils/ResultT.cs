using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Muzonia.Core;

[StructLayout(LayoutKind.Auto)]
public readonly struct Res<T>
{
    internal readonly T val = default!;
    internal readonly ExceptionDispatchInfo? exception = default!;

    public Res(T value)
    {
        val = value;
        exception = null;
    }

    public Res(Exception ex)
    {
        exception = ExceptionDispatchInfo.Capture(ex);
        Unsafe.SkipInit(out val);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<T> Value(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<T> Error(Exception error) => new(error);

    [MemberNotNullWhen(false, nameof(Err))]
    public bool IsVal => exception is null;

    [MemberNotNullWhen(true, nameof(Err))]
    public bool IsErr => exception is not null;

    public T Val
    {
        get
        {
            exception?.Throw();
            return val;
        }
    }

    public T? ValOrDefault => val;

    public Exception? Err => exception?.SourceException;

    public static implicit operator Res<T>(T val) => new(val);

    public static implicit operator Res<T>(Exception err) => new(err);

    public void Deconstruct(out bool HasVal, out T? val, out Exception? err)
    {
        HasVal = IsVal;
        if (HasVal)
        {
            err = default;
            val = this.val;
        }
        else
        {
            val = default;
            err = Err;
        }
    }

    public bool TryGet(out T? Val)
    {
        if (IsVal)
        {
            Val = val;
            return true;
        }
        Val = default;
        return false;
    }

    public static Res<T> Catch<U>(in U data, Func<U, T> func)
    {
        try
        {
            return Value(func(data));
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    public static Res<T> Catch(Func<T> func)
    {
        try
        {
            return Value(func());
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    public void ThrowErr() => exception?.Throw();

    public T Or(T value) => IsVal ? val : value;

    public T OrInvoke(Func<T> func) => IsVal ? val : func();

    public Task<T> OrInvokeAsync(Func<Task<T>> func) =>
        IsVal ? Task.FromResult(val) : func();

    // Sync combinators
    public U Match<U>(Func<T, U> valFunc, Func<Exception, U> errFunc) =>
        IsVal ? valFunc(val) : errFunc(Err);

    public Res<T> Inspect(Action<T>? valFunc, Action<Exception>? errFunc)
    {
        if (IsVal)
        {
            valFunc?.Invoke(val);
        }
        else
        {
            errFunc?.Invoke(Err);
        }
        return this;
    }

    public Res<U> Map<U>(Func<T, U> valFunc) =>
        IsVal ? Res<U>.Value(valFunc(val)) : Res<U>.Error(Err);

    public Res<T> Map(Func<T, T> valFunc) => IsVal ? Value(valFunc(val)) : this;

    public Res<U> Bind<U>(Func<T, Res<U>> valFunc) =>
        IsVal ? valFunc(val) : Res<U>.Error(Err);

    public Res<T> Bind(Func<T, Res<T>> valFunc) => IsVal ? valFunc(val) : this;

    public Res<T> MapErr(Func<Exception, Exception> errFunc) =>
        IsVal ? Value(val) : Error(errFunc(Err));

    public Res<T> BindErr(Func<Exception, Res<T>> errFunc) =>
        IsVal ? Value(val) : errFunc(Err);

    // Async combinators
    // Not async for performance (await in callee)
    public Task<U> AsyncMatch<U>(
        Func<T, Task<U>> valFunc,
        Func<Exception, Task<U>> errFunc
    ) => IsVal ? valFunc(val) : errFunc(Err);

    public async Task<Res<T>> AsyncInspect(
        Func<T, Task>? valFunc,
        Func<Exception, Task>? errFunc
    )
    {
        if (IsVal)
        {
            if (valFunc is not null)
                await valFunc.Invoke(val);
        }
        else
        {
            if (errFunc is not null)
                await errFunc.Invoke(Err);
        }
        return this;
    }

    public async Task<Res<U>> AsyncMap<U>(Func<T, Task<U>> valFunc) =>
        IsVal ? Res<U>.Value(await valFunc(val)) : Res<U>.Error(Err);

    public async Task<Res<T>> AsyncMap(Func<T, Task<T>> valFunc) =>
        IsVal ? Value(await valFunc(val)) : this;

    public async Task<Res<U>> AsyncBind<U>(Func<T, Task<Res<U>>> valFunc) =>
        IsVal ? await valFunc(val) : Res<U>.Error(Err);

    public async Task<Res<T>> AsyncBind(Func<T, Task<Res<T>>> valFunc) =>
        IsVal ? await valFunc(val) : this;

    public async Task<Res<T>> AsyncMapErr(
        Func<Exception, Task<Exception>> errFunc
    ) => IsVal ? this : Error(await errFunc(Err));

    public async Task<Res<T>> AsyncBindErr(
        Func<Exception, Task<Res<T>>> errFunc
    ) => IsVal ? this : await errFunc(Err);
}

public static class XDeconstructResultTExt
{
    public static void Deconstruct<T>(
        this Res<T> res,
        out T? val,
        out Exception? err
    )
        where T : class
    {
        if (res.IsVal)
        {
            err = default;
            val = res.val;
        }
        else
        {
            val = default;
            err = res.Err;
        }
    }

    public static void Deconstruct<T>(
        this Res<T> res,
        out T? val,
        out Exception? err
    )
        where T : struct
    {
        if (res.IsVal)
        {
            err = default;
            val = res.val;
        }
        else
        {
            val = default;
            err = res.Err;
        }
    }
}
