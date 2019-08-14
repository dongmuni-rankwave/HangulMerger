using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        /*
         * https://www.unicode.org/charts/PDF/U1100.pdf
         * https://www.unicode.org/charts/PDF/U3130.pdf
         */

        // 초성 리스트. 00 ~ 18
        private static readonly char[] CHOSUNG_ARR = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ',
            'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ',
            'ㅎ'};

        // 중성 리스트. 00 ~ 20
        private static readonly char[] JUNGSUNG_ARR = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ',
            'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ',
            'ㅡ', 'ㅢ', 'ㅣ'};

        // 종성 리스트. 00 ~ 27 + 1(1개 없음)
        private static readonly char[] JONGSUNG_ARR = { ' ', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ',
            'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ',
            'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

        public static readonly String SAMPLE = "2016.01.12 [ㄱㅠㄹㅣ] - ㄴㅜㄷㅡ ㅎㅏㅇㅣㅋㅣㅇ [60P]";

        /*  0 0 초성 중성 종성 | 초성 5 bit | 중성 5 bit | 종성 5 bit */

        class HanChar
        {
            public char ch;
            public bool isChosung;
            public bool isJungsung;
            public bool isJongsung;
            public int chosungIndex;
            public int jungsungIndex;
            public int jongsungIndex;

            public override string ToString()
            {
                return $"ch: ${ch}, {(int)ch:X}, isChosung:{isChosung}, isJungsung:{isJungsung}, isJongsung:{isJongsung}, chosungIndex:{chosungIndex}, jungsungIndex:{jungsungIndex}, jongsungIndex:{jongsungIndex}";
            }
        }

        static Dictionary<char, HanChar> HanCharDic = new Dictionary<char, HanChar>();

        public static void InitHanCharDic()
        {
            for (int i = 0; i < CHOSUNG_ARR.Length; i++)
            {
                char ch = CHOSUNG_ARR[i];
                HanChar hanChar = new HanChar();
                hanChar.ch = ch;
                hanChar.isChosung = true;
                hanChar.chosungIndex = i;
                HanCharDic.Add(ch, hanChar);
            }

            for (int i = 0; i < JUNGSUNG_ARR.Length; i++)
            {
                char ch = JUNGSUNG_ARR[i];
                HanChar hanChar = new HanChar();
                hanChar.ch = ch;
                hanChar.isJungsung = true;
                hanChar.jungsungIndex = i;
                HanCharDic.Add(ch, hanChar);
            }

            for (int i = 1; i < JONGSUNG_ARR.Length; i++)
            {
                char ch = JONGSUNG_ARR[i];
                HanChar hanChar = HanCharDic.ContainsKey(ch) ? HanCharDic[ch] : new HanChar();
                hanChar.ch = ch;
                hanChar.isJongsung = true;
                hanChar.jongsungIndex = i;
                if (!HanCharDic.ContainsKey(ch))
                    HanCharDic.Add(ch, hanChar);
            }

            for (int i = 0; i < CHOSUNG_ARR.Length; i++)
            {
                char ch = (char)(0x1100 + i);
                HanChar hanChar = new HanChar();
                hanChar.ch = ch;
                hanChar.isChosung = true;
                hanChar.chosungIndex = i;
                HanCharDic.Add(ch, hanChar);
            }

            for (int i = 0; i < JUNGSUNG_ARR.Length; i++)
            {
                char ch = (char)(0x1161 + i);
                HanChar hanChar = new HanChar();
                hanChar.ch = ch;
                hanChar.isJungsung = true;
                hanChar.jungsungIndex = i;
                HanCharDic.Add(ch, hanChar);
            }

            for (int i = 1; i < JONGSUNG_ARR.Length; i++)
            {
                char ch = (char)(0x11A8 + i - 1);
                HanChar hanChar = new HanChar();
                hanChar.ch = ch;
                hanChar.isJongsung = true;
                hanChar.jongsungIndex = i;
                HanCharDic.Add(ch, hanChar);
            }
        }

        enum ParseStatus
        {
            NORMAL,
            CHO_FOUND,
            JUNG_FOUND,
            JONG_CHO_FOUND,
        };

        public static String MergeHangul(String value)
        {
            ParseStatus status = ParseStatus.NORMAL;
            char[] chars = value.ToCharArray();
            HanChar[] buffer = new HanChar[3];
            StringBuilder result = new StringBuilder();
            HanChar notHanChar = new HanChar();

            foreach (char ch in chars)
            {
                HanChar hanChar = HanCharDic.ContainsKey(ch) ? HanCharDic[ch] : notHanChar;
                bool restart = false;

                do
                {
                    restart = false;

                    switch (status)
                    {
                        case ParseStatus.NORMAL:
                            if (hanChar.isChosung)
                            {
                                buffer[0] = hanChar;
                                status = ParseStatus.CHO_FOUND;
                            }
                            else
                            {
                                result.Append(ch);
                            }
                            break;
                        case ParseStatus.CHO_FOUND:
                            if (hanChar.isJungsung)
                            {
                                buffer[1] = hanChar;
                                status = ParseStatus.JUNG_FOUND;
                            }
                            else
                            {
                                result.Append(MergeChar(buffer));
                                status = ParseStatus.NORMAL;
                                restart = true;
                            }
                            break;
                        case ParseStatus.JUNG_FOUND:
                            if (!hanChar.isChosung && hanChar.isJongsung)
                            {
                                buffer[2] = hanChar;
                                result.Append(MergeChar(buffer));
                                status = ParseStatus.NORMAL;
                            }
                            else if (hanChar.isChosung && hanChar.isJongsung)
                            {
                                buffer[2] = hanChar;
                                status = ParseStatus.JONG_CHO_FOUND;
                            }
                            else
                            {
                                result.Append(MergeChar(buffer));
                                status = ParseStatus.NORMAL;
                                restart = true;
                            }
                            break;
                        case ParseStatus.JONG_CHO_FOUND:
                            if (hanChar.isJungsung)
                            {
                                HanChar next_chosung = buffer[2];
                                buffer[2] = null;
                                result.Append(MergeChar(buffer));
                                buffer[0] = next_chosung;
                                buffer[1] = hanChar;
                                status = ParseStatus.JUNG_FOUND;
                            }
                            else
                            {
                                result.Append(MergeChar(buffer));
                                status = ParseStatus.NORMAL;
                                restart = true;
                            }
                            break;
                        default:
                            break;
                    }

                } while (restart);
            }

            if (buffer[0] != null)
            {
                result.Append(MergeChar(buffer));
            }

            return result.ToString();
        }

        private static char MergeChar(HanChar[] buffer)
        {
            char merged = ' ';

            if (buffer[1] == null)
            {
                merged = buffer[0].ch;
            }
            else
            {
                int choIndex = buffer[0].chosungIndex;
                int jungIndex = buffer[1].jungsungIndex;
                int jongIndex = buffer[2] != null ? buffer[2].jongsungIndex : 0;

                int value = 0xAC00 +
                    choIndex * JUNGSUNG_ARR.Length * JONGSUNG_ARR.Length +
                    jungIndex * JONGSUNG_ARR.Length +
                    jongIndex;
                merged = (char)value;
            }

            buffer[0] = buffer[1] = buffer[2] = null;
            return merged;
        }

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

        static void TestMerge()
        {
            foreach (KeyValuePair<char, HanChar> pair in HanCharDic)
            {
                Console.WriteLine($"{pair.Key} -> {pair.Value}");
            }

            String data = "메이크모델 예원영상 korean general photo model YeWon.mp4";

            Console.WriteLine(data);
            String result = MergeHangul(data);
            Console.WriteLine(result);
        }

        static String MergeHangulFile(bool isDir, String path)
        {
            String file = Path.GetFileName(path);
            String dir = Path.GetDirectoryName(path);
            String newFile = MergeHangul(file);

            if (file != newFile)
            {
                Console.WriteLine($"{dir}: {file} -> {newFile}");
                String newPath = Path.Combine(dir, newFile);
                if (isDir)
                {
                    Directory.Move(path, newPath);
                }
                else
                {
                    File.Move(path, newPath);
                }
                return newPath;
            }
            else
            {
                return path;
            }
        }

        private static void Usage()
        {
            string filename = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Console.Error.WriteLine($"{filename} path [ path ... ]");
            Console.Error.WriteLine();
        }

        static void Main(String[] args)
        {
            String dir = @"D:\Mac\pictures\출사";
            int[] count = new int[2];

            Console.WriteLine("Counting....");

            ListDirectory(dir, (isDir, path) => {
                count[0] += isDir ? 0 : 1;
                return path;
            });

            Console.WriteLine($"Total: ${count[0]}");

            ListDirectory(dir, (isDir, path) => {
                if (!isDir)
                {
                    count[1]++;
                    double progress = (double)count[1] / count[0] * 100;
                    Console.WriteLine($"{GetMD5Hash(path)} {path} {progress}");
                }
                return path;
            });
        }

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

        static void Main_HangulMerge(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return;
            }

            InitHanCharDic();

            foreach (string path in args)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        ListDirectory(path, MergeHangulFile);
                    }
                    else if (File.Exists(path))
                    {
                        MergeHangulFile(false, path);
                    }
                    else
                    {
                        Console.Error.WriteLine($"'{path}' is neither file nor directory.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine(".");
        }

    }
}
