using Microsoft.Win32;

namespace ImgurSniper.Libraries.Helper {
    internal class RegistryHelper {
        internal static void CreateRegistry(string path, string name, int value) {
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(path)) {
                if (rk != null) {
                    rk.SetValue(name, value, RegistryValueKind.DWord);
                }
            }
        }
    }
}
