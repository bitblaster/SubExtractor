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

namespace DvdNavigatorCrm
{
	public class DvdTitle
	{
		int angleCount;
		float playbackTime;
		List<TitleCell> cells = new List<TitleCell>();
		SortedList<int, AudioAttributes> audioByTrack = new SortedList<int, AudioAttributes>();
		SortedList<int, AudioAttributes> audioByStream = new SortedList<int, AudioAttributes>();
		SortedList<int, SubpictureAttributes> subsByTrack = new SortedList<int, SubpictureAttributes>();
		SortedList<int, SubpictureAttributes> subsByStream = new SortedList<int, SubpictureAttributes>();

		public DvdTitle(IDvdTitleSet titleSet, int titleIndex, bool removeSmallStartingCells)
		{
			IList<PartOfTitle> parts = titleSet.GetTitleParts(titleIndex);
			foreach(PartOfTitle part in parts)
			{
				ProgramGroupChain chain = titleSet.GetChain(part.ProgramChain);

				if((this.audioByTrack.Count == 0) && (chain.AudioStreams.Count != 0))
				{
					foreach(KeyValuePair<int, int> audioPair in chain.AudioStreams)
					{
						AudioAttributes audio = titleSet.GetAudioAttributes(audioPair.Key + 1);
						this.audioByTrack[audioPair.Key + 1] = audio;
						switch(audio.CodingMode)
						{
						case AudioCodingMode.MPEG1:
						case AudioCodingMode.MPEG2:
							audioByStream[0xC0 + audioPair.Value] = audio;
							break;
						case AudioCodingMode.LPCM:
							audioByStream[0xa0 + audioPair.Value] = audio;
							break;
						case AudioCodingMode.DTS:
							audioByStream[0x88 + audioPair.Value] = audio;
							break;
						case AudioCodingMode.AC3:
						default:
							audioByStream[0x80 + audioPair.Value] = audio;
							break;
						}
					}
				}

				if((this.subsByTrack.Count == 0) && (chain.SubpictureStreams.Count != 0))
				{
					bool[] validFormats = new bool[4] { false, false, false, false };
					foreach(KeyValuePair<int, int[]> subsPair in chain.SubpictureStreams)
					{
						for(int formatIndex = 0; formatIndex < 4; formatIndex++)
						{
							if(subsPair.Value[formatIndex] != 0)
							{
								validFormats[formatIndex] = true;
							}
						}
					}
					bool trackZeroIsInvalidFormat = false;
					foreach(KeyValuePair<int, int[]> subsPair in chain.SubpictureStreams)
					{
						for(int formatIndex = 0; formatIndex < 4; formatIndex++)
						{
							if((subsPair.Value[formatIndex] == 0) && validFormats[formatIndex])
							{
								trackZeroIsInvalidFormat = true;
								break;
							}
						}
						if(trackZeroIsInvalidFormat)
						{
							break;
						}
					}

					foreach(KeyValuePair<int, int[]> subsPair in chain.SubpictureStreams)
					{
						SubpictureAttributes subs = titleSet.GetSubtitleAttributes(subsPair.Key + 1);
						this.subsByTrack[subsPair.Key + 1] = subs;
						for(int formatIndex = 0; formatIndex < 4; formatIndex++)
						{
							int subId = subsPair.Value[formatIndex];
							if(subId == 0)
							{
								if(!validFormats[formatIndex] && trackZeroIsInvalidFormat)
								{
									continue;
								}
							}
							if(!this.subsByStream.ContainsKey(0x20 + subId))
							{
								subs.SubpictureFormat = (SubpictureFormat)formatIndex;
								this.subsByStream[0x20 + subId] = subs;
							}
						}
					}
				}

				int startCell = chain.GetProgramStartCell(part.Program);
				int endCell;
				if(chain.ProgramCount > part.Program)
				{
					endCell = chain.GetProgramStartCell(part.Program + 1);
				}
				else
				{
					endCell = chain.CellCount + 1;
				}
				int programAngleCount = 0;
				bool foundGoodFirstCell = false;
				for(int cellIndex = startCell; cellIndex < endCell; cellIndex++)
				{
					CellInformation cell = chain.GetCell(cellIndex);
					TitleCell tcell = new TitleCell(part.ProgramChain, chain, part.Program, cell);
					switch(cell.CellType)
					{
					case CellType.FirstAngleBlock:
						this.playbackTime += cell.PlaybackTime;
						programAngleCount = 1;
						tcell.CellAngle = 1;
						break;
					case CellType.MiddleAngleBlock:
					case CellType.LastAngleBlock:
						programAngleCount++;
						tcell.CellAngle = programAngleCount;
						break;
					case CellType.Normal:
						if(removeSmallStartingCells && !foundGoodFirstCell && (cell.PlaybackTime < 2.0f))
						{
							// silly little cells at the start of titles should be ignored
							continue;
						}
						this.playbackTime += cell.PlaybackTime;
						break;
					}
					this.cells.Add(tcell);
					foundGoodFirstCell = true;
				}
				this.angleCount = Math.Max(this.angleCount, programAngleCount);
			}

            while(removeSmallStartingCells && (this.cells.Count != 0))
            {
                // There are pointless 1/2 second cells at the end of a lot of titles.  Ditch them here
                TitleCell tcell = this.cells[this.cells.Count - 1];
                if(tcell.Cell.PlaybackTime < 1.0f)
                {
                    this.playbackTime -= tcell.Cell.PlaybackTime;
                    this.cells.RemoveAt(this.cells.Count - 1);
                }
                else
                {
                    break;
                }
            }
		}

        public void TrimCells(int startIndex, int count)
        {
            if(startIndex != 0)
            {
                this.cells.RemoveRange(0, startIndex);
            }
            if(count < this.cells.Count)
            {
                this.cells.RemoveRange(count, this.cells.Count - count);
            }

            this.playbackTime = 0.0f;
            foreach(TitleCell cell in this.cells)
            {
                switch(cell.Cell.CellType)
                {
                case CellType.FirstAngleBlock:
                case CellType.Normal:
                    this.playbackTime += cell.Cell.PlaybackTime;
                    break;
                }
            }
        }

		public int AngleCount { get { return this.angleCount; } }
		public float PlaybackTime { get { return this.playbackTime; } }
		public IList<TitleCell> TitleCells { get { return this.cells.AsReadOnly(); } }

		public IList<int> AudioTracks { get { return this.audioByTrack.Keys; } }
		public IList<int> AudioStreams { get { return this.audioByStream.Keys; } }
		public AudioAttributes GetAudioTrack(int trackId) { return this.audioByTrack[trackId]; }
		public AudioAttributes GetAudioStream(int streamId) { return this.audioByStream[streamId]; }
		public IList<int> SubtitleTracks { get { return this.subsByTrack.Keys; } }
		public IList<int> SubtitleStreams { get { return this.subsByStream.Keys; } }
		public SubpictureAttributes GetSubtitleTrack(int trackId) { return this.subsByTrack[trackId]; }
		public SubpictureAttributes GetSubtitleStream(int streamId) { return this.subsByStream[streamId]; }

		public IList<int> CellSectorList
		{
			get
			{
				List<int> cellList = new List<int>(this.cells.Count * 2);
				foreach(TitleCell cell in this.cells)
				{
					cellList.Add(cell.Cell.FirstVobuStartSector);
					cellList.Add(cell.Cell.LastVobuStartSector);
				}
				return cellList;
			}
		}
	}
}
