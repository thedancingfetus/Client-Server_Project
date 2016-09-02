using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client_Test
{
    class Program
    {
        static string folderPath = "Files\\";
        static string hashForFiles = CreateMd5ForFolder(folderPath);

        // Made a method sourced from too many pages to count.  Basicly send it the name of the server. 
        // Right now the TCP Port is Hard set to 13000 in this method.  Probably should make that read from an INI file
        static void Connect(String server, string checksum)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;
                TcpClient client = new TcpClient(server, port);

                // this idea was sourced from http://technotif.com/creating-simple-tcpip-server-client-transfer-data-using-c-vb-net/
                int blockSize = 1024;
                int thisRead = 0;
                Byte[] data = new byte[blockSize];

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();
                byte[] sendChecksumBuffer = GetBytes(checksum);
                stream.Write(sendChecksumBuffer, 0, sendChecksumBuffer.GetLength(0));
                // Recieve what the server is sending and output to a file
                /*
                Stream fileStream = File.OpenWrite("Test.zip");
                while(true)
                {
                    thisRead = stream.Read(data, 0, blockSize);
                    fileStream.Write(data, 0, thisRead);
                    if (thisRead == 0)
                    {
                        break;
                    }
                }
                */

                // Close everything.
                //fileStream.Close();
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        static public float[] GimmieDatFloat(string ip)
        {
            Int32 port = 13000;
            TcpClient client = new TcpClient(ip, port);
            NetworkStream stream = client.GetStream();
            int blockSize = 1024;
            int thisRead;
            int currentSize;
            int offSet = 0;
            byte[] receiveBytes = new byte[blockSize];
            int count = 1;
            //Console.Write("Starting Read");
            //Array Way
            /*while ((thisRead = stream.Read(receiveBytes, offSet, blockSize)) > 0)
            {
                Console.WriteLine("Loop: " + count);
                currentSize = receiveBytes.Length;
                byte[] temp = new byte[currentSize];
                Console.WriteLine("Made Temp " + temp.Length);
                Buffer.BlockCopy(receiveBytes, 0, temp, 0, receiveBytes.Length);
                Console.WriteLine("Copied receive into Temp");
                receiveBytes = new byte[currentSize + blockSize];
                Console.WriteLine("New Received " + receiveBytes.Length);
                Buffer.BlockCopy(temp, 0, receiveBytes, 0, currentSize);
                Console.WriteLine("Copied from Temp");
                Console.WriteLine("Copied buffer to received");
                offSet += blockSize;
                count++;
            }*/
            //Memory Stream Way
            using (MemoryStream ms = new MemoryStream())
            {
                while ((thisRead = stream.Read(receiveBytes, offSet, blockSize)) > 0)
                {
                    ms.Write(receiveBytes, 0, thisRead);
                }
                receiveBytes = ms.ToArray();
            }
            stream.Close();
           /* int i = receiveBytes.Length - 1;
            while(receiveBytes[i] == 0)
            {
                --i;
            }
            byte[] trimmedBytes = new byte[i + 1];
            Console.WriteLine("trimmedBytes.Length in Function: " + trimmedBytes.Length);
            Buffer.BlockCopy(receiveBytes, 0, trimmedBytes, 0, trimmedBytes.Length);*/
            float[] datFloat = new float[receiveBytes.Length / 4];
            Buffer.BlockCopy(receiveBytes, 0, datFloat, 0, receiveBytes.Length);
            Console.WriteLine("datFloat.Length in Function: " + datFloat.Length);
            return datFloat;
        }

        static void Main()
        {
            string checksum = ChecksumCheck(hashForFiles);
            Console.WriteLine(checksum);
            // This is the client app.  If you hit enter it invokes the Connect method.  
            // The server name in this example is hard set to 'localhost'.  Probably should pull that from an INI            
            Console.WriteLine("Enter Server Name or IP (If you don't have your server in DNS, use IP):");
            string server = "10.10.11.25";
            bool running = true;
            while (running == true)
            {
                Console.WriteLine("Hit Enter to get dat float (Type 'q!' to quit): ");
                string input = Console.ReadLine();
                if (input != "q!")
                {
                    try {

                        float[] datFloat = GimmieDatFloat(server);
                        Console.WriteLine("datFloat.Length: " + datFloat.Length);
                        Console.WriteLine("datFloat[320]: " + datFloat[320]);
                        Console.WriteLine("datFloat[4530]: " + datFloat[4530]);
                    }
                    catch {
                        Console.WriteLine("Server you entered wasn't found, try again (Type 'q!' to quit): ");
                        server = Console.ReadLine();
                        continue;
                    }
                }
                else
                {
                    running = false;
                }    
            }            
        }

        static float[] NormalizeArray(float[] ar)
        {
            int count = ar.Count(i => i != 0);
            float[] normalized = new float[count];
            int nomalNum = 0;
            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i] != 0)
                {
                    normalized[nomalNum] = ar[i];
                    nomalNum++;
                }
            }
            return normalized;
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
                    File.WriteAllText(hashFile, currentHash, Encoding.Default);
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
}
