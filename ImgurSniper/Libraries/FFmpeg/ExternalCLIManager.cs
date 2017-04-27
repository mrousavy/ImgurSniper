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
