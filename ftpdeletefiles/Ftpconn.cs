using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ftpdeletefiles
{
    class Ftpconn
    {
        public static Dictionary<string, DateTime> fdetail;
        public static Dictionary<string, DateTime> Sfdetail;

        public static long TestMeth()
        {
            try
            {
                // Get the object used to communicate with the server.
                //ftp:/servername/path/folder/
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://server/path/folder");
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential("username", "password");

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                List<string> editstr = new List<string>();

                //read each line from FTP folder, store file details (Date|Time|Size|FileName) in list after removing extra space between each column
                while (!reader.EndOfStream)
                {
                    string strTemp = reader.ReadLine();
                    strTemp = Regex.Replace(strTemp, @"\s+", "|");
                    editstr.Add(strTemp);
                }

                reader.Close();
                response.Close();

                long size = 0;
                long totalsize = 0;

                fdetail = new Dictionary<string, DateTime>();
                Sfdetail = new Dictionary<string, DateTime>();

                //calculate total folder size + create list of oldest files to be deleted from the folder 
                foreach (var line in editstr)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        size = Convert.ToInt64(line.Split('|')[2]);
                        totalsize += size;
                        string dt = string.Format("{0:MM-dd-yy}", line.Split('|')[0]);
                        DateTime temp = DateTime.ParseExact(dt, "MM-dd-yy", CultureInfo.InvariantCulture);
                        DateTime d = DateTime.ParseExact(temp.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        fdetail.Add(line.Split('|')[3], d);
                    }
                }
                //sorted list of files from oldest to newest
                foreach (var fd in fdetail.OrderBy(d => d.Value))
                {
                    Sfdetail.Add(fd.Key, fd.Value);
                }

                return totalsize;
            }
            catch (Exception exp)
            {
                throw new Exception("Failed to find folder size: " + exp);
            }
        }

        private static FtpWebRequest Create(string v)
        {
            throw new NotImplementedException();
        }

        //keep deleting files till it reaches the defined size
        public static bool DeleteFtpFiles(long size)
        {
            try
            {
                while (size > 32000000000)
                {
                    foreach (var fd in Sfdetail.Take(250))
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://server/path/folder/" + fd.Key);

                        //network credentials
                        request.Credentials = new NetworkCredential("username", "password");

                        request.Method = WebRequestMethods.Ftp.DeleteFile;
                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        response.Close();
                    }

                    size = Ftpconn.TestMeth();
                }

                return true;
            }
            catch (Exception dexp)
            {
                throw new Exception("Failed to delete file: " + dexp);

            }
        }
    }
}
