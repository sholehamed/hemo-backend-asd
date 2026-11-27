using System.Net;
using FluentFTP;

namespace Application.Common.Utility.FTP;

public class FTP
{
    private readonly string Host;
    private readonly string Password;
    private readonly int Port;
    private readonly string UserName;
    private IFtpClient FtpClient;


    public FTP(string host, string userName, string password, int port)
    {
        Host = host;
        UserName = userName;
        Password = password;
        Port = port;
    }


    private void Connect()
    {
        try
        {
            FtpClient = new FtpClient(Host);
            FtpClient.Credentials = new NetworkCredential
                (UserName, Password);
            FtpClient.Port = Port;

            FtpClient.Connect();

            SetWorkingDirectory("/");
        }
        catch (Exception)
        {
            throw new Exception("اتصال با سرور FTP برقرار نشد.");
        }
    }

    private void Disconnect()
    {
        FtpClient.Disconnect();
        if (FtpClient != null && !FtpClient.IsDisposed) FtpClient.Dispose();
    }

    public void TestServer()
    {
        Connect();
        Disconnect();
    }

    public bool RenameFile(string pathFolder, string path, string destFolder, string dest)
    {
        Connect();


        FtpClient.Rename(pathFolder + path, destFolder + dest);


        Disconnect();

        return true;
    }

    public bool RenameFile(string path, string dest)
    {
        Connect();


        FtpClient.Rename(path, dest);


        Disconnect();

        return true;
    }

    public bool UploadFile(Stream File, string FilePath, string FileName)
    {
        CreateDirectoryIfNotExists(FilePath);
        return UploadFile(File, Path.Combine(FilePath, FileName));
    }

    public bool UploadFile(Stream File, string FullPath)
    {
        Connect();


        var ftpStatus = FtpClient.UploadStream(File, FullPath);


        Disconnect();

        if (ftpStatus == FtpStatus.Success)
            return true;
        throw new Exception("بارگذاری فایل در سرور با خطا مواجه شد");
    }


    public Stream GetFileStream(string filePath)
    {
        var memoryStream = new MemoryStream();

        using (var ftpClient = new FtpClient(Host, UserName, Password))
        {
            ftpClient.Connect();
            ftpClient.DownloadStream(memoryStream, filePath);
            ftpClient.Disconnect();
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public void CreateDirectoryIfNotExists(string directoryPath)
    {
        Connect();
        if (!DirectoryExists(directoryPath))
        {
            var s = FtpClient.CreateDirectory(directoryPath);
        }

        Disconnect();
    }

    public void DeleteFileIfExists(string filePath)
    {
        Connect();
        if (FileExists(filePath)) FtpClient.DeleteFile(filePath);
        Disconnect();
    }

    public void DeleteDirectory(string directoryPath)
    {
        Connect();
        if (DirectoryExists(directoryPath)) FtpClient.DeleteDirectory(directoryPath);
        Disconnect();
    }


    private bool FileExists(string filePath)
    {
        return FtpClient.FileExists(filePath);
    }


    private bool DirectoryExists(string directoryPath)
    {
        return FtpClient.DirectoryExists(directoryPath);
    }


    private void SetWorkingDirectory(string directoryPath)
    {
        FtpClient.SetWorkingDirectory(directoryPath);
    }

    private void SetRootAsWorkingDirectory()
    {
        SetWorkingDirectory("");
    }
}