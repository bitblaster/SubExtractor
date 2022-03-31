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
using System.IO;
using System.Text;

namespace DvdNavigatorCrm
{
	public class DvdTitleSet : IDvdTitleSet, IDisposable
	{
		public const string VTS_ID = "DVDVIDEO-VTS";

		bool disposed;
		IfoReader ifoReader;
		int videoTsFileOffset = -1;
		int vtsStartSector = -1;
        int lastSectorOfTitleSet;
        int lastSectorOfIfo;
        int startSectorMenuVob;
        int startSectorTitleVob;
		int titleCount;
		List<List<PartOfTitle>> titleParts;
		List<DvdTitle> titles;
		ProgramGroupChain[] chains;
        int audioTrackCount;
        AudioAttributes[] audioAttributes = new AudioAttributes[8];
        int subTrackCount;
        SubpictureAttributes[] subAttributes = new SubpictureAttributes[32];
		int menuAudioTrackCount;
		AudioAttributes[] menuAudioAttributes = new AudioAttributes[8];
		int menuSubTrackCount;
		SubpictureAttributes menuSubAttribute;
		int languageCount;
		int firstLanguageCode;
		int menuExistanceFlag;
		ProgramGroupChain[] menuChains;

		public DvdTitleSet(string fileName)
		{
			this.ifoReader = new IfoReader(fileName);
		}

		public bool MenuExists { get { return this.menuExistanceFlag != 0; } }
		public int TitleCount { get { return this.titleCount; } }
		public IList<PartOfTitle> GetTitleParts(int title) { return this.titleParts[title - 1].AsReadOnly(); }
		public IList<DvdTitle> Titles { get { return this.titles.AsReadOnly(); } }
		public int ChainCount { get { return this.chains.Length; } }
		public ProgramGroupChain GetChain(int chain) { return this.chains[chain - 1]; }
		public int MenuChainCount { get { return this.menuChains.Length; } }
		public ProgramGroupChain GetMenuChain(int chain) { return this.menuChains[chain - 1]; }
        public VideoAttributes VideoAttributes { get; private set; }
        public int AudioTrackCount { get { return this.audioTrackCount; } }
		public AudioAttributes GetAudioAttributes(int index) { return this.audioAttributes[index - 1]; }
		public int SubtitleTrackCount { get { return this.subTrackCount; } }
		public SubpictureAttributes GetSubtitleAttributes(int index) { return this.subAttributes[index - 1]; }
		public int MenuAudioTrackCount { get { return this.menuAudioTrackCount; } }
		public AudioAttributes GetMenuAudioAttributes(int index) { return this.menuAudioAttributes[index - 1]; }
		public int MenuSubtitleTrackCount { get { return this.menuSubTrackCount; } }
		public SubpictureAttributes MenuSubtitleAttributes { get { return menuSubAttribute; } }

		public int LastSectorOfTitleSet { get { return this.lastSectorOfTitleSet; } }
		public int LastSectorOfIfo { get { return this.lastSectorOfIfo; } }
		public int StartSectorMenuVob { get { return this.startSectorMenuVob; } }
		public int StartSectorTitleVob { get { return this.startSectorTitleVob; } }
		public int VtsStartSector { get { return this.vtsStartSector; } }
		public int VideoTsFileOffset
		{
			get { return this.videoTsFileOffset; }
			set { this.videoTsFileOffset = value; }
		}

		public bool IsValidTitleSet
		{
			get
			{
				this.ifoReader.SeekFromStart(0);
				byte[] tempBuffer = new byte[VTS_ID.Length];
				this.ifoReader.Read(tempBuffer, VTS_ID.Length);
				if(Encoding.ASCII.GetString(tempBuffer, 0, VTS_ID.Length) != VTS_ID)
				{
					return false;
				}
				return true;
			}
		}

		public string FileName
		{
			get
			{
				return this.ifoReader.FileName;
			}
		}

