using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLmessanger.Services
{
    public class ReadWriteFileTxt
    {
        static char[] charsToTrim = { ' ', '\n', '\r', '\'', '\'' };

        public static List<string> ReadFile(string filePath)
        {
            // если закомментировать, то будет исключение
            // System.ArgumentException: ''windows-1251' is not a supported encoding name. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method. Parameter name: name'
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<String> fileContentList = new List<string>();

            StreamReader fileStream = new StreamReader(filePath, Encoding.GetEncoding("Windows-1251"));//Encoding.GetEncoding("Windows-1251")

            while (!fileStream.EndOfStream)
            {
                //Encoding utf = Encoding.UTF8;
                //Encoding win = Encoding.GetEncoding(1251);

                //byte[] winArr = win.GetBytes(fileStream.ReadLine());
                //byte[] utfArr = Encoding.Convert(utf, win, winArr);
                //string str = utf.GetString(utfArr);

                fileContentList.Add(fileStream.ReadLine().Trim(charsToTrim));
            }
            return fileContentList;
        }

        public static void WriteFile(List<string> listWrite, string path, string nameFile, string typeFile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo newFolder = dirInfo.Parent.CreateSubdirectory("Logs");
            if (!newFolder.Exists)
            {
                newFolder.Create();
            }
            // $"{dirInfo.FullName}\\{nameFile}.{typeFile}"
            using (FileStream fileStream = new FileStream($"{newFolder.FullName}\\{nameFile}.{typeFile}", FileMode.CreateNew))
            {
                StreamWriter writer = new StreamWriter(fileStream, Encoding.GetEncoding("Windows-1251"));

                foreach (string str in listWrite)
                {
                    writer.WriteLine(str);
                }
                writer.Close();
            }
        }
        public static List<string[]> SplitString(List<string> stringList, char parserChar)
        {
            List<string[]> splitStringList = new List<string[]>(stringList.Count);

            for (int i = 0; i < stringList.Count; i++)
            {
                string[] valueColumns = stringList[i].Split(parserChar);
                splitStringList.Add(valueColumns);
            }

            return splitStringList;
        }
    }
}
