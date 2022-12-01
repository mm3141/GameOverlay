//https://github.com/Queuete/ExileApi

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using GameOffsets.Natives;
using Microsoft.Win32.SafeHandles;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;

namespace GameHelper.Utils;

/// <summary>
///     Handle to a process.
/// </summary>
internal class Memory : SafeHandleZeroOrMinusOneIsInvalid {
    /// <summary>
    ///     Initializes a new instance of the <see cref="Memory" /> class.
    /// </summary>
    internal Memory()
        : base(true) {
        Console.WriteLine("Opening a new handle.");
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Memory" /> class.
    /// </summary>
    /// <param name="processId">processId you want to access.</param>
    internal Memory(int processId)
        : base(true) {
        var handle = NativeWrapper.OpenProcess(ProcessAccessFlags.VirtualMemoryRead, processId);
        if (NativeWrapper.HasError) {
            Console.WriteLine($"Failed to open a new handle 0x{handle:X}" +
                              $" due to ErrorNo: {NativeWrapper.LastError}");
        }
        else {
            Console.WriteLine($"Opened a new handle using IntPtr 0x{handle:X}");
        }

        this.SetHandle(handle);
    }

    int rpm_count = 0; //read prosses memoty
    int aw_count = 0; //adress wrong
    internal T ReadMemory<T>(IntPtr address, string from = null) where T : unmanaged {
        string add = !string.IsNullOrEmpty(from) ? " " + from : "";
        T result = default;
        if (this.IsInvalid || address.ToInt64() <= 0) {
            Core.AddToLog("Mem.Read adres wrong: [" + (aw_count += 1) + "]" + add, MessType.Critical);
        }
        //TODO uncomment here and learn the pain of coroutine imperfection :-))
        //Thread.Sleep(1); 
        try {
            if (!NativeWrapper.ReadProcessMemory(this.handle, address, ref result)) {
                Core.AddToLog("Mem.Read err: [" + (rpm_count += 1) + "]" + add, MessType.Critical);
            }

            return result;
        }
        catch (Exception e) {
            Core.AddToLog("Mem.Read exc: " + e.Message + add, MessType.Critical);
            return default;
        }
    }

    /// <summary>
    ///     Reads the std::vector into an array.
    /// </summary>
    /// <typeparam name="T">Object type to read.</typeparam>
    /// <param name="nativeContainer">StdVector address to read from.</param>
    /// <returns>An array of elements of type T.</returns>
    internal T[] ReadStdVector<T>(StdVector nativeContainer)
        where T : unmanaged {
        var typeSize = Marshal.SizeOf<T>();
        var length = nativeContainer.Last.ToInt64() - nativeContainer.First.ToInt64();
        if (length <= 0 || length % typeSize != 0) {
            return Array.Empty<T>();
        }

        return this.ReadMemoryArray<T>(nativeContainer.First, (int)length / typeSize);
    }

    /// <summary>
    ///     Reads the process memory as an array.
    /// </summary>
    /// <typeparam name="T">Array type to read.</typeparam>
    /// <param name="address">memory address to read from.</param>
    /// <param name="nsize">total array elements to read.</param>
    /// <returns>
    ///     An array of type T and of size nsize. In case or any error it returns empty array.
    /// </returns>
    internal T[] ReadMemoryArray<T>(IntPtr address, int nsize)
        where T : unmanaged {
        if (this.IsInvalid || address.ToInt64() <= 0 || nsize <= 0) {
            return Array.Empty<T>();
        }

        var buffer = new T[nsize];
        try {
            if (!NativeWrapper.ReadProcessMemoryArray(
                this.handle, address, buffer, out var numBytesRead)) {
                throw new Exception("Failed To Read the Memory (array)" +
                                    $" due to Error Number: 0x{NativeWrapper.LastError:X}" +
                                    $" on address 0x{address.ToInt64():X} with size {nsize}");
            }

            if (numBytesRead.ToInt32() < nsize) {
                throw new Exception($"Number of bytes read {numBytesRead.ToInt32()} is less " +
                    $"than the passed nsize {nsize} on address 0x{address.ToInt64():X}.");
            }

            return buffer;
        }
        catch (Exception e) {
            Console.WriteLine($"ERROR: {e.Message}");
            return Array.Empty<T>();
        }
    }

    /// <summary>
    ///     Reads the std::wstring. String read is in unicode format.
    /// </summary>
    /// <param name="nativecontainer">native object of std::wstring.</param>
    /// <returns>string.</returns>
    internal string ReadStdWString(StdWString nativecontainer) {
        const int MaxAllowed = 1000;
        if (nativecontainer.Length <= 0 ||
            nativecontainer.Length > MaxAllowed ||
            nativecontainer.Capacity <= 0 ||
            nativecontainer.Capacity > MaxAllowed) {
            return string.Empty;
        }

        if (nativecontainer.Capacity <= 8) {
            var buffer = BitConverter.GetBytes(nativecontainer.Buffer.ToInt64());
            var ret = Encoding.Unicode.GetString(buffer);
            buffer = BitConverter.GetBytes(nativecontainer.ReservedBytes.ToInt64());
            ret += Encoding.Unicode.GetString(buffer);
            return ret[..nativecontainer.Length];
        }
        else {
            var buffer = this.ReadMemoryArray<byte>(nativecontainer.Buffer, nativecontainer.Length * 2);
            return Encoding.Unicode.GetString(buffer);
        }
    }

    /// <summary>
    ///     Reads the string.
    /// </summary>
    /// <param name="address">pointer to the string.</param>
    /// <returns>string read.</returns>
    internal string ReadString(IntPtr address) {
        var buffer = this.ReadMemoryArray<byte>(address, 128);
        var count = Array.IndexOf<byte>(buffer, 0x00, 0);
        if (count > 0) {
            return Encoding.ASCII.GetString(buffer, 0, count);
        }

        return string.Empty;
    }

    /// <summary>
    ///     Reads Unicode string when string length isn't know.
    ///     Use  <see cref="ReadStdWString" /> if string length is known.
    /// </summary>
    /// <param name="address">points to the Unicode string pointer.</param>
    /// <returns>string read from the memory.</returns>
    internal string ReadUnicodeString(IntPtr address) {
        var buffer = this.ReadMemoryArray<byte>(address, 256);
        var count = 0x00;
        for (var i = 0; i < buffer.Length - 2; i++) {
            if (buffer[i] == 0x00 && buffer[i + 1] == 0x00 && buffer[i + 2] == 0x00) {
                count = i % 2 == 0 ? i : i + 1;
                break;
            }
        }

        // let's not return a string if null isn't found.
        if (count == 0) {
            return string.Empty;
        }

        var ret = Encoding.Unicode.GetString(buffer, 0, count);
        return ret;
    }
    
    /// <summary>
    ///     Reads the std::map into a List.
    /// </summary>
    /// <typeparam name="TKey">key type of the stdmap.</typeparam>
    /// <typeparam name="TValue">value type of the stdmap.</typeparam>
    /// <param name="nativeContainer">native object of the std::map.</param>
    /// <param name="keyfilter">Filter the keys based on the function return value.</param>
    /// <returns>a list containing the keys and the values of the stdmap as named tuple.</returns>
    internal List<(TKey Key, TValue Value)> ReadStdMapAsList<TKey, TValue>(StdMap nativeContainer,
        Func<TKey, bool> keyfilter = null) where TKey : unmanaged where TValue : unmanaged {
        const int MaxAllowed = 10000;
        var collection = new List<(TKey Key, TValue Value)>();
        if (nativeContainer.Size <= 0 || nativeContainer.Size > MaxAllowed) {
            return collection;
        }

        var childrens = new Stack<StdMapNode<TKey, TValue>>();
        var head = this.ReadMemory<StdMapNode<TKey, TValue>>(nativeContainer.Head);
        var parent = this.ReadMemory<StdMapNode<TKey, TValue>>(head.Parent);
        childrens.Push(parent);
        var counter = 0;
        while (childrens.Count != 0) {
            var cur = childrens.Pop();
            if (counter++ > nativeContainer.Size + 5) {
                childrens.Clear();
                return collection;
            }

            if (!cur.IsNil && (keyfilter == null || keyfilter(cur.Data.Key))) {
                collection.Add((cur.Data.Key, cur.Data.Value));
            }

            var left = this.ReadMemory<StdMapNode<TKey, TValue>>(cur.Left);
            if (!left.IsNil) {
                childrens.Push(left);
            }

            var right = this.ReadMemory<StdMapNode<TKey, TValue>>(cur.Right);
            if (!right.IsNil) {
                childrens.Push(right);
            }
        }

        return collection;
    }

    /// <summary>
    ///     Reads the StdList into a List.
    /// </summary>
    /// <typeparam name="TValue">StdList element structure.</typeparam>
    /// <param name="nativeContainer">native object of the std::list.</param>
    /// <returns>List containing TValue elements.</returns>
    internal List<TValue> ReadStdList<TValue>(StdList nativeContainer)
        where TValue : unmanaged {
        var retList = new List<TValue>();
        var currNodeAddress = this.ReadMemory<StdListNode>(nativeContainer.Head).Next;
        while (currNodeAddress != nativeContainer.Head) {
            var currNode = this.ReadMemory<StdListNode<TValue>>(currNodeAddress);
            if (currNodeAddress == IntPtr.Zero) {
                Console.WriteLine("Terminating reading of list next nodes because of" +
                                  "unexpected 0x00 found. This is normal if it happens " +
                                  "after closing the game, otherwise report it.");
                break;
            }

            retList.Add(currNode.Data);
            currNodeAddress = currNode.Next;
        }

        return retList;
    }

    /// <summary>
    ///     Reads the std::bucket into a list.
    /// </summary>
    /// <typeparam name="TValue">value type that the std bucket contains.</typeparam>
    /// <param name="nativeContainer">native object of the std::bucket.</param>
    /// <returns>a list containing all the valid values found in std::bucket.</returns>
    internal List<TValue> ReadStdBucket<TValue>(StdBucket nativeContainer)
        where TValue : unmanaged {
        if (nativeContainer.Data == IntPtr.Zero ||
            nativeContainer.Capacity <= 0x00) {
            return new List<TValue>();
        }

        var size = ((int)nativeContainer.Capacity + 1) / 8;
        var ret = new List<TValue>();
        var dataArray = this.ReadMemoryArray<StdBucketNode<TValue>>(nativeContainer.Data, size);
        for (var i = 0; i < dataArray.Length; i++) {
            var data = dataArray[i];
            if (data.Flag0 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer0);
            }

            if (data.Flag1 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer1);
            }

            if (data.Flag2 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer2);
            }

            if (data.Flag3 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer3);
            }

            if (data.Flag4 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer4);
            }

            if (data.Flag5 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer5);
            }

            if (data.Flag6 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer6);
            }

            if (data.Flag7 != StdBucketNode<TValue>.InValidPointerFlagValue) {
                ret.Add(data.Pointer7);
            }
        }

        return ret;
    }

    /// <summary>
    ///     When overridden in a derived class, executes the code required to free the handle.
    /// </summary>
    /// <returns>
    ///     true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
    ///     In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
    /// </returns>
    protected override bool ReleaseHandle() {
        Console.WriteLine($"Releasing handle on 0x{this.handle:X}\n");
        return NativeWrapper.CloseHandle(this.handle);
    }
}
