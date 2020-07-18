#region License Information (GPL v3)

/*
    Source code provocatively stolen from ShareX: https://github.com/ShareX/ShareX.
    (Seriously, awesome work over there, I took some parts of the Code to make
    ImgurSniper.)
    Their License:

    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2017 ShareX Team
    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using ImgurSniper.Libraries.FFmpeg;
using ImgurSniper.Libraries.Helper;
using System;

namespace ImgurSniper.Libraries.ScreenCapture {
    public class ScreenRecorder : IDisposable {

        public bool IsRecording { get; private set; }

        public string CachePath { get; }

        public ScreencastOptions Options { get; set; }

        public event Action RecordingStarted;

        private readonly FFmpegHelper _ffmpegCli;

        public ScreenRecorder(ScreencastOptions options) {
            if (string.IsNullOrEmpty(options.OutputPath)) {
                throw new Exception("Screen recorder cache path is empty.");
            }

            CachePath = options.OutputPath;

            Options = options;

            _ffmpegCli = new FFmpegHelper(Options);
            _ffmpegCli.RecordingStarted += OnRecordingStarted;
        }

        public void StartRecording() {
            if (!IsRecording) {
                IsRecording = true;

                _ffmpegCli.Record();
            }

            IsRecording = false;
        }

        public void StopRecording() {
            _ffmpegCli?.Close();
        }

        public bool FFmpegEncodeAsGif(string path) {
            Helpers.CreateDirectoryFromFilePath(path);
            return _ffmpegCli.EncodeGif(Options.OutputPath, path);
        }

        protected void OnRecordingStarted() {
            RecordingStarted?.Invoke();
        }

        public void Dispose() {
            _ffmpegCli?.Close();
            _ffmpegCli?.Dispose();
        }
    }
}
