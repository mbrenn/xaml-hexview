using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BurnSystems.HexView.Universal
{
    public sealed partial class HexViewControl : UserControl
    {
        static char[] hexLettersSmall =
            new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        static char[] hexLettersBig =
            new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Gets or sets the bytes to be shown
        /// </summary>
        public byte[] Bytes
        {
            get { return (byte[])GetValue(BytesProperty); }
            set
            {
                SetValue(BytesProperty, value);
                this.CurrentlySelected = -1;
                InvalidateHexView();
            }
        }

        public int CurrentlySelected
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets the number of columns that shall be shown. 
        /// If the value is 
        /// </summary>
        public int FixedColumnCount
        {
            get { return (int)GetValue(FixedColumnCountProperty); }
            set
            {
                SetValue(FixedColumnCountProperty, value);
                this.mainContainer.Invalidate();
            }
        }


        /// <summary>
        /// Gets or sets the flag, whether ASCII Translation shall be shown
        /// </summary>
        public bool ShowASCIITranslation
        {
            get { return (bool)GetValue(ShowASCIITranslationProperty); }
            set { SetValue(ShowASCIITranslationProperty, value); }
        }

        /// <summary>
        /// Stores the blocksize of an element
        /// </summary>
        private Vector2 blockSize;

        /// <summary>
        /// Stores the blocksize of a single letter
        /// </summary>
        private Vector2 letterSize;

        /// <summary>
        /// Stores the number of columns
        /// </summary>
        private int hexColumns;

        /// <summary>
        /// Stores the number of rows
        /// </summary>
        private int hexRows;

        public static readonly DependencyProperty BytesProperty =
            DependencyProperty.Register("Bytes", typeof(byte[]), typeof(HexViewControl), new PropertyMetadata(new byte[] { }));

        // Using a DependencyProperty as the backing store for FixedColumnCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FixedColumnCountProperty =
            DependencyProperty.Register("FixedColumnCount", typeof(int), typeof(HexViewControl), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for ShowASCIITranslation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowASCIITranslationProperty =
            DependencyProperty.Register("ShowASCIITranslation", typeof(int), typeof(HexViewControl), new PropertyMetadata(false));

        /// <summary>
        /// This event will be thrown, when the user changes the selection
        /// </summary>
        public event EventHandler<ByteSelectionEventArgs> SelectionChanged;

        public HexViewControl()
        {
            InitializeComponent();
        }

        private void InvalidateHexView()
        {
            this.mainContainer.Invalidate();
        }

        /// <summary>
        /// Updates the scrollbar properties, reflecting the latest screensizes and content sizes. 
        /// Drawing has to be called before
        /// </summary>
        private void UpdateScrollbarProperties()
        {
            var bytes = Bytes;
            if (bytes == null || hexColumns <= 0)
            {
                this.scrollBar.Visibility = Visibility.Collapsed;
                return;
            }

            this.scrollBar.Visibility = Visibility.Visible;

            var rowsInScreen = this.hexRows;
            var totalRows = bytes.Length / this.hexColumns;

            //this.scrollBar.Value = 0;
            this.scrollBar.Minimum = 0;
            this.scrollBar.Maximum = totalRows;
            this.scrollBar.LargeChange = Math.Max(rowsInScreen - 1, 1);
            this.scrollBar.SmallChange = 1;
            this.scrollBar.ViewportSize = rowsInScreen;
        }

        private Vector2 CalculatePositionForHex(int index)
        {
            var row = index / this.hexColumns;
            var column = index % this.hexColumns;

            return new Vector2(
                Convert.ToSingle(column * this.blockSize.X),
                Convert.ToSingle(row * this.blockSize.Y));
        }

        private Vector2 CalculatePositionForASCII(int index)
        {
            var row = index / this.hexColumns;
            var column = index % this.hexColumns;
            var offsetFromRight = this.hexColumns - column;
            var right = this.mainContainer.ActualWidth;

            return new Vector2(
                Convert.ToSingle(right - (offsetFromRight * this.letterSize.X)),
                Convert.ToSingle(row * this.blockSize.Y));
        }

        private int CalculateIndex(Vector2 position)
        {
            var column = Convert.ToInt32(position.X / this.blockSize.X);
            var row = Convert.ToInt32(position.Y / this.blockSize.Y);

            if ( column >= hexColumns )
            {
                return -1;
            }

            return row * hexColumns +column;
        }

        private void mainContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this.mainContainer);
            CurrentlySelected = this.CalculateIndex(
                new Vector2(
                    Convert.ToSingle(point.Position.X), 
                    Convert.ToSingle(point.Position.Y)));

            var realPoint = this.CalculatePositionForHex(CurrentlySelected);

            SelectionBox.Margin = new Thickness(
                realPoint.X - 6,
                realPoint.Y - 2,
                0, 0);
            SelectionBox.Visibility = Visibility.Visible;
            SelectionBox.Width = this.blockSize.X + 6;
            SelectionBox.Height = this.blockSize.Y + 5;

            this.InvalidateHexView();
        }

        private void mainContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.InvalidateHexView();
        }

        private void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.InvalidateHexView();
        }

        private void OnSelectionChanged(int index)
        {
            var ev = this.SelectionChanged;
            if (ev != null)
            {
                ev(this, new ByteSelectionEventArgs(index));
            }
        }

        /// <summary>
        /// Called, when drawing is requestes
        /// </summary>
        /// <param name="sender">Sender calling this method</param>
        /// <param name="args">The arguments for drawing</param>
        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            var format = new CanvasTextFormat { FontSize = 16, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = "Consolas" };
            var boldFormat = new CanvasTextFormat { FontSize = 16, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = "Consolas", FontWeight = FontWeights.Bold };
            var textLayout = new CanvasTextLayout(ds, "W", format, 0.0f, 0.0f);

            this.letterSize =
                new Vector2(
                    Convert.ToSingle(textLayout.DrawBounds.Width),
                    Convert.ToSingle(textLayout.DrawBounds.Height));

            this.blockSize = new Vector2(
                Convert.ToSingle(letterSize.X * 3),
                Convert.ToSingle(letterSize.Y + 8));

            var totalElements = CalculateRowsAndColumns(textLayout);
            var showAscii = this.ShowASCIITranslation;

            // Defines the offset for data as defined by the scrollbar
            var offset = Convert.ToInt32(Math.Floor(this.scrollBar.Value)) * hexColumns;

            // Calculates the amount of bytes, being used to findout the number of bytes
            var amountOfBytes = 0L;
            if (Bytes != null)
            {
                amountOfBytes = Bytes.Length;
            }

            // Draw the bytes themselves
            for (var n = 0; n < totalElements; n++)
            {
                var pos = this.CalculatePositionForHex(n);
                var posASCII = this.CalculatePositionForASCII(n);
                var nByte = n + offset;
                if (nByte >= amountOfBytes)
                {
                    ds.DrawText("..",
                        pos,
                        Colors.Black,
                        format);

                    if (showAscii)
                    {
                        ds.DrawText(
                            ".",
                            posASCII,
                            Colors.Black,
                            format);
                    }
                }
                else
                {
                    var text = GetTextForByte(nByte);

                    var tf = format;
                    if (CurrentlySelected == n)
                    {
                        tf = boldFormat;
                    }

                    ds.DrawText(text,
                            pos,
                            Colors.Black,
                            tf);

                    if (showAscii)
                    {
                        ds.DrawText(
                            new string(new char[] { ConvertToASCII(Bytes[nByte]) }),
                            posASCII,
                            Colors.Black,
                            tf);
                    }
                }
            }

            this.UpdateScrollbarProperties();
        }

        private string GetTextForByte(int index)
        {
            var currentByte = Bytes[index];
            var big = (currentByte & 0xF0) >> 4;
            var small = (currentByte & 0x0F);

            var bigLetter = hexLettersBig[big];
            var smallLetter = hexLettersBig[small];
            var text = $"{bigLetter}{smallLetter}";
            return text;
        }

        private int CalculateRowsAndColumns(CanvasTextLayout textLayout)
        {
            var effectiveSize = this.blockSize;
            if ( this.ShowASCIITranslation)
            {
                effectiveSize.X += this.letterSize.X;
            }

            // Calculates the number of columns and rows in the view
            hexColumns = Convert.ToInt32(Math.Floor(mainContainer.ActualWidth / effectiveSize.X));
            hexRows = Convert.ToInt32(Math.Floor(mainContainer.ActualHeight / effectiveSize.Y));

            // Checks., if the number of columns need to be reduced according to property FieldColumns
            if (this.FixedColumnCount != 0 && this.FixedColumnCount < hexColumns)
            {
                hexColumns = this.FixedColumnCount;
            }

            var totalElements = hexRows * hexColumns;
            return totalElements;
        }

        private static Encoding ISO8859 = System.Text.Encoding.GetEncoding("iso-8859-1");

        /// <summary>
        /// Converts the given byte to an ASCII character
        /// </summary>
        /// <param name="value">Value to be converted</param>
        /// <returns>Converted character</returns>
        private static char ConvertToASCII(byte value)
        {
            /*if (value < 32 || value >= 127)
            {
                return '.';
            }
            else*/
            {
                return ISO8859.GetChars(new byte[] { value })[0];
            }
        }
    }
}
