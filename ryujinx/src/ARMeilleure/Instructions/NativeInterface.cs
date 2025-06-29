using ARMeilleure.Memory;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
#if ANDROID
using System.Runtime.CompilerServices;
#else
using System.Runtime.InteropServices;
#endif

namespace ARMeilleure.Instructions
{
    static class NativeInterface
    {
        private class ThreadContext
        {
            public ExecutionContext Context { get; }
            public IMemoryManager Memory { get; }
            public Translator Translator { get; }

            public ThreadContext(ExecutionContext context, IMemoryManager memory, Translator translator)
            {
                Context = context;
                Memory = memory;
                Translator = translator;
            }
        }

        [ThreadStatic]
        private static ThreadContext Context;

        public static void RegisterThread(ExecutionContext context, IMemoryManager memory, Translator translator)
        {
            Context = new ThreadContext(context, memory, translator);
        }

        public static void UnregisterThread()
        {
            Context = null;
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void Break(ulong address, int imm)
        {
            Statistics.PauseTimer();

            GetContext().OnBreak(address, imm);

            Statistics.ResumeTimer();
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void SupervisorCall(ulong address, int imm)
        {
            Statistics.PauseTimer();

            GetContext().OnSupervisorCall(address, imm);

            Statistics.ResumeTimer();
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void Undefined(ulong address, int opCode)
        {
            Statistics.PauseTimer();

            GetContext().OnUndefined(address, opCode);

            Statistics.ResumeTimer();
        }

        #region "System registers"
#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong GetCtrEl0()
        {
            return GetContext().CtrEl0;
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong GetDczidEl0()
        {
            return GetContext().DczidEl0;
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong GetCntfrqEl0()
        {
            return GetContext().CntfrqEl0;
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong GetCntpctEl0()
        {
            return GetContext().CntpctEl0;
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong GetCntvctEl0()
        {
            return GetContext().CntvctEl0;
        }
        #endregion

        #region "Read"
#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static byte ReadByte(ulong address)
        {
            return GetMemoryManager().ReadGuest<byte>(address);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ushort ReadUInt16(ulong address)
        {
            return GetMemoryManager().ReadGuest<ushort>(address);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static uint ReadUInt32(ulong address)
        {
            return GetMemoryManager().ReadGuest<uint>(address);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong ReadUInt64(ulong address)
        {
            return GetMemoryManager().ReadGuest<ulong>(address);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static V128 ReadVector128(ulong address)
        {
            return GetMemoryManager().ReadGuest<V128>(address);
        }
        #endregion

        #region "Write"
#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void WriteByte(ulong address, byte value)
        {
            GetMemoryManager().WriteGuest(address, value);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void WriteUInt16(ulong address, ushort value)
        {
            GetMemoryManager().WriteGuest(address, value);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void WriteUInt32(ulong address, uint value)
        {
            GetMemoryManager().WriteGuest(address, value);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void WriteUInt64(ulong address, ulong value)
        {
            GetMemoryManager().WriteGuest(address, value);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void WriteVector128(ulong address, V128 value)
        {
            GetMemoryManager().WriteGuest(address, value);
        }
        #endregion

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void EnqueueForRejit(ulong address)
        {
            Context.Translator.EnqueueForRejit(address, GetContext().ExecutionMode);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void SignalMemoryTracking(ulong address, ulong size, byte write)
        {
            GetMemoryManager().SignalMemoryTracking(address, size, write == 1);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void ThrowInvalidMemoryAccess(ulong address)
        {
            throw new InvalidAccessException(address);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static ulong GetFunctionAddress(ulong address)
        {
            TranslatedFunction function = Context.Translator.GetOrTranslate(address, GetContext().ExecutionMode);

            return (ulong)function.FuncPointer.ToInt64();
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static void InvalidateCacheLine(ulong address)
        {
            Context.Translator.InvalidateJitCacheRegion(address, InstEmit.DczSizeInBytes);
        }

#if ANDROID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [UnmanagedCallersOnly]
#endif
        public static byte CheckSynchronization()
        {
            Statistics.PauseTimer();

            ExecutionContext context = GetContext();

            context.CheckInterrupt();

            Statistics.ResumeTimer();

            return (byte)(context.Running ? 1 : 0);
        }

        public static ExecutionContext GetContext()
        {
            return Context.Context;
        }

        public static IMemoryManager GetMemoryManager()
        {
            return Context.Memory;
        }
    }
}
