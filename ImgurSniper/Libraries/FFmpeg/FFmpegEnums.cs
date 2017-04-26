using System.ComponentModel;

namespace ImgurSniper.Libraries.FFmpeg {
    public enum FFmpegVideoCodec {
        [Description("x264 (mp4)")]
        libx264,
        [Description("VP8 (webm)")]
        libvpx,
        [Description("Xvid (avi)")]
        libxvid,
        [Description("Animated GIF (gif)")]
        gif,
        [Description("x265 (mp4)")]
        libx265,
        [Description("H.264 NVENC (mp4)")]
        h264_nvenc,
        [Description("HEVC NVENC (mp4)")]
        hevc_nvenc
    }
    public enum FFmpegAudioCodec {
        [Description("AAC")]
        libvoaacenc,
        [Description("Vorbis")]
        libvorbis,
        [Description("MP3")]
        libmp3lame
    }
    public enum FFmpegPreset {
        [Description("Ultra fast")]
        ultrafast,
        [Description("Super fast")]
        superfast,
        [Description("Very fast")]
        veryfast,
        [Description("Faster")]
        faster,
        [Description("Fast")]
        fast,
        [Description("Medium")]
        medium,
        [Description("Slow")]
        slow,
        [Description("Slower")]
        slower,
        [Description("Very slow")]
        veryslow
    }

    public enum FFmpegNVENCPreset {
        [Description("Default")]
        @default,
        [Description("High quality 2 passes")]
        slow,
        [Description("High quality 1 pass")]
        medium,
        [Description("High performance 1 pass")]
        fast,
        [Description("High performance")]
        hp,
        [Description("High quality")]
        hq,
        [Description("Bluray disk")]
        bd,
        [Description("Low latency")]
        ll,
        [Description("Low latency high quality")]
        llhq,
        [Description("Low latency high performance")]
        llhp,
        [Description("Lossless")]
        lossless,
        [Description("Lossless high performance")]
        losslesshp
    }
    public enum FFmpegTune {
        film, animation, grain, stillimage, psnr, ssim, fastdecode, zerolatency
    }

    public enum FFmpegPaletteGenStatsMode {
        full, diff
    }

    public enum FFmpegPaletteUseDither {
        none,
        bayer,
        heckbert,
        floyd_steinberg,
        sierra2,
        sierra2_4a
    }

}
