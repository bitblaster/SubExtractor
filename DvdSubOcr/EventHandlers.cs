using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class SelectedCharacterArgs : EventArgs
    {
        public SelectedCharacterArgs(OcrCharacter oldSelection, OcrCharacter newSelection)
        {
            this.OldSelection = oldSelection;
            this.Selection = newSelection;
        }

        public OcrCharacter OldSelection { get; private set; }
        public OcrCharacter Selection { get; private set; }
    }
}
