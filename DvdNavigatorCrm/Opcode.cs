/*
 * Copyright (C) 2007, 2008, 2009, 2010 Chris Meadowcroft <crmeadowcroft@gmail.com>
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
	[Flags]
	public enum OpCommand : int
	{
		Unknown = 0,
		Nop = 1,
		Goto = 2,
		Break = 4,
		SetTmpParental = 8,
		Link = 16,
		LinkPGCN = 32,
		LinkPTTN = 64,
		LinkPGN = 128,
		LinkCN = 256,
		JumpTT = 512,
		JumpVTS_TT = 1024,
		JumpVTS_PTT = 2048,
		JumpSS = 0x1000,
		CallSS = 0x2000,
		Exit = 0x4000,
		Set = 0x8000,
		SetSTN = 0x10000,
		SetNVTMR = 0x20000,
		SetGPRMMD = 0x40000,
		SetAMXMD = 0x80000,
		SetHL_BTNN = 0x100000,
	}

	public enum OpLinkSubset
	{
		None = 0,
		TopCell = 1,
		NextCell = 2,
		PrevCell = 3,
		TopPG = 5,
		NextPG = 6,
		PrevPG = 7,
		TopPGC = 9,
		NextPGC = 10,
		PrevPGC = 11,
		GoupPGC = 12,
		TailPGC = 13,
		RSM = 16,
	}

	public enum OpRegister
	{
		Constant = 0,
		GeneralPurpose = 1,
		SystemParameter = 2,
	}

	public enum OpCompare
	{
		None = 0,
		And = 1,		// (op1 & op2) != 0
		Equal = 2,
		NotEqual = 3,
		GreaterThanOrEqual = 4,
		GreaterThan = 5,
		LessThanOrEqual = 6,
		LessThan = 7,
	}

	public enum OpOperation
	{
		None = 0,
		Assign = 1,
		Swap = 2,
		Add = 3,		// +=
		Subtract = 4,	// -=
		Multiply = 5,	// *=
		Divide = 6,		// /=
		Modulo = 7,		// %=
		Rand = 8,		// op0 = rand(op1)
		And = 9,		// &=
		Or = 10,		// |=
		Xor = 11,		// ^=
	}

	public enum SSType
	{
		None = -1,
		FP = 0,
		VMGM_menu = 1,
		VTSM_menu = 2,
		VMGM_pgcn = 3,
	}

	public class OpCode
	{
		public int OperationLine;
		public OpCommand Command = OpCommand.Unknown;
		public OpCompare Compare = OpCompare.None;
		public OpOperation Operation = OpOperation.None;
		public OpLinkSubset LinkSubset = OpLinkSubset.None;
		public OpRegister RegisterType1 = OpRegister.Constant;
		public int Value1;
		public OpRegister RegisterType2 = OpRegister.Constant;
		public int Value2;
		public int LineNumber;
		public int Level;
		public int PGCN;
		public int PTTN;
		public int PGN;
		public int CN;
		public int TTN;
		public int VTS;
		public int RsmCell;
		public int Menu;
		public SSType SSType = SSType.None;
		public int DestRegister = -1;
		public OpRegister SourceType = OpRegister.Constant;
		public int SourceValue = -1;
		public OpRegister AudioSource = OpRegister.Constant;
		public int AudioValue = -1;
		public OpRegister SubpictureSource = OpRegister.Constant;
		public int SubpictureValue = -1;
		public OpRegister AngleSource = OpRegister.Constant;
		public int AngleValue = -1;
		public OpRegister CountdownSource = OpRegister.Constant;
		public int CountdownValue = -1;
		public int CounterState = -1;
		public int HiliteButtonValue = -1;
		public OpRegister HiliteButtonSource = OpRegister.Constant;

		ulong code;

		public OpCode(ulong code, int line)
		{
			this.OperationLine = line;
			this.code = code;

			int op0 = Convert.ToInt32((code >> (7 * 8)) & 0xff);
			int op1 = Convert.ToInt32((code >> (6 * 8)) & 0xff);

			int opType = op0 >> 5;
			int opCmp = (op1 >> 4) & 7;
			bool directCompare = (op1 & 0x80) != 0;
			int direct = (op0 & 0x10) >> 4;
			int setOper = op0 & 0x0f;
			int cmd = op1 & 0x0f;

			switch(opType)
			{
			case 0:
				switch(cmd)
				{
				case 0:
					this.Command = OpCommand.Nop;
					break;
				case 1:
					this.Command = OpCommand.Goto;
					this.LineNumber = Convert.ToInt32(code & 0xff);
					break;
				case 2:
					this.Command = OpCommand.Break;
					break;
				case 3:
					this.Command = OpCommand.SetTmpParental;
					this.LineNumber = Convert.ToInt32(code & 0xff);
					this.Level = Convert.ToInt32((code >> 8) & 0x0f);
					break;
				}
				this.Compare = (OpCompare)opCmp;
				if(this.Compare != OpCompare.None)
				{
					SetReg1(Convert.ToInt32((code >> (4 * 8)) & 0xff));
					if(directCompare)
					{
						this.RegisterType2 = OpRegister.Constant;
						this.Value2 = Convert.ToInt32((code >> (2 * 8)) & 0xffff);
					}
					else
					{
						SetReg2(Convert.ToInt32((code >> (2 * 8)) & 0xff));
					}
				}
				break;
			case 1:
				if(direct == 0)
				{
					SetLink(cmd, code);
					this.Compare = (OpCompare)opCmp;
					if(this.Compare != OpCompare.None)
					{
						SetReg1(Convert.ToInt32((code >> (4 * 8)) & 0xff));
						if(directCompare)
						{
							this.RegisterType2 = OpRegister.Constant;
							this.Value2 = Convert.ToInt32((code >> (2 * 8)) & 0xffff);
						}
						else
						{
							SetReg2(Convert.ToInt32((code >> (2 * 8)) & 0xff));
						}
					}
				}
				else
				{
					switch(cmd)
					{
					case 0:
						this.Command = OpCommand.Nop;
						break;
					case 1:
						this.Command = OpCommand.Exit;
						break;
					case 2:
						this.Command = OpCommand.JumpTT;
						this.TTN = Convert.ToInt32((code >> 16) & 0xff);
						break;
					case 3:
						this.Command = OpCommand.JumpVTS_TT;
						this.TTN = Convert.ToInt32((code >> 16) & 0xff);
						break;
					case 5:
						this.Command = OpCommand.JumpVTS_PTT;
						this.TTN = Convert.ToInt32((code >> 16) & 0xff);
						this.PTTN = Convert.ToInt32((code >> 32) & 0xfff);
						break;
					case 6:
						this.Command = OpCommand.JumpSS;
						this.SSType = (SSType)((code >> 22) & 3);
						switch(this.SSType)
						{
						case SSType.VMGM_menu:
							this.Menu = Convert.ToInt32((code >> 16) & 0x1f);
							break;
						case SSType.VTSM_menu:
							this.Menu = Convert.ToInt32((code >> 16) & 0x1f);
							this.VTS = Convert.ToInt32((code >> 24) & 0xff);
							this.TTN = Convert.ToInt32((code >> 32) & 0xff);
							break;
						case SSType.VMGM_pgcn:
							this.PGCN = Convert.ToInt32((code >> 32) & 0xffff);
							break;
						}
						break;
					case 8:
						this.Command = OpCommand.CallSS;
						this.SSType = (SSType)((code >> 22) & 3);
						this.RsmCell = Convert.ToInt32((code >> 24) & 0xff);
						switch(this.SSType)
						{
						case SSType.VMGM_menu:
						case SSType.VTSM_menu:
							this.Menu = Convert.ToInt32((code >> 16) & 0x1f);
							break;
						case SSType.VMGM_pgcn:
							this.PGCN = Convert.ToInt32((code >> 32) & 0xffff);
							break;
						}
						break;
					}
					this.Compare = (OpCompare)opCmp;
					if(this.Compare != OpCompare.None)
					{
						SetReg1(Convert.ToInt32((code >> 8) & 0xff));
						SetReg2(Convert.ToInt32(code & 0xff));
					}
				}
				break;
			case 2:
				switch(setOper)
				{
				case 0:
					this.Command = OpCommand.Nop;
					break;
				case 1:
					this.Command = OpCommand.SetSTN;
					if(direct == 0)
					{
						if(((code >> 38) & 3) != 0)
						{
							this.AudioSource = OpRegister.GeneralPurpose;
							this.AudioValue = Convert.ToInt32((code >> 32) & 0xf);
						}
						if(((code >> 30) & 3) != 0)
						{
							this.SubpictureSource = OpRegister.GeneralPurpose;
							this.SubpictureValue = Convert.ToInt32((code >> 24) & 0xf);
						}
						if(((code >> 22) & 3) != 0)
						{
							this.AngleSource= OpRegister.GeneralPurpose;
							this.AngleValue = Convert.ToInt32((code >> 16) & 0xf);
						}
					}
					else
					{
						if(((code >> 38) & 3) != 0)
						{
							this.AudioValue = Convert.ToInt32((code >> 32) & 0x3f);
						}
						if(((code >> 30) & 3) != 0)
						{
							this.SubpictureValue = Convert.ToInt32((code >> 24) & 0x3f);
						}
						if(((code >> 22) & 3) != 0)
						{
							this.AngleValue = Convert.ToInt32((code >> 16) & 0x3f);
						}
					}
					break;
				case 2:
					this.Command = OpCommand.SetNVTMR;
					if(direct == 0)
					{
						SetReg(Convert.ToInt32((code >> 32) & 0xff), ref this.CountdownSource, ref this.CountdownValue);
					}
					else
					{
						this.CountdownValue = Convert.ToInt32((code >> 32) & 0xffff);
					}
					this.PGCN = Convert.ToInt32((code >> 16) & 0xffff);
					break;
				case 3:
					this.Command = OpCommand.SetGPRMMD;
					if(((code >> 22) & 3) != 0)
					{
						this.CounterState = 1;
					}
					else
					{
						this.CounterState = 0;
					}
					if(direct == 0)
					{
						SetSource(Convert.ToInt32((code >> 32) & 0xff));
					}
					else
					{
						this.SourceValue = Convert.ToInt32((code >> 32) & 0xffff);
					}
					this.DestRegister = Convert.ToInt32((code >> 16) & 0xf);
					break;
				case 4:
					this.Command = OpCommand.SetAMXMD;
					break;
				case 6:
					this.Command = OpCommand.SetHL_BTNN;
					if(direct == 0)
					{
						this.HiliteButtonSource = OpRegister.GeneralPurpose;
						this.HiliteButtonValue = Convert.ToInt32((code >> 16) & 0xf);
					}
					else
					{
						this.HiliteButtonValue = Convert.ToInt32((code >> 16) & 0x3f);
					}
					break;
				}

				this.Compare = (OpCompare)opCmp;
				if(this.Compare != OpCompare.None)
				{
					SetReg1(Convert.ToInt32((code >> 8) & 0xff));
					SetReg2(Convert.ToInt32(code & 0xff));
				}
				else
				{
					if(cmd != 0)
					{
						SetLink(cmd, code);
					}
				}
				break;
			case 3:
				if(setOper == 0)
				{
					this.Command = OpCommand.Nop;
				}
				else if(setOper <= 11)
				{
					this.Command = OpCommand.Set;
					this.Operation = (OpOperation)setOper;

					this.DestRegister = Convert.ToInt32((code >> (4 * 8)) & 0xf);
					if(direct == 0)
					{
						SetSource(Convert.ToInt32((code >> (2 * 8)) & 0xff));
					}
					else
					{
						this.SourceType = OpRegister.Constant;
						this.SourceValue = Convert.ToInt32((code >> (2 * 8)) & 0xffff);
					}

					this.Compare = (OpCompare)opCmp;
					if(this.Compare != OpCompare.None)
					{
						SetReg1(Convert.ToInt32((code >> (5 * 8)) & 0xff));
						if(directCompare)
						{
							this.RegisterType2 = OpRegister.Constant;
							this.Value2 = Convert.ToInt32(code & 0xffff);
						}
						else
						{
							SetReg2(Convert.ToInt32(code & 0xff));
						}
					}
					else
					{
						if(cmd != 0)
						{
							SetLink(cmd, code);
						}
					}
				}
				break;
			default:
				System.Diagnostics.Debugger.Break();
				break;
			}
		}

		void SetLink(int cmd, ulong code)
		{
			switch(cmd)
			{
			case 0:
				this.Command |= OpCommand.Nop;
				break;
			case 1:
				this.Command |= OpCommand.Link;
				this.LinkSubset = (OpLinkSubset)Convert.ToInt32(code & 0x1f);
				break;
			case 4:
				this.Command |= OpCommand.LinkPGCN;
				this.PGCN = Convert.ToInt32(code & 0xffff);
				break;
			case 5:
				this.Command |= OpCommand.LinkPTTN;
				this.HiliteButtonValue = Convert.ToInt32((code >> 10) & 0x3f);
				this.PTTN = Convert.ToInt32(code & 0x3ff);
				break;
			case 6:
				this.Command |= OpCommand.LinkPGN;
				this.HiliteButtonValue = Convert.ToInt32((code >> 10) & 0x3f);
				this.PGN = Convert.ToInt32(code & 0xff);
				break;
			case 7:
				this.Command |= OpCommand.LinkCN;
				this.HiliteButtonValue = Convert.ToInt32((code >> 10) & 0x3f);
				this.CN = Convert.ToInt32(code & 0xff);
				break;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}. ", this.OperationLine);
			if(this.Compare != OpCompare.None)
			{
				AddRegCompare(sb);
			}

			sb.AppendFormat("{0} ", this.Command);

			if(this.DestRegister != -1)
			{
				sb.AppendFormat("GP{0} ", this.DestRegister);
			}
			if(this.Operation != OpOperation.None)
			{
				AddOperation(sb);
			}
			if(this.SourceValue != -1)
			{
				sb.Append(" ");
				AddReg(sb, this.SourceType, this.SourceValue);
				sb.Append(" ");
			}

			if(this.AudioValue != -1)
			{
				sb.Append("Audio= ");
				AddReg(sb, this.AudioSource, this.AudioValue);
				sb.Append(" ");
				sb.Append(" ");
			}
			if(this.SubpictureValue != -1)
			{
				sb.Append("SubPicture= ");
				AddReg(sb, this.SubpictureSource, this.SubpictureValue);
				sb.Append(" ");
			}
			if(this.AngleValue != -1)
			{
				sb.Append("Angle= ");
				AddReg(sb, this.AngleSource, this.AngleValue);
				sb.Append(" ");
			}
			if(this.CountdownValue != -1)
			{
				sb.Append("Countdown= ");
				AddReg(sb, this.CountdownSource, this.CountdownValue);
				sb.Append(" ");
			}
			if(this.HiliteButtonValue != -1)
			{
				sb.Append("Hilite Button ");
				AddReg(sb, this.HiliteButtonSource, this.HiliteButtonValue);
				sb.Append(" ");
			}

			if(this.LineNumber != 0)
			{
				sb.AppendFormat("line {0} ", this.LineNumber);
			}
			if(this.Level != 0)
			{
				sb.AppendFormat("level {0} ", this.Level);
			}
			if(this.PGCN != 0)
			{
				sb.AppendFormat("PGCN {0} ", this.PGCN);
			}
			if(this.PTTN != 0)
			{
				sb.AppendFormat("PTTN {0} ", this.PTTN);
			}
			if(this.PGN != 0)
			{
				sb.AppendFormat("PGN {0} ", this.PGN);
			}
			if(this.CN != 0)
			{
				sb.AppendFormat("CN {0} ", this.CN);
			}
			if(this.TTN != 0)
			{
				sb.AppendFormat("TTN {0} ", this.TTN);
			}
			if(this.VTS != 0)
			{
				sb.AppendFormat("VTS {0} ", this.VTS);
			}
			if(this.RsmCell != 0)
			{
				sb.AppendFormat("RsmCell {0} ", this.RsmCell);
			}
			if(this.Menu != 0)
			{
				sb.AppendFormat("Menu {0} ", this.Menu);
			}
			if(this.SSType != SSType.None)
			{
				sb.AppendFormat("SSType {0} ", this.SSType);
			}
			if(this.LinkSubset != OpLinkSubset.None)
			{
				sb.AppendFormat("LinkSubset {0} ", this.LinkSubset);
			}
			if(this.CounterState != -1)
			{
				if(this.CountdownValue == 0)
				{
					sb.Append(" NOT Counter");
				}
				else
				{
					sb.Append(" Counter");
				}
			}

			return sb.ToString();
		}

		void AddReg(StringBuilder sb, OpRegister source, int value)
		{
			switch(source)
			{
			case OpRegister.Constant:
				sb.AppendFormat("#{0:x}", value);
				break;
			case OpRegister.GeneralPurpose:
				sb.AppendFormat("GP{0}", value);
				break;
			case OpRegister.SystemParameter:
				sb.AppendFormat("Sys{0}", value);
				break;
			}
		}

		void AddOperation(StringBuilder sb)
		{
			switch(this.Operation)
			{
			case OpOperation.Add:
				sb.Append("+=");
				break;
			case OpOperation.And:
				sb.Append("&=");
				break;
			case OpOperation.Assign:
				sb.Append("=");
				break;
			case OpOperation.Divide:
				sb.Append("/=");
				break;
			case OpOperation.Modulo:
				sb.Append("%=");
				break;
			case OpOperation.Multiply:
				sb.Append("8=");
				break;
			case OpOperation.Or:
				sb.Append("|=");
				break;
			case OpOperation.Rand:
				sb.Append("Rand()");
				break;
			case OpOperation.Subtract:
				sb.Append("-=");
				break;
			case OpOperation.Swap:
				sb.Append("<->");
				break;
			case OpOperation.Xor:
				sb.Append("^=");
				break;
			}
		}

		void AddRegCompare(StringBuilder sb)
		{
			sb.Append("if( ");
			AddReg(sb, this.RegisterType1, this.Value1);

			switch(this.Compare)
			{
			case OpCompare.And:
				sb.Append(" & ");
				break;
			case OpCompare.Equal:
				sb.Append(" == ");
				break;
			case OpCompare.GreaterThan:
				sb.Append(" > ");
				break;
			case OpCompare.GreaterThanOrEqual:
				sb.Append(" >= ");
				break;
			case OpCompare.LessThan:
				sb.Append(" < ");
				break;
			case OpCompare.LessThanOrEqual:
				sb.Append(" <= ");
				break;
			case OpCompare.NotEqual:
				sb.Append(" != ");
				break;
			}

			AddReg(sb, this.RegisterType2, this.Value2);
			sb.Append(") then ");
		}

		void SetReg1(int reg)
		{
			SetReg(reg, ref this.RegisterType1, ref this.Value1);
		}

		void SetReg2(int reg)
		{
			SetReg(reg, ref this.RegisterType2, ref this.Value2);
		}

		void SetSource(int reg)
		{
			SetReg(reg, ref this.SourceType, ref this.SourceValue);
		}

		void SetReg(int reg, ref OpRegister regType, ref int value)
		{
			if(reg <= 0x0f)
			{
				regType = OpRegister.GeneralPurpose;
				value = reg;
			}
			else if((reg >= 0x80) && (reg <= 0x97))
			{
				regType = OpRegister.SystemParameter;
				value = reg - 0x80;
			}
		}
	}
}
