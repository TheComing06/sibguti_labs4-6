using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Lab4
{
    /// <summary>
    /// Класс «Изображение»: обёртка над двумерным массивом пикселей.
    /// Загружает и сохраняет BMP-файлы, даёт доступ к пикселям
    /// через индексатор (копия) и через GetPixel (ссылка ref).
    /// </summary>
    public class Image
    {
        private Pixel[,] _pixels;

        public int Width { get; private set; }

        public int Height { get; private set; }

        /// <summary>
        /// Конструктор: загружает картинку из файла.
        /// </summary>
        public Image(string filePath)
        {
            using (var bitmap = new Bitmap(filePath))
            {
                Width = bitmap.Width;
                Height = bitmap.Height;
                _pixels = new Pixel[Width, Height];

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Color c = bitmap.GetPixel(x, y);
                        _pixels[x, y] = new Pixel(c.R, c.G, c.B);
                    }
                }
            }
        }

        /// <summary>
        /// Индексатор: возвращает КОПИЮ пикселя.
        /// Поэтому image[0, 0].R = 255 не компилируется (CS1612):
        /// изменять было бы копию из временного выражения, а не массив.
        /// Для изменения используйте GetPixel по ref.
        /// </summary>
        public Pixel this[int x, int y]
        {
            get { return _pixels[x, y]; }
            set { _pixels[x, y] = value; }
        }

        /// <summary>
        /// Возвращает ПРЯМУЮ ССЫЛКУ на пиксель внутри массива,
        /// позволяя менять его на месте: ref Pixel p = image.GetPixel(x, y);
        /// </summary>
        public ref Pixel GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));
            return ref _pixels[x, y];
        }

        /// <summary>
        /// Сохраняет изменённую картинку в новый файл (BMP).
        /// </summary>
        public void Save(string filePath)
        {
            using (var bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb))
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Pixel p = _pixels[x, y];
                        bitmap.SetPixel(x, y, Color.FromArgb(p.R, p.G, p.B));
                    }
                }

                bitmap.Save(filePath, ImageFormat.Bmp);
            }
        }

        /// <summary>
        /// Независимая (глубокая) копия изображения:
        /// Clone() у массива значений типов даёт новый массив
        /// с собственными копиями каждого структуры-Pixel.
        /// </summary>
        public Image Clone()
        {
            Image copy = (Image)this.MemberwiseClone();
            copy._pixels = (Pixel[,])this._pixels.Clone();
            return copy;
        }
    }
}
