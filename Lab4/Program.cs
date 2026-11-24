using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Lab4
{
    /// <summary>
    /// Тип делегата под общую сигнатуру фильтров: void Method(Image image).
    /// Объявлен над классом Program — как показано в дополнительном задании.
    /// </summary>
    public delegate void ImageFilterDelegate(Image image);

    internal class Program
    {
        private const string DefaultInputFile = "input.bmp";

        static void Main(string[] args)
        {
            // Аргументы: [0] — путь к картинке, [1] — имя фильтра,
            // [2] — значение яркости (нужно только для brightness).
            string inputPath = args.Length > 0 ? args[0] : DefaultInputFile;

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Файл '" + inputPath + "' не найден — создаю тестовое изображение...");
                CreateTestImage(inputPath);
            }

            Console.WriteLine("Загружаю изображение '" + inputPath + "'...");
            Image image = new Image(inputPath);
            Console.WriteLine("Изображение загружено. Размеры: " + image.Width + "x" + image.Height);

            string filterName = args.Length > 1 ? args[1] : ReadFilterNameFromConsole();

            // Словарь: ключ — название метода, значение — сам метод.
            Dictionary<string, ImageFilterDelegate> availableFilters = new Dictionary<string, ImageFilterDelegate>
            {
                { "invert", ImageFilters.Invert },
                { "grayscale", ImageFilters.Grayscale },
                { "sepia", ImageFilters.Sepia },
                { "contrast", ImageFilters.MakeContrast },
                { "boxblur", ImageFilters.BoxBlur },
                { "channels", ApplyAdjustChannels },
                // Метод с другой сигнатурой — через метод-обёртку нужной сигнатуры.
                { "brightness", ApplyAdjustBrightness }
            };

            Console.WriteLine("Доступные фильтры: " + string.Join(", ", new List<string>(availableFilters.Keys)));

            ImageFilterDelegate selectedFilter;
            if (!availableFilters.TryGetValue(filterName.ToLowerInvariant(), out selectedFilter))
                selectedFilter = null;

            if (selectedFilter == null)
            {
                Console.WriteLine("Фильтр '" + filterName + "' не найден. Пропускаем этап фильтрации.");
            }
            else
            {
                Console.WriteLine("Применяю фильтр '" + filterName + "'...");
                selectedFilter(image);

                string outputFileName = "output_" + filterName + ".bmp";
                image.Save(outputFileName);
                Console.WriteLine("Результат сохранён в '" + outputFileName + "'. Проверьте файл в папке с программой.");
            }
        }

        /// <summary>
        /// Обёртка для AdjustBrightness: сигнатура подходит под делегат,
        /// а значение яркости берётся из аргументов командной строки
        /// (3-й аргумент, «четвёртый» вместе с именем программы) или из консоли.
        /// </summary>
        private static void ApplyAdjustBrightness(Image image)
        {
            string[] args = Environment.GetCommandLineArgs();
            int value;

            if (args.Length > 3 && int.TryParse(args[3], out value))
            {
                Console.WriteLine("Яркость из аргументов: " + value);
            }
            else
            {
                Console.Write("Введите значение яркости (например, 40 или -40): ");
                string line = Console.ReadLine();
                if (!int.TryParse(line, out value))
                {
                    Console.WriteLine("Не удалось прочитать число, яркость = 0.");
                    value = 0;
                }
            }

            ImageFilters.AdjustBrightness(image, value);
        }

        /// <summary>
        /// Обёртка для AdjustChannels: три знаковых байта дельты
        /// читаются из консоли (или из аргументов).
        /// </summary>
        private static void ApplyAdjustChannels(Image image)
        {
            string[] args = Environment.GetCommandLineArgs();
            sbyte deltaR, deltaG, deltaB;

            if (args.Length > 5 &&
                sbyte.TryParse(args[3], out deltaR) &&
                sbyte.TryParse(args[4], out deltaG) &&
                sbyte.TryParse(args[5], out deltaB))
            {
                Console.WriteLine("Дельты из аргументов: R=" + deltaR + ", G=" + deltaG + ", B=" + deltaB);
            }
            else
            {
                deltaR = ReadSByte("delta R (-128..127): ");
                deltaG = ReadSByte("delta G (-128..127): ");
                deltaB = ReadSByte("delta B (-128..127): ");
            }

            ImageFilters.AdjustChannels(image, deltaR, deltaG, deltaB);
        }

        private static sbyte ReadSByte(string prompt)
        {
            Console.Write(prompt);
            string line = Console.ReadLine();
            int parsed;
            if (!int.TryParse(line, out parsed) || parsed < sbyte.MinValue || parsed > sbyte.MaxValue)
            {
                Console.WriteLine("Некорректное значение, беру 0.");
                return 0;
            }
            return (sbyte)parsed;
        }

        private static string ReadFilterNameFromConsole()
        {
            Console.Write("Введите название фильтра: ");
            string name = Console.ReadLine();
            return string.IsNullOrEmpty(name) ? "none" : name;
        }

        /// <summary>
        /// Генерирует тестовое BMP-изображение (цветной градиент с фигурами),
        /// чтобы лабораторную можно было выполнить без внешней картинки.
        /// </summary>
        private static void CreateTestImage(string path)
        {
            const int width = 320;
            const int height = 200;

            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color c = Color.FromArgb(
                            (byte)(255 * x / width),
                            (byte)(255 * y / height),
                            (byte)((255 * (x + y)) / (width + height)));
                        bitmap.SetPixel(x, y, c);
                    }
                }

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.FillRectangle(Brushes.Red, 30, 30, 80, 80);
                    g.FillEllipse(Brushes.DeepSkyBlue, 150, 60, 100, 100);
                    g.DrawLine(new Pen(Color.White, 4), 10, height - 20, width - 10, 20);
                }

                bitmap.Save(path, ImageFormat.Bmp);
            }
        }
    }
}
