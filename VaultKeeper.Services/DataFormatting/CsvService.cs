using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Attributes;
using VaultKeeper.Services.Abstractions.DataFormatting;

namespace VaultKeeper.Services.DataFormatting;

public class CsvService(ILogger<CsvService> logger) : ICsvService
{
    public Result<string> Serialize<T>(IEnumerable<T> objs) where T : class
    {
        logger.LogInformation(nameof(Serialize));

        try
        {
            IEnumerable<PropertyInfo> properties = GetCsvProperties(typeof(T));

            IEnumerable<string> headers = properties.Select(property =>
            {
                CsvColumnAttribute? csvColumnAttribute = property.GetCustomAttribute<CsvColumnAttribute>();
                return string.IsNullOrWhiteSpace(csvColumnAttribute?.Header) ? property.Name : csvColumnAttribute.Header;
            });

            IEnumerable<IEnumerable<string>> valueRows = objs.Select(obj => properties.Select(property => property.GetValue(obj)?.ToString() ?? string.Empty));

            StringBuilder sb = new();
            const char separator = ',';

            sb.AppendJoin(separator, headers);

            foreach (var row in valueRows)
            {
                sb.AppendLine();
                sb.AppendJoin(separator, row);
            }

            return sb.ToString().ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<IEnumerable<T>> Deserialize<T>(string csvText) where T : class, new()
    {
        logger.LogInformation(nameof(Deserialize));

        try
        {
            if (string.IsNullOrWhiteSpace(csvText))
                return Enumerable.Empty<T>().ToOkResult().Logged(logger);

            string[] csvRows = csvText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            if (csvRows.Length < 2)
                return Enumerable.Empty<T>().ToOkResult().Logged(logger);

            IEnumerable<PropertyInfo> properties = GetCsvProperties(typeof(T));
            string[] typeHeaders = [.. properties.Select(GetCsvHeaderName)];
            string[] dataHeaders = ParseCsvRow(csvRows[0]);
            string[] missingHeaders = [.. typeHeaders.Where(x => !dataHeaders.Contains(x))];
            if (missingHeaders.Length > 0)
                return Result.Failed<IEnumerable<T>>(ResultFailureType.BadRequest, $"CSV data is missing the following headers:\n{string.Join(", ", missingHeaders.Select(x => $"\"{x}\""))}");

            List<T> objs = [];

            for (int csvRowIndex = 1; csvRowIndex < csvRows.Length; csvRowIndex++)
            {
                string csvRow = csvRows[csvRowIndex];
                string[] values = ParseCsvRow(csvRow);
                T obj = new();

                for (int valueRowIndex = 0; valueRowIndex < values.Length; valueRowIndex++)
                {
                    string value = values[valueRowIndex];
                    string header = dataHeaders[valueRowIndex];

                    PropertyInfo? property = properties.FirstOrDefault(x => GetCsvHeaderName(x) == header);
                    property?.SetValue(obj, value);
                }

                objs.Add(obj);
            }

            return (objs as IEnumerable<T>).ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<T>>().Logged(logger);
        }
    }

    private static string[] ParseCsvRow(string csvRowText)
    {
        List<string> values = [];
        StringBuilder sb = new();
        bool inOpenQuoteState = false;
        const char comma = ',';
        const char quote = '"';

        for (int i = 0; i < csvRowText.Length; i++)
        {
            char character = csvRowText[i];
            char? nextCharacter = i + 1 < csvRowText.Length ? csvRowText[i + 1] : null;
            char? prevCharacter = i > 0 ? csvRowText[i - 1] : null;

            if (character == quote)
            {
                if (nextCharacter == quote)
                {
                    sb.Append(character);
                    inOpenQuoteState = false;
                    continue; // Escaped quote character.
                }
                if (inOpenQuoteState)
                {
                    values.Add(sb.ToString());
                    sb.Clear();
                }

                inOpenQuoteState = !inOpenQuoteState;
            }
            else if (character == comma && !inOpenQuoteState && prevCharacter != quote)
            {
                values.Add(sb.ToString());
                sb.Clear();
            }
            else if (character != comma || inOpenQuoteState)
            {
                sb.Append(character);
            }
        }

        return [.. values];
    }

    private static string GetCsvHeaderName(PropertyInfo property)
    {
        CsvColumnAttribute? csvColumnAttribute = property.GetCustomAttribute<CsvColumnAttribute>();
        return string.IsNullOrWhiteSpace(csvColumnAttribute?.Header) ? property.Name : csvColumnAttribute.Header;
    }

    private static IEnumerable<PropertyInfo> GetCsvProperties(Type type) => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<CsvIgnoreAttribute>() == null);

    private record ParsedCsvData
    {
        public required string[] Headers { get; init; }
        public required string[] Values { get; init; }
    }
}