		public void Parse()
		{
			this.FindStartSector();
			this.ParsePTT();
			this.ParsePGCI();
			this.ParseMenuChainTable();
			this.ParseAudioAndSubs();
            this.ParseVideoAttributes();
			this.BuildTitles();
		}

		void FindStartSector()
		{
			string videoTsIfoPath = Path.Combine(Path.GetDirectoryName(FileName), "VIDEO_TS.IFO");
			if(File.Exists(videoTsIfoPath))
			{
				int currentVts = int.Parse(Path.GetFileNameWithoutExtension(FileName).Substring(4, 2));
				IfoReader vtsReader = new IfoReader(videoTsIfoPath);
				int ttSrptOffset = (int)vtsReader.ReadUInt32(0xc4) * 0x800;
				vtsReader.SeekFromStart(ttSrptOffset);
				int numberOfEntries = vtsReader.ReadUInt16();
				vtsReader.SeekFromCurrent(2);
				for(int index = 0; index < numberOfEntries; index++)
				{
					vtsReader.SeekFromCurrent(10);
					int vtsNumber = vtsReader.ReadByte();
					vtsReader.ReadByte();
					if(currentVts == vtsNumber)
					{
						this.vtsStartSector = (int)vtsReader.ReadUInt32();
						break;
					}
				}
			}
		}

		void ParsePTT()
		{
            this.lastSectorOfTitleSet = (int)this.ifoReader.ReadUInt32(0x0c);
            this.lastSectorOfIfo = (int)this.ifoReader.ReadUInt32(0x1c);
            this.startSectorMenuVob = (int)this.ifoReader.ReadUInt32(0xc0);
            this.startSectorTitleVob = (int)this.ifoReader.ReadUInt32(0xc4);
			uint pttSrptSector = this.ifoReader.ReadUInt32(0xc8);
			int pttSrptOffset = (int)pttSrptSector * 0x800;

			this.titleCount = this.ifoReader.ReadUInt16(pttSrptOffset);
			this.ifoReader.SeekFromCurrent(2);

			this.titleParts = new List<List<PartOfTitle>>(titleCount);

			int[] pttOffsets = new int[this.titleCount + 1];
			pttOffsets[this.titleCount] = (int)this.ifoReader.ReadUInt32() + 1;
			for(int index = 0; index < this.titleCount; index++)
			{
				pttOffsets[index] = (int)this.ifoReader.ReadUInt32();
			}

			int nextPtt = pttSrptOffset + pttOffsets[0];
			this.ifoReader.SeekFromStart(nextPtt);
			for(int pttIndex = 0; pttIndex < this.titleCount; pttIndex++)
			{
				long endOfPtt = pttSrptOffset + pttOffsets[pttIndex + 1];
				List<PartOfTitle> ptt = new List<PartOfTitle>();
				while(nextPtt < endOfPtt)
				{
					ptt.Add(new PartOfTitle(this.ifoReader.ReadUInt16(), this.ifoReader.ReadUInt16()));
					nextPtt += 4;
				}
				titleParts.Add(ptt);
			}
		}

