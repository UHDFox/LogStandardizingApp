using System.Globalization;
using System.Text;

namespace LogStandardizingApp;

public sealed class LogHandler
{
    private string LogPath { get; }

    public LogHandler(string logPath)
    {
        LogPath = logPath;
    }

    public string ReadLog(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = LogPath;
        }
        
        var fileStream = File.OpenRead(filePath);
        var reader = new StreamReader(fileStream);

        var fileDataBuilder = new StringBuilder();
        while (!reader.EndOfStream)
        {
            fileDataBuilder.Append(reader.ReadLine() + '\n');
        }

        var fileData = fileDataBuilder.ToString();

        return fileData;
    }

    public string ConfigureLog(List<Dictionary<string, string>> logs)
    {
        var finalLog = new StringBuilder();

        foreach (var log in logs)
        {

            finalLog.Append(ConvertTimeFormat(log["Дата"])).Append(" ");
            finalLog.Append(log["Время"]).Append(" ");
            finalLog.Append(log["УровеньЛогирования"]).Append(" ");

            // Если метод не указан, выводим DEFAULT
            if (log.ContainsKey("ВызвавшийМетод"))
            {
                finalLog.Append(log["ВызвавшийМетод"]).Append(" ");
            }
            else
            {
                finalLog.Append("DEFAULT ");
            }

            finalLog.Append(log["Сообщение"]).Append("\n");
        }

        return finalLog.ToString();
    }

    private string ConvertTimeFormat(string nonFormatDate)
    {
        return DateTime.TryParseExact(nonFormatDate, "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime parsedDate)
            ? parsedDate.ToString("yyyy-MM-dd")
            : nonFormatDate;
    }

    public List<Dictionary<string, string>> ParseLogData(string fileData)
    {
        var logData = new Dictionary<string, string>();

        var logs = new List<Dictionary<string, string>>();
        
        var lines = fileData.Split('\n', StringSplitOptions.TrimEntries);

        for (var lineNum = 0; lineNum < lines.Length; lineNum++)
        {
            var line = lines[lineNum];

            if (!string.IsNullOrWhiteSpace(line))
            {
                var lineInfo = line.Split(": ", 2, StringSplitOptions.TrimEntries);
                if (lineInfo.Length == 2)
                {
                    logData[lineInfo[0]] = lineInfo[1];
                }
            }

            if (lineNum + 1 == lines.Length || string.IsNullOrWhiteSpace(lines[lineNum + 1]))
            {
                if (logData.Count > 0)
                {
                    logs.Add(new Dictionary<string, string>(logData)); // Создаём копию
                    logData.Clear();
                }
            }
        }
        
        logs.ForEach(dict =>
        {
            if (dict.ContainsKey("УровеньЛогирования"))
            {
                dict["УровеньЛогирования"] = dict["УровеньЛогирования"]
                    .Replace("INFORMATION", "INFO")
                    .Replace("WARNING", "WARN");
            }
        });

        return logs;
    }
}
    