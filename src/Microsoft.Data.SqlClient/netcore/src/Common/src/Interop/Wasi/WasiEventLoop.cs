using System.Runtime.CompilerServices;
using System;
using System.Threading;
using System.Threading.Tasks;
using ImportsWorld.wit.imports.wasi.io.v0_2_1;

internal static class WasiEventLoop
{
    internal static void Dispatch()
    {
        CallDispatchWasiEventLoop((Thread)null!);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "DispatchWasiEventLoop")]
        static extern void CallDispatchWasiEventLoop(Thread t);
    }

    internal static Task Register(IPoll.Pollable pollable, CancellationToken cancellationToken)
    {
        var handle = pollable.Handle;
        pollable.Handle = 0;
        return CallRegister((Thread)null!, handle, cancellationToken);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "RegisterWasiPollableHandle")]
        static extern Task CallRegister(Thread t, int handle, CancellationToken cancellationToken);
    }

    internal static void RunAsync(Func<Task> func)
    {
        var task = func();
        while (!task.IsCompleted)
        {
            WasiEventLoop.Dispatch();
        }
        var exception = task.Exception;
        if (exception is not null)
        {
            throw exception;
        }
    }

    internal static T RunAsync<T>(Func<Task<T>> func)
    {
        var task = func();
        while (!task.IsCompleted)
        {
            WasiEventLoop.Dispatch();
        }
        var exception = task.Exception;
        if (exception is not null)
        {
            throw exception;
        }
        return task.Result;
    }
}
