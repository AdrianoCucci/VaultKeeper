using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemView : UserControl
{

    public static readonly RoutedEvent<RoutedEventArgs> RevealValueClickEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(RevealValueClick), RoutingStrategies.Bubble, typeof(VaultItemView));

    public event EventHandler<RoutedEventArgs> RevealValueClick
    {
        add => AddHandler(RevealValueClickEvent, value);
        remove => RemoveHandler(RevealValueClickEvent, value);
    }

    public VaultItemViewModel? Model => DataContext as VaultItemViewModel;

    public VaultItemView() => InitializeComponent();

    public void ToggleRevealValue() => UpdateModel(x =>
    {
        x.ValueRevealed = !x.ValueRevealed;

        string icon = x.ValueRevealed ? Icons.LockOpened : Icons.LockClosed;
        IconLock.Bind(PathIcon.DataProperty, IconLock.Resources.GetResourceObservable(icon));

        InputValue.PasswordChar = x.ValueRevealed ? default : '*';
        InputValue.IsReadOnly = x.IsReadOnly || !x.ValueRevealed;

        RaiseEvent(new(RevealValueClickEvent));
    });

    protected override void OnPointerEntered(PointerEventArgs e) => SetPointerEntered(true);

    protected override void OnPointerExited(PointerEventArgs e) => SetPointerEntered(false);

    private void SetPointerEntered(bool pointerEntered) => UpdateModel(x =>
    {
        x.IsPointerEntered = pointerEntered;
        x.PointerEnteredContentOpacity = pointerEntered ? 1 : 0;
    });

    private void SetFocused(bool focused) => UpdateModel(x =>
    {
        x.HasFocus = focused;
        x.NameColumnSpan = focused ? 1 : 2;
    });

    private async Task CopyPropertyToClipboardAsync(Func<VaultItemViewModel, string> selector)
    {
        if (Model != null)
        {
            IClipboard? clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                string text = selector.Invoke(Model);
                await clipboard.SetTextAsync(text);
            }
        }
    }

    private void UpdateModel(Action<VaultItemViewModel> updateAction)
    {
        if (Model != null)
            updateAction.Invoke(Model);
    }

    private void AnyInput_GotFocus(object? sender, GotFocusEventArgs e) => SetFocused((e.Source is TextBox || Model?.HasFocus == true) && Model?.IsReadOnly == false);

    private void AnyInput_LostFocus(object? sender, RoutedEventArgs e) => SetFocused(IsKeyboardFocusWithin);

    private void ToggleRevealValue_Click(object? sender, RoutedEventArgs e) => ToggleRevealValue();

    private async void ButtonCopyName_Click(object? sender, RoutedEventArgs e) => await CopyPropertyToClipboardAsync(x => x.Name);

    private async void ButtonCopyValue_Click(object? sender, RoutedEventArgs e) => await CopyPropertyToClipboardAsync(x => x.Value);
}