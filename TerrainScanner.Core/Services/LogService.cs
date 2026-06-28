using System.Text;

namespace TerrainScanner.Core.Services
{
    /// <summary>
    /// Сервис логирования (Log Service — журнал событий системы).
    /// </summary>
    public static class LogService
    {
        private static readonly StringBuilder _sb = new();

        public static event Action<string>? OnLog;

        public static void Log(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

            _sb.AppendLine(line);

            OnLog?.Invoke(line);
        }

        public static string GetFullLog()
        {
            return _sb.ToString();
        }

        public static void Clear()
        {
            _sb.Clear();
            OnLog?.Invoke("---- Лог очищен ----");
        }
    }
}
