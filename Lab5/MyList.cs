using System;

namespace Lab5
{
    /// <summary>
    /// Generic-версия списка: тот же MyAppayList, но работает с любым
    /// типом данных T. Компилятор подставляет конкретный тип вместо T
    /// при создании объекта: new MyList<string>(), new MyList<double>().
    /// Это даёт переиспользование кода (не надо копировать класс под
    /// каждый тип) и безопасность типов (в MyList<int> нельзя добавить
    /// строку — ошибка на этапе компиляции).
    /// </summary>
    public class MyList<T>
    {
        private T[] _data;
        private int _count;
        private readonly int _defaultCapacity;

        public int Count => _count;
        public int Capacity => _data.Length;

        public MyList()
        {
            _defaultCapacity = 4;
            _data = new T[_defaultCapacity];
            _count = 0;
        }

        public MyList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity),
                    "Ёмкость не может быть отрицательной.");
            }

            _defaultCapacity = 4;
            _data = new T[capacity];
            _count = 0;
        }

        public void Add(T item)
        {
            if (_count == _data.Length)
            {
                Grow();
            }

            _data[_count] = item;
            _count++;
        }

        /// <summary>
        /// Сравнение элементов через EqualityComparer<T>.Default:
        /// для ссылочных типов работает виртуальный Equals, для
        /// value types — без boxing.
        /// </summary>
        public void Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            CheckIndexForAccess(index);

            int lastIndex = _count - 1;
            Array.Copy(_data, index + 1, _data, index, lastIndex - index);

            // default(T): 0 для чисел, null для ссылочных типов —
            // иначе «хвост» держит ссылку и объект не соберётся GC.
            _data[lastIndex] = default(T);
            _count--;
        }

        public void Insert(int index, T item)
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

            Array.Copy(_data, index, _data, index + 1, _count - index);
            _data[index] = item;
            _count++;
        }

        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
            _count = 0;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_data, item, 0, _count);
        }

        public T this[int index]
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

        private void Grow()
        {
            int oldCapacity = _data.Length;
            int newCapacity = oldCapacity == 0 ? _defaultCapacity : oldCapacity * 2;

            T[] newData = new T[newCapacity];
            Array.Copy(_data, newData, _count);
            _data = newData;

            Console.WriteLine("[Resize] Ёмкость увеличена с " + oldCapacity +
                " до " + newCapacity);
        }

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
