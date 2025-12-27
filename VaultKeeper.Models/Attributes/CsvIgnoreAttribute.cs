using System;

namespace VaultKeeper.Models.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CsvIgnoreAttribute : Attribute;