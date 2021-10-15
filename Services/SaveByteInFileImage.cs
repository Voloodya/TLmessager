using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLmessanger.Services
{
    public class SaveByteInFileImage
    {

        public static string SaveImage(Byte [] bytes, string path, string nameFile, string typeFile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo newFolder = dirInfo.Parent.CreateSubdirectory("ImagesQR");
            if (!newFolder.Exists)
            {
                newFolder.Create();
            }

            string fullPath = String.Format(@"{0}\{1}.{2}", newFolder.FullName, nameFile, typeFile);

            try
            {
                File.WriteAllBytes(fullPath, bytes);

                return fullPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
