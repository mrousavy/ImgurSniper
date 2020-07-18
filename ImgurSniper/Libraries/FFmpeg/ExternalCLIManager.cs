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


using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ImgurSniper.Libraries.FFmpeg {
    public abstract class ExternalCliManager : IDisposable {
        public event DataReceivedEventHandler OutputDataReceived;
        public event DataReceivedEventHandler ErrorDataReceived;

        protected Process Process;
        protected bool ProcessRunning;

        public virtual int Open(string path, string args = null) {
            Console.WriteLine($"CLI: \"{path}\" {args}");

            if (File.Exists(path)) {
                using (Process = new Process()) {
                    ProcessStartInfo psi = new ProcessStartInfo(path) {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Arguments = args,
                        WorkingDirectory = Path.GetDirectoryName(path),
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };
                    Process.EnableRaisingEvents = true;
                    if (psi.RedirectStandardOutput) Process.OutputDataReceived += Cli_OutputDataReceived;
                    if (psi.RedirectStandardError) Process.ErrorDataReceived += Cli_ErrorDataReceived;
                    Process.StartInfo = psi;
                    Process.Start();
                    if (psi.RedirectStandardOutput) Process.BeginOutputReadLine();
                    if (psi.RedirectStandardError) Process.BeginErrorReadLine();

                    try {
                        ProcessRunning = true;
                        Process.WaitForExit();
                    } finally {
                        ProcessRunning = false;
                    }

                    return Process.ExitCode;
                }
            }

            return -1;
        }

        private void Cli_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                OutputDataReceived?.Invoke(sender, e);
            }
        }

        private void Cli_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                ErrorDataReceived?.Invoke(sender, e);
            }
        }

        public void WriteInput(string input) {
            if (ProcessRunning && Process?.StartInfo != null && Process.StartInfo.RedirectStandardInput) {
                Process.StandardInput.WriteLine(input);
            }
        }

        public virtual void Close() {
            if (ProcessRunning) {
                Process?.CloseMainWindow();
            }
        }

        public void Dispose() {
            Process?.Dispose();
        }
    }
}
