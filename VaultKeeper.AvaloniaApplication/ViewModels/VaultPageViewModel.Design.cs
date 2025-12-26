using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

[ExcludeFromCodeCoverage]
public partial class VaultPageViewModel
{
    public static readonly DesignContext Design = new();

    public class DesignContext : VaultPageViewModel
    {
        public DesignContext() : base(null!, null!, null!)
        {
            VaultItems = [..Enumerable
                .Range(1, 10)
                .Select(x => new VaultItemViewModel(new()
                {
                    Name = $"Name {x}",
                    Value = $"Value {x}"
                }))];
        }
    }
}
