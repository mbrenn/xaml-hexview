using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
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
                this.UpdateScrollbarProperties();
                //this.UpdateContent();
            }
        }
        
        public int CurrentlySelected
        {
            get;
            private set;
        }

        /// <summary>
        /// Stores the blocksize of an element
        /// </summary>
        private Vector2 blockSize;

        /// <summary>
        /// Stores the number of columns
        /// </summary>
        private int columns;

        /// <summary>
        /// Stores the number of rows
        /// </summary>
        private int rows;

        public static readonly DependencyProperty BytesProperty =
            DependencyProperty.Register("Bytes", typeof(byte[]), typeof(HexViewControl), new PropertyMetadata(new byte[] { }));

        /// <summary>
        /// This event will be thrown, when the user changes the selection
        /// </summary>
        public event EventHandler<ByteSelectionEventArgs> SelectionChanged;

        public HexViewControl()
        {
            InitializeComponent();
        }


        private void UpdateScrollbarProperties()
        {
            var bytes = Bytes;
            if (bytes == null || columns <= 0)
            {
                this.scrollBar.Visibility = Visibility.Collapsed;
                return;
            }

            this.scrollBar.Visibility = Visibility.Visible;

            var rowsInScreen = this.rows;
            var totalRows = bytes.Length / this.columns;

            //this.scrollBar.Value = 0;
            this.scrollBar.Minimum = 0;
            this.scrollBar.Maximum = totalRows;
            this.scrollBar.LargeChange = Math.Max(rowsInScreen - 1, 1);
            this.scrollBar.SmallChange = 1;
            this.scrollBar.ViewportSize = rowsInScreen;
        }

        /*        private void blockPressed(TextBlock block, Size position, int m)
        {
            // Remove selection of old field
            if (this.CurrentlySelected != -1)
            {
                this.byteBlocks[this.CurrentlySelected].FontWeight =
                  FontWeights.Normal;
            }

            this.SelectionBox.Margin = new Thickness(
                position.Width - 6,
                position.Height - 3,
                0, 0);

            this.SelectionBox.Visibility = Visibility.Visible;
            block.FontWeight = FontWeights.Bold;
            this.CurrentlySelected = m;

            this.OnSelectionChanged(m);
        }*/

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.mainContainer.Invalidate();
            this.UpdateScrollbarProperties();
        }

        private Vector2 CalculatePosition(int index)
        {
            var row = index / this.columns;
            var column = index % this.columns;

            return new Vector2(
                Convert.ToSingle(column * this.blockSize.X),
                Convert.ToSingle(row * this.blockSize.Y));
        }

        private int CalculateIndex(Vector2 position)
        {
            var column = Convert.ToInt32(position.X / this.blockSize.X);
            var row = Convert.ToInt32(position.Y / this.blockSize.Y);

            return row * columns +column;
        }

        private void mainContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this.mainContainer);
            CurrentlySelected = this.CalculateIndex(
                new Vector2(
                    Convert.ToSingle(point.Position.X), 
                    Convert.ToSingle(point.Position.Y)));

            var realPoint = this.CalculatePosition(CurrentlySelected);

            SelectionBox.Margin = new Thickness(
                realPoint.X - 6,
                realPoint.Y - 2,
                0, 0);
            SelectionBox.Visibility = Visibility.Visible;
            SelectionBox.Width = this.blockSize.X + 6;
            SelectionBox.Height = this.blockSize.Y + 5;

            mainContainer.Invalidate();
        }

        private void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.mainContainer.Invalidate();
        }

        private void OnSelectionChanged(int index)
        {
            var ev = this.SelectionChanged;
            if (ev != null)
            {
                ev(this, new ByteSelectionEventArgs(index));
            }
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            var format = new CanvasTextFormat { FontSize = 16, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily="Consolas" };
            var boldFormat = new CanvasTextFormat { FontSize = 16, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = "Consolas", FontWeight = FontWeights.Bold };
            var textLayout = new CanvasTextLayout(ds, "123", format, 0.0f, 0.0f);

            this.blockSize = new Vector2(
                Convert.ToSingle(textLayout.DrawBounds.Width),
                Convert.ToSingle(textLayout.DrawBounds.Height + 8));
            // Creates the elements
            columns = Convert.ToInt32(Math.Floor(mainContainer.ActualWidth / textLayout.DrawBounds.Width));
            rows = Convert.ToInt32(Math.Floor(mainContainer.ActualHeight / textLayout.DrawBounds.Height));

            // Defines the offset as defined by the scrollbar
            var offset = Convert.ToInt32(Math.Floor(this.scrollBar.Value)) * columns;

            var amountOfBytes = 0L;
            if (Bytes != null)
            {
                amountOfBytes = Bytes.Length;
            }

            var totalElements = rows * columns;
            for (var n = 0; n < totalElements; n++)
            {
                var pos = this.CalculatePosition(n);
                var nByte = n + offset;
                if (nByte >= amountOfBytes)
                {
                    ds.DrawText("..",
                        pos,
                        Colors.Black,
                        format);
                }
                else
                {
                    var currentByte = Bytes[nByte];
                    var big = (currentByte & 0xF0) >> 4;
                    var small = (currentByte & 0x0F);

                    var bigLetter = hexLettersBig[big];
                    var smallLetter = hexLettersBig[small];

                    var tf = format;
                    if (CurrentlySelected == n)
                    {
                        tf = boldFormat;
                    }

                    ds.DrawText($"{bigLetter}{smallLetter}",
                            pos,
                            Colors.Black,
                            tf);
                }
            }

            this.UpdateScrollbarProperties();
        }
    }
}
