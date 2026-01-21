using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using TaleWorlds.Library;
using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using System.Text;

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Manages System Console, background thread, and input allowing commands to be executed from the system console
    /// </summary>
    public static class SystemConsoleManager
    {
        /// MARK: Properties
        static readonly string systemConsoleOption = "/systemconsole";

        // Import AllocConsole from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        // Import FreeConsole to release it (optional, for cleanup)
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        // Import SetConsoleCtrlHandler to intercept console close events
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);

        // Delegate for console control handler
        private delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

        // Synchronization event to pause input while command executes
        private static readonly AutoResetEvent _commandFinished = new(false);

        // Console control event types
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        // Keep a reference to prevent garbage collection
        private static ConsoleCtrlDelegate _consoleCtrlHandler;

        // A thread-safe queue to store actions that need to run on the main thread
        private static readonly ConcurrentQueue<Action> _executionQueue = new();
        private static volatile bool _isRunning = false;
        private static volatile bool _isCommandRunning = false;
        private static bool _isConsoleAllocated = false;

        // Store what the user is currently typing
        private static StringBuilder _inputBuffer = new();

        // Lock to prevent writing logs while the user is typing and vice versa
        private static readonly object _consoleLock = new();

        // Direct stream to the console window, bypassing the log router
        private static TextWriter _realConsoleWriter;

        /// MARK: WriteLine
        /// <summary>
        /// Writes to the console without mixing jumbling text but does not redraw input prompt
        /// </summary>
        public static void WriteLine(string message)
        {
            if (!_isConsoleAllocated || _realConsoleWriter == null) return;

            lock (_consoleLock)
            {
                _realConsoleWriter.WriteLine(message);
            }
        }

        /// MARK: WriteLog
        /// <summary>
        /// Writes to the console without interupting input if system console is enabled
        /// </summary>
        public static void WriteLog(string message)
        {
            if (!_isConsoleAllocated || _realConsoleWriter == null) return;

            lock (_consoleLock)
            {
                try
                {
                    // If command is running, prompt will be redrawn by InputLoop later
                    if (_isCommandRunning)
                    {
                        _realConsoleWriter.WriteLine(message);
                        return;
                    }

                    // Otherwise, wipe line, write log, and restore prompt
                    System.Console.SetCursorPosition(0, System.Console.CursorTop);
                    _realConsoleWriter.Write(new string(' ', System.Console.WindowWidth - 1));
                    System.Console.SetCursorPosition(0, System.Console.CursorTop);

                    _realConsoleWriter.WriteLine(message);

                    _realConsoleWriter.Write("BLGM > " + _inputBuffer.ToString());

                    int promptLen = 7;
                    System.Console.SetCursorPosition(promptLen + _inputBuffer.Length, System.Console.CursorTop);
                }
                catch
                {
                    // Ignore resize/handle errors
                }
            }
        }

        /// MARK: ShowOnLaunch
        /// <summary>
        /// Open and attach System Console if bannerlord was launched with "/systemconsole"
        /// </summary>
        public static void ShowConsoleIfLaunchedWithCommandLineOption()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool consoleEnabled = args.Any(arg => arg.Equals(systemConsoleOption, StringComparison.OrdinalIgnoreCase));

            if (consoleEnabled)
                ShowConsole();
        }

        /// MARK: ShowConsole
        /// <summary>
        /// Open and attach System Console for debugging, or to show command output or error output
        /// </summary>
        public static void ShowConsole()
        {
            if (_isConsoleAllocated) return;

            if (AllocConsole())
            {
                _isConsoleAllocated = true;

                _consoleCtrlHandler = new ConsoleCtrlDelegate(ConsoleCtrlHandler);
                SetConsoleCtrlHandler(_consoleCtrlHandler, true);

                try
                {
                    // Establish direct connection to window
                    _realConsoleWriter = new StreamWriter(System.Console.OpenStandardOutput())
                    {
                        AutoFlush = true
                    };

                    // Redirect all standard console output to our router to catch Debug.Print/Engine logs
                    System.Console.SetOut(new ThreadSafeConsoleRouter());

                    _realConsoleWriter.WriteLine("All output from Bannerlord Console and any debug output will be visible here");
                    _realConsoleWriter.WriteLine("Commands: 'close' to close console, 'quitgame' to exit game");
                    _realConsoleWriter.WriteLine("\nType 'ls' to discover command categories");
                    _realConsoleWriter.WriteLine("\nYou can also list commands in each category by using 'ls campaign', 'ls gm', 'ls gm.hero'\n");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to attach console output: " + ex.Message);
                }

                StartInputThread();
            }
        }

        /// MARK: CloseCtrlHandler
        /// <summary>
        /// Handles console control events (like clicking the X button)
        /// </summary>
        private static bool ConsoleCtrlHandler(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                    CloseConsole();
                    return true;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    return false;

                default:
                    return false;
            }
        }

        /// MARK: Close Console
        /// <summary>
        /// Closes the console window and stops the input thread without exiting the game
        /// </summary>
        public static void CloseConsole()
        {
            if (!_isConsoleAllocated) return;

            _isRunning = false;

            // Redirect to null to prevent crash if game logs to dead console
            try { System.Console.SetOut(StreamWriter.Null); } catch { }

            try { _realConsoleWriter?.WriteLine("Closing console..."); } catch { }

            if (_consoleCtrlHandler != null)
            {
                SetConsoleCtrlHandler(_consoleCtrlHandler, false);
                _consoleCtrlHandler = null;
            }

            FreeConsole();
            _isConsoleAllocated = false;
            _realConsoleWriter = null;
        }

        /// MARK: StartInputThread
        /// <summary>
        /// Starts background thread listening for console input
        /// </summary>
        private static void StartInputThread()
        {
            if (_isRunning) return;
            _isRunning = true;

            Task.Run(() => InputLoop());
        }

        /// MARK: ReadInput
        /// <summary>
        /// Listens to console input in background without stalling game thread
        /// </summary>
        private static void InputLoop()
        {
            try
            {
                lock (_consoleLock)
                {
                    _realConsoleWriter?.Write("BLGM > ");
                }

                while (_isRunning)
                {
                    if (!IsConsoleAvailable()) break;

                    if (!System.Console.KeyAvailable)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    ConsoleKeyInfo keyInfo = System.Console.ReadKey(intercept: true);
                    string inputToProcess = null;

                    lock (_consoleLock)
                    {
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            _realConsoleWriter?.WriteLine();
                            inputToProcess = _inputBuffer.ToString();
                            _inputBuffer.Clear();
                        }

                        else if (keyInfo.Key == ConsoleKey.Backspace)
                        {
                            if (_inputBuffer.Length > 0)
                            {
                                _inputBuffer.Length--;
                                _realConsoleWriter?.Write("\b \b");
                            }
                        }

                        else if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _inputBuffer.Append(keyInfo.KeyChar);
                            _realConsoleWriter?.Write(keyInfo.KeyChar);
                        }
                    }

                    if (inputToProcess != null)
                    {
                        if (!string.IsNullOrWhiteSpace(inputToProcess))
                        {
                            HandleInput(inputToProcess);
                            _commandFinished.WaitOne();
                        }

                        if (_isRunning && IsConsoleAvailable())
                        {
                            lock (_consoleLock)
                            {
                                _realConsoleWriter?.Write("BLGM > ");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Console Loop Stopped: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper needed to prevent crash when checking KeyAvailable on a closed console
        /// </summary>
        private static bool IsConsoleAvailable()
        {
            try { int h = System.Console.WindowHeight; return true; } catch { return false; }
        }

        /// MARK: HandleInput
        /// <summary>
        /// Checks console input and executes action if match found
        /// </summary>
        private static void HandleInput(string input)
        {
            if (input.Length == 0) return;

            string[] parts = input.Split(' ');
            string command = parts[0].ToLower();
            List<string> args = parts.Skip(1).ToList();

            _isCommandRunning = true;

            Enqueue(() =>
            {
                try
                {
                    ExecuteGameCommand(command, args);
                }

                catch (Exception ex)
                {
                    lock (_consoleLock)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        _realConsoleWriter?.WriteLine($"Command Error: {ex.Message}");
                        System.Console.ResetColor();
                    }
                }

                finally
                {
                    _isCommandRunning = false;
                    _commandFinished.Set();
                }
            });
        }

        /// MARK: Execute
        /// <summary>
        /// Executes command on main thread
        /// </summary>
        public static string ExecuteGameCommand(string command, List<string> args = null)
        {
            string result;
            args ??= new();

            if (CommandLineFunctionality.HasFunctionForCommand(command))
            {
                try
                {
                    result = CommandLineFunctionality.CallFunction(command, args, out bool commandFound);

                    if (!command.StartsWith("gm."))
                        WriteLine(result);
                }

                catch (Exception ex)
                {
                    result = $"Command Execution Failed: {ex}";
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    WriteLine(result);
                    System.Console.ResetColor();
                }
            }
            else
            {
                result = SystemConsoleCommands.ExecuteCustomSystemConsoleCommand(command, args);
                WriteLine(result);
            }

            return result;
        }

        /// MARK: Enqueue
        /// <summary>
        /// Add actions to the queue to be executed on the main thread
        /// </summary>
        private static void Enqueue(Action action)
        {
            _executionQueue.Enqueue(action);
        }

        /// MARK: OnTick
        /// <summary>
        /// Process queued commands on main thread
        /// </summary>
        public static void OnTick()
        {
            while (_executionQueue.TryDequeue(out Action action))
            {
                try
                {
                    action.Invoke();
                }

                catch (Exception ex)
                {
                    // Route internal errors back to WriteLog
                    WriteLog($"Error executing command: {ex}");
                }
            }
        }

        /// MARK: Router
        /// <summary>
        /// Routes all standard Console.WriteLine calls to ThreadSafe WriteLog
        /// </summary>
        private class ThreadSafeConsoleRouter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override void WriteLine(string value) => SystemConsoleManager.WriteLog(value);

            public override void Write(string value)
            {
                if (value == "\n" || value == "\r\n") return;
                SystemConsoleManager.WriteLog(value);
            }
        }
    }
}