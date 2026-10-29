using System.Buffers.Binary;

namespace BlogApp.Application.Features.Media;

public static class ImageHeaderReader
{
    public static (int Width, int Height) TryGetDimensions(Stream stream, string extension)
    {
        try
        {
            var originalPos = stream.CanSeek ? stream.Position : 0;
            if (stream.CanSeek) stream.Position = 0;

            byte[] header = new byte[64];
            int read = stream.Read(header, 0, header.Length);
            if (stream.CanSeek) stream.Position = originalPos;

            if (read < 10) return (0, 0);

            var ext = extension.ToLowerInvariant();
            if (ext is ".png" && read >= 24)
            {
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                {
                    int width = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4));
                    int height = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4));
                    return (width, height);
                }
            }

            if (ext is ".gif" && read >= 10)
            {
                if (header[0] == 'G' && header[1] == 'I' && header[2] == 'F')
                {
                    int width = BinaryPrimitives.ReadInt16LittleEndian(header.AsSpan(6, 2));
                    int height = BinaryPrimitives.ReadInt16LittleEndian(header.AsSpan(8, 2));
                    return (width, height);
                }
            }

            if (ext is ".jpg" or ".jpeg" && stream.CanSeek)
            {
                stream.Position = 0;
                var jpegDims = TryGetJpegDimensions(stream);
                stream.Position = originalPos;
                if (jpegDims.HasValue) return jpegDims.Value;
            }

            return (0, 0);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static (int Width, int Height)? TryGetJpegDimensions(Stream stream)
    {
        int b1 = stream.ReadByte();
        int b2 = stream.ReadByte();
        if (b1 != 0xFF || b2 != 0xD8) return null; // Not JPEG

        while (stream.Position < stream.Length)
        {
            int marker = stream.ReadByte();
            if (marker != 0xFF) continue;

            int type = stream.ReadByte();
            while (type == 0xFF) type = stream.ReadByte(); // Padding

            if (type is 0xD8 or 0xD9 or 0x00) continue; // SOI, EOI, escaped FF

            byte[] lenBytes = new byte[2];
            if (stream.Read(lenBytes, 0, 2) < 2) return null;
            int length = BinaryPrimitives.ReadUInt16BigEndian(lenBytes);

            // SOF0..SOF3, SOF5..SOF7, SOF9..SOF11, SOF13..SOF15
            if (type is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
            {
                stream.ReadByte(); // Precision
                byte[] dimBytes = new byte[4];
                if (stream.Read(dimBytes, 0, 4) < 4) return null;
                int height = BinaryPrimitives.ReadUInt16BigEndian(dimBytes.AsSpan(0, 2));
                int width = BinaryPrimitives.ReadUInt16BigEndian(dimBytes.AsSpan(2, 2));
                return (width, height);
            }

            if (length > 2)
            {
                stream.Seek(length - 2, SeekOrigin.Current);
            }
        }

        return null;
    }
}
