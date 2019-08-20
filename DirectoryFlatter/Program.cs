using FileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryFlatter
{
    class Program
    {
        static void Flatten()
        {
            List<String> imageDirs = new List<string>();
            FileUtil.ListDirectory(@"D:\Mac", (isDir, path) => {
                if (isDir && Directory.GetFiles(path).Where(FileUtil.IsImage).Count() > 0)
                    imageDirs.Add(path);
                return path;
            });

            foreach (var pair in imageDirs
                .Select(dir => new KeyValuePair<string, string>(dir, Path.Combine(@"D:\Mac\pictures", Path.GetFileName(dir))))
                .Where(pair => pair.Key != pair.Value))
            {
                Console.WriteLine($"{pair.Key} -> {pair.Value}");
                Directory.Move(pair.Key, pair.Value);
            }
        }

        static List<string> emptyDirs = new List<string>();

        static bool IsEmptyDir(string dir, Func<string, bool> isAvailableFile)
        {
            bool fileEmpty = Directory.GetFiles(dir).Where(file => isAvailableFile(file)).Count() == 0;
            bool dirEmpty = Directory.GetDirectories(dir).Where(subdir => !IsEmptyDir(subdir, isAvailableFile)).Count() == 0;
            if ( fileEmpty && dirEmpty )
            {
                emptyDirs.Add(dir);
                return true;
            }
            else
            {
                return false;
            }
        }

        static void FindEmpty()
        {
            IsEmptyDir(@"D:\Mac", FileUtil.IsMultimedia);
            foreach ( var dir in emptyDirs )
            {
                Console.WriteLine(dir);
            }
        }

        static void DeleteEmpty()
        {
            IsEmptyDir(@"D:\Mac", FileUtil.IsMultimedia);
            foreach (var dir in emptyDirs)
            {
                Console.WriteLine(dir);
                Directory.Delete(dir, true);
            }
        }

        static void Main(string[] args)
        {
            DeleteEmpty();
        }
    }
}
