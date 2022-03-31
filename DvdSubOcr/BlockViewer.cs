using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DvdSubOcr
{
    public partial class BlockViewer : UserControl
    {
        [DllImport("user32.dll")]
        public static extern int LoadCursorFromFile(String lpFileName);
        [DllImport("user32.dll")]
        static extern IntPtr CreateIconFromResourceEx(byte[] pbIconBits, int
           cbIconBits, bool fIcon, uint dwVersion, int cxDesired, int cyDesired,
           uint uFlags);
        [DllImport("user32.dll")]
        public static extern bool DestroyCursor(int handle);

        public static readonly Cursor BrushCursor = LoadBrushCursor();

        static Cursor LoadBrushCursor()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using(Stream stream = assembly.GetManifestResourceStream("DvdSubOcr.Brush.cur"))
            {
                return new Cursor(stream);
            }
        }

        Bitmap blockView;
        BlockEncode blockEncode;
        Point blockOffset;
        CharacterSplit characterSplitter;
        HashSet<Point> encodePixels = new HashSet<Point>();
        HashSet<Point> splitPixels = new HashSet<Point>();
        string splitResult = "";
        Font splitFontNormal = new Font("Tahoma", 18.0f, FontStyle.Regular);
        Font splitFontItalic = new Font("Tahoma", 18.0f, FontStyle.Italic);
        Font splitFont;
        Font messageFont = new Font("Arial", 14.0f, FontStyle.Italic);
        Brush messageBrush = Brushes.Blue;
        int beforeSplitWidth;
        bool isHighDef;
        List<BlockEncode> otherSelectedBlocks = new List<BlockEncode>();
        List<OtherEncode> otherEncodes = new List<OtherEncode>();
        class OtherEncode
        {
            public OtherEncode(BlockEncode encode)
            {
                this.Encode = encode;
                this.Pixels = new HashSet<Point>();
            }

            public BlockEncode Encode { get; private set; }
            public HashSet<Point> Pixels { get; private set; }
        }

        StringFormat messageFormat;
        SolidBrush feedbackBrush = new SolidBrush(Color.FromArgb(128, Color.Blue));
        Font feedbackNormal = new Font("Arial Unicode MS", 64.0f, FontStyle.Regular);
        Font feedbackItalic = new Font("Arial Unicode MS", 64.0f, FontStyle.Italic);
        OcrCharacter ocrFeedback;
        DateTime ocrShown;
        const int OcrShowTime = 500;
        StringFormat feedbackFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far };

        public BlockViewer()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserMouse, true);
            UpdateStyles();

            this.splitFont = splitFontNormal;

            this.messageFormat = new StringFormat(StringFormatFlags.NoClip);
            this.messageFormat.Alignment = StringAlignment.Center;
            this.messageFormat.LineAlignment = StringAlignment.Far;

            this.blockView = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            UpdateBlockPicture(null, null, null);
        }

        public void ShowFeedback(OcrCharacter ocr)
        {
            this.ocrFeedback = ocr;
            this.ocrShown = DateTime.Now;
            if(!this.timerFeedback.Enabled)
            {
                this.timerFeedback.Start();
            }
            Invalidate();
        }

        public int MovieId { get; set; }
        public string Message { get; set; }

        public void BeginSplit(OcrMap map, bool isHighDef)
        {
            if(this.characterSplitter == null)
            {
                this.isHighDef = isHighDef;
                this.splitResult = "";
                if(this.blockEncode != null)
                {
                    this.beforeSplitWidth = this.Width;
                    if(this.blockEncode.Width * 3 > this.Width)
                    {
                        this.Width = this.blockEncode.Width * 3;
                        this.BringToFront();
                    }
                    this.characterSplitter = new CharacterSplit(this.blockEncode, map, this.MovieId);
                    RedrawBlockPicture();

                    this.Cursor = Cursors.Hand;
                    AutoTestSplits(isHighDef);
                }
                //this.Cursor = BrushCursor;
                Invalidate();
            }
        }

        public void CancelSplit()
        {
            if(this.characterSplitter != null)
            {
                this.Cursor = Cursors.Default;
                this.characterSplitter = null;
                this.splitResult = "";
                this.splitPixels.Clear();

                if(this.Width != this.beforeSplitWidth)
                {
                    this.Width = this.beforeSplitWidth;
                }
                else
                {
                    RedrawBlockPicture();
                }
                //this.Cursor = Cursors.Default;
                Invalidate();
            }
        }

        public bool SplitActive 
        {
            get { return (this.characterSplitter != null); }
        }

        public bool CommitSplit()
        {
            if(this.characterSplitter == null)
            {
                throw new InvalidOperationException("CommitSplit with no split");
            }

            this.Cursor = Cursors.Default;
            if(this.splitPixels.Count == 0)
            {
                return false;
            }

            IList<BlockEncode> encodes1, encodes2;
            this.characterSplitter.FindSplitEncodes(this.splitPixels, out encodes1, out encodes2);

            if((encodes1.Count != 1) || (encodes2.Count != 1))
            {
                MessageBox.Show("A Split must have exactly 2 contiguous parts. Don't worry if you temporarily " +
                    "have to split an i or j from its dot during this stage, a 2nd split will fix this");
                return false;
            }

            this.characterSplitter.OcrMap.AddSplit(this.blockEncode.FullEncode,
                new OcrSplit(encodes1[0].Origin, encodes1[0].FullEncode),
                new OcrSplit(encodes2[0].Origin, encodes2[0].FullEncode), 
                this.MovieId);

            this.splitResult = "";
            this.characterSplitter = null;
            this.splitPixels.Clear();

            if(this.Width != this.beforeSplitWidth)
            {
                this.Width = this.beforeSplitWidth;
            }

            return true;
        }

        public event EventHandler SplitComplete;

        protected virtual void OnSplitComplete()
        {
            EventHandler temp = SplitComplete;
            if(temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        private void AutoTestSplits(bool isHighDef)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.splitResult = "";
                CharacterSplit.SplitResult result = new CharacterSplit.SplitResult();
                if(this.characterSplitter.AutoTestSplits(result, false, isHighDef))
                {
                    this.splitFont = result.Entry1.OcrCharacter.Italic ?
                        this.splitFontItalic : this.splitFontNormal;
                    this.splitResult = new string(result.Entry1.OcrCharacter.Value, 1);
                    if(result.Entry2 != null)
                    {
                        //this.splitResult = this.splitResult.Insert(0, new string(result.Entry2.OcrCharacter.Value, 1));
                        this.splitResult += new string(result.Entry2.OcrCharacter.Value, 1);
                    }
                    this.splitPixels.Clear();
                    foreach(Point p in result.SplitPixels)
                    {
                        this.splitPixels.Add(p);
                    }
                }
                else
                {
                    Cursor.Position = PointToScreen(new Point(this.Width - 10, this.Height - 10));
                }
                RedrawBlockPicture();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public void UpdateBlockPicture(BlockEncode block,
            IEnumerable<BlockEncode> otherSelectedBlocks,
            IEnumerable<BlockEncode> allEncodes)
        {
            this.blockEncode = block;

            List<BlockEncode> othersSelected = new List<BlockEncode>();
            if(otherSelectedBlocks != null)
            {
                foreach(BlockEncode encode in otherSelectedBlocks)
                {
                    othersSelected.Add(encode);
                }
            }
            this.otherSelectedBlocks = othersSelected;

            List<OtherEncode> others = new List<OtherEncode>();
            if(allEncodes != null)
            {
                foreach(BlockEncode encode in allEncodes)
                {
                    if(!object.ReferenceEquals(encode, block))
                    {
                        others.Add(new OtherEncode(encode));
                    }
                }
            }
            this.otherEncodes = others;

            CancelSplit();
            RedrawBlockPicture();
        }

        static void DrawEncode(IList<bool> decodePoints, Point origin, int width, int height, 
            Color defaultColor, HashSet<Point> alternatePoints, Color alternateColor, 
            Bitmap bitmap, HashSet<Point> writtenPoints)
        {
            if(writtenPoints != null)
            {
                writtenPoints.Clear();
            }
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if(decodePoints[y * width + x])
                    {
                        Color color = defaultColor;
                        if((alternatePoints != null) && alternatePoints.Contains(new Point(x, y)))
                        {
                            color = alternateColor;
                        }

                        int drawX = origin.X + 3 * x;
                        int drawY = origin.Y + 3 * y;
                        if((drawX >= 0) && (drawX + 2 < bitmap.Width) &&
                            (drawY >= 0) && (drawY + 2 < bitmap.Height))
                        {
                            Point[] drawPoints = new Point[] {
                                new Point(drawX, drawY),
                                new Point(drawX + 1, drawY),
                                new Point(drawX + 2, drawY),
                                new Point(drawX, drawY + 1),
                                new Point(drawX + 1, drawY + 1),
                                new Point(drawX + 2, drawY + 1),
                                new Point(drawX, drawY + 2),
                                new Point(drawX + 1, drawY + 2),
                                new Point(drawX + 2, drawY + 2)
                            };

                            foreach(Point p in drawPoints)
                            {
                                bitmap.SetPixel(p.X, p.Y, color);
                                if(writtenPoints != null)
                                {
                                    writtenPoints.Add(p);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RedrawBlockPicture()
        {
            using(Graphics g = Graphics.FromImage(this.blockView))
            {
                g.Clear(Color.WhiteSmoke);
                Rectangle rect = this.ClientRectangle;
                rect.Width--;
                rect.Height--;
                g.DrawLine(Pens.Black, rect.Location, new Point(rect.Right, rect.Top));
                g.DrawLine(Pens.Black, new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));
                //g.DrawRectangle(Pens.Black, rect);
            }

            if(this.blockEncode != null)
            {
                IList<bool> blockDecoded = this.blockEncode.DecodeToBoolArray();
                this.blockOffset = new Point(
                    this.Width / 2 - this.blockEncode.TrueWidth * 3 / 2,
                    this.Height / 2 - this.blockEncode.Height * 3 / 2);
                DrawEncode(blockDecoded, this.blockOffset, this.blockEncode.Width,
                    this.blockEncode.Height, Color.Black, this.splitPixels, 
                    Color.LightGreen, this.blockView, this.encodePixels);
                if(this.characterSplitter == null)
                {
                    foreach(OtherEncode other in this.otherEncodes)
                    {
                        Color otherColor = Color.FromArgb(255, 180, 180, 180);
                        foreach(BlockEncode encode in this.otherSelectedBlocks)
                        {
                            if(object.ReferenceEquals(encode, other.Encode))
                            {
                                otherColor = Color.Black;
                                break;
                            }
                        }
                        IList<bool> otherDecoded = other.Encode.DecodeToBoolArray();
                        Point offset = new Point(
                            this.blockOffset.X + (other.Encode.Origin.X - this.blockEncode.Origin.X) * 3,
                            this.blockOffset.Y + (other.Encode.Origin.Y - this.blockEncode.Origin.Y) * 3);
                        DrawEncode(otherDecoded, offset, other.Encode.Width, other.Encode.Height,
                            otherColor, null, Color.LightGreen, this.blockView, other.Pixels);
                    }
                }
            }
            Invalidate();
        }

        private void PaintBucketFill(MouseEventArgs e)
        {
            if(!this.SplitActive)
            {
                return;
            }

            Point blockPoint = new Point((e.X - this.blockOffset.X) / 3, (e.Y - this.blockOffset.Y) / 3);
            IList<bool> blockDecoded = this.blockEncode.DecodeToBoolArray();
            Rectangle blockRect = new Rectangle(0, 0, this.blockEncode.Width, this.blockEncode.Height);
            if(!blockRect.Contains(blockPoint) || !blockDecoded[blockPoint.Y * blockRect.Width + blockPoint.X])
            {
                return;
            }

            HashSet<Point> futurePoints = new HashSet<Point>();
            futurePoints.Add(blockPoint);
            HashSet<Point> checkedPoints = new HashSet<Point>();

            this.splitPixels.Remove(blockPoint);
            blockPoint.X++;
            this.splitPixels.Remove(blockPoint);
            blockPoint.Y++;
            this.splitPixels.Remove(blockPoint);
            blockPoint.X--;
            this.splitPixels.Remove(blockPoint);

            while(futurePoints.Count != 0)
            {
                foreach(Point p in futurePoints)
                {
                    if(!checkedPoints.Contains(p))
                    {
                        checkedPoints.Add(p);
                        if(blockRect.Contains(p) && !this.splitPixels.Contains(p) && blockDecoded[p.Y * blockRect.Width + p.X])
                        {
                            this.splitPixels.Add(p);
                            futurePoints.Add(new Point(p.X - 1, p.Y - 1));
                            futurePoints.Add(new Point(p.X, p.Y - 1));
                            futurePoints.Add(new Point(p.X + 1, p.Y - 1));
                            futurePoints.Add(new Point(p.X - 1, p.Y));
                            futurePoints.Add(new Point(p.X + 1, p.Y));
                            futurePoints.Add(new Point(p.X - 1, p.Y + 1));
                            futurePoints.Add(new Point(p.X, p.Y + 1));
                            futurePoints.Add(new Point(p.X + 1, p.Y + 1));
                        }
                    }
                    futurePoints.Remove(p);
                    break;
                }
            }
            CheckForSplits();
        }

        private void HandleMouseEvent(MouseEventArgs e)
        {
            //base.OnMouseDown(e);
            if(!this.SplitActive)
            {
                return;
            }

            Point blockPoint = new Point((e.X - this.blockOffset.X) / 3,
                (e.Y - this.blockOffset.Y) / 3);
            Rectangle blockRect = new Rectangle(0, 0, this.blockEncode.Width, this.blockEncode.Height);

            bool pointAdded = false;
            if(blockRect.Contains(blockPoint) && !this.splitPixels.Contains(blockPoint))
            {
                this.splitPixels.Add(blockPoint);
                pointAdded = true;
            }
            blockPoint.X++;
            if(blockRect.Contains(blockPoint) && !this.splitPixels.Contains(blockPoint))
            {
                this.splitPixels.Add(blockPoint);
                pointAdded = true;
            }
            blockPoint.Y++;
            if(blockRect.Contains(blockPoint) && !this.splitPixels.Contains(blockPoint))
            {
                this.splitPixels.Add(blockPoint);
                pointAdded = true;
            }
            blockPoint.X--;
            if(blockRect.Contains(blockPoint) && !this.splitPixels.Contains(blockPoint))
            {
                this.splitPixels.Add(blockPoint);
                pointAdded = true;
            }
            if(!pointAdded)
            {
                return;
            }

            CheckForSplits();
        }

        void CheckForSplits()
        {
            this.splitResult = "";
            IList<BlockEncode> encodes1, encodes2;
            this.characterSplitter.FindSplitEncodes(this.splitPixels, out encodes1, out encodes2);
            if(encodes1.Count == 1)
            {
                foreach(OcrEntry entry in this.characterSplitter.OcrMap.FindMatches(encodes1[0].FullEncode, this.MovieId, this.isHighDef, 0))
                {
                    this.splitResult += entry.OcrCharacter.Value;
                    break;
                }
            }
            if(encodes2.Count == 1)
            {
                foreach(OcrEntry entry in this.characterSplitter.OcrMap.FindMatches(encodes2[0].FullEncode, this.MovieId, this.isHighDef, 0))
                {
                    this.splitResult += entry.OcrCharacter.Value;
                    break;
                }
            }

            RedrawBlockPicture();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            HandleMouseEvent(e);
        }

        public class BlockEncodeArgs : EventArgs
        {
            public BlockEncodeArgs(BlockEncode encode)
            {
                this.Encode = encode;
            }

            public BlockEncode Encode { get; private set; }
        }

        public event EventHandler<BlockEncodeArgs> EncodeClicked;

        void OnEncodeClicked(BlockEncode encode)
        {
            EventHandler<BlockEncodeArgs> tempHandler = EncodeClicked;
            if(tempHandler != null)
            {
                tempHandler(this, new BlockEncodeArgs(encode));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if(this.SplitActive)
                {
                    PaintBucketFill(e);
                    //HandleMouseEvent(e);
                }
                else
                {
                    Point p = new Point(e.X, e.Y);
                    foreach(OtherEncode other in this.otherEncodes)
                    {
                        if(other.Pixels.Contains(p))
                        {
                            OnEncodeClicked(other.Encode);
                            break;
                        }
                    }
                }
            }
            else if(e.Button == MouseButtons.Right)
            {
                if(this.SplitActive)
                {
                    this.splitPixels.Clear();
                    this.splitResult = "";
                    RedrawBlockPicture();
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if(this.blockView != null)
            {
                this.blockView.Dispose();
            }
            this.blockView = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            UpdateBlockPicture(this.blockEncode, this.otherSelectedBlocks, 
                this.otherEncodes.ConvertAll(val => val.Encode));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.blockView, this.ClientRectangle);
            if(!string.IsNullOrEmpty(this.splitResult))
            {
                StringFormat format = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip);
                format.Alignment = StringAlignment.Far;
                format.LineAlignment = StringAlignment.Near;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                e.Graphics.DrawString(this.splitResult, this.splitFont, Brushes.Blue, this.ClientRectangle, format);
            }

            if(!string.IsNullOrEmpty(this.Message))
            {
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                e.Graphics.DrawString(this.Message, this.messageFont, this.messageBrush, 
                    this.ClientRectangle, this.messageFormat);
            }

            if(this.ocrFeedback != null)
            {
                Font f = this.feedbackNormal;
                if(this.ocrFeedback.Italic)
                {
                    f = this.feedbackItalic;
                }
                e.Graphics.DrawString(new string(this.ocrFeedback.Value, 1),
                    f, this.feedbackBrush, Convert.ToSingle(this.Width), Convert.ToSingle(this.Height), 
                    this.feedbackFormat);
            }
        }

        private void timerFeedback_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            if((now < this.ocrShown) || ((now - this.ocrShown).TotalMilliseconds > OcrShowTime))
            {
                this.timerFeedback.Stop();
                this.ocrFeedback = null;
                Invalidate();
            }
        }
    }
}
