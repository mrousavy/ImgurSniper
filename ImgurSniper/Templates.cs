namespace ImgurSniper {
    public static class Templates {
        public struct RECT {
            public int Left;
            public int Top;
            public int Width;
            public int Height;

            public RECT(int left, int top, int width, int height) {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }
        }
    }
}