        void ParseVideoAttributes()
        {
            int videoAttrib1 = this.ifoReader.ReadByte(0x200);
            int videoAttrib2 = this.ifoReader.ReadByte(0x201);

            VideoAttributes attribs = new VideoAttributes();
            attribs.CodingMode = ((videoAttrib1 & 0xc0) == 0) ? VideoCodingMode.Mpeg_1 :
                VideoCodingMode.Mpeg_2;
            attribs.Standard = ((videoAttrib1 & 0x30) == 0) ? VideoStandard.NTSC :
                VideoStandard.PAL;
            attribs.AspectRatio = ((videoAttrib1 & 0x0c) == 0) ? VideoAspectRatio._4by3 :
                VideoAspectRatio._16by9;
            attribs.ClosedCaptionLine21Field1 = ((videoAttrib2 & 0x80) != 0);
            attribs.ClosedCaptionLine21Field2 = ((videoAttrib2 & 0x40) != 0);
            switch((videoAttrib2 & 0x38) >> 3)
            {
            case 0:
                attribs.HorizontalResolution = 720;
                attribs.VerticalResolution = (attribs.Standard == VideoStandard.NTSC) ?
                    480 : 576;
                break;
            case 1:
                attribs.HorizontalResolution = 704;
                attribs.VerticalResolution = (attribs.Standard == VideoStandard.NTSC) ?
                    480 : 576;
                break;
            case 2:
                attribs.HorizontalResolution = 352;
                attribs.VerticalResolution = (attribs.Standard == VideoStandard.NTSC) ?
                    480 : 576;
                break;
            case 3:
                attribs.HorizontalResolution = 352;
                attribs.VerticalResolution = (attribs.Standard == VideoStandard.NTSC) ?
                    240 : 288;
                break;
            }
            attribs.IsLetterBoxed = ((videoAttrib2 & 0x04) != 0);
            attribs.PalStandard = ((videoAttrib2 & 0x01) == 0) ? PalStandard.Camera :
                PalStandard.Film;
            this.VideoAttributes = attribs;
        }

		void ParsePGCI()
		{
			uint pgciSector = this.ifoReader.ReadUInt32(0xcc);
			int pgciOffset = (int)pgciSector * 0x800;
			if(((int)pgciSector < 0) || (pgciOffset > this.ifoReader.Length))
			{
				this.titleCount = 0;
				this.chains = new ProgramGroupChain[0];
				return;
			}

			int chainCount = this.ifoReader.ReadUInt16(pgciOffset);
			this.chains = new ProgramGroupChain[chainCount];
			this.ifoReader.SeekFromCurrent(2);
			long endPgci = pgciOffset + this.ifoReader.ReadUInt32() + 1L;

			uint[] chainOffsets = new uint[chainCount];
			for(int index = 0; index < chainCount; index++)
			{
				uint category = this.ifoReader.ReadUInt32();
				chainOffsets[index] = this.ifoReader.ReadUInt32();
				this.chains[index] = new ProgramGroupChain((category & 0x80000000) != 0,
					Convert.ToInt32((category & 0x7f000000) >> 24));
			}
			for(int index = 0; index < chainCount; index++)
			{
				this.chains[index].Parse(this.ifoReader, pgciOffset + (int)chainOffsets[index]);
				// check for phony title set which goes beyond the end of the vob
				for(int cellIndex = 1; cellIndex <= this.chains[index].CellCount; cellIndex++)
				{
					CellInformation cellInfo = this.chains[index].GetCell(cellIndex);
					if(this.startSectorTitleVob + cellInfo.LastVobuEndSector > this.lastSectorOfTitleSet)
					{
						this.titleCount = 0;
						this.chains = new ProgramGroupChain[0];
						return;
					}
				}
			}
		}

