using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace VaultKeeper.AvaloniaApplication.DataTemplates.Common;

public class SwitchTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> ValueTemplates { get; } = [];

    public IDataTemplate? DefaultTemplate { get; set; }

    public Control? Build(object? param)
    {
        var key = param?.ToString();

        if (!string.IsNullOrWhiteSpace(key) && ValueTemplates.TryGetValue(key, out IDataTemplate? template))
            return template!.Build(param);

        return DefaultTemplate?.Build(param);
    }

    public bool Match(object? data)
    {
        var key = data?.ToString();
        return (!string.IsNullOrWhiteSpace(key) && ValueTemplates.ContainsKey(key)) || DefaultTemplate != null;
    }
}
