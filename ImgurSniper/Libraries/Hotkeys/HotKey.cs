using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ImgurSniper.Libraries.Hotkeys {
    public sealed class HotKey : IDisposable {
        private readonly IntPtr _handle;
        private readonly int _id;
        private bool _isKeyRegistered;
        private Dispatcher _currentDispatcher;

        public HotKey(ModifierKeys modifierKeys, Key key, Action<HotKey> onKeyAction = null) {
            Key = key;
            KeyModifier = modifierKeys;
            _id = GetHashCode();
            _handle = GetHiddenWindow();
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            RegisterHotKey();
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;

            if (onKeyAction != null)
                HotKeyPressed += onKeyAction;
        }

        ~HotKey() {
            Dispose();
        }

        private IntPtr GetHiddenWindow() {
            WindowInteropHelper helper = new WindowInteropHelper(new Window());
            helper.EnsureHandle();
            return helper.Handle;
        }

        public event Action<HotKey> HotKeyPressed;

        public Key Key { get; private set; }

        public ModifierKeys KeyModifier { get; private set; }

        private int InteropKey => KeyInterop.VirtualKeyFromKey(Key);

        public void Dispose() {
            try {
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            } catch (Exception) {
                // ignored
            } finally {
                UnregisterHotKey();
            }
        }

        private void OnHotKeyPressed() {
            _currentDispatcher.Invoke(
                delegate {
                    HotKeyPressed?.Invoke(this);
                });
        }

        private void RegisterHotKey() {
            if (Key == Key.None) {
                return;
            }

            if (_isKeyRegistered) {
                UnregisterHotKey();
            }

            _isKeyRegistered = HotKeyWinApi.RegisterHotKey(_handle, _id, KeyModifier, InteropKey);

            if (!_isKeyRegistered) {
                throw new ApplicationException("An unexpected Error occured! (Hotkey may already be in use)");
            }
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled) {
            if (handled) {
                return;
            }

            if (msg.message != HotKeyWinApi.WmHotKey || (int)(msg.wParam) != _id) {
                return;
            }

            OnHotKeyPressed();
            handled = true;
        }

        private void UnregisterHotKey() {
            _isKeyRegistered = !HotKeyWinApi.UnregisterHotKey(_handle, _id);
        }
    }
}
