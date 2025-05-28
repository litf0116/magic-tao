using System;
using System.IO;

namespace TtWork.Lib;

public static class FileExt
{
    private static int CHUNK_SIZE = 1 << 22; // size of 4MB

    public static byte[] sha1(byte[] data)
    {
        return System.Security.Cryptography.SHA1.Create().ComputeHash(data);
    }

    public static string urlSafeBase64Encode(byte[] data)
    {
        var encodedString = Convert.ToBase64String(data);
        encodedString = encodedString.Replace('+', '-').Replace('/', '_');
        return encodedString;
    }

    public static String CalcETag(this byte[] bytes)
    {
        var etag = string.Empty;
        long fileLength = bytes.Length;
        if (fileLength <= CHUNK_SIZE)
        {
            byte[] sha1Data = sha1(bytes);
            int sha1DataLen = sha1Data.Length;
            byte[] hashData = new byte[sha1DataLen + 1];
            System.Array.Copy(sha1Data, 0, hashData, 1, sha1DataLen);
            hashData[0] = 0x16;
            etag = urlSafeBase64Encode(hashData);
        }
        else
        {
            using var ms = new MemoryStream(bytes);
            int chunkCount = (int)(fileLength / CHUNK_SIZE);
            if (fileLength % CHUNK_SIZE != 0)
            {
                chunkCount += 1;
            }

            byte[] allSha1Data = new byte[0];
            for (int i = 0; i < chunkCount; i++)
            {
                byte[] chunkData = new byte[CHUNK_SIZE];
                int bytesReadLen = ms.Read(chunkData, 0, CHUNK_SIZE);
                byte[] bytesRead = new byte[CHUNK_SIZE];
                System.Array.Copy(chunkData, 0, bytesRead, 0, CHUNK_SIZE);
                byte[] chunkDataSha1 = sha1(bytesRead);
                byte[] newAllSha1Data = new byte[chunkDataSha1.Length
                                                 + allSha1Data.Length];
                System.Array.Copy(allSha1Data, 0, newAllSha1Data, 0,
                    allSha1Data.Length);
                System.Array.Copy(chunkDataSha1, 0, newAllSha1Data,
                    allSha1Data.Length, chunkDataSha1.Length);
                allSha1Data = newAllSha1Data;
            }

            byte[] allSha1DataSha1 = sha1(allSha1Data);
            byte[] hashData = new byte[allSha1DataSha1.Length + 1];
            System.Array.Copy(allSha1DataSha1, 0, hashData, 1,
                allSha1DataSha1.Length);
            hashData[0] = (byte)0x96;
            etag = urlSafeBase64Encode(hashData);
        }

        return etag;
    }
}