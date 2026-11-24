using System;
using System.Globalization;
using System.Text;

namespace Lab6
{
    /// <summary>
    /// Калькулятор обратной польской нотации (ОПН).
    /// Три модуля: Tokenize (лексический анализ), ConvertToRpn
    /// (сортировочная станция Дейкстры), Calculate (вычислитель).
    /// Все контейнеры — собственные MyStack/MyAppayList;
    /// System.Collections.Generic в решении не используется.
    /// </summary>
    public static class Calculator
    {
        /// <summary>
        /// Приоритеты бинарных операторов. Чем больше число — тем сильнее связь.
        /// </summary>
        private static int Precedence(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
                default:
                    throw new FormatException("Неизвестный оператор: '" + op + "'.");
            }
        }

        /// <summary>
        /// ^ — правая ассоциативность (2^3^2 = 2^(3^2)), поэтому для него
        /// оператор с вершины стека выталкивается только при СТРОГО большем
        /// приоритете. Для + - * / (левая ассоциативность) — при >=.
        /// </summary>
        private static bool IsRightAssociative(string op)
        {
            return op == "^" || op == UnaryMinusToken;
        }

        /// <summary>
        /// Токен унарного минуса (доп. задание, Часть 5). Вводится на этапе
        /// токенизации, когда '-' распознан как унарный, а не бинарный.
        /// </summary>
        public const string UnaryMinusToken = "neg";

        /// <summary>
        /// Поддерживаемые функции (доп. задание, Часть 5).
        /// </summary>
        private static bool IsFunction(string token)
        {
            return token == "sin" || token == "cos";
        }

