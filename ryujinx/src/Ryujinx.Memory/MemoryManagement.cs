using System;
using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Memory
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public static class MemoryManagement
    {
        public static IntPtr Allocate(ulong size, bool forJit)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Allocate((IntPtr)size);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                return MemoryManagementUnix.Allocate(size, forJit);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr Reserve(ulong size, bool forJit, bool viewCompatible)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Reserve((IntPtr)size, viewCompatible);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                return MemoryManagementUnix.Reserve(size, forJit);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void Commit(IntPtr address, ulong size, bool forJit)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.Commit(address, (IntPtr)size);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                MemoryManagementUnix.Commit(address, size, forJit);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void Decommit(IntPtr address, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.Decommit(address, (IntPtr)size);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                MemoryManagementUnix.Decommit(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void MapView(IntPtr sharedMemory, ulong srcOffset, IntPtr address, ulong size, MemoryBlock owner)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.MapView(sharedMemory, srcOffset, address, (IntPtr)size, owner);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                MemoryManagementUnix.MapView(sharedMemory, srcOffset, address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void UnmapView(IntPtr sharedMemory, IntPtr address, ulong size, MemoryBlock owner)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.UnmapView(sharedMemory, address, (IntPtr)size, owner);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                MemoryManagementUnix.UnmapView(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void Reprotect(IntPtr address, ulong size, MemoryPermission permission, bool forView, bool throwOnFail)
        {
            bool result;

            if (OperatingSystem.IsWindows())
            {
                result = MemoryManagementWindows.Reprotect(address, (IntPtr)size, permission, forView);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                result = MemoryManagementUnix.Reprotect(address, size, permission);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            if (!result && throwOnFail)
            {
                throw new MemoryProtectionException(permission);
            }
        }

        public static bool Free(IntPtr address, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Free(address, (IntPtr)size);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                return MemoryManagementUnix.Free(address);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr CreateSharedMemory(ulong size, bool reserve)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.CreateSharedMemory((IntPtr)size, reserve);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                return MemoryManagementUnix.CreateSharedMemory(size, reserve);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void DestroySharedMemory(IntPtr handle)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.DestroySharedMemory(handle);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                MemoryManagementUnix.DestroySharedMemory(handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr MapSharedMemory(IntPtr handle, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.MapSharedMemory(handle);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                return MemoryManagementUnix.MapSharedMemory(handle, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void UnmapSharedMemory(IntPtr address, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.UnmapSharedMemory(address);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || Common.PlatformInfo.IsBionic)
            {
                MemoryManagementUnix.UnmapSharedMemory(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
