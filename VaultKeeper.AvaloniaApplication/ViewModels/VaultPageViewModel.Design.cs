using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

[ExcludeFromCodeCoverage]
public partial class VaultPageViewModel
{
    public static readonly DesignContext Design = new();

    public class DesignContext() : VaultPageViewModel(null!, null!, null!, null!)
    {
        public override async Task LoadDataAsync()
        {
            SetVaultItems(Enumerable.Range(1, 10).Select(x => new VaultItem
            {
                Name = $"Name {x}",
                Value = $"Value {x}"
            }));

            SetGroups(Enumerable.Range(1, 10).Select(x => new Group
            {
                Id = Guid.NewGuid(),
                Name = $"Group {x}",
            }));
        }
    }
}