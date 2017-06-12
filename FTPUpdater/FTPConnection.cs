/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 10/11/2015
 * Time: 17:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace FTPHashedUpdater
{
  public class FTPConnection
  {
    public string Host { get; private set; }
    public string User { get; private set; }
    public string Pass { get; private set; }

    private FtpWebRequest _ftpRequest = null;
    private FtpWebResponse _ftpResponse = null;
    private Stream _ftpStream = null;
    private const int _bufferSize = 2048;

    public FTPConnection(string hostIP, string userName, string password)
    {
      Host = hostIP;
      User = userName;
      Pass = password;
    }

    /// <summary>
    /// This downloads a specified File and passes any Progress to the provided ProgressDOs(if any)
    /// </summary>
    /// <param name="remoteFile">Remote file.</param>
    /// <param name="localFile">Local file.</param>
    /// <param name="progress">Progress.</param>
    /// <param name="total">Total-Progress.</param>
    public void DownloadFile(string remoteFile, string localFile, ProgressDO progress = null, ProgressDO total = null)
    {
      try
      {
        long totalsize = 0;
        if (progress != null)
        {
          totalsize = GetFileSize(remoteFile);
          progress.TotlalProgress = totalsize;
        }
        /* Create an FTP Request */
        CreateRequest(WebRequestMethods.Ftp.DownloadFile, remoteFile);
        /* Establish Return Communication with the FTP Server */
        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        {
          /* Get the FTP Server's Response Stream */
          using (_ftpStream = _ftpResponse.GetResponseStream())
          {
            /* Open a File Stream to Write the Downloaded File */
            using (FileStream localFileStream = new FileStream(localFile, FileMode.Create))
            {
              /* Buffer for the Downloaded Data */
              byte[] byteBuffer = new byte[_bufferSize];
              int bytesRead = _ftpStream.Read(byteBuffer, 0, _bufferSize);
              /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
              try
              {
                while (bytesRead > 0)
                {
                  localFileStream.Write(byteBuffer, 0, bytesRead);
                  if (progress != null)
                    progress.Progress += bytesRead;
                  if (total != null)
                    total.Progress += bytesRead;

                  bytesRead = _ftpStream.Read(byteBuffer, 0, _bufferSize);
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine(ex.ToString());
              }
              /* Resource Cleanup */
            }
          }
        }
        _ftpRequest = null;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return;
    }

    /// <summary>
    /// Determines the size of a File on the remote FTP-Server
    /// </summary>
    /// <returns>The file size.</returns>
    /// <param name="fileName">File name.</param>
    public long GetFileSize(string fileName)
    {
      try
      {
        CreateRequest(WebRequestMethods.Ftp.GetFileSize, fileName);
        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        {
          return _ftpResponse.ContentLength;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      /* Return an 0-filesize if an Exception Occurs */
      return 0;
    }

    /// <summary>
    /// Prepares the Request and saves it to _ftpRequest.
    /// </summary>
    /// <param name="method">Method to use</param>
    /// <param name="ftptarget">ftp-target-path without host</param>
    private void CreateRequest(String method, String ftptarget)
    {
      /* Create an FTP Request */
      _ftpRequest = (FtpWebRequest)FtpWebRequest.Create(@"ftp://" + Host + "/" + ftptarget);
      /* Log in to the FTP Server with the User Name and Password Provided */
      _ftpRequest.Credentials = new NetworkCredential(User, Pass);
      /* When in doubt, use these options */
      _ftpRequest.UseBinary = true;
      _ftpRequest.UsePassive = true;
      _ftpRequest.KeepAlive = true;
      /* Specify the Type of FTP Request */
      _ftpRequest.Method = method;
    }
  }
}
