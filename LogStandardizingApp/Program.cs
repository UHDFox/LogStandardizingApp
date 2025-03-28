namespace LogStandardizingApp;

class Program
{
    static void Main(string[] args)
    {
        var logHandler = new LogHandler("./inputLogs.txt");
        var data = logHandler.ReadLog("./inputLogs.txt");
        var parsedLogs = logHandler.ParseLogData(data);
        var standartizedLogs = logHandler.ConfigureLog(parsedLogs);
        Console.WriteLine(standartizedLogs);
    }
}