using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
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

        var isFocused = AssociatedObject.GetObservable(InputElement.IsFocusedProperty);

        var visualRoot = AssociatedObject.GetVisualRoot() as Control;

        var parentPressed = Observable.FromEventPattern<PointerPressedEventArgs>(
            add => visualRoot.AddHandler(InputElement.PointerPressedEvent, add, RoutingStrategies.Tunnel),
            action => visualRoot.RemoveHandler(InputElement.PointerPressedEvent, action));

        parentPressed.WithLatestFrom(isFocused, (unit, b) =>
            {
                return new { b, unit };
            })
            .Where(arg => arg.unit.EventArgs.Source is not LightDismissOverlayLayer)
            .Select(arg => arg.unit.EventArgs.Source is Visual v ? AssociatedObject.IsVisualAncestorOf(v) : false)
            .Subscribe(x =>
            {
                OnParentPressed(x);
            });

        Observable.FromEventPattern(AssociatedObject, nameof(InputElement.GotFocus)).Subscribe(_ => ShowFlyout());
        //Observable.FromEventPattern(AssociatedObject, nameof(InputElement.LostFocus)).Subscribe(_ => HideFlyout());
        Observable.FromEventPattern(visualRoot, nameof(Window.Activated)).Subscribe(_ =>
        {
            if (AssociatedObject.IsFocused)
            {
                ShowFlyout();
            }
        });
    }

    private void RejectClose(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    private void OnParentPressed(bool shouldShow)
    {
        if (shouldShow)
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