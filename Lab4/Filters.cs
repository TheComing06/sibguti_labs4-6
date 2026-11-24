using System;

namespace Lab4
{
    /// <summary>
    /// Библиотека статических фильтров для попиксельной обработки изображений.
    /// </summary>
    public static class ImageFilters
    {
        /// <summary>
        /// Ограничение значения законным диапазоном канала (0-255).
        /// </summary>
        private static byte Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        /// <summary>
        /// Задание 1. Инвертирует цвета на изображении (негатив).
        /// </summary>
        public static void Invert(Image image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Pixel p = image[x, y];
                    image[x, y] = new Pixel(
                        (byte)(255 - p.R),
                        (byte)(255 - p.G),
                        (byte)(255 - p.B));
                }
            }
        }

        /// <summary>
        /// Задание 2. Преобразует изображение в оттенки серого
        /// по взвешенной формуле яркости (глаз чувствительнее к зелёному).
        /// </summary>
        public static void Grayscale(Image image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Pixel p = image[x, y];
                    int intensity = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    byte gray = (byte)intensity;
                    image[x, y] = new Pixel(gray, gray, gray);
                }
            }
        }

        /// <summary>
        /// Задание 3. Придает изображению эффект «старой фотографии» (сепия).
        /// </summary>
        public static void Sepia(Image image)
        {
            const int T = 30; // глубина сепии

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Pixel p = image[x, y];
                    int intensity = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);

                    byte newR = Clamp(intensity + 2 * T);
                    byte newG = Clamp(intensity + T);
                    byte newB = Clamp(intensity);

                    image[x, y] = new Pixel(newR, newG, newB);
                }
            }
        }

        /// <summary>
        /// Задание 4. Метод-оркестратор: делает изображение светлее
        /// или темнее. Проход по пикселям здесь, изменение — в исполнителе.
        /// </summary>
        public static void AdjustBrightness(Image image, int value)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // ref — прямая ссылка на пиксель внутри массива,
                    // иначе AdjustPixelBrightness менял бы копию.
                    ref Pixel pixelRef = ref image.GetPixel(x, y);
                    AdjustPixelBrightness(ref pixelRef, value);
                }
            }
        }

        /// <summary>
        /// Задание 4. Приватный метод-исполнитель: меняет яркость
        /// одного пикселя, переданного по ссылке.
        /// </summary>
        private static void AdjustPixelBrightness(ref Pixel p, int value)
        {
            p.R = Clamp(p.R + value);
            p.G = Clamp(p.G + value);
            p.B = Clamp(p.B + value);
        }

        /// <summary>
        /// Задание 5. Повышает контраст: тёмное ещё темнее, светлое ещё светлее.
        /// </summary>
        public static void MakeContrast(Image image)
        {
            const double factor = 1.5;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Pixel p = image[x, y];

                    byte newR = Clamp((int)Math.Round(128 + (p.R - 128) * factor));
                    byte newG = Clamp((int)Math.Round(128 + (p.G - 128) * factor));
                    byte newB = Clamp((int)Math.Round(128 + (p.B - 128) * factor));

                    image[x, y] = new Pixel(newR, newG, newB);
                }
            }
        }

        /// <summary>
        /// Задание 6. Простое коробочное размытие: среднее цвета
        /// квадрата 3x3 вокруг каждого пикселя.
        /// </summary>
        public static void BoxBlur(Image image)
        {
            // Читаем из независимой копии, иначе уже «испорченные»
            // (размытые) соседи попадут в расчёт следующих пикселей.
            Image source = image.Clone();

            // Крайние пиксели пропускаем, чтобы не выходить за границы.
            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    int sumR = 0, sumG = 0, sumB = 0;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            Pixel neighbor = source[x + dx, y + dy];
                            sumR += neighbor.R;
                            sumG += neighbor.G;
                            sumB += neighbor.B;
                        }
                    }

                    image[x, y] = new Pixel(
                        (byte)(sumR / 9),
                        (byte)(sumG / 9),
                        (byte)(sumB / 9));
                }
            }
        }

        /// <summary>
        /// Задание 7. Корректировка отдельных каналов: выборочно
        /// усиливает или ослабляет каждый цветовой канал.
        /// </summary>
        public static void AdjustChannels(Image image, sbyte deltaR, sbyte deltaG, sbyte deltaB)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    ref Pixel p = ref image.GetPixel(x, y);

                    int newR = p.R + deltaR;
                    int newG = p.G + deltaG;
                    int newB = p.B + deltaB;

                    p.R = Clamp(newR);
                    p.G = Clamp(newG);
                    p.B = Clamp(newB);
                }
            }
        }
    }
}
