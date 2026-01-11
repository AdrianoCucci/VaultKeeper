using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    public Result<string> Serialize<T>(IEnumerable<T> records, Encoding? encoding = null) where T : class
    {
        logger.LogInformation(nameof(Serialize));

        try
        {
            IEnumerable<PropertyInfo> properties = GetCsvProperties(typeof(T));

            encoding ??= Encoding.UTF8;
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                Encoding = encoding
            };

            byte[] serializedBytes;

            using (MemoryStream memoryStream = new())
            {
                using StreamWriter writer = new(memoryStream);
                using CsvWriter csvWriter = new(writer, config);

                DefaultClassMap<T> classMap = new();

                foreach (PropertyInfo property in properties)
                {
                    string header = GetCsvHeaderName(property);
                    MemberMap map = classMap.Map(typeof(T), property);
                    map.Name(header);
                }

                csvWriter.Context.RegisterClassMap(classMap);
                csvWriter.WriteRecords(records);
                csvWriter.Flush();
                
                serializedBytes = memoryStream.ToArray();
            }

            string serializedText = encoding.GetString(serializedBytes);
            return serializedText.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<IEnumerable<T>> Deserialize<T>(string csvText, Encoding? encoding = null) where T : class, new()
    {
        logger.LogInformation(nameof(Deserialize));

        try
        {
            if (string.IsNullOrWhiteSpace(csvText))
                return Enumerable.Empty<T>().ToOkResult().Logged(logger);

            IEnumerable<PropertyInfo> properties = GetCsvProperties(typeof(T));
            Dictionary<string, string> typeHeadersMap = properties
                .Select(x => new KeyValuePair<string, string>(x.Name, GetCsvHeaderName(x)))
                .ToDictionary();

            encoding ??= Encoding.UTF8;
            List<T> records = [];

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                Encoding = encoding,
                MissingFieldFound = null,
            };

            using (MemoryStream memoryStream = new(encoding.GetBytes(csvText)))
            {
                using StreamReader reader = new(memoryStream);
                using CsvReader csvReader = new(reader, config);

                _ = csvReader.Read();
                _ = csvReader.ReadHeader();
                string[] dataHeaders = csvReader.HeaderRecord ?? [];
                string[] missingHeaders = [.. typeHeadersMap.Select(kvp => kvp.Value).Where(x => !dataHeaders.Contains(x))];

                if (missingHeaders.Length > 0)
                    return Result.Failed<IEnumerable<T>>(ResultFailureType.BadRequest, $"CSV data is missing the following headers:\n{string.Join(", ", missingHeaders.Select(x => $"\"{x}\""))}");

                while (csvReader.Read())
                {
                    T record = new();

                    foreach (PropertyInfo property in properties)
                    {
                        string header = typeHeadersMap[property.Name];
                        object? value = csvReader.GetField(property.PropertyType, header);
                        property.SetValue(record, value);
                    }

                    records.Add(record);
                }
            }

            return Result.Ok<IEnumerable<T>>(records).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<T>>().Logged(logger);
        }
    }

    private static string GetCsvHeaderName(PropertyInfo property)
    {
        CsvColumnAttribute? csvColumnAttribute = property.GetCustomAttribute<CsvColumnAttribute>();
        return string.IsNullOrWhiteSpace(csvColumnAttribute?.Header) ? property.Name : csvColumnAttribute.Header;
    }

    private static IEnumerable<PropertyInfo> GetCsvProperties(Type type) => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<CsvIgnoreAttribute>() == null);
}
