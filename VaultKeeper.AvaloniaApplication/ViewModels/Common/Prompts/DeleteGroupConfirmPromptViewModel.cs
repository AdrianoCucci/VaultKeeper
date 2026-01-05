using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.Common.Exceptions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class DeleteGroupConfirmPromptViewModel : ConfirmPromptViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(DeleteWithChildren))]
    private CascadeDeleteMode _cascadeDeleteMode;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasCascadeDeleteException), nameof(CascadeDeleteExceptionChildrenNames))]
    private CascadeDeleteException<Group, VaultItem>? _cascadeDeleteException;

    public bool DeleteWithChildren => CascadeDeleteMode == CascadeDeleteMode.DeleteChildren;
    public bool HasCascadeDeleteException => CascadeDeleteException != null;
    public IEnumerable<string> CascadeDeleteExceptionChildrenNames => CascadeDeleteException?.ConflictingChildren.Select(x => x.Name) ?? [];
}
