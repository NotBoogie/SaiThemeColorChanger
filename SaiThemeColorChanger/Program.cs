using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SaiThemeColorChanger
{
    //Hex strat derived from https://social.msdn.microsoft.com/Forums/vstudio/en-US/a0b2133f-ae23-4c0b-b136-dd531952f3c7/find-amp-replace-hex-values-in-a-dll-file-using-c?forum=csharpgeneral
    class Program
    {
        public class ReplacerHelper
        {
            public ReplacerHelper(string search, string replace)
            {
                Search = search;
                Replace = replace;
            }

            public string Search { get; }
            public string Replace { get; }
        }

        static void Main(string[] args)
        {
            string inputPath = "";
            if (args.Length > 0)
                inputPath = args[0];

            if (inputPath.Length == 0)
            {
                System.Console.Out.WriteLine("Drag the sai2.exe file onto this exe, or just paste its full path into here: ");
                inputPath = System.Console.ReadLine();
                while (!Directory.Exists(Path.GetDirectoryName(inputPath)))
                {
                    System.Console.Out.WriteLine("Not a valid path: " + inputPath);
                    inputPath = System.Console.ReadLine();
                }
            }

            if (!Directory.Exists(Path.GetDirectoryName(inputPath)))
            {
                System.Console.Out.WriteLine("Not a valid path: " + inputPath);
                System.Console.ReadKey();
                return;
            }

            string outputPath = inputPath;   //Needs to be the same as the original or Sai throws a weird error with moonrunes 

            List<ReplacerHelper> toReplace = new List<ReplacerHelper>();
            //TODO could probably make this just read out of a text file is people decide my gray is horrible
            //Hex color code -> replacement (won't work with pure white and pure black, but everything else seems fine!)
            //Basically this replaces left hex with the right hex.
            //You can swap out the values to get other colors, I haven't noticed any issues using a version with these values modified
            toReplace.Add(new ReplacerHelper("f8f8f8", "9b9b9b")); //Main panel color
            toReplace.Add(new ReplacerHelper("c0c0c0", "646464")); //Canvas background color
            toReplace.Add(new ReplacerHelper("e8e8e8", "7f7f7f")); //Scrollbar insides
            toReplace.Add(new ReplacerHelper("969696", "343434")); //Scrollbars
            toReplace.Add(new ReplacerHelper("f0f0f0", "7f7f7f")); //Tools background
            toReplace.Add(new ReplacerHelper("d4d4d4", "7f7f7f")); //Inactive scrollbar arrows
            toReplace.Add(new ReplacerHelper("676767", "373737")); //Panel borders 2
            toReplace.Add(new ReplacerHelper("b0b0b0", "646464")); //Active canvas background
            toReplace.Add(new ReplacerHelper("E0E0E0", "646464")); //Tools panel background

            System.Console.Out.WriteLine("Making a backup copy of: " + inputPath);
            makeCopy(inputPath);
            System.Console.Out.WriteLine("Replacing stuff in: " + inputPath);
            replaceHex(inputPath, outputPath, toReplace);
            System.Console.Out.WriteLine("Replaced file saved to: " + outputPath);
            System.Console.Out.WriteLine("Finished");
            System.Console.ReadKey();
        }

        //Fuggin fug fug
        //Cut the hex string -> byte array
        public static byte[] GetByteArray(string str)
        {
            //https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
            return Enumerable.Range(0, str.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => System.Convert.ToByte(str.Substring(x, 2), 16))
                             .ToArray();
        }

        public static void makeCopy(string path)
        {
            string targetPath = Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path) + "- BACKUP with original UI" + Path.GetExtension(path);
            if (!File.Exists(targetPath))
            {
                File.Copy(path, targetPath);
                System.Console.Out.WriteLine("Backup copy generated in " + targetPath);
            }
            else
            {
                System.Console.Out.WriteLine("Backup copy already exists in " + targetPath);
            }
        }

        public static bool findHex(byte[] sequence, int position, byte[] seeker)
        {
            if (position + seeker.Length > sequence.Length) return false;

            for (int i = 0; i < seeker.Length; i++)
            {
                if (seeker[i] != sequence[position + i]) return false;
            }

            return true;
        }

        public static void replaceHex(string targetFile, string resultFile, string searchString, string replacementString)
        {

            var targetDirectory = Path.GetDirectoryName(resultFile);
            if (targetDirectory == null) return;
            Directory.CreateDirectory(targetDirectory);

            byte[] fileContent = File.ReadAllBytes(targetFile);

            byte[] seeker = GetByteArray(searchString);
            byte[] hider = GetByteArray(replacementString);

            for (int i = 0; i < fileContent.Length; i++)
            {
                if (!findHex(fileContent, i, seeker)) continue;

                for (int j = 0; j < seeker.Length; j++)
                {
                    fileContent[i + j] = hider[j];
                }
            }

            File.WriteAllBytes(resultFile, fileContent);
        }
        public static void replaceHex(string targetFile, string resultFile, List<ReplacerHelper> toReplace)
        {

            var targetDirectory = Path.GetDirectoryName(resultFile);
            if (targetDirectory == null) return;
            Directory.CreateDirectory(targetDirectory);

            byte[] fileContent = File.ReadAllBytes(targetFile);

            foreach (ReplacerHelper replacerHelper in toReplace)
            {
                byte[] seeker = GetByteArray(replacerHelper.Search);
                byte[] hider = GetByteArray(replacerHelper.Replace);

                for (int i = 0; i < fileContent.Length; i++)
                {
                    if (!findHex(fileContent, i, seeker)) continue;

                    for (int j = 0; j < seeker.Length; j++)
                    {
                        fileContent[i + j] = hider[j];
                    }
                }
            }
            File.WriteAllBytes(resultFile, fileContent);
        }
    }
}