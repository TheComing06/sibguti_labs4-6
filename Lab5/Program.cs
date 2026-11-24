using System;

namespace Lab5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DemoBasicMinimum();
            DemoConstructorChecks();
            DemoEmptyListOperations();
            DemoIndexWork();
            DemoGenericList();
            DemoGenericSwap();
        }

        /// <summary>
        /// Базовый минимум: Add + отслеживание Count/Capacity,
        /// момент Resize, работа индексатора и защита от неверного индекса.
        /// </summary>
        private static void DemoBasicMinimum()
        {
            Console.WriteLine("=== 1. Базовый минимум ===");

            MyAppayList list = new MyAppayList();
            Console.WriteLine("Создан список: Count=" + list.Count + ", Capacity=" + list.Capacity);

            // Добавляем 3-4 элемента, после каждого выводим состояние.
            for (int i = 1; i <= 4; i++)
            {
                list.Add(i * 10);
                Console.WriteLine("Add(" + (i * 10) + ") -> Count=" + list.Count +
                    ", Capacity=" + list.Capacity);
            }

            // Продолжаем до вызова механизма Resize.
            Console.WriteLine("Продолжаем добавление до срабатывания Resize...");
            for (int i = 5; i <= 6; i++)
            {
                list.Add(i * 10);
                Console.WriteLine("Add(" + (i * 10) + ") -> Count=" + list.Count +
                    ", Capacity=" + list.Capacity);
            }

            // Демонстрация индексатора по корректным индексам.
            Console.Write("Элементы по индексам: ");
            for (int i = 0; i < list.Count; i++)
            {
                Console.Write("list[" + i + "]=" + list[i] + " ");
            }
            Console.WriteLine();

            // Защита от неверного индекса: обращение по list[99] в try-catch.
            try
            {
                int value = list[99];
                Console.WriteLine("Не ожидали: " + value);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Ожидаемое исключение IndexOutOfRangeException: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Проверка конструкторов: ёмкость 0 и отрицательная ёмкость.
        /// </summary>
        private static void DemoConstructorChecks()
        {
            Console.WriteLine("=== 2. Проверка конструктора ===");

            MyAppayList zeroList = new MyAppayList(0);
            Console.WriteLine("new MyAppayList(0): Count=" + zeroList.Count +
                ", Capacity=" + zeroList.Capacity);
            Console.WriteLine("Первый Add должен вызвать Grow:");
            zeroList.Add(42);
            Console.WriteLine("После Add(42): Count=" + zeroList.Count +
                ", Capacity=" + zeroList.Capacity + ", [0]=" + zeroList[0]);

            try
            {
                MyAppayList badList = new MyAppayList(-1);
                Console.WriteLine("Не ожидали: список создан!");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Ожидаемое исключение ArgumentOutOfRangeException: " +
                    e.ParamName);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Операции на пустом списке.
        /// </summary>
        private static void DemoEmptyListOperations()
        {
            Console.WriteLine("=== 3. Операции на пустом списке ===");

            MyAppayList empty = new MyAppayList();

            try
            {
                empty.RemoveAt(0);
                Console.WriteLine("Не ожидали: RemoveAt(0) прошёл без исключения!");
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("RemoveAt(0) в пустом -> IndexOutOfRangeException: " + e.Message);
            }

            // Remove несуществующего элемента не должен кидать исключение.
            empty.Remove(123);
            Console.WriteLine("Remove(123) в пустом -> без исключения, Count=" + empty.Count);

            Console.WriteLine();
        }

        /// <summary>
        /// Работа с индексами: вставка в конец, граничные вставки,
        /// удаление последнего, IndexOf отсутствующего элемента.
        /// </summary>
        private static void DemoIndexWork()
        {
            Console.WriteLine("=== 4. Работа с индексами ===");

            MyAppayList list = new MyAppayList();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            // Вставка в конец (index == Count) — штатная операция.
            list.Insert(list.Count, 99);
            Console.WriteLine("Insert(Count, 99): [ " + Join(list) + " ], Count=" + list.Count);

            try
            {
                list.Insert(list.Count + 1, 100);
                Console.WriteLine("Не ожидали!");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Insert(Count+1, ...) -> ArgumentOutOfRangeException: " + e.ParamName);
            }

            try
            {
                list.Insert(-1, 100);
                Console.WriteLine("Не ожидали!");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Insert(-1, ...) -> ArgumentOutOfRangeException: " + e.ParamName);
            }

            // Удаление последнего элемента штатно, выход за границу — исключение.
            list.RemoveAt(list.Count - 1);
            Console.WriteLine("RemoveAt(Count-1): [ " + Join(list) + " ], Count=" + list.Count);

            try
            {
                list.RemoveAt(list.Count);
                Console.WriteLine("Не ожидали!");
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("RemoveAt(Count) -> IndexOutOfRangeException: " + e.Message);
            }

            // IndexOf отсутствующего элемента: -1, а не исключение.
            Console.WriteLine("IndexOf(-1) = " + list.IndexOf(-1) + " (ожидаем -1)");
            Console.WriteLine("IndexOf(2) = " + list.IndexOf(2));

            list.Clear();
            Console.WriteLine("После Clear(): Count=" + list.Count + ", Capacity=" + list.Capacity);

            Console.WriteLine();
        }

        /// <summary>
        /// Часть 2 (generic): тот же класс работает с любыми типами —
        /// строки и вещественные числа, один код вместо двух классов.
        /// </summary>
        private static void DemoGenericList()
        {
            Console.WriteLine("=== 5. MyList<T> (generic) ===");

            MyList<string> names = new MyList<string>();
            names.Add("Физика");
            names.Add("не страшно");
            names.Add("Математика");
            names.Insert(0, "Первый");
            Console.WriteLine("MyList<string>: [ " + string.Join(", ", ToArray(names)) +
                " ], Count=" + names.Count + ", Capacity=" + names.Capacity);
            Console.WriteLine("IndexOf(\"не страшно\") = " + names.IndexOf("не страшно"));
            names.Remove("не страшно");
            Console.WriteLine("После Remove(\"не страшно\"): [ " +
                string.Join(", ", ToArray(names)) + " ]");

            MyList<double> numbers = new MyList<double>(2);
            numbers.Add(3.14);
            numbers.Add(2.71);
            numbers.Add(1.41); // тут сработает Grow
            Console.WriteLine("MyList<double>: [ " + string.Join(", ", ToArray(numbers)) +
                " ], Count=" + numbers.Count + ", Capacity=" + numbers.Capacity);

            // Безопасность типов: names.Add(42) не скомпилировался бы —
            // компилятор подставил string вместо T.
            Console.WriteLine();
        }

        /// <summary>
        /// Generic-функция: один Swap<T> вместо перегрузок под каждый тип.
        /// </summary>
        private static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        private static void DemoGenericSwap()
        {
            Console.WriteLine("=== 6. Swap<T> (generic-метод) ===");

            int x = 5;
            int y = 10;
            Swap(ref x, ref y);
            Console.WriteLine("int: x=" + x + ", y=" + y);

            string a = "Физика";
            string b = "не страшно";
            Swap(ref a, ref b);
            Console.WriteLine("string: a=\"" + a + "\", b=\"" + b + "\"");

            Console.WriteLine("Swap<T> работает с любыми типами без перегрузок.");
        }

        // --- Вспомогательные методы вывода ---

        private static string Join(MyAppayList list)
        {
            string[] parts = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                parts[i] = list[i].ToString();
            }
            return string.Join(", ", parts);
        }

        private static T[] ToArray<T>(MyList<T> list)
        {
            T[] result = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = list[i];
            }
            return result;
        }
    }
}
