using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using TaleWorlds.Library;
using System.Collections.Generic;
using System.Text;

// This is getting very bloated, but I am sick of refactoring at the moment.
namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Manages System Console, background thread, and input allowing commands to be executed from the system console
    /// </summary>
    public static class SystemConsoleManager
    {
        /// MARK: Properties
        static readonly string systemConsoleOption = "/systemconsole";

        // KERNEL32 IMPORTS

        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        /// <summary>
        /// Retrieves the window handle used by the console associated with the calling process.
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        // USER32 IMPORTS

        /// <summary>
        /// Sets the specified window's show state.
        /// </summary>
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Allows the application to access the window menu for copying and modifying.
        /// </summary>
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        /// <summary>
        /// Deletes an item from the specified menu.
        /// </summary>
        [DllImport("user32.dll")]
        static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const uint SC_CLOSE = 0xF060;
        private const uint MF_BYCOMMAND = 0x00000000;

        // Synchronization event to pause input while command executes
        private static readonly AutoResetEvent _commandFinished = new(false);

        // A thread-safe queue to store actions that need to run on the main thread
        private static readonly ConcurrentQueue<Action> _executionQueue = new();
        private static volatile bool _isRunning = false;
        private static volatile bool _isCommandRunning = false;
        private static bool _isConsoleAllocated = false;
        private static volatile bool _isConsoleVisible = false;
        private static Task _inputTask;

        // Input state
        private static StringBuilder _inputBuffer = new();
        private static int _cursorIndex = 0; // Tracks cursor position within the buffer
        private static List<string> _commandHistory = new();
        private static int _historyIndex = 0;

        // Lock to prevent writing logs while the user is typing and vice versa
        private static readonly object _consoleLock = new();

        // Direct stream to the console window, bypassing the log router
        private static TextWriter _realConsoleWriter;

        /// MARK: WriteStyledPrompt
        /// <summary>
        /// Writes the colorized BLGM prompt and sets the color for user input
        /// </summary>
        private static void WriteStyledPrompt()
        {
            if (_realConsoleWriter == null) return;

            // BLGM (Light Blue / Cyan)
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            _realConsoleWriter.Write("BLGM ");

            // > (Green)
            System.Console.ForegroundColor = ConsoleColor.Green;
            _realConsoleWriter.Write("> ");

            // User Input (Light Gold / Yellow)
            System.Console.ForegroundColor = ConsoleColor.Yellow;
        }

        /// MARK: WriteLine
        /// <summary>
        /// Writes to the console without mixing jumbling text but does not redraw input prompt
        /// </summary>
        public static void WriteLine(string message)
        {
            if (!_isConsoleAllocated || _realConsoleWriter == null) return;

            lock (_consoleLock)
            {
                try
                {
                    // Ensure output is default color
                    System.Console.ResetColor();
                    _realConsoleWriter.WriteLine(message);
                }

                catch
                {
                    // Handle closed console race condition
                }
            }
        }

        /// MARK: WriteLog
        /// <summary>
        /// Writes to the console without interupting input if system console is enabled
        /// </summary>
        public static void WriteLog(string message)
        {
            // We write logs even if hidden, so history is preserved when reopened
            if (!_isConsoleAllocated || _realConsoleWriter == null) return;

            lock (_consoleLock)
            {
                try
                {
                    // If command is running, prompt will be redrawn by InputLoop later
                    if (_isCommandRunning)
                    {
                        System.Console.ResetColor();
                        _realConsoleWriter.WriteLine(message);
                        return;
                    }

                    // Otherwise, wipe line, write log, and restore prompt
                    try
                    {
                        System.Console.SetCursorPosition(0, System.Console.CursorTop);
                        _realConsoleWriter.Write(new string(' ', System.Console.WindowWidth - 1));
                        System.Console.SetCursorPosition(0, System.Console.CursorTop);
                    }

                    catch
                    {
                        //Console resized or buffer error
                    }

                    // Write Log in default color
                    System.Console.ResetColor();
                    _realConsoleWriter.WriteLine(message);

                    // Only restore prompt if visible, otherwise we just log the output
                    if (_isConsoleVisible)
                    {
                        WriteStyledPrompt();
                        _realConsoleWriter.Write(_inputBuffer.ToString());

                        // Restore Cursor to the specific index
                        int promptLen = 7;
                        try
                        {
                            System.Console.SetCursorPosition(promptLen + _cursorIndex, System.Console.CursorTop);
                        }

                        catch
                        {
                            //Cursor out of bounds
                        }
                    }
                }

                catch
                {
                    // Ignore general IO errors if console is dying
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

        /// MARK: ApplyConsoleSettings
        /// <summary>
        /// Sets the title and resizing the console window
        /// </summary>
        private static void ApplyConsoleSettings()
        {
            try
            {
                System.Console.Title = "BLGM Console";

                // Increase width by ~33%
                int currentWidth = System.Console.WindowWidth;
                int newWidth = (int)(currentWidth * 1.33);

                if (newWidth <= System.Console.LargestWindowWidth)
                {
                    // Adjust buffer if needed before resizing window
                    if (System.Console.BufferWidth < newWidth)
                        System.Console.BufferWidth = newWidth;

                    System.Console.WindowWidth = newWidth;
                }
            }
            catch
            {
                // Ignore resize errors (e.g. if constrained by screen size)
            }
        }

        /// MARK: ShowConsole
        /// <summary>
        /// Open and attach System Console for debugging, or to show command output or error output
        /// </summary>
        public static void ShowConsole()
        {
            // If we already have a handle, just show the window
            // This prevents re-allocating and breaking Input Handles
            IntPtr existingHandle = GetConsoleWindow();
            if (existingHandle != IntPtr.Zero)
            {
                ShowWindow(existingHandle, SW_SHOW);
                _isConsoleVisible = true;
                _isConsoleAllocated = true; // Ensure flag is true
                ApplyConsoleSettings(); // Re-apply settings on re-show
                StartInputThread(); // Ensure input loop is running
                return;
            }

            if (_isConsoleAllocated)
                return;

            if (AllocConsole())
            {
                _isConsoleAllocated = true;
                _isConsoleVisible = true;

                // Disable the "X" Close button to prevent killing the game process
                DisableCloseButton();

                // Apply Title and Size
                ApplyConsoleSettings();

                // Reset Console Streams (Critical for re-opening or .NET will use dead handles)
                ResetConsoleStreams();

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
                    _realConsoleWriter.WriteLine("Commands: 'clear', 'close', 'quitgame', 'ls'");
                    _realConsoleWriter.WriteLine("Tab completion and history navigation is also available");
                    _realConsoleWriter.WriteLine("NOTE: Use the 'close' command to close this window. The X button is disabled.");
                    _realConsoleWriter.WriteLine("\nType 'help' for more info.");
                    _realConsoleWriter.WriteLine("-------------------------------------------------------------------------------");
                }

                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to attach console output: " + ex.Message);
                }

                StartInputThread();
            }
        }

        /// MARK: DisableCloseButton
        /// <summary>
        /// Removes the Close (X) button from the System Menu to prevent accidental process termination.
        /// </summary>
        private static void DisableCloseButton()
        {
            IntPtr windowHandle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(windowHandle, false);

            if (windowHandle != IntPtr.Zero && sysMenu != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
            }
        }

        /// MARK: ResetConsoleStreams
        /// <summary>
        /// Forces .NET to refresh internal handles for In/Out.
        /// Required because System.Console caches handles that become invalid after FreeConsole().
        /// </summary>
        private static void ResetConsoleStreams()
        {
            try
            {
                // Re-initialize Standard Output
                var stdOut = new StreamWriter(System.Console.OpenStandardOutput());
                stdOut.AutoFlush = true;
                System.Console.SetOut(stdOut);

                // Re-initialize Standard Input
                var stdIn = new StreamReader(System.Console.OpenStandardInput());
                System.Console.SetIn(stdIn);
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting console streams: {ex}");
            }
        }

        /// MARK: Close Console
        /// <summary>
        /// Hides the console window instead of freeing it. This prevents breaking Input Handles.
        /// </summary>
        public static void CloseConsole()
        {
            if (!_isConsoleAllocated) return;

            IntPtr handle = GetConsoleWindow();

            if (handle != IntPtr.Zero)
            {
                try { _realConsoleWriter?.WriteLine("Hiding console..."); } catch { }

                // Hide window and pause input loop processing
                ShowWindow(handle, SW_HIDE);
                _isConsoleVisible = false;
            }
        }

        /// MARK: StartInputThread
        /// <summary>
        /// Starts background thread listening for console input
        /// </summary>
        private static void StartInputThread()
        {
            if (_isRunning) return;
            _isRunning = true;

            // Keep reference to task so we can wait on it during close
            _inputTask = Task.Run(() => InputLoop());
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
                    // Only print prompt if it hasn't been printed by WriteLog already
                    // This prevents double prompts like "BLGM > BLGM > "
                    if (System.Console.CursorLeft == 0)
                    {
                        WriteStyledPrompt();
                    }
                    _cursorIndex = 0;
                }

                while (_isRunning)
                {
                    // If console is hidden, sleep to save CPU and avoid reading ghost input
                    if (!_isConsoleVisible || !_isConsoleAllocated)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    try
                    {
                        if (!System.Console.KeyAvailable)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        ConsoleKeyInfo keyInfo = System.Console.ReadKey(intercept: true);
                        ProcessKey(keyInfo);
                    }

                    catch (InvalidOperationException)
                    {
                        // Handle invalidation if it occurs
                        break;
                    }

                    catch (IOException)
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Console Loop Stopped: {ex.Message}");
            }

            finally
            {
                _isRunning = false;
            }
        }

        /// MARK: ProcessKey
        /// <summary>
        /// Processes a specific key press event and handles buffer manipulation
        /// </summary>
        private static void ProcessKey(ConsoleKeyInfo keyInfo)
        {
            string inputToProcess = null;

            lock (_consoleLock)
            {
                int promptLen = 7; // "BLGM > ".Length

                // Execution
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    System.Console.ResetColor(); // Reset color for output
                    _realConsoleWriter?.WriteLine();
                    inputToProcess = _inputBuffer.ToString();

                    // Save to history
                    if (!string.IsNullOrWhiteSpace(inputToProcess))
                    {
                        _commandHistory.Add(inputToProcess);
                        _historyIndex = _commandHistory.Count;
                    }

                    _inputBuffer.Clear();
                    _cursorIndex = 0;
                }

                // Tab Completion
                else if (keyInfo.Key == ConsoleKey.Tab)
                {
                    HandleTabCompletion(promptLen);
                }

                // Editing (Backspace) ---
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (_cursorIndex > 0)
                    {
                        _inputBuffer.Remove(_cursorIndex - 1, 1);
                        _cursorIndex--;
                        RedrawInputLine(promptLen);
                    }
                }

                // Navigation (Left/Right) ---
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (_cursorIndex > 0)
                    {
                        _cursorIndex--;
                        System.Console.CursorLeft = promptLen + _cursorIndex;
                    }
                }

                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (_cursorIndex < _inputBuffer.Length)
                    {
                        _cursorIndex++;
                        System.Console.CursorLeft = promptLen + _cursorIndex;
                    }
                }

                // History (Up/Down)
                else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.DownArrow)
                {
                    HandleHistory(keyInfo.Key, promptLen);
                }

                // Typing
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    _inputBuffer.Insert(_cursorIndex, keyInfo.KeyChar);
                    _cursorIndex++;

                    // Optimization: if typing at end, just append
                    if (_cursorIndex == _inputBuffer.Length)
                    {
                        // Ensure input is gold/yellow
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        _realConsoleWriter?.Write(keyInfo.KeyChar);
                    }

                    else
                    {
                        RedrawInputLine(promptLen);
                    }
                }
            }

            // Handle Execution Outside Lock
            if (inputToProcess != null && !string.IsNullOrWhiteSpace(inputToProcess))
            {
                HandleInput(inputToProcess);
                _commandFinished.WaitOne();

                // Restore prompt if still running and visible
                if (_isRunning && _isConsoleAllocated && _isConsoleVisible)
                {
                    lock (_consoleLock)
                    {
                        WriteStyledPrompt();
                        _cursorIndex = 0;
                    }
                }
            }
        }

        /// MARK: RedrawInputLine
        /// <summary>
        /// Redraws the input buffer on the console, used after editing actions like backspace or history navigation
        /// </summary>
        private static void RedrawInputLine(int promptLen)
        {
            int originalLeft = promptLen + _cursorIndex;
            System.Console.CursorLeft = promptLen;

            // Calculate exact space needed. Using WindowWidth - 1 caused wrapping to next line.
            int clearLen = System.Console.WindowWidth - promptLen - 1;
            if (clearLen > 0)
                _realConsoleWriter?.Write(new string(' ', clearLen));

            System.Console.CursorLeft = promptLen;

            // Ensure input color
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            _realConsoleWriter?.Write(_inputBuffer.ToString());

            System.Console.CursorLeft = originalLeft;
        }

        /// MARK: HandleHistory
        /// <summary>
        /// Cycles through command history
        /// </summary>
        private static void HandleHistory(ConsoleKey key, int promptLen)
        {
            if (key == ConsoleKey.UpArrow && _historyIndex > 0)
                _historyIndex--;

            else if (key == ConsoleKey.DownArrow && _historyIndex < _commandHistory.Count)
                _historyIndex++;

            else
                return;

            _inputBuffer.Clear();
            if (_historyIndex < _commandHistory.Count)
                _inputBuffer.Append(_commandHistory[_historyIndex]);

            // Redraw full line
            System.Console.CursorLeft = promptLen;

            int clearLen = System.Console.WindowWidth - promptLen - 1;
            if (clearLen > 0)
                _realConsoleWriter?.Write(new string(' ', clearLen));

            System.Console.CursorLeft = promptLen;

            // Ensure input color
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            _realConsoleWriter?.Write(_inputBuffer.ToString());

            _cursorIndex = _inputBuffer.Length;
        }

        /// MARK: TabCompletion
        /// <summary>
        /// Handles tab key presses to trigger autocomplete or display suggestions
        /// </summary>
        private static void HandleTabCompletion(int promptLen)
        {
            string currentInput = _inputBuffer.ToString();

            // Use fuzzy matching to get candidates via Helpers
            var matches = SystemConsoleHelper.GetFuzzyMatches(currentInput);

            if (matches.Count == 1)
            {
                // Single match found
                _inputBuffer.Clear();
                _inputBuffer.Append(matches[0]);

                // Append space if it is a full command (not a group)
                if (SystemConsoleHelper.GetAllRegisteredCommands().Contains(matches[0]))
                    _inputBuffer.Append(" ");

                // Wipe and Redraw
                System.Console.CursorLeft = promptLen;

                // Formatting causing wrap-around
                int clearLen = System.Console.WindowWidth - promptLen - 1;
                if (clearLen > 0)
                    _realConsoleWriter?.Write(new string(' ', clearLen));

                System.Console.CursorLeft = promptLen;

                // Ensure input color
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                _realConsoleWriter?.Write(_inputBuffer.ToString());

                _cursorIndex = _inputBuffer.Length;
            }

            else if (matches.Count > 1)
            {
                // Multiple matches: Auto-complete to common prefix via Helpers
                string commonPrefix = SystemConsoleHelper.GetCommonPrefix(matches);
                if (commonPrefix.Length > currentInput.Length)
                {
                    _inputBuffer.Clear();
                    _inputBuffer.Append(commonPrefix);

                    // Wipe and Redraw
                    System.Console.CursorLeft = promptLen;

                    int clearLen = System.Console.WindowWidth - promptLen - 1;
                    if (clearLen > 0)
                        _realConsoleWriter?.Write(new string(' ', clearLen));

                    System.Console.CursorLeft = promptLen;

                    // Ensure input color
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    _realConsoleWriter?.Write(_inputBuffer.ToString());

                    _cursorIndex = _inputBuffer.Length;
                }

                else
                {
                    // Ambiguous: Print candidates
                    System.Console.ResetColor(); // List should be white
                    _realConsoleWriter?.WriteLine();
                    foreach (var m in matches) _realConsoleWriter?.WriteLine("  " + m);

                    WriteStyledPrompt();
                    _realConsoleWriter?.Write(_inputBuffer.ToString());
                    _cursorIndex = _inputBuffer.Length;
                }
            }
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

            // Remove invalid args
            while (args != null && args.Count > 0 && string.IsNullOrWhiteSpace(args[0]))
                args.Remove(args[0]);

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
                    if (!command.StartsWith("gm.")) WriteLine(result);
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
                try { action.Invoke(); }
                catch (Exception ex) { WriteLog($"Error executing command: {ex}"); }
            }
        }

        /// MARK: Router
        /// <summary>
        /// Routes all standard Console.WriteLine calls to ThreadSafe WriteLog
        /// </summary>
        private class ThreadSafeConsoleRouter : TextWriter
        {
            /// <summary>
            /// Gets the character encoding
            /// </summary>
            public override Encoding Encoding => Encoding.UTF8;

            /// <summary>
            /// Writes a line to the thread-safe logger
            /// </summary>
            public override void WriteLine(string value) => WriteLog(value);

            /// <summary>
            /// Writes a string to the thread-safe logger, ignoring simple newlines to prevent formatting issues
            /// </summary>
            public override void Write(string value)
            {
                if (value == "\n" || value == "\r\n") return;
                WriteLog(value);
            }
        }
    }
}