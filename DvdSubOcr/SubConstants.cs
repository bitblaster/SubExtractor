using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class SubConstants
    {
        public const int SectionMarginX = 16;
        public const int SectionMarginY = 3;

        public const int MinimumCountablePixelCount = 8;
        public const int MinimumInterestingMatches = 3;
        public const int NormalPixelCount = 65;
        public const int MinimumNormalPixelCount = 40;
        public const int MaximumNormalPixelCount = 100;
        public const int MinimumLowPixelCount = 30;

        public const int MaximumOcrPieces = 10;

        public const int NormalFontHeight = 18;

        public const int MaximumMillisecondsOnScreen = 9999;
        public const int PtsSlushMilliseconds = 50;

        public const float CenteredSubtitleMinVertical = 0.30f;
        public const float UnpositionedSubtitleMaxVertical = 0.75f;
        public const float LeftCenterMinimum = 0.45f;
        public const float RightCenterMaximum = 0.55f;

        public const float WindowsToDvdFontHeightConversion = 0.75f;
        public const float WindowsToDvd4x3FontHeightConversion = 0.75f;

        public const Char ChangeOfSpeakerPrefix = '-';

        public const int FontScaleX16x9Dvd = 100;
        public const int FontScaleX4x3Dvd = 100;
        public const int FontScaleNormal = 100;

        public const int MaxSizeCheckAngledAutoSplit = 1000;

        public const int DefaultDvdHorizontalPixels = 720;
        public const int DefaultDvdVerticalPixels = 480;
        public const int DvdLineEdgeSlush = 6;

        public static readonly char[] MovieSpecificChars = new char[] 
            { '1', 'l', 'I', 'Ι', '.', '\'', ',', '-', '—', '_', '\\', '/', '|', '+', 
              'o', 'O', '°', 'ฺ', '่', '๋', 'ํ', 'Ο', 'ο', '"', OcrCharacter.UnmatchedValue };
        public static readonly char[] MovieSpecificSpanishChars = new char[] { 'i', '¡', 'ί' };
        public const string CheapSplitSymbols = "\'\".,:--—_|„!ΤO";
        public const string VeryCheapSplitCharacters = "lIÌÍÎÏiìíîïj][ΙΞІЇӀιΙΓГҐгτ";
        public const string GoodSplitSymbols = "@#$%&{}ัิีึื";
        public const string OhCharacters = "oOΟο";
        public const string DashCharacters = "-—_";

        public const string TrustedForHeightCharacters =
            "ÀÁÂÃÄÅĆÈÉÊËÌÍÎÏÑŃÒÓÔÕÖŚÙÚÛÜÝŹŻàáâãäåćèéêëìíîïñńòóôõöśùúûüýźż" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789?@#$%" +
            "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบผพภมยรลวศษสหฬอฮฯเแ" +
            "АБВГЕЖЗИКЛМНОПРСТФХЧШЪЫЬЭЮЯабвгежзийклмнопрстухцчшщъыьэюя";
        public const string DiacriticCharacters = "ัิีึืฺุู็่้๊๋์ํ๎";
        public const string UntrustedForBaselineCharacters = "\'\".,-—_=:;°„าโใไๅๆฦฤฟปฝУдф" + DiacriticCharacters;
        public const string DiacriticLowCharacters = "ฺุู";
        public const string DiacriticMedCharacters = "ัิีึืํ";
        public const string DiacriticHighCharacters = "็่้๊๋์๎";

        public const string MistakenForItalicCharacters = ".-—_,\'\"„";
        public const string MistakenForItalicRepeatingCharacters = ".-—_";
        public const string UntrustworthyAsItalics = ",.;:\'\"„*~-—_=+|♪♥€£¥¢@#$%^&/\\<>";

        public const string CharactersThatSignalLineBreak = "-—,.(<{♪♥¡¿*~_=+|";
        public const string CharactersToRemoveBeforeSDH = "-—_ ";
        public const string CharactersNeverAfterASpace = ".,;:?!";
        public const string CharactersNeverAfterASpaceFrench = ".,";
        public const string CharactersThatEndQuotes = ".?!";

        public const string UsuallyInTwoPiecesCharacters = "ij;:\"„?¿!¡ÀÁÂÃÄĆÈÉÊËÌÍÎÏÑŃÒÓÔÕÖŚÙÚÛÜÝŹŻàáâãäåćèéêëìíîïñńòóôõöśùúûüýźżЫыЙйί";

        public const string UselessForFontMatchingCharacters = ",.\'\"„*~-—_|^/\\♥♪" + DiacriticCharacters;
        public const string LikelyValidForOcrSymbols = @"?¿Øø[]{}()ßŒœÆæ♪♥€£¥¢@#$%&";
        public const string CheapMatchesDuringOcrCharacters = @"Iloi";

        public static readonly Font LargeFormFont = new Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        public static readonly Font LargeHelpFont = new Font("Arial", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        public static readonly Font LargeMainToolStripFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    }
}
