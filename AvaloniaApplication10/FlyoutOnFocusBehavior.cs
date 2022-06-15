using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using ReactiveUI;
using VisualExtensions = Avalonia.VisualExtensions;

namespace AvaloniaApplication10;

public class FlyoutOnFocusBehavior : Behavior<Control>
{
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();

        var visualRoot = AssociatedObject.GetVisualRoot() as Control;
        OnPressed(visualRoot)
            .Where(args => args.EventArgs.Source is not LightDismissOverlayLayer)
            .Select(args => args.EventArgs.Source is Visual v ? AssociatedObject.IsVisualAncestorOf(v) : false)
            .Do(ToggleFlyout)
            .Subscribe();

        Observable.FromEventPattern(AssociatedObject, nameof(InputElement.GotFocus)).Subscribe(_ => ShowFlyout());
    }

    private static IObservable<EventPattern<PointerPressedEventArgs>> OnPressed(Control? visualRoot)
    {
        var parentPressed = Observable.FromEventPattern<PointerPressedEventArgs>(
            add => visualRoot.AddHandler(InputElement.PointerPressedEvent, add, RoutingStrategies.Tunnel),
            action => visualRoot.RemoveHandler(InputElement.PointerPressedEvent, action));
        return parentPressed;
    }

    private void RejectClose(object? sender, CancelEventArgs e)
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
        var flyout = GetFlyout();

        if (!flyout.IsOpen)
        {
            return;
        }

        flyout.Closing -= RejectClose;
        flyout.Closing += DoNothing;
        flyout.Hide(); 
        ((IPopupHostProvider)flyout).PopupHost?.Hide();
    }

    private void ShowFlyout()
    {
        var flyout = GetFlyout();


        if (flyout.IsOpen)
        {
            return;
        }

        flyout.Closing += RejectClose;
        flyout.Closing -= DoNothing;
        GetFlyout().ShowAt(AssociatedObject);
    }

    private void DoNothing(object? sender, CancelEventArgs e)
    {
    }

    public FlyoutBase? GetFlyout()
    {
        return FlyoutBase.GetAttachedFlyout(AssociatedObject);
    }
}