		void ParseMenuChainTable()
		{
			uint pgciUtSector = this.ifoReader.ReadUInt32(0xd0);
			int pgciUtOffset = (int)pgciUtSector * 0x800;
			if(((int)pgciUtSector <= 0) || (pgciUtOffset > this.ifoReader.Length))
			{
				this.menuChains = new ProgramGroupChain[0];
				return;
			}

			this.ifoReader.SeekFromStart(pgciUtOffset);
			this.languageCount = this.ifoReader.ReadUInt16();
			this.ifoReader.SeekFromCurrent(6);
			this.firstLanguageCode = this.ifoReader.ReadUInt16();
			this.ifoReader.SeekFromCurrent(1);
			this.menuExistanceFlag = this.ifoReader.ReadByte();

			int vmgmLuOffset = pgciUtOffset + (int)this.ifoReader.ReadUInt32();

			int chainCount = this.ifoReader.ReadUInt16(vmgmLuOffset);
			this.menuChains = new ProgramGroupChain[chainCount];
			this.ifoReader.SeekFromCurrent(2);
			long endvmgmLu = vmgmLuOffset + this.ifoReader.ReadUInt32() + 1L;

			uint[] chainOffsets = new uint[chainCount];
			for(int index = 0; index < chainCount; index++)
			{
				uint category = this.ifoReader.ReadUInt32();
				chainOffsets[index] = this.ifoReader.ReadUInt32();
				this.menuChains[index] = new ProgramGroupChain((category & 0x80000000) != 0,
					Convert.ToInt32((category & 0x7f000000) >> 24));
			}

			for(int index = 0; index < chainCount; index++)
			{
				this.menuChains[index].Parse(this.ifoReader, vmgmLuOffset + (int)chainOffsets[index]);
				// check for phony title set which goes beyond the end of the vob
				for(int cellIndex = 1; cellIndex <= this.menuChains[index].CellCount; cellIndex++)
				{
					CellInformation cellInfo = this.menuChains[index].GetCell(cellIndex);
					if(this.startSectorMenuVob + cellInfo.LastVobuEndSector > this.lastSectorOfTitleSet)
					{
						this.menuChains = new ProgramGroupChain[0];
						return;
					}
				}
			}
		}

		AudioAttributes ParseAudioAtCurrent(int audioIndex)
		{
			AudioAttributes audio = new AudioAttributes();
			audio.TrackId = audioIndex + 1;
			int codingByte = this.ifoReader.ReadByte();
			audio.CodingMode = (AudioCodingMode)(codingByte >> 5);
			int samplingByte = this.ifoReader.ReadByte();
			audio.Channels = (samplingByte & 0x07) + 1;

			byte[] lang = new byte[2];
			this.ifoReader.Read(lang, 2);
            if(((codingByte & 0x0c) != 0) && (lang[0] != 0) && (lang[1] != 0))
            {
				audio.Language = Encoding.ASCII.GetString(lang);
			}
			else
			{
				audio.Language = string.Empty;
			}

			this.ifoReader.SeekFromCurrent(1);
			audio.CodeExtension = (AudioCodeExtension)(this.ifoReader.ReadByte());
			this.ifoReader.SeekFromCurrent(2);
			return audio;
		}

		SubpictureAttributes ParseSubsAtCurrent(int subIndex)
		{
			SubpictureAttributes subs = new SubpictureAttributes();
			subs.TrackId = subIndex + 1;
			int codingByte = this.ifoReader.ReadByte();
			this.ifoReader.SeekFromCurrent(1);

			byte[] lang = new byte[2];
			this.ifoReader.Read(lang, 2);
			if((codingByte & 0x03) != 0)
			{
				subs.Language = Encoding.ASCII.GetString(lang);
			}
			else
			{
				subs.Language = string.Empty;
			}

			this.ifoReader.SeekFromCurrent(1);
			subs.CodeExtension = (SubpictureCodeExtension)(this.ifoReader.ReadByte());
			return subs;
		}

        void ParseAudioAndSubs()
        {
            this.ifoReader.SeekFromStart(0x202);
            this.audioTrackCount = this.ifoReader.ReadUInt16();
			for(int audioIndex = 0; audioIndex < this.audioTrackCount; audioIndex++)
            {
				this.audioAttributes[audioIndex] = ParseAudioAtCurrent(audioIndex);
			}

            this.ifoReader.SeekFromStart(0x254);
            this.subTrackCount = this.ifoReader.ReadUInt16();
			for(int subIndex = 0; subIndex < this.subTrackCount; subIndex++)
            {
				this.subAttributes[subIndex] = ParseSubsAtCurrent(subIndex);
            }

			this.ifoReader.SeekFromStart(0x102);
			this.menuAudioTrackCount = this.ifoReader.ReadUInt16();
			for(int audioIndex = 0; audioIndex < this.menuAudioTrackCount; audioIndex++)
			{
				this.menuAudioAttributes[audioIndex] = ParseAudioAtCurrent(audioIndex);
			}

			this.ifoReader.SeekFromStart(0x154);
			this.menuSubTrackCount = this.ifoReader.ReadUInt16();
			if(menuSubTrackCount == 1)
			{
				this.menuSubAttribute = ParseSubsAtCurrent(0);
			}
		}

