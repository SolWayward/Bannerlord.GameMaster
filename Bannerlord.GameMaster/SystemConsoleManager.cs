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

namespace Bannerlord.GameMaster
{
    /// <summary>
    /// Manages System Console, background thread, and input allowing commands to be executed from the system console
    /// </summary>
    public static class SystemConsoleManager
    {
        static readonly string systemConsoleOption = "/systemconsole";

        // Import AllocConsole from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        // Import FreeConsole to release it (optional, for cleanup)
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
        
        // A thread-safe queue to store actions that need to run on the main thread
        private static readonly ConcurrentQueue<Action> _executionQueue = new();
        private static bool _isRunning = false;

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

        /// <summary>
        /// Open and attach System Console for debugging, or to show command output or error output
        /// </summary>
        public static void ShowConsole()
        {
            // Allocate a new console window
            if (AllocConsole())
            {
                // Redirect the standard output stream to the new console.
                try
                {
                    // Create a writer that wraps the new console's output stream
                    StreamWriter writer = new (System.Console.OpenStandardOutput())
                    {
                        AutoFlush = true
                    };

                    // Set Console.Out to use this writer
                    System.Console.SetOut(writer);

                    System.Console.WriteLine("[Console Attached Successfully]");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to attach console output: " + ex.Message);
                }

                StartInputThread();
            }
        }

        /// <summary>
        /// Starts background thread listening for console input
        /// </summary>
        private static void StartInputThread()
        {
            if (_isRunning) return;
            _isRunning = true;

            // Start the background listener loop
            Task.Run(() => InputLoop());
        }

        /// <summary>
        /// Listens to console input in background without stalling game thread
        /// </summary>
        private static void InputLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // This blocks this specific thread, but NOT the game
                    string input = System.Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        // Parse the command and queue the action
                        HandleInput(input);
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error reading input: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks console input and executes action if match found
        /// </summary>
        /// <param name="input"></param>
        private static void HandleInput(string input)
        {
            if (input.Length == 0)
                return;

            // Split command and args
            string[] parts = input.Split(' ');
            string command = parts[0].ToLower();
            List<string> args = parts.Skip(1).ToList();

            // Exit
            if (command == "exit")
            {
                System.Console.WriteLine("Exiting console loop...");
                _isRunning = false;
            }
            
            // Redirect command to game console
            else
            {
                Enqueue(() => ExecuteEngineCommand(command, args));
            }
        }

        private static void ExecuteEngineCommand(string command, List<string> args)
        {
            try
            {
                // Pass command to game console
                string result = CommandLineFunctionality.CallFunction(command, args, out bool commandFound);
            }

            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"Command Execution Failed: {ex.Message}");
                System.Console.ResetColor();
            }
        }

        /// <summary>
        /// Add actions to the queue
        /// </summary>
        /// <param name="action"></param>
        private static void Enqueue(Action action)
        {
            _executionQueue.Enqueue(action);
        }

        /// <summary>
        /// Process queued commands
        /// </summary>
        public static void OnTick()
        {
            // Process all queued commands
            while (_executionQueue.TryDequeue(out Action action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error executing command: {ex}");
                }
            }
        }
    }
}