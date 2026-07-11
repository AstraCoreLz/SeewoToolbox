using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;

namespace SeewoToolbox.Services;



public class WrapPanelResizingAnimationAssist
{
    public static readonly AttachedProperty<bool> IsResizingAnimationEnabledProperty =
        AvaloniaProperty.RegisterAttached<WrapPanelResizingAnimationAssist, Panel, bool>("IsResizingAnimationEnabled");

    public static void SetIsResizingAnimationEnabled(Panel obj, bool value) => obj.SetValue(IsResizingAnimationEnabledProperty, value);
    public static bool GetIsResizingAnimationEnabled(Panel obj) => obj.GetValue(IsResizingAnimationEnabledProperty);

    public static readonly AttachedProperty<Easing> EasingProperty =
        AvaloniaProperty.RegisterAttached<WrapPanelResizingAnimationAssist, Visual, Easing>("Easing", new CubicEaseInOut(), inherits: true);

    public static void SetEasing(Visual obj, Easing value) => obj.SetValue(EasingProperty, value);
    public static Easing GetEasing(Visual obj) => obj.GetValue(EasingProperty);

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<WrapPanelResizingAnimationAssist, Visual, TimeSpan>("Duration", TimeSpan.FromMilliseconds(225), inherits: true);

    public static void SetDuration(Visual obj, TimeSpan value) => obj.SetValue(DurationProperty, value);
    public static TimeSpan GetDuration(Visual obj) => obj.GetValue(DurationProperty);

    static WrapPanelResizingAnimationAssist()
    {
        IsResizingAnimationEnabledProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is Panel panel)
                HandleIsResizingAnimationEnabledChanged(panel, args.NewValue.Value);
        });
    }

    private static void HandleIsResizingAnimationEnabledChanged(Panel panel, bool enabled)
    {
        if (enabled)
        {
            foreach (var child in panel.Children.OfType<Control>())
            {
                TrySetupControl(child);
            }
            panel.Children.CollectionChanged += ChildrenOnCollectionChanged;
        }
        else
        {
            foreach (var child in panel.Children.OfType<Control>())
            {
                ClearControlAnimation(child);
            }
            panel.Children.CollectionChanged -= ChildrenOnCollectionChanged;
        }
    }

    private static void TrySetupControl(Control control)
    {
        if (!control.IsLoaded)
        {
            control.Loaded += (_, _) => SetupControlAnimation(control);
            return;
        }
        SetupControlAnimation(control);
    }

    private static void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems?.OfType<Control>() ?? Enumerable.Empty<Control>())
        {
            TrySetupControl(item);
        }
        foreach (var item in e.OldItems?.OfType<Control>() ?? Enumerable.Empty<Control>())
        {
            ClearControlAnimation(item);
        }
    }

    private static void SetupControlAnimation(Control control)
    {
        var compositionVisual = ElementComposition.GetElementVisual(control);
        if (compositionVisual == null)
        {
            control.AttachedToVisualTree += (_, _) => SetupControlAnimation(control);
            return;
        }

        var compositor = compositionVisual.Compositor;
        var easing = GetEasing(control);
        var duration = GetDuration(control);

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = nameof(compositionVisual.Offset);
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", easing);
        offsetAnimation.Duration = duration;

        var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Target = nameof(compositionVisual.Opacity);
        opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", easing);
        opacityAnimation.Duration = duration;

        var implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection[nameof(compositionVisual.Offset)] = offsetAnimation;
        implicitAnimationCollection[nameof(compositionVisual.Opacity)] = opacityAnimation;
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;
    }

    private static void ClearControlAnimation(Control control)
    {
        var compositionVisual = ElementComposition.GetElementVisual(control);
        if (compositionVisual == null) return;
        var compositor = compositionVisual.Compositor;
        compositionVisual.ImplicitAnimations = compositor.CreateImplicitAnimationCollection();
    }
}



public class PopupIntroAnimationBehavior
{
    public static readonly AttachedProperty<bool> IsIntroAnimationEnabledProperty =
        AvaloniaProperty.RegisterAttached<PopupIntroAnimationBehavior, Window, bool>("IsIntroAnimationEnabled");

    public static void SetIsIntroAnimationEnabled(Window obj, bool value) => obj.SetValue(IsIntroAnimationEnabledProperty, value);
    public static bool GetIsIntroAnimationEnabled(Window obj) => obj.GetValue(IsIntroAnimationEnabledProperty);

    static PopupIntroAnimationBehavior()
    {
        IsIntroAnimationEnabledProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is Window window && args.NewValue.Value)
            {
                window.Opened += WindowOnOpened;
            }
        });
    }

    private static void WindowOnOpened(object? sender, EventArgs e)
    {
        if (sender is not Window window)
            return;

        window.Opened -= WindowOnOpened;

        var visual = ElementComposition.GetElementVisual(window);
        if (visual == null) return;

        var compositor = visual.Compositor;

        var animationOpacity = compositor.CreateScalarKeyFrameAnimation();
        animationOpacity.Target = nameof(visual.Opacity);
        animationOpacity.Duration = TimeSpan.FromSeconds(0.15);
        animationOpacity.InsertKeyFrame(0f, 0f);
        animationOpacity.InsertKeyFrame(1f, 1f, Easing.Parse("0.22, 1, 0.36, 1"));
        visual.StartAnimation(nameof(visual.Opacity), animationOpacity);

        visual.CenterPoint = new Vector3D(window.Bounds.Width / 2, window.Bounds.Height / 2, 0);
        var animationScale = compositor.CreateVector3DKeyFrameAnimation();
        animationScale.Target = nameof(visual.Scale);
        animationScale.Duration = TimeSpan.FromSeconds(0.15);
        animationScale.InsertKeyFrame(0f, visual.Scale with { X = 0.925, Y = 0.925 });
        animationScale.InsertKeyFrame(1f, visual.Scale with { X = 1, Y = 1 }, Easing.Parse("0.22, 1, 0.36, 1"));
        visual.StartAnimation(nameof(visual.Scale), animationScale);
    }
}

