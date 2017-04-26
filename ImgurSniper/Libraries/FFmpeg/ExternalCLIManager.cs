using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ImgurSniper.Libraries.FFmpeg {
    public abstract class ExternalCLIManager : IDisposable {
        public event DataReceivedEventHandler OutputDataReceived;
        public event DataReceivedEventHandler ErrorDataReceived;

        protected Process process;
        protected bool processRunning;

        public virtual int Open(string path, string args = null) {
            Console.WriteLine("CLI: \"{0}\" {1}", path, args);

            if (File.Exists(path)) {
                using (process = new Process()) {
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
                    process.EnableRaisingEvents = true;
                    if (psi.RedirectStandardOutput) process.OutputDataReceived += Cli_OutputDataReceived;
                    if (psi.RedirectStandardError) process.ErrorDataReceived += Cli_ErrorDataReceived;
                    process.StartInfo = psi;
                    process.Start();
                    if (psi.RedirectStandardOutput) process.BeginOutputReadLine();
                    if (psi.RedirectStandardError) process.BeginErrorReadLine();

                    try {
                        processRunning = true;
                        process.WaitForExit();
                    } finally {
                        processRunning = false;
                    }

                    return process.ExitCode;
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
            if (processRunning && process != null && process.StartInfo != null && process.StartInfo.RedirectStandardInput) {
                process.StandardInput.WriteLine(input);
            }
        }

        public virtual void Close() {
            if (processRunning && process != null) {
                process.CloseMainWindow();
            }
        }

        public void Dispose() {
            if (process != null) {
                process.Dispose();
            }
        }
    }
}
