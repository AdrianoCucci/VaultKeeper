using System;

namespace VaultKeeper.Models.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CsvColumnAttribute() : Attribute
{
    public string? Header { get; init; }
}