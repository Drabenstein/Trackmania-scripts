using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using AutoHotkey.Interop;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Timer = System.Timers.Timer;

namespace TmTest
{
    internal class Program
    {
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static Timer timer;
        private static string mapNumberString;
        private static TimeSpan timeout;

        private static volatile bool isTrackmaniaShutdownByTimer = false;

        private static readonly Stats Stats = new Stats();

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Logs/TmTest-.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                ConsoleWindowManager.Hide();

                if (args.Length != 3)
                {
                    throw new ArgumentException(
                        "You must pass 3 arguments (TmForever path, map number 1-3 and map timeout in format mm:ss");
                }

                KillTrackmania(true);

                var fullPath = new FileInfo(args[0]);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = fullPath.Name,
                    WorkingDirectory = fullPath.DirectoryName
                };

                mapNumberString = args[1];
                int mapNum = int.Parse(mapNumberString);

                Stats.ParticipantId = await GetParticipantIdAsync(mapNum);
                Stats.Map = mapNum;

                SetupTimer(args[2]);

                Log.Information("Starting game");
                var process = Process.Start(processStartInfo);
                Log.Information("Waiting for finish");

                var mapStartScriptName = $"run-mk{mapNumberString}.ahk";
                SetupAndStartAutohotkey(mapStartScriptName);

                SetForegroundWindow(process.MainWindowHandle);

                WaitForTrackmaniaToExit();
                Log.Information("Trackmania finished");

                if (isTrackmaniaShutdownByTimer)
                {
                    MessageBox.Show("Czas upłynął / Time has elapsed", "Czas upłynął / Time has elapsed", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error {@Exception} occured during {@Stats}.", e, Stats);
            }
            finally
            {
                Stats.EndTime = DateTime.Now;

                using (var context = new StatsDbContext())
                {
                    await context.Stats.AddAsync(Stats);
                    await context.SaveChangesAsync();
                }

                Log.Information("Participant stats: {@Stats}", Stats);
                Log.CloseAndFlush();
            }
        }

        private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Stats.TimerTriggered = true;
            isTrackmaniaShutdownByTimer = true;
            KillTrackmania(false);
            timer.Stop();
        }

        private static void KillTrackmania(bool force)
        {
            var processesToKill = Process.GetProcessesByName("TmForever").ToArray();
            foreach (var processToKill in processesToKill)
            {
                if (force)
                {
                    processToKill.Kill();
                }
                else
                {
                    processToKill.CloseMainWindow();
                }
            }
        }

        private static void SetupTimer(string timerIntervalParameter)
        {
            timeout = TimeSpan.ParseExact(timerIntervalParameter, @"mm\:ss", CultureInfo.InvariantCulture);
            Stats.TimerInterval = timeout;
            timer = new Timer(timeout.TotalMilliseconds);
            timer.Elapsed += TimerOnElapsed;
        }

        private static void SetupAndStartAutohotkey(string mapStartScriptName)
        {
            var ahk = AutoHotkeyEngine.Instance;

            ahk.ExecRaw("BlockInput, MouseMove");
            ahk.ExecRaw("MouseMove 0, 0, 0");

            var handler = new Func<string, string>(fromAhk =>
            {
                Log.Information($"Received: {fromAhk}");

                if (fromAhk == "RETRY")
                {
                    Stats.Retries++;
                }
                else if (fromAhk == "START")
                {
                    Stats.StartTime = DateTime.Now;
                    timer.Start();
                }
                else if (fromAhk == "STOP")
                {
                    timer.Stop();
                    KillTrackmania(false);
                }

                return ".NET likes cakes";
            });

            ahk.InitalizePipesModule(handler);

            ahk.LoadFile(mapStartScriptName);
        }

        private static void WaitForTrackmaniaToExit()
        {
            var processesToWait = Process.GetProcessesByName("TmForever").ToArray();
            foreach (var processToWait in processesToWait)
            {
                processToWait.WaitForExit();
            }
        }

        private static async Task<int> GetParticipantIdAsync(int mapNum)
        {
            int participantId;
            using (var context = new StatsDbContext())
            {
                Player player;
                if (mapNum == 0)
                {
                    player = new Player
                    {
                        StartedAt = DateTime.Now
                    };
                    await context.AddAsync(player);
                    await context.SaveChangesAsync();
                }
                else
                {
                    player = await context.Players.OrderByDescending(x => x.Id).FirstAsync();
                }

                participantId = player.Id;
            }

            return participantId;
        }
    }
}
