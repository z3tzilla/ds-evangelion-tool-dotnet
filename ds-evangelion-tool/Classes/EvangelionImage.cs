using System;
using System.Drawing;
using System.IO;

namespace DsEvangelionTool
{
    /// <summary>
    /// Evangelion Image
    /// </summary>
    /// <!--
    ///     Image format: binary, 4-bit (16 colors). Stores color number in palette.
    ///     
    ///     The image is split into a series of 8x8 tiles which are written sequentially:
    ///     Tile1 Tile2 Tile3
    ///     Tile4 Tile5 Tile6
    ///     
    ///     Values inside the tiles are stored like this:
    ///     2  1  4  3  6  5  8  7
    ///     10 9  12 11 14 13 16 15
    ///     ...
    ///     58 57 60 59 62 61 64 63
    /// -->
    class EvangelionImage
    {
        #region Properties
        /// <summary>Image Width</summary>
        public int Width { get; private set; }
        /// <summary>Image Height</summary>
        public int Height { get; private set; }
        /// <summary>Пиксели изображения</summary>
        public int[,] Pixels { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create new blank image
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        public EvangelionImage(int width, int height)
        {
            createImage(width, height);
        }

        /// <summary>
        /// Create an image from file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="type">File type</param>
        /// <param name="width">Image width for binary file</param>
        public EvangelionImage(string filePath, ImageType type, int width = 0)
        {
            LoadFromFile(filePath, type, width);
        }

        /// <summary>
        /// Validation + Initialization
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        private void createImage(int width, int height)
        {
            if (width % 8 != 0) throw new ArgumentException("Width must be a multiple of 8", "width");
            if (height % 8 != 0) throw new ArgumentException("Height must be a multiple of 8", "height");

            Width = width;
            Height = height;
            Pixels = new int[width, height];
        }
        #endregion

        #region Saving
        /// <summary>
        /// Save image to file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="type">File type</param>
        public void SaveToFile(string filePath, ImageType type)
        {
            switch (type)
            {
                case ImageType.Binary:
                    SaveToBinary(filePath);
                    break;
                case ImageType.Png:
                    SaveToPng(filePath);
                    break;
            }
        }

        /// <summary>
        /// Save as Png
        /// </summary>
        /// <param name="filePath">Path to file</param>
        private void SaveToPng(string filePath)
        {
            var pngImage = new Bitmap(Width, Height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int col = Pixels[x, y] * 0x11;
                    pngImage.SetPixel(x, y, Color.FromArgb(col, col, col));
                }
            }

            pngImage.Save(filePath);
        }

        /// <summary>
        /// Save as binary
        /// </summary>
        /// <param name="filePath">Path to file</param>
        private void SaveToBinary(string filePath)
        {
            byte[] binaryImage = new byte[(Height * Width) / 2];
            int index = 0;

            for (int tileY = 0; tileY < Height / 8; tileY++)
            {
                for (int tileX = 0; tileX < Width / 8; tileX++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x += 2)
                        {
                            binaryImage[index++] =
                                Convert.ToByte(Pixels[tileX * 8 + x + 1, tileY * 8 + y] * 0x10 +
                                Pixels[tileX * 8 + x, tileY * 8 + y]);
                        }
                    }
                }
            }

            File.WriteAllBytes(filePath, binaryImage);
        }
        #endregion

        #region Loading
        /// <summary>
        /// Load image from a file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="type">File type</param>
        /// <param name="width">Image width, required for binary</param>
        public void LoadFromFile(string filePath, ImageType type, int width = 0)
        {
            switch (type)
            {
                case ImageType.Binary:
                    loadFromBinary(filePath, width);
                    break;
                case ImageType.Png:
                    loadFromPng(filePath);
                    break;
            }
        }

        /// <summary>
        /// Load from Png
        /// </summary>
        /// <param name="filePath">Path to file</param>
        private void loadFromPng(string filePath)
        {
            var pngImage = new Bitmap(filePath);
            createImage(pngImage.Width, pngImage.Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Pixels[x, y] = pngImage.GetPixel(x, y).R / 0x11;
                }
            }
        }

        /// <summary>
        /// Load from binary
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="width">Image width</param>
        private void loadFromBinary(string filePath, int width)
        {
            if (width == 0) throw new ArgumentException("Width not specified for binary format", "width");

            byte[] binaryImage = File.ReadAllBytes(filePath);
            int height = (binaryImage.GetLength(0) / width) * 2;
            int index = 0;

            createImage(width, height);

            for (int tileY = 0; tileY < Height / 8; tileY++)
            {
                for (int tileX = 0; tileX < Width / 8; tileX++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x += 2)
                        {
                            byte byte1 = (byte)((binaryImage[index] & 0xF0) >> 4);
                            byte byte2 = (byte)(binaryImage[index] & 0x0F);

                            Pixels[tileX * 8 + x + 1, tileY * 8 + y] = byte1;
                            Pixels[tileX * 8 + x, tileY * 8 + y] = byte2;

                            index++;
                        }
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Supported image types
    /// </summary>
    public enum ImageType
    {
        /// <summary>In-game binary format</summary>
        Binary,
        /// <summary>Png</summary>
        Png
    }
}