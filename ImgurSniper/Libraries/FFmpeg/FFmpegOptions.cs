using ImgurSniper.Libraries.Helper;
using System;

namespace ImgurSniper.Libraries.FFmpeg {
    public class FFmpegOptions {
        // General
        public bool OverrideCliPath { get; set; } = false;
        public string CliPath { get; set; } = "";
        public string VideoSource { get; set; } = FFmpegHelper.SourceGdiGrab;
        public string AudioSource { get; set; } = FFmpegHelper.SourceNone;
        public FFmpegVideoCodec VideoCodec { get; set; } = FFmpegVideoCodec.libx264;
        public FFmpegAudioCodec AudioCodec { get; set; } = FFmpegAudioCodec.libvoaacenc;
        public string UserArgs { get; set; } = "";
        public bool UseCustomCommands { get; set; } = false;
        public string CustomCommands { get; set; } = "";
        public bool ShowError { get; set; } = true;

        // Video
        public FFmpegPreset X264Preset { get; set; } = FFmpegPreset.ultrafast;
        public int X264Crf { get; set; } = 28;
        public int VPxBitrate { get; set; } = 3000; // kbit/s
        public int XviDQscale { get; set; } = 10;
        public FFmpegNVENCPreset NvencPreset { get; set; } = FFmpegNVENCPreset.llhp;
        public int NvencBitrate { get; set; } = 3000; // kbit/s
        public FFmpegPaletteGenStatsMode GifStatsMode { get; set; } = FFmpegPaletteGenStatsMode.full;
        public FFmpegPaletteUseDither GifDither { get; set; } = FFmpegPaletteUseDither.sierra2_4a;

        // Audio
        public int AacBitrate { get; set; } = 128; // kbit/s
        public int VorbisQscale { get; set; } = 3;
        public int Mp3Qscale { get; set; } = 4;

        public string FFmpegPath => !string.IsNullOrEmpty(CliPath) ? Helpers.GetAbsolutePath(CliPath) : "";

        public string Extension {
            get {
                if (!VideoSource.Equals(FFmpegHelper.SourceNone, StringComparison.InvariantCultureIgnoreCase)) {
                    switch (VideoCodec) {
                        case FFmpegVideoCodec.libx264:
                        case FFmpegVideoCodec.libx265:
                        case FFmpegVideoCodec.h264_nvenc:
                        case FFmpegVideoCodec.hevc_nvenc:
                        case FFmpegVideoCodec.gif:
                            return "mp4";
                        case FFmpegVideoCodec.libvpx:
                            return "webm";
                        case FFmpegVideoCodec.libxvid:
                            return "avi";
                    }
                } else if (!AudioSource.Equals(FFmpegHelper.SourceNone, StringComparison.InvariantCultureIgnoreCase)) {
                    switch (AudioCodec) {
                        case FFmpegAudioCodec.libvoaacenc:
                            return "m4a";
                        case FFmpegAudioCodec.libvorbis:
                            return "ogg";
                        case FFmpegAudioCodec.libmp3lame:
                            return "mp3";
                    }
                }

                return "mp4";
            }
        }

        public bool IsSourceSelected => IsVideoSourceSelected || IsAudioSourceSelected;

        public bool IsVideoSourceSelected => !string.IsNullOrEmpty(VideoSource) && !VideoSource.Equals(FFmpegHelper.SourceNone, StringComparison.InvariantCultureIgnoreCase);

        public bool IsAudioSourceSelected => !string.IsNullOrEmpty(AudioSource) && !AudioSource.Equals(FFmpegHelper.SourceNone, StringComparison.InvariantCultureIgnoreCase) &&
            (!IsVideoSourceSelected || VideoCodec != FFmpegVideoCodec.gif);

        public FFmpegOptions() {
        }

        public FFmpegOptions(string ffmpegPath) {
            CliPath = Helpers.GetVariableFolderPath(ffmpegPath);
        }
    }
}
