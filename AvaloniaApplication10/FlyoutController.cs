using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;

namespace AvaloniaApplication10;

public class FlyoutController
{
    private readonly FlyoutBase flyout;
    private readonly Control parent;
    private bool isOpen;

    public FlyoutController(FlyoutBase flyout, Control parent)
    {
        this.flyout = flyout;
        this.parent = parent;
    }

    public bool IsOpen
    {
        get => isOpen;
        set
        {
            ToggleFlyout(value);
            isOpen = value;
        }
    }

    private static void DoNothing(object? sender, CancelEventArgs e)
    {
    }

    private static void RejectClose(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    private void ToggleFlyout(bool isVisible)
    {
        if (isVisible)
        {
            ShowFlyout();
        }
        else
        {
            HideFlyout();
        }
    }

    private void HideFlyout()
    {
        if (!flyout.IsOpen)
        {
            return;
        }

        flyout.Closing -= RejectClose;
        flyout.Closing += DoNothing;
        flyout.Hide();
        ((IPopupHostProvider) flyout).PopupHost?.Hide();
    }

    private void ShowFlyout()
    {
        if (flyout.IsOpen)
        {
            return;
        }

        flyout.Closing += RejectClose;
        flyout.Closing -= DoNothing;
        flyout.ShowAt(parent);
    }
}