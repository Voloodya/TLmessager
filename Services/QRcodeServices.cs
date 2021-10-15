using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TLmessanger.Services
{
    public class QRcodeServices
    {

        public static Bitmap CreateBitmapFromBytes(Byte[] bytesQRcode)
        {
            QRCodeData qrCodeData = new QRCodeData(bytesQRcode, QRCodeData.Compression.Uncompressed);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            return qrCodeImage;
        }

        public static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            using (var ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        }
}
