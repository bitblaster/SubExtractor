using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DvdSubOcr;

namespace DvdSubExtractor
{
    class SdhSubLine
    {
        public IList<OcrCharacter> Text { get; set; }
        public int RectangleIndex { get; set; }
        public bool LineStartRemoved { get; set; }
        public SubtitleLine OriginalLine { get; set; }

        public static void RemoveSDH(IList<SdhSubLine> lines)
        {
            for(int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                SdhSubLine sdh = lines[lineIndex];
                IList<OcrCharacter> currentText = sdh.Text;
                List<OcrCharacter> newText = null;
                bool removedStuff;
                do
                {
                    removedStuff = false;
                    for(int ocrIndex = 0; ocrIndex < currentText.Count; ocrIndex++)
                    {
                        switch(currentText[ocrIndex].Value)
                        {
                        case '[':
                            for(int ocrIndex2 = ocrIndex + 1; ocrIndex2 < currentText.Count; ocrIndex2++)
                            {
                                if(currentText[ocrIndex2].Value == ']')
                                {
                                    if((ocrIndex > 0) && (currentText[ocrIndex - 1].Value == ' '))
                                    {
                                        ocrIndex--;
                                    }
                                    if((ocrIndex2 < currentText.Count - 1) && (currentText[ocrIndex2 + 1].Value == ' '))
                                    {
                                        ocrIndex2++;
                                    }
                                    newText = new List<OcrCharacter>(currentText);
                                    newText.RemoveRange(ocrIndex, ocrIndex2 - ocrIndex + 1);
                                    removedStuff = true;
                                    break;
                                }
                            }
                            if(!removedStuff && (lineIndex < lines.Count - 1))
                            {
                                SdhSubLine sdh2 = lines[lineIndex + 1];
                                for(int ocrIndex2 = 0; ocrIndex2 < sdh2.Text.Count; ocrIndex2++)
                                {
                                    OcrCharacter c2 = sdh2.Text[ocrIndex2];
                                    if(c2.Value == '[')
                                    {
                                        break;
                                    }
                                    if(c2.Value == ']')
                                    {
                                        if((ocrIndex > 0) && (currentText[ocrIndex - 1].Value == ' '))
                                        {
                                            ocrIndex--;
                                        }
                                        newText = new List<OcrCharacter>(currentText.Take(ocrIndex));
                                        removedStuff = true;

                                        if((ocrIndex2 < sdh2.Text.Count - 1) && (sdh2.Text[ocrIndex2 + 1].Value == ' '))
                                        {
                                            ocrIndex2++;
                                        }
                                        sdh2.Text = new List<OcrCharacter>(sdh2.Text.Skip(ocrIndex2 + 1));
                                        sdh2.LineStartRemoved = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case '(':
                            for(int ocrIndex2 = ocrIndex + 1; ocrIndex2 < currentText.Count; ocrIndex2++)
                            {
                                if(currentText[ocrIndex2].Value == ')')
                                {
                                    if((ocrIndex > 0) && (currentText[ocrIndex - 1].Value == ' '))
                                    {
                                        ocrIndex--;
                                    }
                                    if((ocrIndex2 < currentText.Count - 1) && (currentText[ocrIndex2 + 1].Value == ' '))
                                    {
                                        ocrIndex2++;
                                    }
                                    newText = new List<OcrCharacter>(currentText);
                                    newText.RemoveRange(ocrIndex, ocrIndex2 - ocrIndex + 1);
                                    removedStuff = true;
                                    break;
                                }
                            }
                            if(!removedStuff && (lineIndex < lines.Count - 1))
                            {
                                SdhSubLine sdh2 = lines[lineIndex + 1];
                                for(int ocrIndex2 = 0; ocrIndex2 < sdh2.Text.Count; ocrIndex2++)
                                {
                                    OcrCharacter c2 = sdh2.Text[ocrIndex2];
                                    if(c2.Value == '(')
                                    {
                                        break;
                                    }
                                    if(c2.Value == ')')
                                    {
                                        if((ocrIndex > 0) && (currentText[ocrIndex - 1].Value == ' '))
                                        {
                                            ocrIndex--;
                                        }
                                        newText = new List<OcrCharacter>(currentText.Take(ocrIndex));
                                        removedStuff = true;

                                        if((sdh2.Text.Count > ocrIndex2 + 1) && (sdh2.Text[ocrIndex2 + 1].Value == ' '))
                                        {
                                            ocrIndex2++;
                                        }
                                        sdh2.Text = new List<OcrCharacter>(sdh2.Text.Skip(ocrIndex2 + 1));
                                        sdh2.LineStartRemoved = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case ':':
                            {
                                bool doRemoval = true;
                                if((ocrIndex > 0) && (ocrIndex < currentText.Count - 1))
                                {
                                    // check for time values (8:00) which obviously aren't SDH text
                                    if(Char.IsDigit(currentText[ocrIndex - 1].Value) && Char.IsDigit(currentText[ocrIndex + 1].Value))
                                    {
                                        doRemoval = false;
                                    }
                                }
                                if(doRemoval)
                                {
                                    bool hasLower = false;
                                    bool hasSpace = false;
                                    foreach(OcrCharacter ocr in currentText.Take(ocrIndex))
                                    {
                                        if(Char.IsLower(ocr.Value))
                                        {
                                            hasLower = true;
                                        }
                                        if(ocr.Value == ' ')
                                        {
                                            hasSpace = true;
                                        }
                                    }
                                    if(hasSpace && hasLower)
                                    {
                                        doRemoval = false;
                                    }
                                }
                                if(doRemoval)
                                {
                                    newText = new List<OcrCharacter>(currentText.Skip(ocrIndex + 1));
                                    if((newText.Count > 0) && (newText[0].Value == ' '))
                                    {
                                        newText.RemoveAt(0);
                                    }
                                    ocrIndex = 0;
                                    removedStuff = true;
                                }
                            }
                            break;
                        }
                        if(removedStuff)
                        {
                            if(ocrIndex == 0)
                            {
                                sdh.LineStartRemoved = true;
                            }
                            else
                            {
                                bool allLinebreaks = true;
                                for(int testIndex = 0; testIndex < ocrIndex; testIndex++)
                                {
                                    Char startChar = currentText[testIndex].Value;
                                    if(!SubConstants.CharactersThatSignalLineBreak.Contains(startChar) && !Char.IsWhiteSpace(startChar))
                                    {
                                        allLinebreaks = false;
                                        break;
                                    }
                                }
                                if(allLinebreaks)
                                {
                                    sdh.LineStartRemoved = true;
                                }
                            }

                            if(sdh.LineStartRemoved && (newText.Count != 0))
                            {
                                bool emptyLine = true;
                                foreach(OcrCharacter ocr in newText)
                                {
                                    if(!SubConstants.CharactersThatSignalLineBreak.Contains(ocr.Value) && !Char.IsWhiteSpace(ocr.Value))
                                    {
                                        emptyLine = false;
                                        break;
                                    }
                                }
                                if(emptyLine)
                                {
                                    newText.Clear();
                                }
                            }
                            sdh.Text = newText;
                            currentText = newText;
                            break;
                        }
                    }
                }
                while(removedStuff);
            }
        }
    }
}
