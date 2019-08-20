using FileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DupFinder
{
    class DirInfo
    {
        public string Name { get; private set; }
        public string[] Files { get; private set; }

        public DirInfo(string name)
        {
            this.Name = name;
            this.Files = Directory.GetFiles(name).Where(FileUtil.IsImage).ToArray();
        }

        public override string ToString()
        {
            return $"{Name} ({Files.Length})";
        }
    }

    class FileInfo
    {
        public String Name { get; private set; }
        public String Hash { get; private set; }

        public FileInfo(string name, string hash)
        {
            Name = name;
            Hash = hash;
        }
    }

    class HashInfo
    {
        public String Hash { get; private set; }
        public List<String> Paths { get; private set; }

        public HashInfo(string hash)
        {
            Hash = hash;
            Paths = new List<string>();
        }

        public void AddPath(string path)
        {
            Paths.Add(path);
        }

        public override string ToString()
        {
            return Hash + "\n" + String.Join("\n", Paths);
        }
    }

    class DupCounter
    {
        public String Dir1 { get; private set; }
        public String Dir2 { get; private set; }
        public int _count = 0;
        public int Count { get { return _count; } }

        public DupCounter(string dir1, string dir2)
        {
            Dir1 = dir1;
            Dir2 = dir2;
        }

        public int AddCount()
        {
            return ++_count;
        }
    }

    class Program
    {
        SortedSet<string> dirs = new SortedSet<string>();
        List<FileInfo> fileInfos = new List<FileInfo>();
        Dictionary<string, HashInfo> hashMap = new Dictionary<string, HashInfo>();
        Dictionary<string, DirInfo> dirMap;
        Dictionary<string, DupCounter> dupCounterMap = new Dictionary<string, DupCounter>();

        string log = @"C:\Users\dmlim\source\repos\HangulMerger\HangulMerger\bin\Release\hash2.log";
        Regex regex = new Regex(@"^(?<hash>[0-9a-f]{32}) (?<path>.+) [\d\.]+$");

        void br()
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        void PrintTitle(string message)
        {
            br();
            Console.WriteLine(message);
            br();
        }

        void Run()
        {
            PrintTitle("Checking Files...");

            using (StreamReader reader = new StreamReader(log))
            {
                for (string line = null; (line = reader.ReadLine()) != null;)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        string hash = match.Groups["hash"].Value;
                        string path = match.Groups["path"].Value;
                        string dir = Path.GetDirectoryName(path);
                        string file = Path.GetFileName(path);
                        if (File.Exists(path))
                        {
                            fileInfos.Add(new FileInfo(path, hash));
                            dirs.Add(dir);
                            HashInfo hashInfo = null;
                            if (!hashMap.TryGetValue(hash, out hashInfo))
                                hashMap.Add(hash, hashInfo = new HashInfo(hash));
                            hashInfo.AddPath(path);
                        }
                    }
                }
            }

            PrintTitle("Counting Files...");

            dirMap = dirs.Select(dir => new DirInfo(dir)).ToDictionary(di => di.Name, di => di);

            foreach (DirInfo dirInfo in from dirInfo in dirMap.Values
                orderby dirInfo.Files.Length
                select dirInfo)
            {
                Console.WriteLine(dirInfo);
            }

            //foreach (DirInfo dirInfo in dirMap.Values)
            //{
            //    Console.WriteLine(dirInfo);
            //}

            PrintTitle("Duplicated Names");

            foreach ( IGrouping<string, string> group in dirMap.Values
                .Select(dirInfo => Path.GetFileName(dirInfo.Name))
                .GroupBy(name => name)
                .Where(group => group.Count() > 1) )
            {
                Console.WriteLine(group.Key);
            }

            PrintTitle("Counting Duplicates...");

            foreach (HashInfo hashInfo in hashMap.Values.Where(hashInfo => hashInfo.Paths.Count > 1))
            {
                for (int i = 0; i < hashInfo.Paths.Count - 1; i++)
                {
                    string dir1 = Path.GetDirectoryName(hashInfo.Paths[i]);
                    for (int j = i + 1; j < hashInfo.Paths.Count; j++)
                    {
                        string dir2 = Path.GetDirectoryName(hashInfo.Paths[j]);
                        string key = dir1 + "|" + dir2;
                        DupCounter dupCounter = null;
                        if (!dupCounterMap.TryGetValue(key, out dupCounter))
                        {
                            dupCounterMap.Add(key, dupCounter = new DupCounter(dir1, dir2));
                        }
                        dupCounter.AddCount();
                    }
                }
            }

            PrintTitle("Show Directory Dup Counts");

            foreach (DupCounter dupCounter in dupCounterMap.Values)
            {
                DirInfo dirInfo1 = dirMap[dupCounter.Dir1];
                DirInfo dirInfo2 = dirMap[dupCounter.Dir2];
                Console.WriteLine(dirInfo1);
                Console.WriteLine(dirInfo2);
                Console.WriteLine($"duplicate: {dupCounter.Count}");
                br();
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
