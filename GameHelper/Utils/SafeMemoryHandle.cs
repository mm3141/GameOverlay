// <copyright file="SafeMemoryHandle.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using GameOffsets.Native;
    using Microsoft.Win32.SafeHandles;
    using ProcessMemoryUtilities.Managed;
    using ProcessMemoryUtilities.Native;

    /// <summary>
    /// Handle to a process.
    /// </summary>
    internal class SafeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const int MaxInfiniteCounter = 3;
        private int readStdMapInfiniteCounter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeMemoryHandle"/> class.
        /// </summary>
        internal SafeMemoryHandle()
            : base(true)
        {
            Console.WriteLine("Opening a new handle.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeMemoryHandle"/> class.
        /// </summary>
        /// <param name="processId">processId you want to access.</param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeMemoryHandle(int processId)
            : base(true)
        {
            var handle = NativeWrapper.OpenProcess(ProcessAccessFlags.VirtualMemoryRead, processId);
            if (NativeWrapper.HasError)
            {
                Console.WriteLine($"Failed to open a new handle 0x{handle:X}" +
                    $" due to ErrorNo: {NativeWrapper.LastError}");
            }
            else
            {
                Console.WriteLine($"Opened a new handle using IntPtr 0x{handle:X}");
            }

            this.SetHandle(handle);
        }

        /// <summary>
        /// Reads the process memory as type T.
        /// </summary>
        /// <typeparam name="T">type of data structure to read.</typeparam>
        /// <param name="address">address to read the data from.</param>
        /// <returns>data from the process in T format.</returns>
        internal T ReadMemory<T>(IntPtr address)
            where T : unmanaged
        {
            T result = default;
            if (this.IsInvalid || address.ToInt64() <= 0)
            {
                return result;
            }

            try
            {
                if (!NativeWrapper.ReadProcessMemory(this.handle, address, ref result))
                {
                    throw new Exception($"Failed To Read the Memory" +
                        $" due to Error Number: 0x{NativeWrapper.LastError:X}");
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// Reads the std::vector into an array.
        /// </summary>
        /// <typeparam name="T">Object type to read.</typeparam>
        /// <param name="nativeContainer">StdVector address to read from.</param>
        /// <returns>An array of elements of type T.</returns>
        internal T[] ReadStdVector<T>(StdVector nativeContainer)
            where T : unmanaged
        {
            var typeSize = Marshal.SizeOf<T>();
            var length = nativeContainer.Last.ToInt64() - nativeContainer.First.ToInt64();
            if (length == 0)
            {
                return new T[0];
            }

            if (length % typeSize != 0)
            {
                throw new ArgumentException($"The buffer is not aligned for '{typeof(T).Name}'");
            }

            return this.ReadMemoryArray<T>(nativeContainer.First, (int)length / typeSize);
        }

        /// <summary>
        /// Reads the process memory as an array.
        /// </summary>
        /// <typeparam name="T">Array type to read.</typeparam>
        /// <param name="address">memory address to read from.</param>
        /// <param name="nsize">total array elements to read.</param>
        /// <returns>
        /// An array of type T and of size nsize. In case or any error it returns empty array.
        /// </returns>
        internal T[] ReadMemoryArray<T>(IntPtr address, int nsize)
            where T : unmanaged
        {
            if (this.IsInvalid || address.ToInt64() <= 0 || nsize <= 0)
            {
                return new T[0];
            }

            var buffer = new T[nsize];
            try
            {
                if (!NativeWrapper.ReadProcessMemoryArray<T>(
                    this.handle, address, buffer, out IntPtr numBytesRead))
                {
                    throw new Exception($"Failed To Read the Memory" +
                        $" due to Error Number: 0x{NativeWrapper.LastError:X}");
                }

                if (numBytesRead.ToInt32() < nsize)
                {
                    throw new Exception($"Number of bytes read is less than the passed nsize.");
                }

                return buffer;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                return new T[0];
            }
        }

        /// <summary>
        /// Reads the std::wstring. String read is in unicode format.
        /// </summary>
        /// <param name="nativecontainer">native object of std::wstring.</param>
        /// <returns>string.</returns>
        internal string ReadStdWString(StdWString nativecontainer)
        {
            int length = nativecontainer.Length.ToInt32();
            const int MaxAllowed = 1000;
            if (length < 0 || length > MaxAllowed)
            {
                throw new Exception($"ERROR: Reading std::wstring, Length is invalid {length}");
            }

            int capacity = nativecontainer.Capacity.ToInt32();
            if (capacity < 0 || capacity > MaxAllowed)
            {
                throw new Exception($"ERROR: Reading std::wstring, Capacity is invalid {capacity}");
            }

            if (length == 0 || capacity == 0)
            {
                return string.Empty;
            }

            if (capacity <= 8)
            {
                byte[] buffer = BitConverter.GetBytes(nativecontainer.Buffer.ToInt64());
                string ret = Encoding.Unicode.GetString(buffer);
                buffer = BitConverter.GetBytes(nativecontainer.ReservedBytes.ToInt64());
                ret += Encoding.Unicode.GetString(buffer);
                return ret[0..length];
            }
            else
            {
                byte[] buffer = this.ReadMemoryArray<byte>(nativecontainer.Buffer, (int)length * 2);
                return Encoding.Unicode.GetString(buffer);
            }
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="address">pointer to the string.</param>
        /// <returns>string read.</returns>
        internal string ReadString(IntPtr address)
        {
            var buffer = this.ReadMemoryArray<byte>(address, 128);
            int count = Array.IndexOf<byte>(buffer, 0x00, 0);
            return Encoding.ASCII.GetString(buffer, 0, count);
        }

        /// <summary>
        /// Reads the std::map into a List.
        /// </summary>
        /// <typeparam name="TKey">key type of the stdmap.</typeparam>
        /// <typeparam name="TValue">value type of the stdmap.</typeparam>
        /// <param name="nativeContainer">native object of the std::map.</param>
        /// <param name="validate_size">Validate the total while loop iteration with given map size.</param>
        /// <param name="keyfilter">Filter the keys based on the function return value.</param>
        /// <returns>a list containing the keys and the values of the stdmap as named tuple.</returns>
        internal List<(TKey Key, TValue Value)> ReadStdMapAsList<TKey, TValue>(
            StdMap nativeContainer,
            bool validate_size,
            Func<TKey, bool> keyfilter = null)
            where TKey : unmanaged
            where TValue : unmanaged
        {
            const int MaxAllowed = 10000;
            var size = nativeContainer.Size;
            var collection = new List<(TKey Key, TValue Value)>();
            if (size <= 0 || size > MaxAllowed)
            {
                return collection;
            }

            var childrens = new Stack<StdMapNode<TKey, TValue>>();
            var head = this.ReadMemory<StdMapNode<TKey, TValue>>(nativeContainer.Head);
            var parent = this.ReadMemory<StdMapNode<TKey, TValue>>(head.Parent);
            childrens.Push(parent);
            ulong counter = 0;
            while (childrens.Count != 0)
            {
                var cur = childrens.Pop();
                counter++;
                if (validate_size)
                {
                    // Validating for read size vs actual size.
                    if (counter > size)
                    {
                        throw new Exception("ERROR: Reading HashMap failed" +
                            $" current loop counter {counter} is greater than" +
                            $" the HashMap size ({size}).");
                    }
                }
                else
                {
                    // Validating for infinite loop.
                    if (counter > MaxAllowed)
                    {
                        this.readStdMapInfiniteCounter++;
#if DEBUG
                        Console.WriteLine($"ERROR ({this.readStdMapInfiniteCounter}):" +
                            $" Reading HashMap failed current counter {counter} is greater than" +
                            $" Maximum allowed HashMap size ({MaxAllowed}).");
#endif
                        if (this.readStdMapInfiniteCounter > MaxInfiniteCounter)
                        {
                            throw new Exception("ERROR: Reading HashMap failed" +
                                $" current counter {counter} is greater than" +
                                $" Maximum allowed HashMap size ({MaxAllowed}).");
                        }

                        break;
                    }
                }

                if (!cur.IsNil && (keyfilter == null || keyfilter(cur.Data.Key)))
                {
                    collection.Add((cur.Data.Key, cur.Data.Value));
                }

                var left = this.ReadMemory<StdMapNode<TKey, TValue>>(cur.Left);
                if (!left.IsNil)
                {
                    childrens.Push(left);
                }

                var right = this.ReadMemory<StdMapNode<TKey, TValue>>(cur.Right);
                if (!right.IsNil)
                {
                    childrens.Push(right);
                }
            }

            return collection;
        }

        /// <summary>
        /// Reads the StdList into a List.
        /// </summary>
        /// <typeparam name="TValue">StdList element structure.</typeparam>
        /// <param name="nativeContainer">native object of the std::list.</param>
        /// <returns>List containing TValue elements.</returns>
        internal List<TValue> ReadStdList<TValue>(StdList nativeContainer)
            where TValue : unmanaged
        {
            var retList = new List<TValue>();
            var currNodeAddress = this.ReadMemory<StdListNode>(nativeContainer.Head).Next;
            while (currNodeAddress != nativeContainer.Head)
            {
                var currNode = this.ReadMemory<StdListNode<TValue>>(currNodeAddress);
                if (currNodeAddress == IntPtr.Zero)
                {
                    Console.WriteLine("Terminating Preloads finding because of" +
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
        /// When overridden in a derived class, executes the code required to free the handle.
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
        /// In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
        /// </returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            Console.WriteLine($"Releasing handle on 0x{this.handle:X}\n");
            return NativeWrapper.CloseHandle(this.handle);
        }
    }
}
