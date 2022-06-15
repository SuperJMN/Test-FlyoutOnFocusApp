using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AvaloniaApplication10;

public class FlyoutOnFocusBehavior : Behavior<Control>
{
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();

        if (AssociatedObject is null)
        {
            return;
        }

        var flyoutBase = FlyoutBase.GetAttachedFlyout(AssociatedObject);
        if (flyoutBase is null)
        {
            return;
        }

        if (AssociatedObject.GetVisualRoot() is not Control visualRoot)
        {
            return;
        }

        var flyoutController = new FlyoutController(flyoutBase, AssociatedObject);

        OnPressed(visualRoot)
            .Select(ea => ea.EventArgs.Source)
            .Where(interactive => interactive is not LightDismissOverlayLayer)
            .Select(interactive => IsVisualAncestor(interactive, AssociatedObject))
            .Do(isAncestor => flyoutController.IsOpen = isAncestor)
            .Subscribe();

        Observable.FromEventPattern(AssociatedObject, nameof(InputElement.GotFocus)).Subscribe(_ => flyoutController.IsOpen = true);
    }

    private static bool IsVisualAncestor(IInteractive? interactive, IVisual associatedObject)
    {
        return interactive is Visual visual && associatedObject.IsVisualAncestorOf(visual);
    }

    private static IObservable<EventPattern<PointerPressedEventArgs>> OnPressed(IInteractive visualRoot)
    {
        var parentPressed = Observable.FromEventPattern<PointerPressedEventArgs>(
            add => visualRoot.AddHandler(InputElement.PointerPressedEvent, add, RoutingStrategies.Tunnel),
            action => visualRoot.RemoveHandler(InputElement.PointerPressedEvent, action));
        return parentPressed;
    }
}