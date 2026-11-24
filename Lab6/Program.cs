using System;
using System.Globalization;

namespace Lab6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunSelfTests();

            Console.WriteLine();
            Console.WriteLine("=== Калькулятор ОПН (ОС: итоговый проект курса) ===");

            // Выражение можно передать аргументом командной строки
            // (тогда программа не ждёт ввода) или ввести с клавиатуры.
            string expression;
            if (args.Length > 0)
            {
                expression = args[0];
                Console.WriteLine("Выражение из аргументов: " + expression);
            }
            else
            {
                Console.Write("Введите математическое выражение: ");
                expression = Console.ReadLine();
            }

            EvaluateAndPrint(expression);
        }

        /// <summary>
        /// Часть 4. Сборка: выводим ВСЕ три значения — исходное выражение,
        /// его ОПН-представление и финальный результат. Вывод только числа
        /// требование не выполняет.
        /// </summary>
        private static void EvaluateAndPrint(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                Console.WriteLine("Пустое выражение — нечего вычислять.");
                return;
            }

            try
            {
                // 1. Лексический анализ (опционально, но полезно для отладки).
                MyAppayList<string> tokens = Calculator.Tokenize(expression);
                Console.WriteLine("Исходное выражение : " + expression);
                Console.Write("Токены             :");
                for (int i = 0; i < tokens.Count; i++)
                {
                    Console.Write(" " + tokens[i]);
                }
                Console.WriteLine();

                // 2. Конвертер: инфиксная -> ОПН.
                string rpn = Calculator.ConvertToRpn(expression);
                Console.WriteLine("ОПН                : " + rpn);

                // 3. Вычислитель: ОПН -> число.
                double result = Calculator.Calculate(rpn);
                Console.WriteLine("Результат          : " +
                    result.ToString(CultureInfo.InvariantCulture));
            }
            catch (FormatException e)
            {
                Console.WriteLine("Ошибка разбора выражения: " + e.Message);
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine("Ошибка вычисления: " + e.Message);
            }
            catch (ArithmeticException e)
            {
                Console.WriteLine("Ошибка вычисления: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Ошибка стека: " + e.Message);
            }
        }

        /// <summary>
        /// Самопроверка по критериям сдачи. Каждый тест печатает
        /// PASS/FAIL; все выражения намеренно без пробелов, с пробелами
        /// и в смешанном виде — результат обязан совпасть.
        /// </summary>
        private static void RunSelfTests()
        {
            Console.WriteLine("=== Тесты ===");

            // Приоритеты и вложенные скобки.
            Check("2+2*2", "2 2 2 * +", "6");
            Check("(2+2)*2", "2 2 + 2 *", "8");
            Check("2*(3+4)-5/1", "2 3 4 + * 5 1 / -", "9");
            Check("(((1+2)))", "1 2 +", "3");

            // Вещественные числа и многозначные числа.
            Check("12+3.5", "12 3.5 +", "15.5");
            Check("100/4", "100 4 /", "25");

            // Классический пример: mixed (со скобками, приоритетами,
            // правой ассоциативностью ^) — 3 + 8/65536.
            Check("3 + 4 * 2 / ( 1 - 5 ) ^ 2 ^ 3",
                "3 4 2 * 1 5 - 2 3 ^ ^ / +", "3.0001220703125");

            // Ассоциативность: вычитание слева направо.
            Check("8-2-3", "8 2 - 3 -", "3");
            // ^ — правая ассоциативность: 2^3^2 = 2^(3^2) = 512.
            Check("2^3^2", "2 3 2 ^ ^", "512");

            // Деление на ноль и нехватка операндов — исключение, а не крах.
            CheckThrows("10/0", typeof(DivideByZeroException));
            CheckThrows("5+", typeof(FormatException));
            CheckThrows("2+(3*4", typeof(FormatException));
            CheckThrows("2+3)", typeof(FormatException));
            CheckThrows("2 $ 3", typeof(FormatException));
            // Подаётся сразу в Calculate: нехватка операндов.
            CheckThrowsRpn("3 4 + +", typeof(FormatException));
            CheckThrowsRpn("3 4", typeof(FormatException));

            // Доп. задание (Часть 5): унарный минус и функции.
            Check("-3+5", "3 neg 5 +", "2");
            Check("-(2+3)*4", "2 3 + neg 4 *", "-20");
            Check("3*-2", "3 2 neg *", "-6");
            Check("sin(0)+cos(0)", "0 sin 0 cos +", "1");
        }

        private static void Check(string expression, string expectedRpn, string expectedValue)
        {
            try
            {
                string rpn = Calculator.ConvertToRpn(expression);
                string value = Calculator.Calculate(rpn).ToString(CultureInfo.InvariantCulture);

                bool ok = rpn == expectedRpn && value == expectedValue;
                Console.WriteLine((ok ? "PASS " : "FAIL ") + "'" + expression +
                    "' -> ОПН '" + rpn + "' = " + value +
                    (ok ? "" : " (ожидалось ОПН '" + expectedRpn + "' = " + expectedValue + ")"));
            }
            catch (Exception e)
            {
                Console.WriteLine("FAIL '" + expression + "' -> неожиданное исключение " +
                    e.GetType().Name + ": " + e.Message);
            }
        }

        private static void CheckThrows(string expression, Type expectedException)
        {
            try
            {
                string rpn = Calculator.ConvertToRpn(expression);
                double value = Calculator.Calculate(rpn);
                Console.WriteLine("FAIL '" + expression + "' -> ожидалось " +
                    expectedException.Name + ", а получено " +
                    value.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                bool ok = e.GetType() == expectedException;
                Console.WriteLine((ok ? "PASS " : "FAIL ") + "'" + expression + "' -> " +
                    e.GetType().Name + ": " + e.Message +
                    (ok ? "" : " (ожидалось " + expectedException.Name + ")"));
            }
        }

        private static void CheckThrowsRpn(string rpnInput, Type expectedException)
        {
            try
            {
                double value = Calculator.Calculate(rpnInput);
                Console.WriteLine("FAIL Calculate('" + rpnInput + "') -> ожидалось " +
                    expectedException.Name + ", а получено " +
                    value.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                bool ok = e.GetType() == expectedException;
                Console.WriteLine((ok ? "PASS " : "FAIL ") + "Calculate('" + rpnInput + "') -> " +
                    e.GetType().Name + ": " + e.Message +
                    (ok ? "" : " (ожидалось " + expectedException.Name + ")"));
            }
        }
    }
}
