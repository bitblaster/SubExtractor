using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class VobNumber
    {
        public int IfoFileNumber;
        public int IfoOffset;
        public int IfoRemainder;

        public static VobNumber Calculate(IList<long> vobSizes, long cellStart)
        {
            VobNumber vob = new VobNumber();
            vob.IfoFileNumber = 1;
            foreach(long size in vobSizes)
            {
                if(cellStart < size)
                {
                    vob.IfoOffset = (int)cellStart;
                    vob.IfoRemainder = (int)(size - vob.IfoOffset);
                    return vob;
                }
                else
                {
                    cellStart -= size;
                    vob.IfoFileNumber++;
                }
            }
            throw new ArgumentOutOfRangeException("cellStart");
        }
    }
}
