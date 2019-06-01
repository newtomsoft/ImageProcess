using System;
using System.IO;
using System.Linq;
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
            // add mimes not known by FindMimeFromData
            if (mime == genericMime)
            {
                var buffer_0_3 = new ArraySegment<byte>(buffer, 0, 4);
                var buffer_4_7 = new ArraySegment<byte>(buffer, 4, 4);
                var buffer_8_11 = new ArraySegment<byte>(buffer, 8, 4);
                var buffer_16_22 = new ArraySegment<byte>(buffer, 16, 7);

                byte[] array_RIFF = { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
                byte[] array_WEBP = { (byte)'W', (byte)'E', (byte)'B', (byte)'P' };
                byte[] array_jP = { (byte)'j', (byte)'P', (byte)' ', (byte)' ' };
                byte[] array_ftypjp2 = { (byte)'f', (byte)'t', (byte)'y', (byte)'p', (byte)'j', (byte)'p', (byte)'2' };

                if (buffer_0_3.SequenceEqual(array_RIFF) && buffer_8_11.SequenceEqual(array_WEBP))
                {
                    return webpMime;
                }
                if (buffer_4_7.SequenceEqual(array_jP) && buffer_16_22.SequenceEqual(array_ftypjp2))
                {
                    return jpeg2000Mime;
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

