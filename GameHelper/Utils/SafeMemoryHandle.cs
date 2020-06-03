// <copyright file="SafeMemoryHandle.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.ConstrainedExecution;
    using System.Security.Permissions;
    using System.Text;
    using GameOffsets.Native;
    using Microsoft.Win32.SafeHandles;
    using ProcessMemoryUtilities.Managed;
    using ProcessMemoryUtilities.Native;

    /// <summary>
    /// Handle to a process.
    /// </summary>
    public class SafeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeMemoryHandle"/> class.
        /// </summary>
        public SafeMemoryHandle()
            : base(true)
        {
            Console.WriteLine("Opening a new handle.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeMemoryHandle"/> class.
        /// </summary>
        /// <param name="processId">processId you want to access.</param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeMemoryHandle"/> class.
        /// </summary>
        /// <param name="processId">pid of the process.</param>
        public SafeMemoryHandle(int processId)
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
        /// Reads the process memory from a specific address of a specific size.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <param name="nsize">Size in bytes to read.</param>
        /// <returns>process memory in byte[] format.</returns>
        public byte[] ReadMemory(IntPtr address, int nsize)
        {
            if (this.IsInvalid || address.ToInt64() <= 0 || nsize <= 0)
            {
                return new byte[0];
            }

            var buffer = new byte[nsize];
            try
            {
                if (!NativeWrapper.ReadProcessMemoryArray<byte>(
                    this.handle, address, buffer, out IntPtr numBytesRead))
                {
                    throw new Exception($"Failed To Read the Memory" +
                        $"due to Error Number: 0x{NativeWrapper.LastError:X}");
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
                return new byte[0];
            }
        }

        /// <summary>
        /// Reads the process memory as type T.
        /// </summary>
        /// <typeparam name="T">type of data structure to read.</typeparam>
        /// <param name="address">address to read the data from.</param>
        /// <returns>data from the process in T format.</returns>
        public T ReadMemory<T>(IntPtr address)
            where T : unmanaged
        {
            T result = default;
            if (this.IsInvalid || address.ToInt64() <= 0)
            {
                return result;
            }

            try
            {
                uint status = NtDll.NtReadVirtualMemory(
                    this.handle,
                    address,
                    ref result,
                    out IntPtr numBytesRead);
                if (!NtDll.NtSuccess(status))
                {
                    throw new Exception($"Failed To Read the Memory" +
                        $"due to Error Number: 0x{status:X}");
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
        /// Reads the std::map into a dictionary.
        /// </summary>
        /// <typeparam name="TKey">key type of the stdmap.</typeparam>
        /// <typeparam name="TValue">value type of the stdmap.</typeparam>
        /// <param name="nativeContainer">native object of the std::map.</param>
        /// <returns>a dictonary containing the keys and the values of the stdmap.</returns>
        public Dictionary<TKey, TValue> ReadStdMap<TKey, TValue>(StdMap nativeContainer)
            where TKey : unmanaged
            where TValue : unmanaged
        {
            var size = nativeContainer.Size;
            var collection = new Dictionary<TKey, TValue>();
            if (size <= 0 || size > 1000)
            {
                throw new Exception($"ERROR: Reading HashMap failed, Size is invalid {size}.");
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
                if (counter > size)
                {
                    throw new Exception("ERROR: Reading HashMap failed" +
                        $" current counter {counter} is greater than" +
                        $" HashMap size ({size}).");
                }

                if (!cur.IsNil)
                {
                    collection.Add(cur.Data.Key, cur.Data.Value);
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
        /// Reads the std::wstring. String read is in unicode format.
        /// </summary>
        /// <param name="nativecontainer">native object of std::wstring.</param>
        /// <returns>string.</returns>
        public string ReadStdWString(StdWString nativecontainer)
        {
            int length = nativecontainer.Length.ToInt32();
            if (length <= 0 || length > 1000)
            {
                throw new Exception($"ERROR: Reading std::wstring, Length is invalid {length}");
            }

            int capacity = nativecontainer.Capacity.ToInt32();
            if (capacity <= 0 || capacity > 1000)
            {
                throw new Exception($"ERROR: Reading std::wstring, Capacity is invalid {capacity}");
            }

            if (capacity <= 8)
            {
                byte[] buffer = BitConverter.GetBytes(nativecontainer.Buffer.ToInt64());
                string ret = Encoding.Unicode.GetString(buffer);
                buffer = BitConverter.GetBytes(nativecontainer.ReservedBytes.ToInt64());
                return ret + Encoding.Unicode.GetString(buffer);
            }
            else
            {
                byte[] buffer = this.ReadMemory(nativecontainer.Buffer, (int)length * 2);
                return Encoding.Unicode.GetString(buffer);
            }
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
