using System;
using System.Collections.Generic;
using System.IO;

namespace FileUtils
{
    public static class FileUtil
    {
        public static void ListDirectory(String dir, Func<bool, String, String> onFile)
        {
            String[] files = Directory.GetFiles(dir);
            foreach (String file in files)
            {
                onFile.Invoke(false, file);
            }

            String[] dirs = Directory.GetDirectories(dir);
            foreach (String d in dirs)
            {
                String rd = onFile.Invoke(true, d);
                ListDirectory(rd, onFile);
            }
        }

        static HashSet<string> IMAGE_EXTS = new HashSet<string>(new string[] {
            ".gif", ".jpeg", ".jpg", ".png", ".webp"
        });

        public static bool IsImage(string path)
        {
            string ext = Path.GetExtension(path);
            return ext != null && IMAGE_EXTS.Contains(ext.ToLower());
        }

    }
}
