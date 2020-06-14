// <copyright file="GameProcess.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Coroutine;
    using GameHelper.Utils;
    using GameOffsets;

    /// <summary>
    /// Allows process manipulation. It uses the (time/event based) co-routines
    /// to continuously monitor & open a process with the specific name. It exposes public
    /// variables/events for the caller to use.
    ///
    /// Base class OnControllerReady is only triggered when all static addresses are found.
    ///
    /// Limitation: This class will not open a game process if multiple processes match
    /// the name because it does not know which process to select.
    /// </summary>
    public class GameProcess : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameProcess"/> class.
        /// </summary>
        public GameProcess()
        {
            CoroutineHandler.Start(this.FindAndOpen());
            CoroutineHandler.Start(this.FindStaticAddresses());
        }

        /// <summary>
        /// Gets the event raised when GameProcess has opened a new game.
        /// </summary>
        public Event OnOpened { get; private set; } = new Event();

        /// <summary>
        /// Gets the event raised just before the GameProcess has closed the game.
        /// </summary>
        public Event OnClose { get; private set; } = new Event();

        /// <summary>
        /// Gets the event raised when the game has changed its size, position or both.
        /// </summary>
        public Event OnMoved { get; private set; } = new Event();

        /// <summary>
        /// Gets the event raised when the game Foreground property has changed.
        /// </summary>
        public Event OnForegroundChanged { get; private set; } = new Event();

        /// <summary>
        /// Gets the Base Address of the game.
        /// </summary>
        public new IntPtr Address => this.Information.MainModule.BaseAddress;

        /// <summary>
        /// Gets the static addresses (along with their names) found in the GameProcess
        /// based on the GameOffsets.StaticOffsets file.
        /// </summary>
        public Dictionary<string, IntPtr> StaticAddresses { get; private set; } =
            new Dictionary<string, IntPtr>();

        /// <summary>
        /// Gets the game diagnostics information.
        /// </summary>
        public Process Information { get; private set; } = null;

        /// <summary>
        /// Gets the game handle.
        /// </summary>
        public SafeMemoryHandle Handle { get; private set; } = null;

        /// <summary>
        /// Gets the game size & position with respect to the monitor screen.
        /// </summary>
        public Rectangle WindowArea { get; private set; } = Rectangle.Empty;

        /// <summary>
        /// Gets a value indicating whether the game is foreground or not.
        /// </summary>
        public bool Foreground { get; private set; } = false;

        /// <summary>
        /// Closes the handle for the game and releases all the resources.
        /// </summary>
        /// <param name="monitorForNewGame">
        /// Set to true if caller wants to start monitoring for new game process after closing.
        /// </param>
        public void Close(bool monitorForNewGame = true)
        {
            CoroutineHandler.RaiseEvent(this.OnClose);
            this.WindowArea = Rectangle.Empty;
            this.Foreground = false;
            this.Handle?.Dispose();
            this.Information?.Close();
            if (monitorForNewGame)
            {
                CoroutineHandler.Start(this.FindAndOpen());
            }
        }

        /// <inheritdoc/>
        protected override void OnAddressUpdated(IntPtr newAddress)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the list of processes from the list of processes running on the system
        /// based on the GameOffsets.GameProcessName class.
        /// </summary>
        /// <returns>
        /// co-routine IWait.
        /// </returns>
        private IEnumerator<Wait> FindAndOpen()
        {
            var processesInfo = new List<Process>();
            while (true)
            {
                yield return new Wait(1);
                processesInfo.Clear();
                foreach (var process in Process.GetProcessesByName(GameProcessDetails.ProcessName))
                {
                    if (process.MainWindowTitle.ToLower() == GameProcessDetails.WindowTitle)
                    {
                        processesInfo.Add(process);
                    }
                }

                if (processesInfo.Count == 1)
                {
                    this.Information = processesInfo[0];
                    if (this.Open())
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Monitors the game process for changes.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private IEnumerator<Wait> Monitor()
        {
            while (true)
            {
                // Have to check MainWindowHandle because
                // sometime HasExited returns false even when game isn't running..
                if (this.Information.HasExited ||
                    this.Information.MainWindowHandle.ToInt32() <= 0x00)
                {
                    this.Close();
                    break;
                }
                else
                {
                    this.UpdateIsForeground();
                    this.UpdateWindowRectangle();
                }

                yield return new Wait(1);
            }
        }

        /// <summary>
        /// Finds the static addresses in the GameProcess based on the
        /// GameOffsets.StaticOffsetsPatterns file.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private IEnumerator<Wait> FindStaticAddresses()
        {
            while (true)
            {
                yield return new Wait(this.OnOpened);
                var baseAddress = this.Information.MainModule.BaseAddress;
                var procSize = this.Information.MainModule.ModuleMemorySize;
                var patternsInfo = PatternFinder.Find(this.Handle, baseAddress, procSize);
                foreach (var patternInfo in patternsInfo)
                {
                    int offsetDataValue = this.Handle.ReadMemory<int>(baseAddress + patternInfo.Value);
                    IntPtr address = baseAddress + patternInfo.Value + offsetDataValue + 0x04;
                    if (this.StaticAddresses.ContainsKey(patternInfo.Key))
                    {
                        this.StaticAddresses[patternInfo.Key] = address;
                    }
                    else
                    {
                        this.StaticAddresses.Add(patternInfo.Key, address);
                    }
                }

                CoroutineHandler.RaiseEvent(this.OnControllerReady);
            }
        }

        /// <summary>
        /// Opens the handle for the game process.
        /// </summary>
        private bool Open()
        {
            this.Handle = new SafeMemoryHandle(this.Information.Id);
            if (this.Handle.IsInvalid)
            {
                return false;
            }

            CoroutineHandler.Start(this.Monitor());
            CoroutineHandler.RaiseEvent(this.OnOpened);
            return true;
        }

        /// <summary>
        /// Updates the Foreground Property of the GameProcess class.
        /// </summary>
        private void UpdateIsForeground()
        {
            bool foreground = GetForegroundWindow() == this.Information.MainWindowHandle;
            if (foreground != this.Foreground)
            {
                this.Foreground = foreground;
                CoroutineHandler.RaiseEvent(this.OnForegroundChanged);
            }
        }

        /// <summary>
        /// Gets the game process window area with reference to the monitor screen.
        /// </summary>
        private void UpdateWindowRectangle()
        {
            GetClientRect(this.Information.MainWindowHandle, out var size);
            ClientToScreen(this.Information.MainWindowHandle, out var pos);
            Rectangle sizePos = size.ToRectangle(pos);
            if (sizePos != this.WindowArea && sizePos.Size != Size.Empty)
            {
                this.WindowArea = sizePos;
                CoroutineHandler.RaiseEvent(this.OnMoved);
            }
        }

#pragma warning disable SA1204 // Static elements should appear before instance elements
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, out Point lpPoint);

#pragma warning restore SA1204 // Static elements should appear before instance elements

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            private readonly int left;
            private readonly int top;
            private readonly int right;
            private readonly int bottom;

            public Rectangle ToRectangle(Point point)
            {
                return new Rectangle(point.X, point.Y, this.right - this.left, this.bottom - this.top);
            }
        }
    }
}
