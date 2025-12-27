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
    private const char _separator = ',';

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
            sb.AppendJoin(_separator, headers);

            foreach (var row in valueRows)
            {
                sb.AppendLine();
                sb.AppendJoin(_separator, row);
            }

            return sb.ToString().ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<IEnumerable<T>> Deserialize<T>(string csv) where T : class, new()
    {
        logger.LogInformation(nameof(Deserialize));

        try
        {
            if (string.IsNullOrWhiteSpace(csv))
                return Enumerable.Empty<T>().ToOkResult().Logged(logger);

            string[] csvRows = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            if (csvRows.Length < 2)
                return Enumerable.Empty<T>().ToOkResult().Logged(logger);

            string[] headers = csvRows[0].Split(_separator);
            IEnumerable<PropertyInfo> properties = GetCsvProperties(typeof(T));
            List<T> objs = [];

            for (int csvRowIndex = 1; csvRowIndex < csvRows.Length; csvRowIndex++)
            {
                string csvRow = csvRows[csvRowIndex];
                string[] values = csvRow.Split(_separator);
                T obj = new();

                for (int valueRowIndex = 0; valueRowIndex < values.Length; valueRowIndex++)
                {
                    string value = values[valueRowIndex];
                    string header = headers[valueRowIndex];

                    PropertyInfo? property = properties.FirstOrDefault(x =>
                    {
                        CsvColumnAttribute? csvColumnAttribute = x.GetCustomAttribute<CsvColumnAttribute>();
                        return string.IsNullOrWhiteSpace(csvColumnAttribute?.Header) ? x.Name == header : csvColumnAttribute.Header == header;
                    });

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

    private static IEnumerable<PropertyInfo> GetCsvProperties(Type type) => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<CsvIgnoreAttribute>() == null);
}
