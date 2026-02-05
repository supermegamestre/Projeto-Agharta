using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace SmallUtilities
{
    public class List2D<x> : IEnumerable<x>
    {
        private x[] list;

        public int rows { get; private set; }
        public int columns { get; private set; }

        public int Count => list.Length;

        public List2D(int rows, int columns)
        {
            if(rows <= 0 || columns <= 0) throw new ArgumentOutOfRangeException(nameof(rows) + ", " + nameof(columns));

            this.rows = rows;
            this.columns = columns;
            list = new x[rows*columns];
        }

        private int index(int row, int column)
        {
            if(row < 0 || row >= rows || column < 0 || column >= columns) throw new IndexOutOfRangeException($"invalid index: [{row}, {column}]");
            return row * columns + column;
        }

        public x this[int row, int column]
        {
            get => list[index(row, column)];
            set => list[index(row, column)] = value;
        }

        public void resize(int newRows, int newColumns)
        {
            if(newRows <= 0 || newColumns <= 0) throw new ArgumentOutOfRangeException(nameof(newRows ) + ", " + nameof(newColumns));

            int usedRows = -1;
            int usedColumns = -1;

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < columns; j++)
                {
                    if (!EqualityComparer<x>.Default.Equals(list[i * columns + j], default!))
                    {
                        if(i > usedRows) usedRows = i;
                        if(j > usedColumns) usedColumns = j;
                    }
                }
            }

            if(usedRows >= 0)
            {
                newRows = Math.Max(newRows, usedRows + 1);
                newColumns = Math.Max(newColumns, usedColumns + 1);
            }

            if (newRows == rows && newColumns == columns)
                return;

            var newList = new x[newRows * newColumns];
            int minRows = Math.Min(rows, newRows);
            int minColumns = Math.Min(columns, newColumns);

            for (int i = 0; i < minRows; i++)
                Array.Copy(list, i * columns, newList, i * newColumns, minColumns);

            list = newList;
            rows = newRows;
            columns = newColumns;
        }

        private void autoResize (int row, int column)
        {
            if(row >= rows || column >= columns)
            {
                int newRows = Math.Max(row + 1, rows);
                int newColumns = Math.Max(column + 1, columns);
                resize(newRows, newColumns);
            }
        }

        public IEnumerator<x> GetEnumerator()
        {
            for(int i = 0; i < list.Length; i++)
                yield return list[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class BoolListReaders
    {
        public static bool allTrue(IEnumerable<bool> bools)
        {
            foreach (bool check in bools)
                if (!check)
                    return false;
            return true;
        }
        public static bool allFalse(IEnumerable<bool> bools)
        {
            foreach (bool check in bools)
                if (check)
                    return false;
            return true;
        }
    }
}
