using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LogStandardizingApp;

public sealed class LogHandler
{
    private string LogPath { get; } = "./inputLogs.txt";
    private List<string> Problems { get; set; } = new List<string>();

    public LogHandler(string? logPath)
    {
        LogPath = logPath;
    }

    public string ReadLog(string? filePath)
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
            ValidateKeys(log);

            if (log.ContainsKey("Дата") && log.ContainsKey("Время") && log.ContainsKey("УровеньЛогирования"))
            {

                finalLog.Append(ConvertTimeFormat(log["Дата"])).Append(" ");
                
                if (!IsValidTimeFormat(log["Время"]))
                {
                    Problems.Add($"Неверный формат времени: {log["Время"]}");
                    continue;
                }
                
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
            else
            {
                Problems.Add("Неправильный формат лога: " + string.Join(", ", log.Values));
            }
        }
        
                
        if (Problems.Any())
        {
            File.WriteAllLines("./problems.txt", Problems);
            
            Console.WriteLine($"{Problems.Count} problems found");
        }

        return finalLog.ToString();
    }
    
    private void ValidateKeys(Dictionary<string, string> log)
    {
        var validKeys = new List<string> { "Дата", "Время", "УровеньЛогирования", "ВызвавшийМетод", "Сообщение" };
        
        var incorrectKeys = log.Keys.Where(key => !validKeys.Contains(key)).ToList();
        
        if (incorrectKeys.Any())
        {
            Problems.Add("Неправильный ключ в логе: " + string.Join(", ", incorrectKeys));
        }
    }
    
    private bool IsValidTimeFormat(string time)
    {
        // Проверяем, что время имеет формат HH:mm:ss.SSS или HHmmss:SSS
        var regex = new Regex(@"^\d{2}:\d{2}:\d{2}(\.\d+)?$|^\d{6}:\d{3}$");
        return regex.IsMatch(time);
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
                else
                {
                    // Если строка не соответствует нужному формату, добавим ее в список на запись в проблемные
                    Problems.Add(line);
                }
            }

            if (lineNum + 1 == lines.Length || string.IsNullOrWhiteSpace(lines[lineNum + 1]))
            {
                if (logData.Count > 0)
                {
                    logs.Add(new Dictionary<string, string>(logData));
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
    