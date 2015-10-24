using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurnSystems.HexView.Universal
{
    public class ByteSelectionEventArgs : EventArgs
    {
        public int indexByte
        {
            get;
            private set;
        }

        public ByteSelectionEventArgs(int indexByte)
        {
            this.indexByte = indexByte;
        }
    }
}
