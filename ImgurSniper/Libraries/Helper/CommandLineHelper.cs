using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImgurSniper.Libraries.Helper {
    public static class CommandLineHelper {
        public enum Argument {
            Snipe,
            Autostart,
            Gif,
            Upload
        }
        private static readonly string[] Parameters = { "autostart", "gif", "upload" };


        /// <summary>
        /// Get Command line arguments from this Application
        /// </summary>
        /// <returns>Commandline Arguments object with Argument and File List if instant upload</returns>
        public static CommandlineArgs GetCommandlineArguments() {
            List<string> args = new List<string>(Environment.GetCommandLineArgs());

            if (args.Count < 1)
                return new CommandlineArgs(Argument.Snipe);

            // "-autostart" or "/autostart" -> "autostart"
            Regex regexParam = new Regex("^(-|/)");
            args = new List<string>(args.Select(arg => regexParam.Replace(arg, "")));

            //Get Parameter from Arguments
            string param = args.FirstOrDefault(arg => Parameters.Contains(arg));

            switch (param) {
                case "autostart":
                    return new CommandlineArgs(Argument.Autostart);
                case "gif":
                    return new CommandlineArgs(Argument.Gif);
                case "upload":
                    return new CommandlineArgs(Argument.Upload, (args.AsParallel().Where(a => ImageHelper.IsImage(a))).ToList());
                default:
                    return new CommandlineArgs(Argument.Snipe);
            }
        }
    }

    public struct CommandlineArgs {
        public CommandLineHelper.Argument Argument;
        public List<string> UploadFiles;

        public CommandlineArgs(CommandLineHelper.Argument argument, List<string> uploadFiles) {
            Argument = argument;
            UploadFiles = uploadFiles;
        }

        public CommandlineArgs(CommandLineHelper.Argument argument) {
            Argument = argument;
            UploadFiles = null;
        }
    }
}
