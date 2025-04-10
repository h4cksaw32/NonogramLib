using System.Drawing;

namespace NonogramLib
{
    public static class Parser
    {
        /// <summary>
        /// Parses a file into a black and white puzzle.
        /// Refer to the source code README for details regarding the file format.
        /// </summary>
        /// <remarks>
        /// Skips the "file signature".
        /// </remarks>
        /// <param name="path">The path of the file.</param>
        public static PuzzleBW ParseBW(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            return ParseBW(file);
        }
        /// <inheritdoc cref="ParseBW(string)"/>
        /// <param name="file">The stream used to access the file.</param>
        public static PuzzleBW ParseBW(FileStream file)
        {
            file.Position = 1; //Skip the file signature
            //Read the size of the puzzle
            int columns = file.ReadByte();
            int rows = file.ReadByte();
            bool[] data = new bool[rows * columns];
            int buffer = 0; //Buffer to read cell data
            int index = 0; //Indexed of cell being assigned
            int mask = 0; //Mask to select individual bits
            while (index < data.Length)
            {
                if (mask == 0)
                {
                    //Reset mask and read next byte
                    mask = 0b10000000;
                    buffer = file.ReadByte();
                }
                if (buffer == -1) data[index] = false; //If end of file is reached, remaining cells will be blank
                else
                {
                    //Assign cell and read next bit
                    data[index] = (buffer & mask) != 0;
                    mask >>>= 1;
                }
                index++;
            }
            file.Close();
            return new PuzzleBW(columns, rows, data);
        }
        /// <summary>
        /// Serializes a black and white puzzle into a byte array.
        /// Refer to the source code README for details regarding the file format.
        /// </summary>
        /// <param name="p">The puzzle to be serialized</param>
        public static byte[] SerializeBW(PuzzleBW p)
        {
            List<byte> output = new();
            output.Add(0); //File signature
            //Puzzle size data
            output.Add((byte)p.width);
            output.Add((byte)p.height);
            byte buffer = 0; //Buffer to add each byte to the output
            byte add = 0b10000000; //An "anti-mask" to add individual bits
            foreach (bool cell in p.cells)
            {
                if (cell) buffer += add;
                add >>>= 1;
                if (add == 0)
                {
                    //Add byte to output and process new byte
                    output.Add(buffer);
                    add = 0b10000000;
                    buffer = 0;
                }
            }
            if (add > 0) output.Add(buffer); //Adds remaining bits if end of cell data is reached
            return output.ToArray();
        }
        /// <summary>
        /// Converts a black and white puzzle using <see cref="SerializeBW(PuzzleBW)"/> and writes it to a file.
        /// </summary>
        /// <param name="path">The path to be written to.</param>
        /// <param name="p">The puzzle to be serialized.</param>
        public static void SerializeBW(string path, PuzzleBW p)
        {
            FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            byte[] data = SerializeBW(p);
            foreach (byte b in data) file.WriteByte(b);
            file.Close();
        }
        /// <summary>
        /// Parses a file into a colored puzzle.
        /// Refer to the source code README for details regarding the file format.
        /// </summary>
        /// <remarks>
        /// Skips the "file signature".
        /// </remarks>
        /// <param name="path">The path of the file.</param>
        public static PuzzleCol ParseCol(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            return ParseCol(file);
        }
        /// <inheritdoc cref="ParseCol(string)"/>
        /// <param name="file">The stream used to access the file.</param>
        public static PuzzleCol ParseCol(FileStream file)
        {
            file.Position = 1; //Skip the file signature
            Color[] colors = new Color[file.ReadByte()]; //Reads the palette size
            byte r, g, b, a; //Buffers for reading each byte of a RGBA value
            for (int i = 0; i < colors.Length; i++)
            {
                //Read 4-byte RGBA values and add them to the palette
                r = (byte)file.ReadByte();
                g = (byte)file.ReadByte();
                b = (byte)file.ReadByte();
                a = (byte)file.ReadByte();
                colors[i] = Color.FromArgb(a, r, g, b);
            }
            //Read size of the puzzle
            int columns = file.ReadByte();
            int rows = file.ReadByte();
            byte[] data = new byte[rows * columns];
            int buffer = 0; //Buffer for reading cell data
            int index = 0; //Index of cell being assigned
            while (index < data.Length)
            {
                //Assigns one byte to each cell
                buffer = file.ReadByte();
                data[index] = (byte)(buffer > -1 ? buffer : 0); //If end of file is reached, remaining cells will be blank
                index++;
            }
            file.Close();
            return new PuzzleCol(columns, rows, data, [.. colors]);
        }
        /// <summary>
        /// Serializes a colored puzzle into a byte array.
        /// Refer to the source code README for details regarding the file format.
        /// </summary>
        /// <param name="p">The puzzle to be serialized</param>
        public static byte[] SerializeCol(PuzzleCol p)
        {
            List<byte> output = new();
            output.Add(254); //File signature
            output.Add((byte)p.palette.Count); //Add palette size
            foreach (Color c in p.palette)
            {
                //Write RGBA bytes for each color in the palette
                output.Add(c.R);
                output.Add(c.G);
                output.Add(c.B);
                output.Add(c.A);
            }
            //Write size data
            output.Add((byte)p.width);
            output.Add((byte)p.height);
            output.AddRange(p.cells); //Write cell data
            return output.ToArray();
        }
        /// <summary>
        /// Converts a colored puzzle using <see cref="SerializeCol(PuzzleCol)"/> and writes it to a file.
        /// </summary>
        /// <param name="path">The path to be written to.</param>
        /// <param name="p">The puzzle to be serialized.</param>
        public static void SerializeCol(string path, PuzzleCol p)
        {
            FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            byte[] data = SerializeCol(p);
            foreach (byte b in data) file.WriteByte(b);
            file.Close();
        }
    }
}
