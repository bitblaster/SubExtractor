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
	public class VideoManagerTitleSet : IDisposable
	{
		public const string VMG_ID = "DVDVIDEO-VMG";

		bool disposed;
		IfoReader ifoReader;
		int videoTsFileOffset = -1;
        int lastSectorOfTitleSet;
        int lastSectorOfIfo;
		int volumeCount;
		int volumeNumber;
		int sideId;
		int titleSetCount;
        int startSectorMenuVob;
		int firstPlayPGCAddress;
		int languageCount;
		int firstLanguageCode;
		int menuExistanceFlag;
		List<TitleInfo> titleInfos = new List<TitleInfo>();
		ProgramGroupChain firstChain;
		ProgramGroupChain[] chains;
        int audioTrackCount;
        AudioAttributes[] audioAttributes = new AudioAttributes[8];
        int subTrackCount;
        SubpictureAttributes[] subAttributes = new SubpictureAttributes[32];

		public VideoManagerTitleSet(string fileName)
		{
			this.ifoReader = new IfoReader(fileName);
		}

		public List<TitleInfo> TitleInfos { get { return titleInfos; } }
		public int ChainCount { get { return this.chains.Length; } }
		public ProgramGroupChain GetChain(int chain) { return this.chains[chain - 1]; }
		public int AudioTrackCount { get { return this.audioTrackCount; } }
		public AudioAttributes GetAudioAttributes(int index) { return this.audioAttributes[index - 1]; }
		public int SubtitleTrackCount { get { return this.subTrackCount; } }
		public SubpictureAttributes GetSubtitleAttributes(int index) { return this.subAttributes[index - 1]; }
		public int LastSectorOfTitleSet { get { return this.lastSectorOfTitleSet; } }
		public int LastSectorOfIfo { get { return this.lastSectorOfIfo; } }
		public int StartSectorMenuVob { get { return this.startSectorMenuVob; } }
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
				byte[] tempBuffer = new byte[VMG_ID.Length];
				this.ifoReader.Read(tempBuffer, VMG_ID.Length);
				if(Encoding.ASCII.GetString(tempBuffer, 0, VMG_ID.Length) != VMG_ID)
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
			this.ParseHeader();
			this.ParsePGCI();
			this.ParseAudioAndSubs();
		}

		void ParseHeader()
		{
			this.lastSectorOfTitleSet = (int)this.ifoReader.ReadUInt32(0x0c);
			this.lastSectorOfIfo = (int)this.ifoReader.ReadUInt32(0x1c);

			this.volumeCount = (int)this.ifoReader.ReadUInt16(0x26);
			this.volumeNumber = (int)this.ifoReader.ReadUInt16(0x28);
			this.sideId = (int)this.ifoReader.ReadByte(0x2a);
			this.titleSetCount = (int)this.ifoReader.ReadUInt16(0x3e);
			this.firstPlayPGCAddress = (int)this.ifoReader.ReadUInt32(0x84);
			this.startSectorMenuVob = (int)this.ifoReader.ReadUInt32(0xc0);

			int ttSrptOffset = (int)this.ifoReader.ReadUInt32(0xc4) * 0x800;
			this.ifoReader.SeekFromStart(ttSrptOffset);
			int numberOfEntries = this.ifoReader.ReadUInt16();
			this.ifoReader.SeekFromCurrent(6);
			for(int index = 0; index < numberOfEntries; index++)
			{
				TitleInfo info = new TitleInfo();
				int titleType = this.ifoReader.ReadByte();
				info.IsSequential = (titleType & 0x40) != 0;
				info.CommandLocation = (CommandLocation)titleType & CommandLocation.All;
				info.NumberOfAngles = this.ifoReader.ReadByte();
				info.NumberOfChapters = this.ifoReader.ReadUInt16();
				this.ifoReader.SeekFromCurrent(2);
				info.VtsNumber = this.ifoReader.ReadByte();
				info.TitleWithinVts = this.ifoReader.ReadByte();
				info.StartSector = (int)this.ifoReader.ReadUInt32();
				this.titleInfos.Add(info);
			}
		}

		void ParsePGCI()
		{
			uint pgciUtSector = this.ifoReader.ReadUInt32(0xc8);
			int pgciUtOffset = (int)pgciUtSector * 0x800;
			if(((int)pgciUtSector <= 0) || (pgciUtOffset > this.ifoReader.Length))
			{
				this.chains = new ProgramGroupChain[0];
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
			this.chains = new ProgramGroupChain[chainCount];
			this.ifoReader.SeekFromCurrent(2);
			long endvmgmLu = vmgmLuOffset + this.ifoReader.ReadUInt32() + 1L;

			uint[] chainOffsets = new uint[chainCount];
			for(int index = 0; index < chainCount; index++)
			{
				uint category = this.ifoReader.ReadUInt32();
				chainOffsets[index] = this.ifoReader.ReadUInt32();
				this.chains[index] = new ProgramGroupChain((category & 0x80000000) != 0,
					Convert.ToInt32((category & 0x7f000000) >> 24));
			}

			this.firstChain = new ProgramGroupChain(true, 0);
			this.firstChain.Parse(this.ifoReader, this.firstPlayPGCAddress);
			for(int index = 0; index < chainCount; index++)
			{
				this.chains[index].Parse(this.ifoReader, vmgmLuOffset + (int)chainOffsets[index]);
				// check for phony title set which goes beyond the end of the vob
				for(int cellIndex = 1; cellIndex <= this.chains[index].CellCount; cellIndex++)
				{
					CellInformation cellInfo = this.chains[index].GetCell(cellIndex);
					if(this.startSectorMenuVob + cellInfo.LastVobuEndSector > this.lastSectorOfTitleSet)
					{
						this.chains = new ProgramGroupChain[0];
						return;
					}
				}
			}
		}

        void ParseAudioAndSubs()
        {
            this.ifoReader.SeekFromStart(0x202);
            this.audioTrackCount = this.ifoReader.ReadUInt16();
            for(int audioIndex = 0; audioIndex < this.audioTrackCount; audioIndex++)
            {
                this.ifoReader.SeekFromStart(0x204 + audioIndex * 8);
                AudioAttributes audio = new AudioAttributes();
				audio.TrackId = audioIndex + 1;
                int codingByte = this.ifoReader.ReadByte();
                audio.CodingMode = (AudioCodingMode)(codingByte >> 5);
                int samplingByte = this.ifoReader.ReadByte();
                audio.Channels = (samplingByte & 0x07) + 1;
                if((codingByte & 0x0c) != 0)
                {
                    byte[] lang = new byte[2];
                    if(this.ifoReader.Read(lang, 2) == 2)
                    {
                        audio.Language = Encoding.ASCII.GetString(lang);
                    }
                }
                if(audio.Language == null)
                {
                    audio.Language = string.Empty;
                }
                this.ifoReader.SeekFromStart(0x204 + audioIndex * 8 + 5);
                audio.CodeExtension = (AudioCodeExtension)(this.ifoReader.ReadByte());

                audioAttributes[audioIndex] = audio;
            }

            this.ifoReader.SeekFromStart(0x254);
            this.subTrackCount = this.ifoReader.ReadUInt16();
            for(int subIndex = 0; subIndex < this.subTrackCount; subIndex++)
            {
                this.ifoReader.SeekFromStart(0x256 + subIndex * 6);
                SubpictureAttributes subs = new SubpictureAttributes();
				subs.TrackId = subIndex + 1;
				int codingByte = this.ifoReader.ReadByte();
                this.ifoReader.SeekFromCurrent(1);
                if((codingByte & 0x03) != 0)
                {
                    byte[] lang = new byte[2];
                    if(this.ifoReader.Read(lang, 2) == 2)
                    {
                        subs.Language = Encoding.ASCII.GetString(lang);
                    }
                }
                this.ifoReader.SeekFromStart(0x256 + subIndex * 6 + 5);
                subs.CodeExtension = (SubpictureCodeExtension)(this.ifoReader.ReadByte());
                this.subAttributes[subIndex] = subs;
            }
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\n", this.ifoReader.FileName);
            sb.AppendFormat("Last Sector Title Set 0x{0:x}\n", this.lastSectorOfTitleSet);
            sb.AppendFormat("Last Sector IFO 0x{0:x}\n", this.lastSectorOfIfo);
			sb.AppendFormat("Volume Count {0}\n", this.volumeCount);
			sb.AppendFormat("Volume Number {0}\n", this.volumeNumber);
			sb.AppendFormat("Side Id {0}\n", this.sideId);
			sb.AppendFormat("TitleSet Count {0}\n", this.titleSetCount);
			sb.AppendFormat("Start Sector Menu VOB 0x{0:x}\n", this.startSectorMenuVob);
			sb.AppendFormat("Language Count {0}\n", this.languageCount);
			sb.AppendFormat("First Language Code {0}\n", this.firstLanguageCode);
			sb.AppendFormat("Menu Existance Flag 0x{0:x}\n", this.menuExistanceFlag);
			sb.AppendFormat("FirstPlay PGC Address {0}\n", this.firstPlayPGCAddress);

			sb.AppendFormat("{0} Titles\n\n", this.titleInfos.Count);
			foreach(TitleInfo info in this.titleInfos)
			{
				sb.AppendFormat("Vts {0} Seq {1} Angles {2} Chapters {3} Title within Vts {4}\n",
					info.VtsNumber, info.IsSequential, info.NumberOfAngles, info.NumberOfChapters,
					info.TitleWithinVts);
				sb.AppendFormat("StartSector {0} Command Location {1}\n\n", info.StartSector, info.CommandLocation);
			}

			sb.AppendFormat("First Chain\n\n");
			sb.Append(this.firstChain.ToString());
			sb.Append("\n");

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

		~VideoManagerTitleSet()
        {
            Dispose(false);
        }
	}
}
