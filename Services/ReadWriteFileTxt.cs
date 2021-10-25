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
        static char[] charsToTrim = { ' ', '\n', '\r', '\''};
        private static readonly object _locker = new object();
        private static readonly UTF8Encoding encoder = new UTF8Encoding(false);

       public static List<string> ReadFile(string filePath)
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<string> fileContentList = new List<string>();

            StreamReader fileStream = new StreamReader(filePath, encoder);//Encoding.GetEncoding("Windows-1251")

            while (!fileStream.EndOfStream)
            {
                string str = fileStream.ReadLine().Trim().Trim(charsToTrim);
                if(str?.Length > 0) fileContentList.Add(str);                
            }
            fileStream.Close();
            return fileContentList;
        }

        public static string WriteFile(List<string> listWrite, string path, string nameFile, string typeFile, string newpath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo newFolder = dirInfo.Parent.CreateSubdirectory(newpath);
            try
            {
                if (!newFolder.Exists)
                {
                    newFolder.Create();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            string fullPath = Path.Combine(newFolder.FullName, $"{nameFile}.{typeFile}");
            // This text is added only once to the file.
            //try
            //{
            //    lock (_locker)
            //    {
            //        if (!File.Exists(fullPath))
            //        {
            //            using (StreamWriter writer = new StreamWriter(fullPath, false, encoder))
            //            {

            //                foreach (string str in listWrite)
            //                {
            //                    writer.WriteLine(str);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            using (StreamWriter writer = new StreamWriter(fullPath, true, encoder))
            //            {
            //                foreach (string str in listWrite)
            //                {
            //                    writer.WriteLine(str);
            //                }
            //            }
            //        }
            //    }
            //    return fullPath;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    return fullPath;
            //}
            lock (_locker)
            {
                try
                {
                    File.AppendAllLines(fullPath,listWrite);
                    return fullPath;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }

        }

        public static string WriteFile(string str, string path, string nameFile, string typeFile, string newpath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo newFolder = dirInfo.Parent.CreateSubdirectory(newpath);            

            string fullPath = Path.Combine(newFolder.FullName, $"{nameFile}.{typeFile}");
            // This text is added only once to the file.

            try
            {
                if (!newFolder.Exists)
                {
                    newFolder.Create();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message);  Console.WriteLine(ex.Message); }

            //try
            //{
            //    lock (_locker)
            //    {

            //        if (!File.Exists(fullPath))
            //        {
            //            using (StreamWriter writer = new StreamWriter(fullPath, false, encoder))
            //            {
            //                writer.WriteLine(str);
            //            }
            //        }
            //        else
            //        {

            //            using (StreamWriter writer = new StreamWriter(fullPath, true, encoder))
            //            {
            //                writer.WriteLine(str);
            //            }

            //        }
            //    }
            //    return fullPath;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    return null;
            //}
            lock (_locker)
            {
                try
                {
                    File.AppendAllText(fullPath, str + Environment.NewLine);
                    return fullPath;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
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

        public static void DeleteFile(string pathFull)
        {
            try
            {
                File.Delete(pathFull);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    
}
