using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool_Utility.Encryption;

namespace Application.Common.Utility.FTP
{
    public static class FileManager
    {
        public static string GetFileLink(Guid FileID, Guid UserID)
        {
     
            return (FileID.ToString().Replace("-", "") + "," + UserID.ToString().Replace("-", "") + "," +
                    DateTime.Now.AddMinutes(5.0).ToString("s")).base64Encrypt();
        }
    }
}