using FileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BulkHash
{
    class Program
    {
        static string GetMD5Hash(String file)
        {
            using (Stream fs = new BufferedStream(new FileStream(file, FileMode.Open, FileAccess.Read), 10 * 1024 * 1024))
            {
                return HexEncode(MD5.Create().ComputeHash(fs));
            }
        }

        static string HexEncode(byte[] data)
        {
            StringBuilder sBuilder = new StringBuilder();
            foreach (byte b in data)
                sBuilder.Append(b.ToString("x2"));
            return sBuilder.ToString();
        }

        static void Main_Hash(String[] args)
        {
            string dir = args[0];
            int[] count = new int[2];

            Console.WriteLine("Counting....");

            FileUtil.ListDirectory(dir, (isDir, path) => {
                count[0] += !isDir && FileUtil.IsImage(path) ? 1 : 0;
                return path;
            });

            Console.WriteLine($"Total: ${count[0]}");

            FileUtil.ListDirectory(dir, (isDir, path) => {
                if (!isDir && FileUtil.IsImage(path))
                {
                    count[1]++;
                    double progress = (double)count[1] / count[0] * 100;
                    Console.WriteLine($"{GetMD5Hash(path)} {path} {progress}");
                }
                return path;
            });
        }

        static void Main(string[] args)
        {
            Main_Hash(new string[] { @"D:\Mac" });
        }
    }
}
