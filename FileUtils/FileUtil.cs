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

        static HashSet<string> VIDEO_EXTS = new HashSet<string>(new string[] {
            ".avi", ".mkv", ".mov", ".mp4",
        });

        public static bool ContainsExt(string path, HashSet<string> hashSet)
        {
            string ext = Path.GetExtension(path);
            return ext != null && hashSet.Contains(ext.ToLower());
        }

        public static bool IsImage(string path)
        {
            return ContainsExt(path, IMAGE_EXTS);
        }

        public static bool IsVideo(string path)
        {
            return ContainsExt(path, VIDEO_EXTS);
        }

        public static bool IsMultimedia(string path)
        {
            return IsImage(path) || IsVideo(path);
        }
    }
}
