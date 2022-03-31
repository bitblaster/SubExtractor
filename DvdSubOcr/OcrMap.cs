using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DvdSubOcr
{
    public class OcrMap
    {
        const char MapConnector = '#';
        const int Version1 = 1;
        const int Version2 = 2;
        const int Version3 = 3;
        const int Version4 = 4;
        const int CurrentVersion = 5;

        public class SplitMapEntry
        {
            public SplitMapEntry(OcrSplit split1, OcrSplit split2, IEnumerable<int> movieIds)
            {
                this.Split1 = split1;
                this.Split2 = split2;
                this.MovieIds = new List<int>(movieIds);
            }

            public OcrSplit Split1 { get; private set; }
            public OcrSplit Split2 { get; private set; }
            public IList<int> MovieIds { get; private set; }
        }

        Dictionary<string, IList<OcrEntry>> ocrMap = new Dictionary<string, IList<OcrEntry>>();
        Dictionary<string, BlockReducer> fullToReduc = new Dictionary<string, BlockReducer>();
        Dictionary<string, BlockReducer> tempFullToReduc = new Dictionary<string, BlockReducer>();
        Dictionary<string, IList<KeyValuePair<string, int>>> reducToFull = new Dictionary<string, IList<KeyValuePair<string, int>>>();
        Dictionary<int, string> movieNameMap = new Dictionary<int, string>();
        SortedDictionary<string, int> reverseMovieNameMap = new SortedDictionary<string, int>();
        Dictionary<string, SplitMapEntry> ocrSplits = new Dictionary<string, SplitMapEntry>();
        HashSet<string> lAndIWords = new HashSet<string>();
        int maxMovieId;

        public OcrMap()
        {
        }

        public int AddMovieName(string name)
        {
            string simpleName = SimplifyMovieName(name);
            int movieId;
            if(!this.reverseMovieNameMap.TryGetValue(simpleName, out movieId))
            {
                movieId = ++maxMovieId;
                this.movieNameMap[maxMovieId] = simpleName;
                this.reverseMovieNameMap[simpleName] = maxMovieId;
            }
            return movieId;
        }

        string SimplifyMovieName(string name)
        {
            string result = name.ToLowerInvariant();
            result = new string(result.Where(c => !Char.IsSymbol(c) && !Char.IsNumber(c) && (c != '-') && (c != '_')).ToArray());
            result = result.Replace(" angle ", " ").Replace(" disc ", " ").Replace(" track ", " ").Replace(" volume ", " ").Replace(" vol ", " ");
            result = new string(result.Where(c => !Char.IsWhiteSpace(c)).ToArray());
            return result;
        }

        public IEnumerable<KeyValuePair<string, int>> Movies
        {
            get
            {
                return this.reverseMovieNameMap;
            }
        }

        public IEnumerable<string> HighDefMatches
        {
            get
            {
                return this.fullToReduc.Keys;
            }
        }

        static HashSet<char> movieSpecificChars = new HashSet<char>();

        static bool useSpanishSpecialChars;

        public static bool UseSpanishSpecialChars
        {
            get { return useSpanishSpecialChars; }
            set
            {
                if(value != useSpanishSpecialChars)
                {
                    useSpanishSpecialChars = value;
                    movieSpecificChars.Clear();
                }
            }
        }

        static bool CheckForMovie(OcrCharacter ocr)
        {
            if(movieSpecificChars.Count == 0)
            {
                foreach(char c in SubConstants.MovieSpecificChars)
                {
                    movieSpecificChars.Add(c);
                }
                if(useSpanishSpecialChars)
                {
                    foreach(char c in SubConstants.MovieSpecificSpanishChars)
                    {
                        movieSpecificChars.Add(c);
                    }
                }
            }
            return movieSpecificChars.Contains(ocr.Value);
        }

        BlockReducer FindReduc(string fullEncode)
        {
            BlockReducer reduc;
            if(!this.fullToReduc.TryGetValue(fullEncode, out reduc))
            {
                if(!this.tempFullToReduc.TryGetValue(fullEncode, out reduc))
                {
                    reduc = new BlockReducer(new BlockEncode(fullEncode));
                    this.tempFullToReduc[fullEncode] = reduc;
                }
            }
            return reduc;
        }

        public IEnumerable<OcrEntry> FindMatches(string fullEncode, int movieId, bool isHighDef, int? extraPieces = null)
        {
            IList<OcrEntry> result;
            if(this.ocrMap.TryGetValue(fullEncode, out result))
            {
                foreach(OcrEntry entry in result)
                {
                    bool checkForMovie = CheckForMovie(entry.OcrCharacter);
                    if(!checkForMovie || entry.MovieIds.Contains(movieId))
                    {
                        if(!extraPieces.HasValue || (entry.ExtraPieceCount == extraPieces.Value))
                        {
                            yield return entry;
                        }
                    }
                }
            }

            if(!isHighDef)
            {
                yield break;
            }

            BlockReducer reduc = FindReduc(fullEncode);
            Dictionary<OcrEntry, int> matchPossibles = new Dictionary<OcrEntry, int>();
            foreach(KeyValuePair<string, int> reducedEncode in reduc.ReducedEncodes)
            {
                IList<KeyValuePair<string, int>> reducMatches;
                if(!this.reducToFull.TryGetValue(reducedEncode.Key, out reducMatches))
                {
                    continue;
                }

                foreach(KeyValuePair<string, int> reducMat in reducMatches)
                {
                    IList<OcrEntry> matchEntries;
                    if(this.ocrMap.TryGetValue(reducMat.Key, out matchEntries))
                    {
                        foreach(OcrEntry matchEntry in matchEntries)
                        {
                            bool checkForMovie = CheckForMovie(matchEntry.OcrCharacter);
                            if(!checkForMovie || matchEntry.MovieIds.Contains(movieId))
                            {
                                if(!extraPieces.HasValue || (matchEntry.ExtraPieceCount == extraPieces.Value))
                                {
                                    int matchCount;
                                    matchPossibles.TryGetValue(matchEntry, out matchCount);
                                    matchPossibles[matchEntry] = matchCount + reducedEncode.Value * reducMat.Value;
                                }
                            }
                        }
                    }
                }
            }

            if(matchPossibles.Count != 0)
            {
                List<KeyValuePair<OcrEntry, int>> possibles = new List<KeyValuePair<OcrEntry, int>>(matchPossibles);
                possibles.Sort(
                    delegate(KeyValuePair<OcrEntry, int> a, KeyValuePair<OcrEntry, int> b)
                    {
                        if(a.Key.ExtraPieceCount != b.Key.ExtraPieceCount)
                        {
                            return -a.Key.ExtraPieceCount.CompareTo(b.Key.ExtraPieceCount);
                        }
                        return -a.Value.CompareTo(b.Value);
                    });
                foreach(var v in possibles)
                {
                    yield return v.Key;
                }
            }
        }

        public bool IsMatch(string fullEncode1, string fullEncode2, bool isHighDef)
        {
            if(!isHighDef)
            {
                return (fullEncode1 == fullEncode2);
            }

            if(fullEncode1 == fullEncode2)
            {
                return true;
            }

            BlockReducer reduc1 = FindReduc(fullEncode1);
            BlockReducer reduc2 = FindReduc(fullEncode2);

            foreach(var matchReducEncode in reduc1.ReducedEncodes)
            {
                if(reduc2.ReducedEncodes.Any(r2 => r2.Key == matchReducEncode.Key))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsMatch(Point p1, string fullEncode1, Point p2, string fullEncode2, bool isHighDef)
        {
            if(!isHighDef)
            {
                return (fullEncode1 == fullEncode2) && (p1 == p2);
            }

            if((Math.Abs(p2.X - p1.X) > 2) || (Math.Abs(p2.Y - p1.Y) > 2))
            {
                return false;
            }

            if(fullEncode1 == fullEncode2)
            {
                return true;
            }

            BlockReducer reduc1 = FindReduc(fullEncode1);
            BlockReducer reduc2 = FindReduc(fullEncode2);

            foreach(var matchReducEncode in reduc1.ReducedEncodes)
            {
                if(reduc2.ReducedEncodes.Any(r2 => r2.Key == matchReducEncode.Key))
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<KeyValuePair<string, IList<OcrEntry>>> Matches
        {
            get
            {
                return this.ocrMap;
            }
        }

        public IEnumerable<OcrEntry> GetMatchesForMovie(int movieId, bool exactMovieMatchOnly)
        {
            foreach(KeyValuePair<string, IList<OcrEntry>> match in this.Matches)
            {
                foreach(OcrEntry entry in match.Value)
                {
                    if((!exactMovieMatchOnly && !CheckForMovie(entry.OcrCharacter)) || 
                        entry.MovieIds.Contains(movieId))
                    {
                        yield return entry;
                    }
                }
            }
        }

        public void AddMatch(OcrEntry newEntry, int movieId, bool isHighDef)
        {
            IList<OcrEntry> entries;
            bool found = false;
            if(!this.ocrMap.TryGetValue(newEntry.FullEncode, out entries))
            {
                entries = new List<OcrEntry>();
                ocrMap[newEntry.FullEncode] = entries;
            }
            else
            {
                for(int index = 0; index < entries.Count; index++)
                {
                    OcrEntry entry = entries[index];
                    if(newEntry.IsBitPatternEqual(entry) && 
                        newEntry.OcrCharacter.Equals(entry.OcrCharacter))
                    {
                        if(newEntry.OcrCharacter.Equals(entry.OcrCharacter))
                        {
                            if(!entry.MovieIds.Contains(movieId))
                            {
                                entry.MovieIds.Add(movieId);
                            }
                            found = true;
                        }
                        else
                        {
                            if(entry.MovieIds.Contains(movieId))
                            {
                                entry.MovieIds.Remove(movieId);
                                if(entry.MovieIds.Count == 0)
                                {
                                    entries.RemoveAt(index);
                                    index--;
                                }
                            }
                        }
                    }
                }
            }

            if(!found)
            {
                if(!newEntry.MovieIds.Contains(movieId))
                {
                    newEntry.MovieIds.Add(movieId);
                }
                entries.Add(newEntry);
            }

            if(isHighDef && (newEntry.OcrCharacter.Value != OcrCharacter.UnmatchedValue))
            {
                AddReducEntry(newEntry.FullEncode);
                foreach(var v in newEntry.ExtraPieces)
                {
                    AddReducEntry(v.Value);
                }
            }
        }

        void AddReducToFull(string fullEncode, BlockReducer reduc)
        {
            foreach(KeyValuePair<string, int> reducEncode in reduc.ReducedEncodes)
            {
                IList<KeyValuePair<string, int>> reverseList;
                if(!this.reducToFull.TryGetValue(reducEncode.Key, out reverseList))
                {
                    reverseList = new List<KeyValuePair<string, int>>();
                    this.reducToFull[reducEncode.Key] = reverseList;
                    reverseList.Add(new KeyValuePair<string, int>(fullEncode, 1));
                }
                else
                {
                    bool found = false;
                    for(int index = 0; index < reverseList.Count; index++)
                    {
                        var entry = reverseList[index];
                        if(entry.Key == fullEncode)
                        {
                            reverseList[index] = new KeyValuePair<string, int>(entry.Key, entry.Value + 1);
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        reverseList.Add(new KeyValuePair<string, int>(fullEncode, 1));
                    }
                }
            }
        }

        void AddReducEntry(string fullEncode)
        {
            BlockReducer reduc;
            if(!this.fullToReduc.TryGetValue(fullEncode, out reduc))
            {
                if(!this.tempFullToReduc.TryGetValue(fullEncode, out reduc))
                {
                    reduc = new BlockReducer(new BlockEncode(fullEncode));
                }
                else
                {
                    this.tempFullToReduc.Remove(fullEncode);
                }
                this.fullToReduc[fullEncode] = reduc;
                AddReducToFull(fullEncode, reduc);
            }
        }

        void RemoveReducToFull(string fullEncode, BlockReducer reduc)
        {
            foreach(KeyValuePair<string, int> reducString in reduc.ReducedEncodes)
            {
                IList<KeyValuePair<string, int>> matches;
                if(this.reducToFull.TryGetValue(reducString.Key, out matches))
                {
                    for(int index = 0; index < matches.Count; index++)
                    {
                        var match = matches[index];
                        if(match.Key == fullEncode)
                        {
                            if(reducString.Value >= match.Value)
                            {
                                matches.RemoveAt(index);
                            }
                            else
                            {
                                matches[index] = new KeyValuePair<string, int>(match.Key, match.Value - reducString.Value);
                            }
                            break;
                        }
                    }
                    if(matches.Count == 0)
                    {
                        this.reducToFull.Remove(reducString.Key);
                    }
                }
            }
        }

        public bool RemoveMatch(OcrEntry oldEntry, int movieId)
        {
            bool found = false;
            IList<OcrEntry> entries;
            if(this.ocrMap.TryGetValue(oldEntry.FullEncode, out entries))
            {
                for(int index = 0; index < entries.Count; index++)
                {
                    OcrEntry entry = entries[index];
                    if(oldEntry.IsBitPatternEqual(entry))
                    {
                        found = true;
                        entry.MovieIds.Remove(movieId);
                        if(entry.MovieIds.Count == 0)
                        {
                            entries.RemoveAt(index);
                            index--;
                        }
                    }
                }
                if(entries.Count == 0)
                {
                    this.ocrMap.Remove(oldEntry.FullEncode);

                    BlockReducer reduc;
                    if(this.fullToReduc.TryGetValue(oldEntry.FullEncode, out reduc))
                    {
                        this.fullToReduc.Remove(oldEntry.FullEncode);
                        RemoveReducToFull(oldEntry.FullEncode, reduc);
                    }
                }
            }
            return found;
        }

        public bool RemoveMatch(OcrEntry oldEntry)
        {
            bool found = false;
            IList<OcrEntry> entries;
            if(this.ocrMap.TryGetValue(oldEntry.FullEncode, out entries))
            {
                for(int index = 0; index < entries.Count; index++)
                {
                    OcrEntry entry = entries[index];
                    if(oldEntry.IsBitPatternEqual(entry))
                    {
                        found = true;
                        entries.RemoveAt(index);
                        index--;
                    }
                }
                if(entries.Count == 0)
                {
                    this.ocrMap.Remove(oldEntry.FullEncode);

                    BlockReducer reduc;
                    if(this.fullToReduc.TryGetValue(oldEntry.FullEncode, out reduc))
                    {
                        this.fullToReduc.Remove(oldEntry.FullEncode);
                        RemoveReducToFull(oldEntry.FullEncode, reduc);
                    }
                }
            }
            return found;
        }

        public void AddSplit(string fullEncode, int movieId)
        {
            SplitMapEntry entry;
            if(this.ocrSplits.TryGetValue(fullEncode, out entry))
            {
                if(!entry.MovieIds.Contains(movieId))
                {
                    entry.MovieIds.Add(movieId);
                }
            }
        }

        public void AddSplit(string fullEncode, OcrSplit split1, OcrSplit split2, int movieId)
        {
            SplitMapEntry entry;
            if(this.ocrSplits.TryGetValue(fullEncode, out entry))
            {
                if((entry.Split1.Equals(split1) && entry.Split2.Equals(split2)) ||
                    (entry.Split1.Equals(split2) && entry.Split2.Equals(split1)))
                {
                    if(!entry.MovieIds.Contains(movieId))
                    {
                        entry.MovieIds.Add(movieId);
                    }
                    return;
                }
            }
            this.ocrSplits[fullEncode] = new SplitMapEntry(split1, split2, new int[] { movieId });
        }

        public IEnumerable<KeyValuePair<string, SplitMapEntry>> Splits
        {
            get
            {
                return this.ocrSplits;
            }
        }

        public SplitMapEntry FindSplit(string fullEncode)
        {
            SplitMapEntry entry;
            if(this.ocrSplits.TryGetValue(fullEncode, out entry))
            {
                return entry;
            }
            return null;
        }

        public void RemoveSplit(string fullEncode, int movieId)
        {
            SplitMapEntry entry;
            if(this.ocrSplits.TryGetValue(fullEncode, out entry))
            {
                if(entry.MovieIds.Contains(movieId))
                {
                    if(entry.MovieIds.Count == 1)
                    {
                        this.ocrSplits.Remove(fullEncode);
                    }
                    else
                    {
                        entry.MovieIds.Remove(movieId);
                    }
                }
            }
        }

        public void RemoveSplit(string fullEncode)
        {
            this.ocrSplits.Remove(fullEncode);
        }

        public IEnumerable<string> SpellingWords
        {
            get
            {
                foreach(string word in this.lAndIWords)
                {
                    yield return word;
                }
            }
        }

        public bool FindLandIWord(string word)
        {
            return this.lAndIWords.Contains(word);
        }

        public void AddLandIWord(string word)
        {
            this.lAndIWords.Add(word);
        }

        public void RemoveLandIWord(string word)
        {
            if(this.lAndIWords.Contains(word))
            {
                this.lAndIWords.Remove(word);
            }
        }

        public void ClearLandIWords()
        {
            this.lAndIWords.Clear();
        }

        public static bool UseProgramExeForStorage { get; set; }

        public const string DatabasePrefix = "OcrMap";
        public const string DatabaseExtension = ".bin";
        public const string DatabaseOriginalName = "OcrMapOrig";

        public static string StorageFilePath(bool useProgramExeDirectory)
        {
            string storageDir = useProgramExeDirectory ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) :
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(storageDir, DatabasePrefix + DatabaseExtension);
        }

        public static string StorageFile
        {
            get
            {
                return StorageFilePath(UseProgramExeForStorage);
            }
        }

        public void Load()
        {
            if(!File.Exists(StorageFile))
            {
                return;
            }

            using(FileStream fileStream = new FileStream(StorageFile, FileMode.Open, FileAccess.Read))
            {
                using(BinaryReader reader = new BinaryReader(fileStream, Encoding.Unicode))
                {
                    int version = reader.ReadInt32();

                    if((version < Version1) || (version > CurrentVersion))
                    {
                        return;
                    }

                    Dictionary<int, int> duplicateMovieIds = new Dictionary<int, int>();
                    int dupOcrsFound = 0;
                    int dupOcrsUsed = 0;
                    int dupSplitsFound = 0;
                    int dupSplitsUsed = 0;

                    int movieCount = reader.ReadInt32();
                    for(int index = 0; index < movieCount; index++)
                    {
                        int id = reader.ReadInt32();
                        string name = reader.ReadString();
                        string simpleName = SimplifyMovieName(name);
                        int dupId;
                        if(this.reverseMovieNameMap.TryGetValue(simpleName, out dupId))
                        {
                            duplicateMovieIds[id] = dupId;
                            System.Diagnostics.Debug.WriteLine(name + " Dup Movie of " + simpleName);
                        }
                        else
                        {
                            this.movieNameMap[id] = simpleName;
                            this.reverseMovieNameMap[simpleName] = id;
                            this.maxMovieId = Math.Max(this.maxMovieId, id);
                        }
                    }

                    int ocrCount = reader.ReadInt32();
                    for(int index = 0; index < ocrCount; index++)
                    {
                        int entryCount = reader.ReadInt32();
                        string fullEncode = reader.ReadString();
                        List<OcrEntry> entries = new List<OcrEntry>();
                        this.ocrMap[fullEncode] = entries;
                        for(int entryIndex = 0; entryIndex < entryCount; entryIndex++)
                        {
                            OcrCharacter ocr = new OcrCharacter(reader.ReadChar(), reader.ReadBoolean());
                            int extraPieceCount = reader.ReadInt32();
                            OcrEntry entry = null;
                            if(extraPieceCount == 0)
                            {
                                entry = new OcrEntry(fullEncode, ocr);
                            }
                            else
                            {
                                List<KeyValuePair<Point, string>> extras = new List<KeyValuePair<Point, string>>();
                                for(int count = 0; count < extraPieceCount; count++)
                                {
                                    Point offset = new Point(reader.ReadInt32(), reader.ReadInt32());
                                    string combo = reader.ReadString();
                                    extras.Add(new KeyValuePair<Point,string>(offset, combo));
                                }
                                entry = new OcrEntry(fullEncode, ocr, extras);
                            }

                            int movieIdCount = reader.ReadInt32();
                            for(int movieIndex = 0; movieIndex < movieIdCount; movieIndex++)
                            {
                                int id = reader.ReadInt32();
                                int dupId;
                                if(duplicateMovieIds.TryGetValue(id, out dupId))
                                {
                                    dupOcrsFound++;
                                    if(!entry.MovieIds.Contains(dupId))
                                    {
                                        dupOcrsUsed++;
                                        entry.MovieIds.Add(dupId);
                                    }
                                }
                                else
                                {
                                    entry.MovieIds.Add(id);
                                }
                            }
                            entries.Add(entry);
                        }
                    }

                    int ocrSplitCount = reader.ReadInt32();
                    for(int index = 0; index < ocrSplitCount; index++)
                    {
                        string key = reader.ReadString();

                        string encode1 = reader.ReadString();
                        Point offset1 = new Point(reader.ReadInt32(), reader.ReadInt32());
                        OcrSplit split1 = new OcrSplit(offset1, encode1);
                        string encode2 = reader.ReadString();
                        Point offset2 = new Point(reader.ReadInt32(), reader.ReadInt32());
                        OcrSplit split2 = new OcrSplit(offset2, encode2);

                        int movieIdCount = reader.ReadInt32();
                        List<int> movieIds = new List<int>();
                        for(int movieIndex = 0; movieIndex < movieIdCount; movieIndex++)
                        {
                            int id = reader.ReadInt32();
                            int dupId;
                            if(duplicateMovieIds.TryGetValue(id, out dupId))
                            {
                                dupSplitsFound++;
                                if(!movieIds.Contains(dupId))
                                {
                                    dupSplitsUsed++;
                                    movieIds.Add(dupId);
                                }
                            }
                            else
                            {
                                movieIds.Add(id);
                            }
                        }
                        this.ocrSplits[key] = new SplitMapEntry(split1, split2, movieIds);
                    }

                    if(version > Version1)
                    {
                        int lAndICount = reader.ReadInt32();
                        for(int index = 0; index < lAndICount; index++)
                        {
                            string word = reader.ReadString();
                            this.lAndIWords.Add(word);
                        }
                    }

                    this.fullToReduc.Clear();
                    this.reducToFull.Clear();
                    if(version > Version2)
                    {
                        int fullToReducCount = reader.ReadInt32();

                        if((version == Version3) || (version == Version4))
                        {
                            for(int index = 0; index < fullToReducCount; index++)
                            {
                                string full = reader.ReadString();
                                int reducCount = reader.ReadInt32();
                                for(int reducIndex = 0; reducIndex < reducCount; reducIndex++)
                                {
                                    reader.ReadString();
                                }

                                AddReducEntry(full);
                            }
                        }
                        else
                        {
                            for(int index = 0; index < fullToReducCount; index++)
                            {
                                string full = reader.ReadString();
                                int reducCount = reader.ReadInt32();
                                KeyValuePair<string, int>[] reducs = new KeyValuePair<string, int>[reducCount];
                                for(int reducIndex = 0; reducIndex < reducCount; reducIndex++)
                                {
                                    reducs[reducIndex] = new KeyValuePair<string,int>(reader.ReadString(), reader.ReadInt32());
                                }
                                BlockReducer reduc = new BlockReducer(full, reducs);
                                AddReducToFull(full, reduc);
                                this.fullToReduc.Add(full, reduc);
                            }
                        }
                    }

                    string stats = String.Format("Dups {0} Ocrs {1} OcrsUsed {2} Splits {3} SplitsUsed {4}", duplicateMovieIds.Count,
                        dupOcrsFound, dupOcrsUsed, dupSplitsFound, dupSplitsUsed);
                    System.Diagnostics.Debug.WriteLine(stats);
                }
            }
        }

        public void Save()
        {
            using(FileStream fileStream = new FileStream(StorageFile, FileMode.Create, FileAccess.Write))
            {
                using(BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode))
                {
                    writer.Write(CurrentVersion);

                    writer.Write(this.movieNameMap.Count);
                    foreach(KeyValuePair<int, string> movieName in this.movieNameMap)
                    {
                        writer.Write(movieName.Key);
                        writer.Write(movieName.Value);
                    }

                    writer.Write(this.ocrMap.Count);
                    foreach(List<OcrEntry> ocrEntries in this.ocrMap.Values)
                    {
                        writer.Write(ocrEntries.Count);
                        writer.Write(ocrEntries[0].FullEncode);
                        foreach(OcrEntry entry in ocrEntries)
                        {
                            writer.Write(entry.OcrCharacter.Value);
                            writer.Write(entry.OcrCharacter.Italic);
                            writer.Write(entry.ExtraPieceCount);
                            if(entry.ExtraPieceCount > 0)
                            {
                                foreach(KeyValuePair<Point, string> piece in entry.ExtraPieces)
                                {
                                    writer.Write(piece.Key.X);
                                    writer.Write(piece.Key.Y);
                                    writer.Write(piece.Value);
                                }
                            }
                            writer.Write(entry.MovieIds.Count);
                            foreach(int movieId in entry.MovieIds)
                            {
                                writer.Write(movieId);
                            }
                        }
                    }

                    writer.Write(this.ocrSplits.Count);
                    foreach(KeyValuePair<string, SplitMapEntry> entry in this.ocrSplits)
                    {
                        writer.Write(entry.Key);
                        writer.Write(entry.Value.Split1.FullEncode);
                        writer.Write(entry.Value.Split1.Offset.X);
                        writer.Write(entry.Value.Split1.Offset.Y);
                        writer.Write(entry.Value.Split2.FullEncode);
                        writer.Write(entry.Value.Split2.Offset.X);
                        writer.Write(entry.Value.Split2.Offset.Y);
                        writer.Write(entry.Value.MovieIds.Count);
                        foreach(int movieId in entry.Value.MovieIds)
                        {
                            writer.Write(movieId);
                        }
                    }

                    writer.Write(this.lAndIWords.Count);
                    foreach(string word in this.lAndIWords)
                    {
                        writer.Write(word);
                    }

                    writer.Write(this.fullToReduc.Count);
                    foreach(var v in this.fullToReduc)
                    {
                        writer.Write(v.Key);
                        writer.Write(v.Value.ReducedEncodes.Count());
                        foreach(KeyValuePair<string, int> reduc in v.Value.ReducedEncodes)
                        {
                            writer.Write(reduc.Key);
                            writer.Write(reduc.Value);
                        }
                    }
                }
            }
        }
    }
}
