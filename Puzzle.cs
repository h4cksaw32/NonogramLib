using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Data;
using System.Collections;
using System.ComponentModel;

namespace NonogramLib
{
    /// <summary>
    /// Provides nonogram keys for the puzzle.
    /// </summary>
    /// <typeparam name="T">The data type used in the puzzle.</typeparam>
    public interface IPuzzleKeys<T>
    {
        ///<value>
        ///A list containing the number of adjacent identical cells in each row.
        ///</value>
        public List<int>[] rowKeys { get; protected set; }
        ///<value>
        ///A list containing the number of adjacent identical cells in each column.
        ///</value>
        public List<int>[] columnKeys { get; protected set; }
        /// <value>
        /// A list containing the value in each "block" of adjacent identical cells in each row.
        /// </value>
        /// <seealso cref="rowKeys"/>
        public List<T>[] rowValueKeys { get; protected set; }
        /// <value>
        /// A list containing the value in each "block" of adjacent identical cells in each column.
        /// </value>
        /// <seealso cref="columnKeys"/>
        public List<T>[] columnValueKeys { get; protected set; }
        /// <summary>
        /// A method for calculating <see cref="rowKeys"/> and <see cref="rowValueKeys"/>
        /// </summary>
        public void UpdateRowKeys() { }
        /// <summary>
        /// A method for calculating <see cref="columnKeys"/> and <see cref="columnValueKeys"/>
        /// </summary>
        public void UpdateColumnKeys() { }
        /// <inheritdoc cref="UpdateRowKeys()"/>
        /// <param name="y">The row to calculate.</param>
        public void UpdateRowKeys(int y) { }
        /// <inheritdoc cref="UpdateColumnKeys()"/>
        /// <param name="x">The column to calculate.</param>
        public void UpdateColumnKeys(int x) { }
    }
    /// <summary>
    /// An abstract class representing a nonogram grid.
    /// </summary>
    /// <typeparam name="T">The data type in each cell.</typeparam>
    public abstract class PuzzleGrid<T> : IPuzzleKeys<T>
    {
        /// <summary>
        /// Represents the cells in the grid.
        /// </summary>
        /// <remarks>
        /// This is a one-dimensional array for more flexible access.
        /// </remarks>
        public T[] cells { get; set; }
        public readonly int width;
        public readonly int height;
        /// <summary>
        /// The default value for a blank cell.
        /// </summary>
        public static readonly T DEFAULT = default;
        /// <inheritdoc cref="IPuzzleKeys{T}.rowKeys"/>
        public List<int>[] rowKeys { get; set; }
        /// <inheritdoc cref="IPuzzleKeys{T}.columnKeys"/>
        public List<int>[] columnKeys { get; set; }
        /// <inheritdoc cref="IPuzzleKeys{T}.rowValueKeys"/>
        public List<T>[] rowValueKeys { get; set; }
        /// <inheritdoc cref="IPuzzleKeys{T}.columnValueKeys"/>
        public List<T>[] columnValueKeys { get; set; }
        /// <value>
        /// The title of the puzzle.
        /// </value>
        public string title { get; set; } = "";
        public PuzzleGrid(int cols, int rows)
        {
            width = cols;
            height = rows;
            cells = new T[cols * rows];
            Array.Fill<T>(cells, default); // Fills the grid with blank cells
            rowKeys = new List<int>[rows];
            columnKeys = new List<int>[cols];
            rowValueKeys = new List<T>[rows];
            columnValueKeys = new List<T>[cols];
            //Create empty keys
            for (int i = 0; i < rows; i++) rowKeys[i] = new(0);
            for (int i = 0; i < cols; i++) columnKeys[i] = new(0);
            for (int i = 0; i < rows; i++) rowValueKeys[i] = new(0);
            for (int i = 0; i < cols; i++) columnValueKeys[i] = new(0);
        }
        public PuzzleGrid(int cols, int rows, T[] data)
        {
            // Check if the amount of cells matches the size of the grid.
            if (data.Length != cols * rows) throw new ArgumentException("Data does not match the size of the puzzle.");
            width = cols;
            height = rows;
            cells = data;
            rowKeys = new List<int>[rows];
            columnKeys = new List<int>[cols];
            rowValueKeys = new List<T>[rows];
            columnValueKeys = new List<T>[cols];
            //Calculate the keys of the grid
            UpdateRowKeys();
            UpdateColumnKeys();
        }
        public T GetCell(int x, int y)
        {
            return cells[y * width + x];
        }
        public void SetCell(int x, int y, T value)
        {
            if (!cells[y * width + x]?.Equals(value) ?? false)
            {
                cells[y * width + x] = value;
                UpdateColumnKeys(x);
                UpdateRowKeys(y);
            }
        }
        /// <summary>
        /// Set the cell by array index instead of coordinates.
        /// </summary>
        /// <param name="i">The index in the <see cref="cells"/> array.</param>
        /// <param name="value">THe value to set the cell.</param>
        public void SetCell(int i, T value)
        {
            if (!cells[i]?.Equals(value) ?? false)
            {
                cells[i] = value;
                UpdateColumnKeys(i % width);
                UpdateRowKeys((int) i / width);
            }
        }
        /// <summary>
        /// Assigns an entirely new set of cells.
        /// </summary>
        /// <param name="value">The array to replace to cells with.</param>
        public void SetCells(T[] value)
        {
            cells = value;
            UpdateColumnKeys();
            UpdateRowKeys();
        }
        /// <inheritdoc cref="IPuzzleKeys{T}.UpdateRowKeys()"/>
        public void UpdateRowKeys()
        {
            for (int y = 0; y < height; y++)
            {
                UpdateRowKeys(y);
            }
        }
        /// <inheritdoc cref="IPuzzleKeys{T}.UpdateColumnKeys()"/>
        public void UpdateColumnKeys()
        {
            for (int x = 0; x < width; x++)
            {
                UpdateColumnKeys(x);
            }
        }
        /// <inheritdoc cref="IPuzzleKeys{T}.UpdateRowKeys(int)"/>
        public void UpdateRowKeys(int y)
        {
            List<int> buffer = new(); //Contains the keys for the row
            List<T> bufferVal = new(); //Contains the value keys for the row
            T cell;
            T prev = DEFAULT;
            int counter = 0; //Counter for the number of adjacent identical cells
            for (int x = 0; x < width; x++)
            {
                cell = GetCell(x, y);
                if (cell.Equals(prev)) counter++;
                else
                {
                    if (!prev.Equals(DEFAULT)) //Ensures that blocks of blank cells are not counted
                    { 
                        buffer.Add(counter);
                        bufferVal.Add(prev);
                    }
                    counter = 1;
                }
                //Add the next block length if the end of the row is reached
                if (x == width - 1 && counter > 0 && !cell.Equals(DEFAULT))
                {
                    buffer.Add(counter);
                    bufferVal.Add(cell);
                    break;
                }
                prev = cell;
            }
            //Adds a "0" and a blank value if the keys are empty
            if (buffer.Count == 0) 
            {
                buffer.Add(0);
                bufferVal.Add(DEFAULT);
            }
            rowKeys[y] = buffer;
            rowValueKeys[y] = bufferVal;
        }
        /// <inheritdoc cref="IPuzzleKeys{T}.UpdateColumnKeys(int)"/>
        /// <remarks>
        /// Works identically to <see cref="UpdateRowKeys(int)"/>, except that it processes in columns.
        /// </remarks>
        public void UpdateColumnKeys(int x)
        {
            List<int> buffer = new();
            List<T> bufferVal = new();
            T cell;
            T prev = DEFAULT;
            int counter = 0;
            for (int y = 0; y < height; y++)
            {
                cell = GetCell(x, y);
                if (cell.Equals(prev)) counter++;
                else
                {
                    if (!prev.Equals(DEFAULT)) 
                    {
                        buffer.Add(counter);
                        bufferVal.Add(prev);
                    }
                    counter = 1;
                }
                if (y == height - 1 && counter > 0 && !cell.Equals(DEFAULT))
                {
                    buffer.Add(counter);
                    bufferVal.Add(cell);
                    break;
                }
                prev = cell;
            }
            if (buffer.Count == 0) 
            {
                buffer.Add(0);
                bufferVal.Add(DEFAULT);
            }
            columnKeys[x] = buffer;
            columnValueKeys[x] = bufferVal;
        }
    }
    /// <summary>
    /// A nonogram grid representing a black and white puzzle.
    /// </summary>
    /// <remarks>
    /// Each cell is a boolean, with <c>true</c> representing filled cells.
    /// </remarks>
    public class PuzzleBW : PuzzleGrid<bool>
    {
        public static readonly new bool DEFAULT = false;
        public PuzzleBW(int cols, int rows) : base(cols, rows) { }
        public PuzzleBW(int cols, int rows, bool[] data) : base(cols, rows, data) { }
        /// <summary>
        /// Converts puzzle to a <see cref="Bitmap"/> thumbnail.
        /// </summary>
        /// <param name="alpha">Decides whether blank cells are transparent (<c>true</c>) or white (<c>false</c>)</param>
        /// <returns></returns>
        public Bitmap ToBitmap(bool alpha = false)
        {
            Bitmap image = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    image.SetPixel(x, y, GetCell(x, y) ? Color.Black : (alpha ? Color.Transparent : Color.White));
                }
            }
            return image;
        }
    }
    /// <summary>
    /// A nonogram grid representing a colored puzzle.
    /// </summary>
    /// <remarks>
    /// This grid uses an indexed palette.
    /// Each cell contains a <c>byte</c> value representing the index in the <see cref="palette"/>.
    /// </remarks>
    public class PuzzleCol : PuzzleGrid <byte>
    {
        public static readonly new byte DEFAULT = 0;
        /// <summary>
        /// The palette of the puzzle.
        /// </summary>
        /// <remarks>
        /// The background is the first color in the palette.
        /// WARNING: The size of the palette cannot exceed 255.
        /// </remarks>
        public List<Color> palette = new();
        public Color background { get => palette[0]; }
        public PuzzleCol(int cols, int rows) : base(cols, rows) 
        {
            palette.Add(Color.White);
        }
        public PuzzleCol(int cols, int rows, List<Color> colors) : base(cols, rows)
        {
            if (colors.Count == 0) throw new ArgumentException("Color palette must have at least one color.");
            else if (colors.Count > 255) throw new ArgumentException("Color palette is too large (max = 255).");
            palette = colors;
        }
        public PuzzleCol(int cols, int rows, byte[] data, List<Color> colors) : base(cols, rows, data) 
        {
            if (colors.Count == 0) throw new ArgumentException("Color palette must have at least one color.");
            else if (colors.Count > 255) throw new ArgumentException("Color palette is too large (max = 255).");
            palette = colors;
        }
        /// <summary>
        /// Converts the puzzle to a <see cref="Bitmap"/> thumbnail.
        /// </summary>
        public Bitmap ToBitmap()
        {
            Bitmap image = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    image.SetPixel(x, y, palette[GetCell(x, y)]);
                }
            }
            return image;
        }
    }
}
