using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImgurSniper.Libraries.Helper {
    public static class CommandLineHelpers {
        public enum Argument {
            Snipe,
            Autostart,
            GIF,
            Upload
        }
        private static string[] parameters = { "autostart", "gif", "upload" };


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
            string param = args.FirstOrDefault(arg => parameters.Contains(arg));

            switch (param) {
                case "autostart":
                    return new CommandlineArgs(Argument.Autostart);
                case "gif":
                    return new CommandlineArgs(Argument.GIF);
                case "upload":
                    return new CommandlineArgs(Argument.Upload, (args.AsParallel().Where(a => ImageHelper.IsImage(a))).ToList());
                default:
                    return new CommandlineArgs(Argument.Snipe);
            }
        }
    }

    public struct CommandlineArgs {
        public CommandLineHelpers.Argument Argument;
        public List<string> UploadFiles;

        public CommandlineArgs(CommandLineHelpers.Argument argument, List<string> uploadFiles) {
            Argument = argument;
            UploadFiles = uploadFiles;
        }

        public CommandlineArgs(CommandLineHelpers.Argument argument) {
            Argument = argument;
            UploadFiles = null;
        }
    }
}