		void BuildTitles()
		{
			this.titles = new List<DvdTitle>(this.titleCount);
			List<IList<int>> allCells = new List<IList<int>>();
			for(int titleIndex = 1; titleIndex <= this.titleCount; titleIndex++)
			{
				DvdTitle newTitle = new DvdTitle(this, titleIndex, true);
				allCells.Add(newTitle.CellSectorList);
				this.titles.Add(newTitle);
			}

			List<int> weakTitles = new List<int>();
			// dispose of any titles which are just empty, matches, or subsets of another title
			for(int titleIndex = 0; titleIndex < allCells.Count; titleIndex++)
			{
				IList<int> titleCells = allCells[titleIndex];
				if(titleCells.Count == 0)
				{
					weakTitles.Add(titleIndex);
					continue;
				}

				for(int testIndex = 0; testIndex < this.titles.Count; testIndex++)
				{
					if(testIndex == titleIndex)
					{
						continue;
					}

					IList<int> testCells = allCells[testIndex];
					if((testCells.Count == titleCells.Count) && (testIndex < titleIndex))
					{
						bool match = true;
						for(int cellIndex = 0; cellIndex < testCells.Count; cellIndex++)
						{
							if(testCells[cellIndex] != titleCells[cellIndex])
							{
								match = false;
								break;
							}
						}
						if(match)
						{
							weakTitles.Add(titleIndex);
							break;
						}
					}
					else if(testCells.Count > titleCells.Count)
					{
						bool subset = false;
						for(int cellIndex = 0; cellIndex < testCells.Count - titleCells.Count + 1; cellIndex++)
						{
							if(testCells[cellIndex] == titleCells[0])
							{
								subset = true;
								for(int innerCellIndex = 1; innerCellIndex < titleCells.Count; innerCellIndex++)
								{
									if(testCells[cellIndex + innerCellIndex] != titleCells[innerCellIndex])
									{
										subset = false;
										break;
									}
								}
								break;
							}
						}
						if(subset)
						{
							//weakTitles.Add(titleIndex);
							break;
						}
					}
				}
			}
			for(int weakIndex = 0; weakIndex < weakTitles.Count; weakIndex++)
			{
				this.titleCount--;
				this.titles.RemoveAt(weakTitles[weakIndex] - weakIndex);
				this.titleParts.RemoveAt(weakTitles[weakIndex] - weakIndex);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\n", this.ifoReader.FileName);
            sb.AppendFormat("VTS Start Sector 0x{0:x}\n", this.vtsStartSector);
            sb.AppendFormat("Last Sector Title Set 0x{0:x}\n", this.lastSectorOfTitleSet);
            sb.AppendFormat("Last Sector IFO 0x{0:x}\n", this.lastSectorOfIfo);
            sb.AppendFormat("Start Sector Menu VOB 0x{0:x}\n", this.startSectorMenuVob);
            sb.AppendFormat("Start Sector Title VOB 0x{0:x}\n", this.startSectorTitleVob);
			sb.AppendFormat("Language Count {0}\n", this.languageCount);
			sb.AppendFormat("First Language Code {0}\n", this.firstLanguageCode);
			sb.AppendFormat("Menu Existance Flag 0x{0:x}\n", this.menuExistanceFlag);

			sb.AppendFormat("{0} Menu Chains\n\n", this.menuChains.Length);
			for(int index = 0; index < this.menuChains.Length; index++)
			{
				sb.AppendFormat("Menu Chain {0}\n\n", index + 1);
				sb.Append(this.menuChains[index].ToString());
				sb.Append("\n");
			}

			if(this.menuAudioTrackCount != 0)
			{
				sb.Append("Menu Audio Tracks ");
				for(int index = 0; index < this.menuAudioTrackCount; index++)
				{
					AudioAttributes audio = this.menuAudioAttributes[index];
					if(audio.CodeExtension == AudioCodeExtension.Unspecified)
					{
						sb.AppendFormat(" {0} ({1} {2} {3}ch)", index + 1,
							audio.CodingMode, DvdLanguageCodes.GetLanguageText(audio.Language),
							audio.Channels);
					}
					else
					{
						sb.AppendFormat(" {0} ({1} {2} {3}ch {4})", index + 1,
							audio.CodingMode, DvdLanguageCodes.GetLanguageText(audio.Language),
							audio.Channels, audio.CodeExtension);
					}
				}
				sb.Append("\n\n");
			}

			if(this.menuSubTrackCount != 0)
			{
				sb.Append("Menu Subtitle Track ");
				SubpictureAttributes subs = this.menuSubAttribute;
				if(subs.CodeExtension == SubpictureCodeExtension.UnSpecified)
				{
					sb.AppendFormat(" {0} ({1})", 1,
						DvdLanguageCodes.GetLanguageText(subs.Language));
				}
				else
				{
					sb.AppendFormat(" {0} ({1} {2})", 1,
						DvdLanguageCodes.GetLanguageText(subs.Language), subs.CodeExtension);
				}
				sb.Append("\n\n");
			}

			sb.AppendFormat("{0} Titles\n\n", this.titleCount);
			for(int titleIndex = 0; titleIndex < this.titleCount; titleIndex++)
			{
				List<PartOfTitle> ptt = this.titleParts[titleIndex];
				sb.AppendFormat("Title {0}\n\n", titleIndex + 1);
				foreach(PartOfTitle pot in ptt)
				{
					sb.AppendFormat("Chain {0} Program {1}\n", pot.ProgramChain, pot.Program);
				}
				sb.Append("\n");
			}

			sb.AppendFormat("{0} Chains\n\n", this.chains.Length);
			for(int index = 0; index < this.chains.Length; index++)
			{
				sb.AppendFormat("Chain {0}\n\n", index + 1);
				sb.Append(this.chains[index].ToString());
				sb.Append("\n");
			}

            if(this.audioTrackCount != 0)
            {
                sb.Append("\nAudio Tracks ");
                for(int index = 0; index < this.audioTrackCount; index++)
                {
                    AudioAttributes audio = this.audioAttributes[index];
					if(audio.CodeExtension == AudioCodeExtension.Unspecified)
					{
						sb.AppendFormat(" {0} ({1} {2} {3}ch)", index + 1,
							audio.CodingMode, DvdLanguageCodes.GetLanguageText(audio.Language),
							audio.Channels);
					}
					else
					{
						sb.AppendFormat(" {0} ({1} {2} {3}ch {4})", index + 1,
							audio.CodingMode, DvdLanguageCodes.GetLanguageText(audio.Language), 
							audio.Channels, audio.CodeExtension);
					}
                }
                sb.Append("\n");
            }

            if(this.subTrackCount != 0)
            {
                sb.Append("\nSubtitle Tracks ");
                for(int index = 0; index < this.subTrackCount; index++)
                {
                    SubpictureAttributes subs = this.subAttributes[index];
					if(subs.CodeExtension == SubpictureCodeExtension.UnSpecified)
					{
						sb.AppendFormat(" {0} ({1})", index + 1, 
							DvdLanguageCodes.GetLanguageText(subs.Language));
					}
					else
					{
						sb.AppendFormat(" {0} ({1} {2})", index + 1, 
							DvdLanguageCodes.GetLanguageText(subs.Language), subs.CodeExtension);
					}
                }
                sb.Append("\n");
            }

			return sb.ToString();
		}

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DvdTitleSet()
        {
            Dispose(false);
        }
	}
}
