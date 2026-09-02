using Windows.Storage.Streams;

namespace HyperMoeland.Models;

/// <summary>媒体会话快照（曲名 / 歌手 / 封面 / 播放状态）。</summary>
internal sealed record MediaSessionInfo(
    string Title,
    string Artist,
    IRandomAccessStreamReference? Thumbnail,
    bool IsPlaying);
