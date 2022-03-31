using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class SpellingCorrectionResult
    {
        public string OriginalWord { get; set; }
        public int CharacterIndex { get; set; }
        public IEnumerable<string> Choices { get; set; }
    }

    public class SubtitleLine
    {
        public SubtitleLine(IEnumerable<OcrCharacter> characters,
            IEnumerable<Rectangle> characterBounds, IEnumerable<int> blockIndexes,
            Rectangle bounds, int colorIndex)
        {
            this.Text = new List<OcrCharacter>(characters);
            this.TextBounds = new List<Rectangle>(characterBounds);
            this.BlockIndexes = new List<int>(blockIndexes);
            this.Bounds = bounds;
            this.ColorIndex = colorIndex;
        }

        public IList<OcrCharacter> Text { get; private set; }
        public IList<Rectangle> TextBounds { get; private set; }
        public IList<int> BlockIndexes { get; private set; }
        public Rectangle Bounds { get; private set; }
        public int ColorIndex { get; private set; }
        public string Comment { get; set; }
        public int RectangleIndex { get; set; }
        public static string CharactersNeverAfterASpace { get; set; }

        public IEnumerable<KeyValuePair<bool, string>> SplitByItalics()
        {
            if(this.Text.Count != 0)
            {
                bool isItalic = this.Text[0].Italic;
                StringBuilder sb = new StringBuilder();
                foreach(OcrCharacter ocr in this.Text)
                {
                    if(ocr.Italic != isItalic)
                    {
                        yield return new KeyValuePair<bool, string>(isItalic, sb.ToString());
                        isItalic = ocr.Italic;
                        sb.Length = 0;
                    }
                    sb.Append(ocr.Value);
                }
                yield return new KeyValuePair<bool, string>(isItalic, sb.ToString());
            }
        }

        public override string ToString()
        {
            string text = this.Text.Aggregate(new StringBuilder(), (sb, ocr) => sb.Append(ocr.Value), sb => sb.ToString());
            return text;
        }

        public enum DominantFontStyle
        {
            Normal = 0,
            Italic = 1,
        }

        public DominantFontStyle DominantFont
        {
            get
            {
                int normalCount = 0, italicCount = 0, normalCharacterCount = 0, italicCharacterCount = 0;
                foreach(OcrCharacter ocr in this.Text)
                {
                    if(Char.IsLetterOrDigit(ocr.Value) || Char.IsSymbol(ocr.Value))
                    {
                        if(ocr.Italic)
                        {
                            italicCharacterCount++;
                        }
                        else
                        {
                            normalCharacterCount++;
                        }
                    }
                    else
                    {
                        if(ocr.Italic)
                        {
                            italicCount++;
                        }
                        else
                        {
                            normalCount++;
                        }
                    }
                }
                if((normalCharacterCount > 0) || (italicCharacterCount > 0))
                {
                    return (normalCharacterCount >= italicCharacterCount) ? DominantFontStyle.Normal :
                        DominantFontStyle.Italic;
                }
                return (normalCount >= italicCount) ? DominantFontStyle.Normal :
                        DominantFontStyle.Italic;
            }
        }

        public void InsertSpaces(FontKerning fontKerning, FontKerning fontKerningItalic)
        {
            if((fontKerning.Separations.Count == 0) && (fontKerningItalic.Separations.Count == 0))
            {
                return;
            }

            // fix punctuation that's mistakenly italic or not based on surrounding characters
            if(this.Text.Count > 1)
            {
                for(int index = 0; index < this.Text.Count; index++)
                {
                    char c = Text[index].Value;
                    if(SubConstants.MistakenForItalicCharacters.Contains(c))
                    {
                        bool isItalic = Text[index].Italic;
                        if(((index == 0) || (Text[index - 1].Italic != isItalic)) &&
                            ((index == this.Text.Count - 1) || (Text[index + 1].Italic != isItalic)))
                        {
                            this.Text[index] = new OcrCharacter(c, !isItalic);
                        }
                        else if(SubConstants.MistakenForItalicRepeatingCharacters.Contains(c))
                        {
                            int mistakeLength = 1;
                            for(int subIndex = index + 1; subIndex < this.Text.Count; subIndex++)
                            {
                                if(this.Text[subIndex].Value != c)
                                {
                                    break;
                                }
                                mistakeLength++;
                            }
                            if((index != 0) || (index + mistakeLength < this.Text.Count))
                            {
                                bool startItalic, endItalic;
                                if(index != 0)
                                {
                                    startItalic = this.Text[index - 1].Italic;
                                }
                                else
                                {
                                    startItalic = this.Text[index + mistakeLength].Italic;
                                }
                                if(index + mistakeLength < this.Text.Count)
                                {
                                    endItalic = this.Text[index + mistakeLength].Italic;
                                }
                                else
                                {
                                    endItalic = this.Text[index - 1].Italic;
                                }
                                if(startItalic == endItalic)
                                {
                                    for(int fixIndex = index; fixIndex < index + mistakeLength; fixIndex++)
                                    {
                                        this.Text[fixIndex] = new OcrCharacter(c, startItalic);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            IList<int> peaks = fontKerning.Peaks;
            IList<int> italicPeaks = fontKerningItalic.Peaks;

            int leftIndex = 0;
            for(int rightIndex = 1; rightIndex < this.Text.Count; rightIndex++)
            {
                char cLeft = Text[leftIndex].Value;
                char cRight = Text[rightIndex].Value;

                if(LineLayout.IsDiacritic(cRight))
                {
                    continue;
                }

                if(CharactersNeverAfterASpace.Contains(cRight))
                {
                    leftIndex = rightIndex;
                    continue;
                }

                int peakCount = -1;
                int diff = TextBounds[rightIndex].Left - TextBounds[leftIndex].Right;
                diff += FontKerning.FindKerning(Text[leftIndex], Text[rightIndex]);

                IList<int> diffPeaks = this.Text[leftIndex].Italic ? italicPeaks : peaks;
                int lastDiff = 1000;
                foreach(int peak in diffPeaks)
                {
                    int diffFromPeak = Math.Abs(diff - peak);
                    if(diffFromPeak < lastDiff)
                    {
                        peakCount++;
                        lastDiff = diffFromPeak;
                    }
                }

                if((Text[leftIndex].Italic != Text[rightIndex].Italic))
                {
                    if(Char.IsLetter(cLeft) && Char.IsLetter(cRight))
                    {
                        peakCount = 1;
                    }

                    if(peakCount > 0)
                    {
                        switch(cLeft)
                        {
                        case '-':
                        case '—':
                            peakCount = 0;
                            break;
                        default:
                            switch(cRight)
                            {
                            case '-':
                            case '—':
                            case '?':
                            case '!':
                            case '.':
                            case ',':
                                peakCount = 0;
                                break;
                            }
                            break;
                        }
                    }
                    else
                    {
                        if(peakCount == 0)
                        {
                            switch(cLeft)
                            {
                            case '-':
                            case '—':
                                break;
                            default:
                                switch(cRight)
                                {
                                case '-':
                                case '—':
                                case '?':
                                case '!':
                                case '.':
                                case ',':
                                    break;
                                default:
                                    Console.WriteLine("Leaving NO space between italics and non-italics");
                                    break;
                                }
                                break;
                            }
                        }
                    }
                }

                if((peakCount > 0) && (cRight == '\"') && Text[rightIndex].Italic && SubConstants.CharactersThatEndQuotes.Contains(cLeft))
                {
                    peakCount = 0;
                }

                if(peakCount > 0)
                {
                    this.Text.Insert(rightIndex, new OcrCharacter(' ', this.Text[leftIndex].Italic && this.Text[rightIndex].Italic));
                    this.TextBounds.Insert(rightIndex, Rectangle.Empty);
                    rightIndex++;
                }
                leftIndex = rightIndex;
            }
        }

        public void CorrectObviousErrorsInLine()
        {
            bool? startedWithItalics = null;
            int untrustworthyCount = 0;
            for(int index = 0; index < this.Text.Count; index++)
            {
                OcrCharacter ocr = this.Text[index];
                if(SubConstants.UntrustworthyAsItalics.Contains(ocr.Value))
                {
                    untrustworthyCount++;
                }
                else
                {
                    if(startedWithItalics.HasValue && (startedWithItalics.Value != ocr.Italic))
                    {
                        untrustworthyCount = 0;
                    }
                    else
                    {
                        while(untrustworthyCount > 0)
                        {
                            this.Text[index - untrustworthyCount] = new OcrCharacter(
                                this.Text[index - untrustworthyCount].Value, ocr.Italic);
                            untrustworthyCount--;
                        }
                    }
                    startedWithItalics = ocr.Italic;
                }
            }
            if(startedWithItalics.HasValue)
            {
                int index = this.Text.Count;
                while(untrustworthyCount > 0)
                {
                    this.Text[index - untrustworthyCount] = new OcrCharacter(
                        this.Text[index - untrustworthyCount].Value, startedWithItalics.Value);
                    untrustworthyCount--;
                }
            }
        }

        public IEnumerable<SpellingCorrectionResult> CorrectSpelling(OcrMap ocrMap, HashSet<string> ignoredWords)
        {
            StringBuilder sb = new StringBuilder();
            for(int index = 0; index < this.Text.Count; index++)
            {
                OcrCharacter ocr = this.Text[index];
                if((ocr.Value != '\'') && !Char.IsLetter(ocr.Value))
                {
                    if(sb.Length != 0)
                    {
                        if(!ignoredWords.Contains(sb.ToString()))
                        {
                            SpellingCorrectionResult newSpelling = CheckWord(ocrMap, sb.ToString(), index - sb.Length);
                            if(newSpelling != null)
                            {
                                yield return newSpelling;
                            }
                        }
                        sb = new StringBuilder();
                    }
                }
                else
                {
                    // drop a leading ' from words before spell-checking
                    if((ocr.Value != '\'') || (sb.Length != 0))
                    {
                        sb.Append(ocr.Value);
                    }
                    else
                    {
                        sb = new StringBuilder();
                    }
                }
            }

            if((sb.Length != 0) && !ignoredWords.Contains(sb.ToString()))
            {
                SpellingCorrectionResult newSpelling = CheckWord(ocrMap, sb.ToString(), this.Text.Count - sb.Length);
                if(newSpelling != null)
                {
                    yield return newSpelling;
                }
            }
        }

        public void CorrectSpelling(OcrMap ocrMap)
        {
            StringBuilder sb = new StringBuilder();
            for(int index = 0; index < this.Text.Count; index++)
            {
                OcrCharacter ocr = this.Text[index];
                if((ocr.Value != '\'') && !Char.IsLetter(ocr.Value))
                {
                    if(sb.Length != 0)
                    {
                        CheckWord(ocrMap, sb.ToString(), index - sb.Length);
                        sb = new StringBuilder();
                    }
                }
                else
                {
                    sb.Append(ocr.Value);
                }
            }

            if(sb.Length != 0)
            {
                CheckWord(ocrMap, sb.ToString(), this.Text.Count - sb.Length);
            }
        }

        SpellingCorrectionResult CheckWord(OcrMap ocrMap, string word, int charIndex)
        {
            List<int> testCharacters = new List<int>();
            if((word[0] == 'l') || (word[0] == 'I'))
            {
                testCharacters.Add(0);
            }

            bool hasLowers = false;
            bool allUppers = true;

            string caseTestWord = word;
            if((caseTestWord.Length > 2) && (caseTestWord[caseTestWord.Length - 1] == 's'))
            {
                caseTestWord = caseTestWord.Substring(0, caseTestWord.Length - 1);
            }
            if((caseTestWord.Length > 1) && (caseTestWord[caseTestWord.Length - 1] == '\''))
            {
                caseTestWord = caseTestWord.Substring(0, caseTestWord.Length - 1);
            }

            foreach(char c in caseTestWord)
            {
                bool isLower = Char.IsLower(c);
                if((c != 'l') && isLower)
                {
                    hasLowers = true;
                    allUppers = false;
                    break;
                }
                if(c == 'I')
                {
                    hasLowers = true;
                }
            }

            if(hasLowers)
            {
                for(int index = 1; index < word.Length; index++)
                {
                    if(word[index] == 'I')
                    {
                        if((testCharacters.Count == 0) && (caseTestWord.Length > 3))
                        {
                            string foundWord;
                            if(allUppers)
                            {
                                foundWord = word.Replace('l', 'I');
                            }
                            else
                            {
                                foundWord = word.Replace('I', 'l');
                            }
                            for(int subIndex = 0; subIndex < word.Length; subIndex++)
                            {
                                this.Text[subIndex + charIndex] = new OcrCharacter(
                                    foundWord[subIndex], this.Text[subIndex + charIndex].Italic);
                            }
                            return null;
                        }
                        testCharacters.Add(index);
                    }
                }
            }
            else
            {
                for(int index = 1; index < word.Length; index++)
                {
                    if(word[index] == 'l')
                    {
                        if((testCharacters.Count == 0) && (caseTestWord.Length > 3) && allUppers)
                        {
                            string foundWord = word.Replace('l', 'I');
                            for(int subIndex = 0; subIndex < word.Length; subIndex++)
                            {
                                this.Text[subIndex + charIndex] = new OcrCharacter(
                                    foundWord[subIndex], this.Text[subIndex + charIndex].Italic);
                            }
                            return null;
                        }
                        testCharacters.Add(index);
                    }
                }
            }

            if(testCharacters.Count == 0)
            {
                return null;
            }

            List<string> testWords = new List<string>();
            StringBuilder sb = new StringBuilder(word);
            sb[testCharacters[0]] = 'l';
            testWords.Add(sb.ToString());
            sb[testCharacters[0]] = 'I';
            testWords.Add(sb.ToString());
            for(int index = 1; index < testCharacters.Count; index++)
            {
                List<string> newWords = new List<string>();
                foreach(string testWord in testWords)
                {
                    sb = new StringBuilder(testWord);
                    if(testWord[testCharacters[index]] == 'l')
                    {
                        sb[testCharacters[index]] = 'I';
                    }
                    else
                    {
                        sb[testCharacters[index]] = 'l';
                    }
                    newWords.Add(sb.ToString());
                }
                testWords.AddRange(newWords);
            }

            foreach(string testWord in testWords)
            {
                if(ocrMap.FindLandIWord(testWord))
                {
                    if(testWord != word)
                    {
                        for(int index = 0; index < word.Length; index++)
                        {
                            this.Text[index + charIndex] = new OcrCharacter(testWord[index],
                                    this.Text[index + charIndex].Italic);
                        }
                    }
                    return null;
                }
            }

            return new SpellingCorrectionResult()
            {
                CharacterIndex = charIndex,
                Choices = testWords,
                OriginalWord = word
            };
        }
    }
}
