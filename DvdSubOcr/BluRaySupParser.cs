using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public static class BluRaySupParser
    {
        private class SupSegment
        {
            /// <summary>
            /// Type of segment
            /// </summary>
            public int Type;

            /// <summary>
            /// segment size in bytes
            /// </summary>
            public int Size;

            /// <summary>
            /// segment PTS time stamp
            /// </summary>
            public long PtsTimestamp;

            /// <summary>
            /// segment DTS time stamp
            /// </summary>
            public long DtsTimestamp;
        }

        private static byte[] packetHeader = 
        {
		    0x50, 0x47,				// 0:  "PG"
		    0x00, 0x00, 0x00, 0x00,	// 2:  PTS - presentation time stamp
		    0x00, 0x00, 0x00, 0x00,	// 6:  DTS - decoding time stamp
		    0x00,					// 10: segment_type
		    0x00, 0x00,				// 11: segment_length (bytes following till next PG)
    	};

        private static byte[] headerPCSStart = 
        {
		    0x00, 0x00, 0x00, 0x00,	// 0: video_width, video_height
		    0x10, 					// 4: hi nibble: frame_rate (0x10=24p), lo nibble: reserved
		    0x00, 0x00,				// 5: composition_number (increased by start and end header)
		    (byte)0x80,				// 7: composition_state (0x80: epoch start)
		    0x00,					// 8: palette_update_flag (0x80), 7bit reserved
		    0x00,					// 9: palette_id_ref (0..7)
		    0x01,					// 10: number_of_composition_objects (0..2)
		    0x00, 0x00,				// 11: 16bit object_id_ref
		    0x00,					// 13: window_id_ref (0..1)
		    0x00,					// 14: object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
		    0x00, 0x00, 0x00, 0x00	// 15: composition_object_horizontal_position, composition_object_vertical_position
	    };

        private static byte[] headerPCSEnd = 
        {
		    0x00, 0x00, 0x00, 0x00,	// 0: video_width, video_height
		    0x10,					// 4: hi nibble: frame_rate (0x10=24p), lo nibble: reserved
		    0x00, 0x00,				// 5: composition_number (increased by start and end header)
		    0x00,					// 7: composition_state (0x00: normal)
		    0x00,					// 8: palette_update_flag (0x80), 7bit reserved
		    0x00,					// 9: palette_id_ref (0..7)
		    0x00,					// 10: number_of_composition_objects (0..2)
	    };

        private static byte[] headerODSFirst = 
        {
		    0x00, 0x00,				// 0: object_id
		    0x00,					// 2: object_version_number
		    (byte)0xC0,				// 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
		    0x00, 0x00, 0x00,		// 4: object_data_length - full RLE buffer length (including 4 bytes size info)
		    0x00, 0x00, 0x00, 0x00,	// 7: object_width, object_height
	    };

        private static byte[] headerODSNext = 
        {
		    0x00, 0x00,				// 0: object_id
		    0x00,					// 2: object_version_number
		    (byte)0x40,				// 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
    	};

        private static byte[] headerWDS = 
        {
		    0x01,					// 0 : number of windows (currently assumed 1, 0..2 is legal)
		    0x00,					// 1 : window id (0..1)
		    0x00, 0x00, 0x00, 0x00,	// 2 : x-ofs, y-ofs
		    0x00, 0x00, 0x00, 0x00	// 6 : width, height
	    };

        /// <summary>
        /// Parses a BluRay sup file 
        /// </summary>
        /// <param name="fileName">BluRay sup file name</param>
        /// <param name="log">Parsing info is logged here</param>
        /// <returns>List of BluRaySupPictures</returns>
        public static IList<PcsData> ParseBluRaySup(string fileName, StringBuilder log)
        {
            using(var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ParseBluRaySup(fs, log);
            }
        }

        const int HeaderSize = 13;

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms">memory stream containing sup data</param>
        /// <param name="log">Text parser log</param>
        public static IList<PcsData> ParseBluRaySup(Stream ms, StringBuilder log)
        {
            GC.Collect();

            SupSegment segment;
            long position = ms.Position;
            int segmentCount = 0;
            Dictionary<int, IList<PaletteInfo>> palettes = new Dictionary<int, IList<PaletteInfo>>();
            bool forceFirstOds = true;
            Dictionary<int, IList<OdsData>> bitmapObjects = new Dictionary<int, IList<OdsData>>();
            PcsData latestPcs = null;
            int latestCompNum = -1;
            List<PcsData> pcsList = new List<PcsData>();

            while(position < ms.Length)
            {
                ms.Seek(position, SeekOrigin.Begin);
                byte[] buffer = new byte[HeaderSize];
                ms.Read(buffer, 0, buffer.Length);
                segment = ParseSegmentHeader(buffer, log);
                position += HeaderSize;
                buffer = new byte[segment.Size];
                ms.Read(buffer, 0, buffer.Length);
                log.Append(segmentCount + ": ");

                switch(segment.Type)
                {
                case 0x14: // Palette
                    if(latestPcs != null)
                    {
                        log.AppendLine(string.Format("0x14 - Palette - PDS offset={0} size={1}", position, segment.Size));
                        PdsData pds = ParsePds(buffer, segment);
                        log.AppendLine(pds.Message);
                        if(pds.PaletteInfo != null)
                        {
                            if(!palettes.ContainsKey(pds.PaletteId))
                            {
                                palettes[pds.PaletteId] = new List<PaletteInfo>();
                            }
                            else
                            {
                                if(latestPcs.PaletteUpdate)
                                {
                                    palettes[pds.PaletteId].RemoveAt(palettes[pds.PaletteId].Count - 1);
                                }
                                else
                                {
                                    log.AppendLine("Extra Palette");
                                }
                            }
                            palettes[pds.PaletteId].Add(pds.PaletteInfo);
                        }
                    }
                    break;

                case 0x15: // Image bitmap data
                    if(latestPcs != null)
                    {
                        log.AppendLine(string.Format("0x15 - Bitmap data - ODS offset={0} size={1}", position, segment.Size));
                        OdsData ods = ParseOds(buffer, segment, forceFirstOds);
                        log.AppendLine(ods.Message);
                        if(!latestPcs.PaletteUpdate)
                        {
                            IList<OdsData> odsList;
                            if(ods.IsFirst)
                            {
                                odsList = new List<OdsData>();
                                odsList.Add(ods);
                                bitmapObjects[ods.ObjectId] = odsList;
                            }
                            else
                            {
                                if(bitmapObjects.TryGetValue(ods.ObjectId, out odsList))
                                {
                                    odsList.Add(ods);
                                }
                                else
                                {
                                    log.AppendLine(string.Format("INVALID ObjectId {0} in ODS, offset={1}", ods.ObjectId, position));
                                }
                            }
                        }
                        else
                        {
                            log.AppendLine(string.Format("Bitmap Data Ignore due to PaletteUpdate offset={0}", position));
                        }
                        forceFirstOds = false;
                    }
                    break;

                case 0x16: // Picture time codes
                    if(latestPcs != null)
                    {
                        if(CompletePcs(latestPcs, bitmapObjects, palettes))
                        {
                            pcsList.Add(latestPcs);
                        }
                        latestPcs = null;
                    }

                    log.AppendLine(string.Format("0x16 - Picture codes, offset={0} size={1}", position, segment.Size));
                    forceFirstOds = true;
                    PcsData nextPcs = ParsePicture(buffer, segment);
                    log.AppendLine(nextPcs.Message);
                    latestPcs = nextPcs;
                    latestCompNum = nextPcs.CompNum;
                    if(latestPcs.CompositionState == CompositionState.EpochStart)
                    {
                        bitmapObjects.Clear();
                        palettes.Clear();
                    }
                    if(latestPcs.PcsObjects.Count > 1)
                    {
                        Debug.Write("");
                    }
                    break;

                case 0x17: // Window display
                    if(latestPcs != null)
                    {
                        log.AppendLine(string.Format("0x17 - Window display offset={0} size={1}", position, segment.Size));
                        int windowCount = buffer[0];
                        int offset = 0;
                        for(int nextWindow = 0; nextWindow < windowCount; nextWindow++)
                        {
                            int windowId = buffer[1 + offset];
                            int x = BigEndianInt16(buffer, 2 + offset);
                            int y = BigEndianInt16(buffer, 4 + offset);
                            int width = BigEndianInt16(buffer, 6 + offset);
                            int height = BigEndianInt16(buffer, 8 + offset);
                            log.AppendLine(string.Format("WinId: {4}, X: {0}, Y: {1}, Width: {2}, Height: {3}",
                                x, y, width, height, windowId));
                            offset += 9;
                        }
                    }
                    break;

                case 0x80:
                    forceFirstOds = true;
                    log.AppendLine(string.Format("0x80 - END offset={0} size={1}", position, segment.Size));
                    if(latestPcs != null)
                    {
                        if(CompletePcs(latestPcs, bitmapObjects, palettes))
                        {
                            pcsList.Add(latestPcs);
                        }
                        latestPcs = null;
                    }
                    break;

                default:
                    log.AppendLine(string.Format("0x?? - END offset={0} UNKOWN SEGMENT TYPE={1}", position, segment.Type));
                    break;
                }

                //Debug.WriteLine(log.ToString());
                //log.Length = 0;

                position += segment.Size;
                segmentCount++;
            }

            if(latestPcs != null)
            {
                if(CompletePcs(latestPcs, bitmapObjects, palettes))
                {
                    pcsList.Add(latestPcs);
                }
                latestPcs = null;
            }

            for(int pcsIndex = 1; pcsIndex < pcsList.Count; pcsIndex++)
            {
                pcsList[pcsIndex - 1].PtsEnd = pcsList[pcsIndex].Pts;
            }
            pcsList.RemoveAll(pcs => pcs.PcsObjects.Count == 0);

            GC.Collect();

            foreach(PcsData pcs in pcsList)
            {
                foreach(IList<OdsData> odsList in pcs.BitmapObjects)
                {
                    if(odsList.Count > 1)
                    {
                        int bufSize = 0;
                        foreach(OdsData ods in odsList)
                        {
                            bufSize += ods.Fragment.ImagePacketSize;
                        }
                        byte[] buf = new byte[bufSize];
                        int offset = 0;
                        foreach(OdsData ods in odsList)
                        {
                            Buffer.BlockCopy(ods.Fragment.ImageBuffer, 0, buf, offset, ods.Fragment.ImagePacketSize);
                            offset += ods.Fragment.ImagePacketSize;
                        }
                        odsList[0].Fragment.ImageBuffer = buf;
                        odsList[0].Fragment.ImagePacketSize = bufSize;
                        while(odsList.Count > 1)
                        {
                            odsList.RemoveAt(1);
                        }
                    }
                }
            }

            GC.Collect();
            return pcsList;
        }

        static bool CompletePcs(PcsData pcs, IDictionary<int, IList<OdsData>> bitmapObjects, Dictionary<int, IList<PaletteInfo>> palettes)
        {
            if(pcs.PcsObjects.Count == 0)
            {
                return true;
            }

            if(!palettes.ContainsKey(pcs.PaletteId))
            {
                Debug.WriteLine("CompletePcs Unknown PaletteId " + pcs.PaletteId);
                return false;
            }
            pcs.PaletteInfos = new List<PaletteInfo>(palettes[pcs.PaletteId]);

            pcs.BitmapObjects = new List<IList<OdsData>>();
            for(int index = 0; index < pcs.PcsObjects.Count; index++)
            {
                int objId = pcs.PcsObjects[index].ObjectId;
                if(!bitmapObjects.ContainsKey(objId))
                {
                    Debug.WriteLine("CompletePcs Unknown ObjectId " + objId);
                    return false;
                }
                pcs.BitmapObjects.Add(bitmapObjects[objId]);
            }

            return true;
        }

        /// <summary>
        /// Convert an integer to a string with leading zeroes 
        /// </summary>
        /// <param name="i">Integer value to convert</param>
        /// <param name="digits">Number of digits to display - note that a 32bit number can have only 10 digits</param>
        /// <returns>String version of integer with trailing zeroes</returns>
        public static string ZeroTrim(int i, int digits)
        {
            string s = i.ToString();
            return s.PadLeft(digits, '0');
        }

        /**
         * Convert time in milliseconds to array containing hours, minutes, seconds and milliseconds
         * @param ms Time in milliseconds
         * @return Array containing hours, minutes, seconds and milliseconds (in this order)
         */
        public static int[] MillisecondsToTime(long ms)
        {
            int[] time = new int[4];
            // time[0] = hours
            time[0] = (int)(ms / (60 * 60 * 1000));
            ms -= time[0] * 60 * 60 * 1000;
            // time[1] = minutes
            time[1] = (int)(ms / (60 * 1000));
            ms -= time[1] * 60 * 1000;
            // time[2] = seconds
            time[2] = (int)(ms / 1000);
            ms -= time[2] * 1000;
            time[3] = (int)ms;
            return time;
        }

        public static string PtsToTimeString(long pts)
        {
            int[] time = MillisecondsToTime((pts + 45) / 90);
            return ZeroTrim(time[0], 2) + ":" + ZeroTrim(time[1], 2) + ":" + ZeroTrim(time[2], 2) + "." + ZeroTrim(time[3], 3);
        }

        static PcsData ParsePicture(byte[] buffer, SupSegment segment)
        {
            StringBuilder sb = new StringBuilder();

            PcsData pcs = new PcsData();
            pcs.Size = new Size(BigEndianInt16(buffer, 0), BigEndianInt16(buffer, 2));
            pcs.FramesPerSecondType = buffer[4];                // hi nibble: frame_rate, lo nibble: reserved
            pcs.CompNum = BigEndianInt16(buffer, 5);
            pcs.CompositionState = GetCompositionState(buffer[7]);
            pcs.Pts = segment.PtsTimestamp;
            // 8bit  palette_update_flag (0x80), 7bit reserved
            pcs.PaletteUpdate = (buffer[8] == 0x80);
            if(pcs.PaletteUpdate)
            {
                Debug.Write("");
            }
            pcs.PaletteId = buffer[9];	// 8bit  palette_id_ref
            int compositionObjectCount = buffer[10];	// 8bit  number_of_composition_objects (0..2)

            sb.AppendFormat("CompNum: {0}, Pts: {1}, State: {2}, PalUpdate: {3}, PalId {4}",
                pcs.CompNum, PtsToTimeString(pcs.Pts), pcs.CompositionState, pcs.PaletteUpdate, pcs.PaletteId);

            if(pcs.CompositionState == CompositionState.Invalid)
            {
                sb.Append("Illegal composition state Invalid");
            }
            else
            {
                int offset = 0;
                pcs.PcsObjects = new List<PcsObject>();
                for(int compObjIndex = 0; compObjIndex < compositionObjectCount; compObjIndex++)
                {
                    PcsObject pcsObj = ParsePcs(buffer, segment, offset);
                    pcs.PcsObjects.Add(pcsObj);
                    sb.AppendLine();
                    sb.AppendFormat("ObjId: {0}, WinId: {1}, Forced: {2}, X: {3}, Y: {4}",
                        pcsObj.ObjectId, pcsObj.WindowId, pcsObj.IsForced, pcsObj.Origin.X, pcsObj.Origin.Y);
                    offset += 8;
                }
            }

            pcs.Message = sb.ToString();
            return pcs;
        }

        /// <summary>
        /// Parse an PCS packet which contains width/height info
        /// </summary>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <param name="msg">reference to message string</param>
        /// <param name="buffer">Raw data buffer, starting right after segment</param>
        private static PcsObject ParsePcs(byte[] buffer, SupSegment segment, int offset)
        {
            PcsObject pcs = new PcsObject();
            // composition_object:
            pcs.ObjectId = BigEndianInt16(buffer, 11 + offset);	// 16bit object_id_ref
            pcs.WindowId = buffer[13 + offset];
            // skipped:  8bit  window_id_ref
            // object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
            int forcedCropped = buffer[14 + offset];
            pcs.IsForced = ((forcedCropped & 0x40) == 0x40);
            pcs.Origin = new Point(BigEndianInt16(buffer, 15 + offset), BigEndianInt16(buffer, 17 + offset));
            return pcs;
        }

        /// <summary>
        /// parse an ODS packet which contain the image data 
        /// </summary>
        /// <param name="buffer">raw byte date, starting right after segment</param>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <param name="msg">reference to message string</param>
        /// <returns>true if this is a valid new object (neither invalid nor a fragment)</returns>
        private static OdsData ParseOds(byte[] buffer, SupSegment segment, bool forceFirst)
        {
            int objId = BigEndianInt16(buffer, 0);		// 16bit object_id
            int objVer = buffer[2];		// 16bit object_id nikse - index 2 or 1???
            int objSeq = buffer[3];		// 8bit  first_in_sequence (0x80),
            // last_in_sequence (0x40), 6bits reserved
            bool first = ((objSeq & 0x80) == 0x80) || forceFirst;
            bool last = (objSeq & 0x40) == 0x40;

            ImageObjectFragment info = new ImageObjectFragment();
            if(first)
            {
                int width = BigEndianInt16(buffer, 7);		// object_width
                int height = BigEndianInt16(buffer, 9);		// object_height

                info.ImagePacketSize = segment.Size - 11; // Image packet size (image bytes)
                info.ImageBuffer = new byte[info.ImagePacketSize];
                Buffer.BlockCopy(buffer, 11, info.ImageBuffer, 0, info.ImagePacketSize);

                return new OdsData
                {
                    IsFirst = true,
                    Size = new Size(width, height),
                    ObjectId = objId,
                    ObjectVersion = objVer,
                    Fragment = info,
                    Message = "ObjId: " + objId + ", ver: " + objVer + ", seq: " + (first ? "first" : "") +
                        ((first && last) ? "/" : "") + (last ? "" + "last" : "") + ", width: " + width +
                        ", height: " + height,
                };
            }
            else
            {
                info.ImagePacketSize = segment.Size - 4;
                info.ImageBuffer = new byte[info.ImagePacketSize];
                Buffer.BlockCopy(buffer, 4, info.ImageBuffer, 0, info.ImagePacketSize);

                return new OdsData
                {
                    IsFirst = false,
                    ObjectId = objId,
                    ObjectVersion = objVer,
                    Fragment = info,
                    Message = "Continued ObjId: " + objId + ", ver: " + objVer + ", seq: " + (last ? "" + "last" : ""),
                };
            }

        }

        private static CompositionState GetCompositionState(byte type)
        {
            switch(type)
            {
            case 0x00:
                return CompositionState.Normal;
            case 0x40:
                return CompositionState.AcquPoint;
            case 0x80:
                return CompositionState.EpochStart;
            case 0xC0:
                return CompositionState.EpochContinue;
            default:
                return CompositionState.Invalid;
            }
        }

        private static int BigEndianInt16(byte[] buffer, int index)
        {
            return (buffer[index + 1]) + (buffer[index + 0] << 8);
        }

        private static uint BigEndianInt32(byte[] buffer, int index)
        {
            return (uint)((buffer[index + 3]) + (buffer[index + 2] << 8) + (buffer[index + 1] << 0x10) + (buffer[index + 0] << 0x18));
        }

        private static SupSegment ParseSegmentHeader(byte[] buffer, StringBuilder log)
        {
            SupSegment segment = new SupSegment();
            if(buffer[0] == 0x50 && buffer[1] == 0x47) // 80 + 71 - P G
            {
                segment.PtsTimestamp = BigEndianInt32(buffer, 2); // read PTS
                segment.DtsTimestamp = BigEndianInt32(buffer, 6); // read PTS              
                segment.Type = buffer[10];
                segment.Size = BigEndianInt16(buffer, 11);
            }
            else
            {
                log.AppendLine("Unable to read segment - PG missing!");
            }
            return segment;
        }

        /// <summary>
        /// parse an PDS packet which contain palette info
        /// </summary>
        /// <param name="buffer">Buffer of raw byte data, starting right after segment</param>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <param name="msg">reference to message string</param>
        /// <returns>number of valid palette entries (-1 for fault)</returns>
        private static PdsData ParsePds(byte[] buffer, SupSegment segment)
        {
            int paletteId = buffer[0];	// 8bit palette ID (0..7)
            // 8bit palette version number (incremented for each palette change)
            int paletteUpdate = buffer[1];

            PaletteInfo p = new PaletteInfo();
            p.PaletteSize = (segment.Size - 2) / 5;

            if(p.PaletteSize <= 0)
            {
                return new PdsData
                {
                    Message = "Empty palette",
                };
            }

            if(paletteUpdate != 0)
            {
                //Debug.WriteLine("paletteUpdate");
            }

            p.PaletteBuffer = new byte[p.PaletteSize * 5];
            Buffer.BlockCopy(buffer, 2, p.PaletteBuffer, 0, p.PaletteSize * 5); // save palette buffer in palette object

            return new PdsData
            {
                Message = "PalId: " + paletteId + ", update: " + paletteUpdate + ", " + p.PaletteSize + " entries",
                PaletteId = paletteId,
                PaletteVersion = paletteUpdate,
                PaletteInfo = p,
            };
        }
    }
}
