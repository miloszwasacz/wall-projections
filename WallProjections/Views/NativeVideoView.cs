using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.VisualTree;
using WallProjections.Models.Interfaces;

namespace WallProjections.Views;

/// <summary>
/// Avalonia VideoView for Windows, macOS and Linux.
/// </summary>
public class NativeVideoView : NativeControlHost
{
    public static readonly DirectProperty<NativeVideoView, IMediaPlayer?> MediaPlayerProperty =
        AvaloniaProperty.RegisterDirect<NativeVideoView, IMediaPlayer?>(
            nameof(MediaPlayer),
            o => o.MediaPlayer,
            (o, v) => o.MediaPlayer = v,
            defaultBindingMode: BindingMode.TwoWay
        );

    private readonly IDisposable _playerHandler;
    private readonly BehaviorSubject<IMediaPlayer?> _mediaPlayers = new(null);
    private readonly BehaviorSubject<IPlatformHandle?> _platformHandles = new(null);

    public IPlatformHandle? Handle;

    private IDisposable? _disposables;
    private IDisposable? _isEffectivelyVisible;

    public NativeVideoView()
    {
        _playerHandler = _platformHandles.WithLatestFrom(_mediaPlayers).Subscribe(x =>
        {
            var (handle, player) = x;
            if (handle is not null && player is not null)
                player.SetHandle(handle);
        });
    }

    public IMediaPlayer? MediaPlayer
    {
        get => _mediaPlayers.Value;
        set => _mediaPlayers.OnNext(value);
    }

    /// <inheritdoc />
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);
        _platformHandles.OnNext(handle);
        Handle = handle;
        return handle;
    }

    /// <inheritdoc />
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _playerHandler.Dispose();
        base.DestroyNativeControlCore(control);
        _mediaPlayers.Dispose();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isEffectivelyVisible = this.GetVisualAncestors().OfType<Control>()
            .Select(v => v.GetObservable(IsVisibleProperty))
            .CombineLatest(v => v.All(o => o))
            .DistinctUntilChanged()
            .Subscribe(v => IsVisible = v);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isEffectivelyVisible?.Dispose();
        _isEffectivelyVisible = null;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _disposables?.Dispose();
        _disposables = null;
    }
}
