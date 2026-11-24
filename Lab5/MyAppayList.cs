using System;

namespace Lab5
{
    /// <summary>
    /// Упрощённый аналог List<int>: динамический массив,
    /// который сам увеличивает ёмкость при заполнении.
    /// </summary>
    public class MyAppayList
    {
        // Приватные поля (внутреннее состояние)
        private int[] _data;
        private int _count;

        // Стандартный размер массива при создании списка
        private readonly int _defaultCapacity;

        // Публичные свойства (внешний интерфейс, только для чтения)
        public int Count => _count;
        public int Capacity => _data.Length;

        /// <summary>
        /// Конструктор по умолчанию: ёмкость = defaultCapacity, счётчик = 0.
        /// </summary>
        public MyAppayList()
        {
            _defaultCapacity = 4;
            _data = new int[_defaultCapacity];
            _count = 0;
        }

        /// <summary>
        /// Конструктор с заданной начальной ёмкостью.
        /// new MyAppayList(0) — список без места, первый Add вызовет Grow.
        /// new MyAppayList(-1) — исключение (нельзя массив отрицательной длины).
        /// </summary>
        public MyAppayList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity),
                    "Ёмкость не может быть отрицательной.");
            }

            _defaultCapacity = 4;
            _data = new int[capacity];
            _count = 0;
        }

        /// <summary>
        /// Добавление элемента в конец. Если место кончилось — Grow().
        /// </summary>
        public void Add(int item)
        {
            if (_count == _data.Length)
            {
                Grow();
            }

            _data[_count] = item;
            _count++;
        }

        /// <summary>
        /// Удаление ПЕРВОГО найденного элемента. Если элемента нет —
        /// ничего не происходит (без исключения).
        /// </summary>
        public void Remove(int item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// Удаление элемента по индексу: сдвигаем "хвост" влево.
        /// Освободившуюся ячейку обнуляем (default): иначе за списком
        /// продолжил бы ссылаться старый объект и GC не смог бы его собрать.
        /// Для int это просто гигиена, для ссылочных типов — утечка памяти.
        /// </summary>
        public void RemoveAt(int index)
        {
            CheckIndexForAccess(index);

            int lastIndex = _count - 1;
            Array.Copy(_data, index + 1, _data, index, lastIndex - index);
            _data[lastIndex] = default(int); // 0 для int; null для ссылочных типов
            _count--;
        }

        /// <summary>
        /// Вставка элемента по индексу. Разрешена вставка и в конец
        /// (index == Count). Вставка за пределы [0..Count] — исключение.
        /// </summary>
        public void Insert(int index, int item)
        {
            if (index < 0 || index > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    "Индекс должен быть в диапазоне [0..Count].");
            }

            if (_count == _data.Length)
            {
                Grow();
            }

            // Сдвигаем элементы [index..Count) вправо на одну позицию.
            Array.Copy(_data, index, _data, index + 1, _count - index);
            _data[index] = item;
            _count++;
        }

        /// <summary>
        /// Очистка списка: счётчик в 0, массив обнуляем (см. RemoveAt).
        /// </summary>
        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
            _count = 0;
        }

        /// <summary>
        /// Индекс первого найденного элемента или -1, если элемента нет.
        /// </summary>
        public int IndexOf(int item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_data[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Индексатор: доступ к элементам как у обычного массива.
        /// Обращение по некорректному индексу — IndexOutOfRangeException.
        /// </summary>
        public int this[int index]
        {
            get
            {
                CheckIndexForAccess(index);
                return _data[index];
            }
            set
            {
                CheckIndexForAccess(index);
                _data[index] = value;
            }
        }

        /// <summary>
        /// Вспомогательный метод: увеличение ёмкости (удвоение).
        /// Старый массив после _data = newData() становится недостижим
        /// и собирается garbage collector'ом.
        /// </summary>
        private void Grow()
        {
            int oldCapacity = _data.Length;
            int newCapacity = oldCapacity == 0 ? _defaultCapacity : oldCapacity * 2;

            int[] newData = new int[newCapacity];
            Array.Copy(_data, newData, _count);
            _data = newData;

            Console.WriteLine("[Resize] Ёмкость увеличена с " + oldCapacity +
                " до " + newCapacity);
        }

        /// <summary>
        /// Вспомогательный метод: проверка индекса для доступа/удаления.
        /// Валидный диапазон существующих элементов: [0..Count-1].
        /// </summary>
        private void CheckIndexForAccess(int index)
        {
            if (index < 0 || index >= _count)
            {
                throw new IndexOutOfRangeException(
                    "Индекс " + index + " вне диапазона [0.." + (_count - 1) + "].");
            }
        }
    }
}
