using System;
using System.IO;
using System.Net;
using System.Net.Mime;

namespace BlueWind.Crawler.Core
{
    public static class HttpUtility
    {
        public static Stream GetResponse(string uri)
        {
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch
            {
                return null;
            }
            request.MaximumAutomaticRedirections = 4;
            request.MaximumResponseHeadersLength = 4;
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    responseStream = response.GetResponseStream();
                }
            }
            catch (Exception ex)
            {
            }

            return responseStream;
        }
        public static Stream PostResponse(string url, string raw)
        {
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }
            catch
            {
                return null;
            }
            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());

            try
            {
                requestWriter.Write(raw);
            }
            finally
            {
                requestWriter.Close();
                requestWriter = null;
            }

            HttpWebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    responseStream = response.GetResponseStream();
                }
            }
            catch (Exception ex)
            {
            }

            return responseStream;
        }
        public static bool GetResponseAndWriteFile(string url, string path)
        {
            Stream stream = GetResponse(url);
            if (stream != null)
            {
                return stream.WriteToBinaryFile(path);
            }
            return false;
        }
        public static byte[] ReadToEnd(this Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
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
        public static bool WriteToBinaryFile(this Stream buffer, string path)
        {
            try
            {
                File.WriteAllBytes(path, buffer.ReadToEnd());
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }
        public static string GetResponseString(string url)
        {
            Stream stream = GetResponse(url);
            if (stream != null)
            {
                try
                {
                    return new StreamReader(stream).ReadToEnd();
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }
        public static string PostResponseString(string url,string raw)
        {
            Stream stream = PostResponse(url,raw);
            if (stream != null)
                return new StreamReader(stream).ReadToEnd();
            return "";
        }
    }
}