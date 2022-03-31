/*
 * Copyright (C) 2007, 2008 Christopher R Meadowcroft <crmeadowcroft@gmail.com>
 *
 * This file is part of DvdSubOcr, a free DVD Subtitle OCR program.
 * See for updates.
 *
 * DvdSubOcr is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * DvdSubOcr is distributed in the hope that it will be useful,
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
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class CellIdVobId : IEquatable<CellIdVobId>
    {
        public int CellId { get; set; }
        public int VobId { get; set; }

        public override string ToString()
        {
            return "Cell " + CellId.ToString() + " Vob " + VobId.ToString();
        }

        public bool Equals(CellIdVobId other)
        {
            return (other.CellId == this.CellId) && (other.VobId == this.VobId);
        }
    }
}
