using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.Setup;

namespace VaultKeeper.AvaloniaApplication.Views.Setup;

public partial class SetupPageStep1View : ViewBase<SetupPageStep1ViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> FormSubmittedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(FormSubmitted), RoutingStrategies.Bubble, typeof(SetupPageStep1View));

    public static readonly RoutedEvent<RoutedEventArgs> BackupLoadedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(BackupLoaded), RoutingStrategies.Bubble, typeof(SetupPageStep1View));


    public event EventHandler<RoutedEventArgs> FormSubmitted { add => AddHandler(FormSubmittedEvent, value); remove => RemoveHandler(FormSubmittedEvent, value); }

    public event EventHandler<RoutedEventArgs> BackupLoaded { add => AddHandler(BackupLoadedEvent, value); remove => RemoveHandler(BackupLoadedEvent, value); }

    public SetupPageStep1View() => InitializeComponent();

    private async void SetPasswordFormView_Submitted(object? sender, FormActionEventArgs<SetPasswordForm> e)
    {
        if (Model == null) return;

        bool didComplete = await Model.ProcessFormSubmissionAsync(e.Form);
        if (didComplete)
            RaiseEvent(new(FormSubmittedEvent, this));
    }

    private async void ImportDataButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model == null) return;

        bool didComplete = await Model.ImportBackupDataAsync();
        if (didComplete)
            RaiseEvent(new(BackupLoadedEvent, this));
    }
}