        private static bool IsOperator(string token)
        {
            switch (token)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "^":
                    return true;
                default:
                    return false;
            }
        }

        // ---------------------------------------------------------------------
        // Часть 1. Лексический анализатор (парсер)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Разбивает инфиксное выражение на список токенов ПОСИМВОЛЬНЫМ
        /// проходом: цифры и точка копятся в буфер StringBuilder, операторы
        /// и скобки распознаются как односимвольные токены, буквы — в имя
        /// функции. Split/Regex как основной механизм разбора не используются.
        /// Одинаково корректно работает с пробелами, без них и в смешанном виде.
        /// </summary>
        public static MyAppayList<string> Tokenize(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            MyAppayList<string> tokens = new MyAppayList<string>();
            StringBuilder numberBuffer = new StringBuilder();

            // Ожидание операнда определяет роль символа '-' или '+':
            // в начале выражения, после '(' или после оператора минус
            // обязан быть унарным; после числа или ')' — бинарным.
            bool expectOperand = true;

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (c == ' ' || c == '\t')
                {
                    // Пробел — только разделитель: число, накопленное в буфере,
                    // этим завершается. Иначе "12 3" превратилось бы в "123".
                    FlushNumber(numberBuffer, tokens);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    numberBuffer.Append(c);
                    expectOperand = false;
                    continue;
                }

                if (c == '.')
                {
                    if (numberBuffer.ToString().Contains("."))
                    {
                        throw new FormatException(
                            "Некорректное число: в нём более одной точки ('" +
                            numberBuffer + c + "'), позиция " + i + ".");
                    }

                    numberBuffer.Append(c);
                    expectOperand = false;
                    continue;
                }

                // Буква — начало имени функции (sin, cos).
                if (char.IsLetter(c))
                {
                    FlushNumber(numberBuffer, tokens);

                    StringBuilder name = new StringBuilder();
                    while (i < expression.Length && char.IsLetter(expression[i]))
                    {
                        name.Append(expression[i]);
                        i++;
                    }
                    i--; // цикл сам прибавит ещё один шаг

                    string token = name.ToString().ToLowerInvariant();
                    if (!IsFunction(token))
                    {
                        throw new FormatException(
                            "Неизвестный идентификатор '" + token +
                            "' (позиция " + (i - name.Length + 1) + "). " +
                            "Поддерживаются только функции sin и cos.");
                    }

                    if (!expectOperand)
                    {
                        throw new FormatException(
                            "Перед функцией '" + token + "' (позиция " +
                            (i - name.Length + 1) + ") не хватает оператора.");
                    }

                    tokens.Add(token);
                    // После функции снова ожидаем операнд.
                    continue;
                }

                if (c == '(')
                {
                    if (!expectOperand)
                    {
                        throw new FormatException(
                            "Перед '(' (позиция " + i + ") не хватает оператора.");
                    }

                    FlushNumber(numberBuffer, tokens);
                    tokens.Add("(");
                    continue;
                }

                if (c == ')')
                {
                    FlushNumber(numberBuffer, tokens);
                    if (expectOperand)
                    {
                        throw new FormatException(
                            "Пустые или незакрытые данные перед ')' (позиция " + i + ").");
                    }

                    tokens.Add(")");
                    expectOperand = false;
                    continue;
                }

                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
                {
                    FlushNumber(numberBuffer, tokens);

                    bool isUnary = expectOperand && (c == '-' || c == '+');
                    if (expectOperand && !isUnary)
                    {
                        throw new FormatException(
                            "Ожидался операнд, а найден оператор '" + c +
                            "' (позиция " + i + ").");
                    }

                    if (isUnary)
                    {
                        // Отличие унарного минуса от бинарного: позиция.
                        // '-' унарный, если стоит в начале выражения, сразу
                        // после '(' или сразу после другого оператора.
                        if (c == '+')
                        {
                            // Унарный плюс ничего не меняет — просто пропускаем.
                            continue;
                        }

                        tokens.Add(UnaryMinusToken);
                        // Ожидаем операнд и после унарного оператора.
                        continue;
                    }

                    tokens.Add(c.ToString());
                    expectOperand = true;
                    continue;
                }

                throw new FormatException(
                    "Некорректный символ '" + c + "' в позиции " + i +
                    ". Допустимы цифры, '.', операторы + - * / ^, скобки, " +
                    "функции sin/cos и пробелы.");
            }

            FlushNumber(numberBuffer, tokens);
            return tokens;
        }

        /// <summary>
        /// Превращает накопленный буфер цифр в один токен числа.
        /// Без этого «12» распалось бы на токены «1» и «2».
        /// Вызывается и после последнего символа строки — иначе последнее
        /// число выражения «3+42» потерялось бы.
        /// </summary>
        private static void FlushNumber(StringBuilder numberBuffer, MyAppayList<string> tokens)
        {
            if (numberBuffer.Length == 0)
            {
                return;
            }

            string text = numberBuffer.ToString();
            double value;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                throw new FormatException("Не удалось распознать число '" + text + "'.");
            }

            tokens.Add(text);
            numberBuffer.Clear();
        }

        // ---------------------------------------------------------------------
        // Часть 2. Вычислитель ОПН
        // ---------------------------------------------------------------------

        /// <summary>
        /// Вычисляет строку, уже представленную в ОПН (токены через пробел).
        /// Здесь Split разрешён и ожидается условием Части 2.
        /// Стек промежуточных значений — собственный MyStack.
        /// </summary>
        public static double Calculate(string rpnInput)
        {
            if (rpnInput == null)
            {
                throw new ArgumentNullException(nameof(rpnInput));
            }

            string[] tokens = rpnInput.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            MyStack<double> stack = new MyStack<double>();

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                double number;

                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                {
                    stack.Push(number);
                    continue;
                }

                if (token == UnaryMinusToken || IsFunction(token))
                {
                    if (stack.Count < 1)
                    {
                        throw new FormatException(
                            "Не хватает операнда для '" + token + "' (позиция " + i + ").");
                    }

                    double operand = stack.Pop();
                    double result;
                    if (token == UnaryMinusToken) result = -operand;
                    else if (token == "sin") result = Math.Sin(operand);
                    else result = Math.Cos(operand);

                    stack.Push(result);
                    continue;
                }

                if (IsOperator(token))
                {
                    // Порядок извлечения важен: первым снимается ПРАВЫЙ операнд.
                    // Для вычитания и деления перестановка меняет результат
                    // (5-3 и 5/3), для сложения и умножения — нет, и на таких
                    // тестах ошибка была бы не видна.
                    if (stack.Count < 2)
                    {
                        throw new FormatException(
                            "Не хватает операндов для оператора '" + token +
                            "' (позиция " + i + ").");
                    }

                    double right = stack.Pop();
                    double left = stack.Pop();
                    stack.Push(ApplyBinary(token, left, right));
                    continue;
                }

                throw new FormatException("Неизвестный токен в ОПН: '" + token + "'.");
            }

            if (stack.Count != 1)
            {
                throw new FormatException(
                    "Выражение в ОПН некорректно: на стеке осталось " + stack.Count +
                    " значений вместо одного (лишний операнд или пропущен оператор).");
            }

            double answer = stack.Pop();

            if (double.IsNaN(answer))
            {
                throw new ArithmeticException("Результат вычисления не определён (NaN).");
            }

            if (double.IsInfinity(answer))
            {
                throw new ArithmeticException("Результат вычисления вышел за пределы вещественного числа (Infinity).");
            }

            return answer;
        }

        private static double ApplyBinary(string op, double left, double right)
        {
            switch (op)
            {
                case "+":
                    return left + right;
                case "-":
                    return left - right;
                case "*":
                    return left * right;
                case "/":
                    // В C# деление double на 0.0 НЕ бросает исключение:
                    // даёт Infinity, а 0.0/0.0 — NaN. Поэтому проверку
                    // деления на ноль делаем вручную.
                    if (right == 0.0)
                    {
                        throw new DivideByZeroException(
                            "Деление на ноль: " + FormatNumber(left) + " / " + FormatNumber(right) + ".");
                    }

                    return left / right;
                case "^":
                    return Math.Pow(left, right);
                default:
                    throw new FormatException("Неизвестный оператор: '" + op + "'.");
            }
        }

        // ---------------------------------------------------------------------
        // Часть 3. Конвертер в ОПН (сортировочная станция Дейкстры)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Преобразует инфиксное выражение в строку ОПН.
        /// Скобки в ОПН не сохраняются: порядок вычислений задаётся
        /// тем, в каком операторы выталкиваются из стека.
        /// </summary>
        public static string ConvertToRpn(string expression)
        {
            MyAppayList<string> tokens = Tokenize(expression);

            MyStack<string> operators = new MyStack<string>();
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];
                double unused;

                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out unused))
                {
                    Append(output, token);
                    continue;
                }

                if (token == UnaryMinusToken || IsFunction(token))
                {
                    // Унарные операторы и функции кладём на стек: они
                    // применятся, когда будет готов их единственный операнд.
                    operators.Push(token);
                    continue;
                }

                if (token == "(")
                {
                    operators.Push(token);
                    continue;
                }

                if (token == ")")
                {
                    // Выталкиваем всё до ближайшей '(' — так скобка
                    // «ограничивает» стек и приоритет вложенного выражения.
                    bool foundOpening = false;
                    while (!operators.IsEmpty)
                    {
                        string top = operators.Peek();
                        if (top == "(")
                        {
                            foundOpening = true;
                            operators.Pop();
                            break;
                        }

                        Append(output, operators.Pop());
                    }

                    if (!foundOpening)
                    {
                        throw new FormatException(
                            "Несбалансированные скобки: ')' без парной '(' (токен " + i + ").");
                    }

                    // Если сразу за '(' стояла функция, она тоже готова.
                    if (!operators.IsEmpty && IsFunction(operators.Peek()))
                    {
                        Append(output, operators.Pop());
                    }

                    continue;
                }

                if (IsOperator(token))
                {
                    int current = Precedence(token);
                    while (!operators.IsEmpty)
                    {
                        string top = operators.Peek();
                        if (top == "(")
                        {
                            break;
                        }

                        if (IsFunction(top) || top == UnaryMinusToken)
                        {
                            Append(output, operators.Pop());
                            continue;
                        }

                        int topPrecedence = Precedence(top);
                        bool shouldPop = IsRightAssociative(token)
                            ? topPrecedence > current
                            : topPrecedence >= current;

                        if (!shouldPop)
                        {
                            break;
                        }

                        Append(output, operators.Pop());
                    }

                    operators.Push(token);
                    continue;
                }

                throw new FormatException("Неизвестный токен: '" + token + "'.");
            }

            while (!operators.IsEmpty)
            {
                string top = operators.Pop();
                if (top == "(")
                {
                    throw new FormatException(
                        "Несбалансированные скобки: '(' без парной ')'.");
                }

                Append(output, top);
            }

            return output.ToString();
        }

        private static void Append(StringBuilder output, string token)
        {
            if (output.Length > 0)
            {
                output.Append(' ');
            }

            output.Append(token);
        }

        private static string FormatNumber(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
