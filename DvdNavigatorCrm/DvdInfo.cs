/*
 * Copyright (C) 2007, 2008 Chris Meadowcroft <crmeadowcroft@gmail.com>
 *
 * This file is part of CMPlayer, a free video player.
 * See http://sourceforge.net/projects/crmplayer for updates.
 *
 * CMPlayer is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * CMPlayer is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DvdNavigatorCrm
{
	public enum AudioCodingMode
	{
		AC3 = 0,
		MPEG1 = 2,
		MPEG2 = 3,
		LPCM = 4,
		DTS = 6,
	}

	public enum AudioCodeExtension
	{
		Unspecified = 0,
		Normal = 1,
		VisuallyImpaired = 2,
		DirectorsComments = 3,
		AlternateDirectorsComments = 4,
	}

	public enum SubpictureCodeExtension
	{
		UnSpecified = 0,
		Normal = 1,
		Large = 2,
		Children = 3,
		Captions = 5,
		Large2 = 6,
		Childrens = 7,
		Forced = 9,
		Directors = 13,
		LargeDirectors = 14,
		DirectorsForChildren = 15,
	}

	public enum SubpictureFormat
	{
		FourBy3 = 0,
		Wide = 1,
		LetterBox = 2,
		PanScan = 3,
	}

	public struct PartOfTitle
	{
		public int ProgramChain;
		public int Program;

		public PartOfTitle(int pchain, int program)
		{
			this.ProgramChain = pchain;
			this.Program = program;
		}
	}

    public struct VideoAttributes
    {
        public VideoCodingMode CodingMode { get; set; }
        public VideoStandard Standard { get; set; }
        public VideoAspectRatio AspectRatio { get; set; }
        public bool ClosedCaptionLine21Field1 { get; set; }
        public bool ClosedCaptionLine21Field2 { get; set; }
        public int VerticalResolution { get; set; }
        public int HorizontalResolution { get; set; }
        public bool IsLetterBoxed { get; set; }
        public PalStandard PalStandard { get; set; }
        public Size Size { get { return new Size(HorizontalResolution, VerticalResolution); } }
    }

    public enum VideoCodingMode
    {
        Mpeg_1 = 0,
        Mpeg_2 = 1,
    }

    public enum VideoStandard
    {
        NTSC = 0,
        PAL = 1,
    }

    public enum VideoAspectRatio
    {
        _4by3 = 0,
        _16by9 = 3,
    }

    public enum PalStandard
    {
        Camera = 0,
        Film = 1,
    }

    public struct AudioAttributes
    {
		public int TrackId;
        public AudioCodingMode CodingMode;
        public string Language;
        public int Channels;
        public AudioCodeExtension CodeExtension;
    }

    public struct SubpictureAttributes
    {
		public int TrackId;
		public string Language;
        public SubpictureCodeExtension CodeExtension;
		public SubpictureFormat SubpictureFormat;
    }

	public struct TitleCell
	{
		public int ProgramChainIndex;
		public ProgramGroupChain ProgramChain;
		public int ProgramIndex;
		public CellInformation Cell;
		public int CellAngle;

		public TitleCell(int chainIndex, ProgramGroupChain chain, int programIndex, CellInformation cell)
		{
			this.ProgramChainIndex = chainIndex;
			this.ProgramChain = chain;
			this.ProgramIndex = programIndex;
			this.Cell = cell;
			this.CellAngle = 0;
		}
	}

	[Flags]
	public enum CommandLocation
	{
		None = 0,
		Button = 8,
		PrePost = 16,
		Cell = 32,
		All = Button | PrePost | Cell,
	}

	public struct TitleInfo
	{
		public bool IsSequential;
		public CommandLocation CommandLocation;
		public int NumberOfAngles;
		public int NumberOfChapters;
		public int VtsNumber;
		public int TitleWithinVts;
		public int StartSector;
	}
}
