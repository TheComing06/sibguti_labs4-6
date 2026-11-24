using System;

namespace Lab6
{
    /// <summary>
    /// Собственный стек поверх собственного MyAppayList&lt;T&gt;.
    /// System.Collections.Generic.Stack&lt;T&gt; в решении запрещён.
    ///
    /// Вершина стека — ПОСЛЕДНИЙ элемент списка. Тогда Push = Add (O(1))
    /// и Pop = RemoveAt(Count-1) (O(1)): при удалении последнего элемента
    /// сдвигать нечего. Если бы вершиной был первый элемент (index 0),
    /// каждый Pop сдвигал бы все N элементов — O(N).
    /// </summary>
    public class MyStack<T>
    {
        private readonly MyAppayList<T> _items;

        public MyStack()
        {
            _items = new MyAppayList<T>();
        }

        public int Count => _items.Count;

        public bool IsEmpty => _items.Count == 0;

        /// <summary>
        /// Кладёт элемент на вершину стека.
        /// </summary>
        public void Push(T item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Снимает и возвращает элемент с вершины. Пустой стек — исключение.
        /// </summary>
        public T Pop()
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Pop из пустого стека: снимать нечего.");
            }

            int topIndex = _items.Count - 1;
            T top = _items[topIndex];
            _items.RemoveAt(topIndex);
            return top;
        }

        /// <summary>
        /// Возвращает элемент с вершины, не снимая его.
        /// </summary>
        public T Peek()
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Peek пустого стека: смотреть не на что.");
            }

            return _items[_items.Count - 1];
        }

        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Индексация от вершины: this[0] — верхний элемент.
        /// Нужна для отладки и вывода содержимого стека.
        /// </summary>
        public T this[int fromTop]
        {
            get
            {
                if (fromTop < 0 || fromTop >= _items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(fromTop),
                        "Нет такого элемента от вершины стека.");
                }

                return _items[_items.Count - 1 - fromTop];
            }
        }
    }
}
