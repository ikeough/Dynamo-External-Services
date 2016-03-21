using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using ExternalServiceInterfaces;

namespace Dynamo.Dropbox
{
    public static class Dropbox
    {
        private const string ServiceName = "Dropbox";

        /// <summary>
        /// Download a file from Dropbox given a Dropbox path.
        /// </summary>
        /// <param name="dropboxPath">The path to the file in your dropbox.
        /// The path will be of the form: /folder/subfolder/file.txt</param>
        /// <returns>A path to the downloaded file on disk. This will be located in the current user's temp directory.
        /// An exception will be thrown if the user is not logged in to Dropbox.</returns>
        public static string DownloadFileByPath(string dropboxPath)
        {
            var task = Task.Run(()=> _DownloadFileByDropboxPath(dropboxPath));
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                var error = "";
                foreach (var innerEx in ex.InnerExceptions)
                {
                    error += innerEx.Message + "\n";
                }
                throw new Exception(error);
            }

            return task.Result;
        }

        private static async Task<string> _DownloadFileByDropboxPath(string dropboxPath)
        {
            var service = OAuthServices.Instance.GetServiceByName(ServiceName);
            if (service == null)
            {
                throw new Exception("The " + ServiceName + " service is not registered.");
            }

            if (service.AuthenticationData == null)
            {
                await service.LoginAsync();
            }

            var dropbox = service.Client as DropboxClient;

            if (dropbox == null)
            {
                throw new Exception("The " + ServiceName + " client could not be retrieved.");
            }

            try
            {
                var lastSlash = dropboxPath.LastIndexOf('/');
                var folderPath = dropboxPath.Remove(lastSlash);
                var fileName = dropboxPath.Substring(lastSlash + 1);

                var fileData = new List<Metadata>();

                var list = await dropbox.Files.ListFolderAsync(folderPath, true,false,true);
                do
                {
                    list = await dropbox.Files.ListFolderContinueAsync(list.Cursor);
                    fileData.AddRange(list.Entries.Where(m => m.IsFile));
                }
                while (list.HasMore);
                
                var file = list.Entries.FirstOrDefault(i => i.IsFile && i.PathLower == dropboxPath.ToLower(CultureInfo.InvariantCulture));
                if (file == null)
                {
                    throw new Exception("The request file could not be found.");
                }

                // Build a file path in the temp directory.
                var tempDir = Path.GetTempPath();
                var filePath = Path.Combine(tempDir, fileName);

                using (var response = await dropbox.Files.DownloadAsync(file.PathDisplay))
                {
                    var stream = await response.GetContentAsStreamAsync();
                    var fileStream = File.Create(filePath);
                    stream.CopyTo(fileStream);
                    fileStream.Close();
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error communicating with Dropbox :" + ex.StackTrace);
            }
            
        }
    }
}
