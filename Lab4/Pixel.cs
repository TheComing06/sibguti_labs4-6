namespace Lab4
{
    /// <summary>
    /// Структура «Пиксель»: три байта, хранящие красную, зелёную
    /// и синюю компоненты цвета (значения от 0 до 255).
    /// </summary>
    public struct Pixel
    {
        public byte R;
        public byte G;
        public byte B;

        public Pixel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
