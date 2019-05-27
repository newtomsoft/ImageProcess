using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;

class MimeType
{
    [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
    private static extern uint FindMimeFromData(
    IntPtr pBC,
    [MarshalAs(UnmanagedType.LPStr)] string pwzUrl,
    [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
    uint cbSize,
    [MarshalAs(UnmanagedType.LPStr)] string pwzMimeProposed,
    uint dwMimeFlags,
    out uint ppwzMimeOut,
    uint dwReserverd
    );

    public static string getFromFile(string filename)
    {
        const string genericMime = "application/octet-stream";
        const string webpMime = "image/webp";
        const string jpeg2000Mime = "image/jp2";

        if (!File.Exists(filename))
            throw new FileNotFoundException(filename + " not found");

        byte[] buffer = new byte[256];
        using (FileStream fs = new FileStream(filename, FileMode.Open))
        {
            if (fs.Length >= 256)
                fs.Read(buffer, 0, 256);
            else
                fs.Read(buffer, 0, (int)fs.Length);
        }
        try
        {
            uint mimetype;
            FindMimeFromData((IntPtr)0, null, buffer, 256, null, 0, out mimetype, 0);
            IntPtr mimeTypePtr = new IntPtr(mimetype);
            string mime = Marshal.PtrToStringUni(mimeTypePtr);
            Marshal.FreeCoTaskMem(mimeTypePtr);
            // TODO clean code
            if (mime == genericMime)
            {
                byte b0 = buffer[0];
                byte b1 = buffer[1];
                byte b2 = buffer[2];
                byte b3 = buffer[3];
                byte b4 = buffer[4];
                byte b5 = buffer[5];
                byte b6 = buffer[6];
                byte b7 = buffer[7];
                byte b8 = buffer[8];
                byte b9 = buffer[9];
                byte b10 = buffer[10];
                byte b11 = buffer[11];
                byte b16 = buffer[16];
                byte b17 = buffer[17];
                byte b18 = buffer[18];
                byte b19 = buffer[19];
                byte b20 = buffer[20];
                byte b21 = buffer[21];
                byte b22 = buffer[22];
                if ( b0 == 'R' && b1 == 'I' && b2 == 'F' && b3 == 'F' &&
                    b8 == 'W' && b9 == 'E' && b10 == 'B' && b11 == 'P')
                {
                    mime = webpMime;
                    return mime;
                }
                if (b4 == 'j' && b5 == 'P' && b6 == ' ' && b7 == ' ' &&
                    b16 == 'f' && b17 == 't' && b18 == 'y' && b19 == 'p' && b20 == 'j' && b21 == 'p' && b22 == '2')
                {
                    mime = jpeg2000Mime;
                    return mime;
                }
            }
            return mime;
        }
        catch (Exception)
        {
            return genericMime;
        }
    }
}

