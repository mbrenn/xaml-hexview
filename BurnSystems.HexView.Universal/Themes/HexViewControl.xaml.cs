using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
                this.UpdateContent();
            }
        }
        
        public int CurrentlySelected
        {
            get;
            private set;
        }

        /// <summary>
        /// Stores the textblocks storing the byte information
        /// </summary>
        private TextBlock[] byteBlocks;

        /// <summary>
        /// Stores the blocksize of an element
        /// </summary>
        private Size blockSize;

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

            this.scrollBar.Value = 0;
            this.scrollBar.Minimum = 0;
            this.scrollBar.Maximum = totalRows;
            this.scrollBar.LargeChange = Math.Max(rowsInScreen - 1,1);
            this.scrollBar.SmallChange = 1;
            this.scrollBar.ViewportSize = rowsInScreen;
        }

        private void UpdateContent()
        {
            // Defines the offset as defined by the scrollbar
            var offset = Convert.ToInt32(Math.Floor(this.scrollBar.Value)) * columns;
            if (byteBlocks == null)
            {
                // Layout not yet created
                return;
            }

            var amountOfBytes = 0L;
            if (Bytes != null)
            {
                amountOfBytes = Bytes.Length;
            }

            var totalElements = byteBlocks.Length;
            for (var n = 0; n < totalElements; n++)
            {
                var nByte = n + offset;
                if (nByte >= amountOfBytes)
                {
                    byteBlocks[n].Text = "..";
                }
                else
                {
                    var currentByte = Bytes[nByte];
                    var big = (currentByte & 0xF0) >> 4;
                    var small = (currentByte & 0x0F);

                    var bigLetter = hexLettersBig[big];
                    var smallLetter = hexLettersBig[small];

                    byteBlocks[n].Text = $"{bigLetter}{smallLetter}";
                }
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecreateElements(e.NewSize);
        }

        private Size CalculatePosition(int index)
        {
            var row = index / this.columns;
            var column = index % this.columns;

            return new Size(
                column * this.blockSize.Width,
                row * this.blockSize.Height);
        }

        private void RecreateElements(Size newSize)
        {
            this.mainContainer.Children.Clear();
            if (Double.IsNaN(newSize.Width) || Double.IsNaN(newSize.Height))
            {
                // Nothing to do yet
                return;
            }

            // Create an examplary textblock being used to estimate the number of necessary elements
            var textBox = new TextBlock();
            textBox.Text = "999";
            textBox.FontFamily = new FontFamily("Consolas");
            textBox.Arrange(new Rect(0, 0, 1000, 1000));
            textBox.Measure(new Size(1000, 1000));
            this.blockSize = textBox.DesiredSize;

            // Calculates the selection box sizes
            this.SelectionBox.Width = this.blockSize.Width + 2;
            this.SelectionBox.Height = this.blockSize.Height + 2;

            // Creates the elements
            columns = Convert.ToInt32(Math.Floor(newSize.Width / blockSize.Width));
            rows = Convert.ToInt32(Math.Floor(newSize.Height / blockSize.Height));
            
            byteBlocks = new TextBlock[columns * rows];

            var watch = new Stopwatch();
            watch.Start();
            for (var n = 0; n < rows * columns; n++)
            {
                var block = new TextBlock();
                block.Width = blockSize.Width;
                block.Height = blockSize.Height;
                mainContainer.Children.Add(block);
                var position = this.CalculatePosition(n);
                Canvas.SetLeft(block, position.Width);
                Canvas.SetTop(block, position.Height);
                block.Text = "AA ";
                block.FontFamily = textBox.FontFamily;

                int m = n;
                block.PointerPressed += (x, y) =>
                  {
                      Debug.WriteLine(m);

                      if (this.CurrentlySelected != -1)
                      {
                          this.byteBlocks[this.CurrentlySelected].FontWeight =
                            FontWeights.Normal;
                      }

                      this.SelectionBox.Margin = new Thickness(
                          position.Width - 5,
                          position.Height - 2,
                          0, 0);
                      this.SelectionBox.Visibility = Visibility.Visible;
                      block.FontWeight = FontWeights.Bold;
                      this.CurrentlySelected = m;
                  };

                this.byteBlocks[n] = block;
            }

            watch.Stop();
            Debug.WriteLine($"Elapsed: {watch.Elapsed.ToString()}");

            this.UpdateScrollbarProperties();
            this.UpdateContent();
        }

        private void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.UpdateContent();
        }
    }
}
