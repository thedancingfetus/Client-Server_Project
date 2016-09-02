using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

class MyTcpListener
{
    static string folderPath = "Files\\";
    static string hashForFiles = CreateMd5ForFolder(folderPath);
    
    public static void Main()
    {
        string checksum = ChecksumCheck(hashForFiles);
        Console.WriteLine(checksum);
        TcpListener server = null;
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 13000;
            // IP Address of the server
            IPAddress localAddr = IPAddress.Parse("10.10.11.25");

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // When connection is recieved:
                // Accept Client connection
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");
                NetworkStream stream = client.GetStream();

                float[] bigFloat = new float[50000];
                Random rand = new Random();
                for (int i = 0; i < bigFloat.Length; i++)
                {
                    int multiplyer = rand.Next();
                    bigFloat[i] = 1.456662f * multiplyer;
                }
                Console.Write("bigFLoat.Lenth: " + bigFloat.Length + "\n");
                Console.Write("bigFloat[319]: " + bigFloat[319] + "\n");
                Console.Write("bigFloat[320]: " + bigFloat[320] + "\n");
                Console.Write("bigFloat[4530]: " + bigFloat[4530] + "\n");

                byte[] sourceBytes = new byte[bigFloat.Length * 4];

                Console.WriteLine("sourceBytes.Length: " + sourceBytes.Length);

                Buffer.BlockCopy(bigFloat, 0, sourceBytes, 0, sourceBytes.Length);
                float[] test = new float[sourceBytes.Length / 4];

                Buffer.BlockCopy(sourceBytes, 0, test, 0, sourceBytes.Length);

                Console.Write("test.Lenth: " + test.Length + "\n");
                Console.Write("test[319]: " + test[319] + "\n");
                Console.Write("test[320]: " + test[320] + "\n");
                Console.Write("test[4530]: " + test[4530] + "\n");

                int blockSize = 1024;
                int chunks = (sourceBytes.Length + blockSize - 1) / blockSize;
                Console.WriteLine(chunks);
                int offset = 0;
                int streamLength = 0;
                while (chunks > 0)
                {
                    streamLength = sourceBytes.Length - offset;
                    if (streamLength >= blockSize)
                    {
                        streamLength = blockSize;
                    }
                    stream.Write(sourceBytes, offset, streamLength);
                    --chunks;
                    offset += blockSize;
                }
                stream.Close();
                Console.WriteLine("size of last streamed buffer: " + streamLength);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }


        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }
    
    static byte[] GetBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    static string GetString(byte[] bytes)
    {
        return Encoding.Unicode.GetString(bytes).TrimEnd('\0');
    }

    public static string ChecksumCheck(string currentHash)
    {
        string checksum;
        string hashFile = "checksum.txt";
        if (File.Exists(hashFile))
        {
            checksum = File.ReadAllText(hashFile);
            Console.WriteLine("Verifying Files are up to date");
            if (checksum != currentHash)
            {
                Console.WriteLine("Files changed");
                File.WriteAllText(hashFile, currentHash,Encoding.Default);
                checksum = currentHash;
            }
            else
            {
                Console.WriteLine("Files have not changed");
            }
        }
        else
        {
            File.WriteAllText(hashFile, hashForFiles, Encoding.Default);
            checksum = hashForFiles;
        }
        return checksum;
    }

    public static string CreateMd5ForFolder(string path)
    {
        // assuming you want to include nested folders
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                             .OrderBy(p => p).ToList();

        MD5 md5 = MD5.Create();

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];

            // hash path
            string relativePath = file.Substring(path.Length + 1);
            byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

            // hash contents
            byte[] contentBytes = File.ReadAllBytes(file);
            if (i == files.Count - 1)
                md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            else
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
        }

        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
    }
}