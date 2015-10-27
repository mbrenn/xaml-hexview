using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace BurnSystems.HexView.Universal.App
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ByteView0.Bytes = new byte[] { 0x33, 0x34, 0x35, 0x37 };

            var amount = 256 * 4;
            var otherBytes = new byte[amount];
            for (var n = 0; n < amount; n++)
            {
                otherBytes[n] = (byte)n;
            }

            ByteView2.Bytes = otherBytes;
        }

        private void txtTestString_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var text = txtTestString.Text;
            ByteView0.Bytes = Encoding.ASCII.GetBytes(text);
            ByteView2.Bytes = Encoding.UTF8.GetBytes(text);
        }
    }
}
