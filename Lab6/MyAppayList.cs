using System;

namespace Lab6
{
    /// <summary>
    /// Собственный динамический список из Лабораторной работы 5,
    /// доведённый до generic-версии: MyAppayList&lt;T&gt;.
    /// В Lab6 запрещено использовать System.Collections.Generic,
    /// поэтому все контейнеры (включая стек) строятся поверх этого класса.
    /// </summary>
    public class MyAppayList<T>
    {
        private T[] _data;
        private int _count;
        private readonly int _defaultCapacity;

        public int Count => _count;
        public int Capacity => _data.Length;

        public MyAppayList()
        {
            _defaultCapacity = 4;
            _data = new T[_defaultCapacity];
            _count = 0;
        }

        public MyAppayList(int capacity)
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

        public void RemoveAt(int index)
        {
            CheckIndexForAccess(index);

            int lastIndex = _count - 1;
            Array.Copy(_data, index + 1, _data, index, lastIndex - index);

            // Обнуляем вакантный «хвост»: для ссылочных типов иначе
            // массив держал бы ссылку и GC не собрал бы объект.
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
