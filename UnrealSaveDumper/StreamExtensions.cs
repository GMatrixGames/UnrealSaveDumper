﻿namespace UnrealSaveDumper;

public static class StreamExtensions
{
    public static byte[] ReadToEnd(this Stream stream)
    {
        var originalPosition = 0L;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            var readBuffer = new byte[4096];

            var totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead != readBuffer.Length) continue;
                var nextByte = stream.ReadByte();
                if (nextByte == -1) continue;
                var temp = new byte[readBuffer.Length * 2];
                Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                Buffer.SetByte(temp, totalBytesRead, (byte) nextByte);
                readBuffer = temp;
                totalBytesRead++;
            }

            var buffer = readBuffer;
            if (readBuffer.Length == totalBytesRead) return buffer;
            buffer = new byte[totalBytesRead];
            Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);

            return buffer;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    public static async Task AsyncRead(this Stream stream, byte[] data, CancellationToken cancellationToken = default)
    {
        var ret = await stream.ReadAsync(data, cancellationToken);

        if (ret != data.Length)
        {
            Log.Error("Tried to read data into stream of length {DataLength}, but read only {Return}", data.Length, ret);
        }
    }
}