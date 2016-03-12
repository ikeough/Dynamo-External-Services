using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Team;
using ExternalServiceInterfaces;

namespace DropboxService
{
    public class DropBoxService : IOAuthService
    {
        private string oauth2State;

        #region public methods

        public string Name
        {
            get { return "Dropbox"; }
        }

        public IOAuthAuthenticationData AuthenticationData { get; private set; }

        public Uri AuthorizationUri { get; }

        public Uri RedirectUri { get; private set; }

        public string ApiKey { get; private set; }

        public DropBoxService()
        {
            var redirectVar = Environment.GetEnvironmentVariable("DROPBOX_REDIRECT_URI");
            RedirectUri = new Uri(string.IsNullOrEmpty(redirectVar)?"":redirectVar);

            ApiKey = Environment.GetEnvironmentVariable("DROPBOX_API_KEY");
            this.oauth2State = Guid.NewGuid().ToString("N");
            AuthorizationUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, ApiKey, RedirectUri, oauth2State);
        }

        public object GetClient()
        {
            InitializeCertPinning();

            // Specify socket level timeout which decides maximum waiting time when on bytes are
            // received by the socket.
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                // Specify request level timeout which decides maximum time taht can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(20)
            };

            try
            {
                var config = new DropboxClientConfig("Dynamo")
                {
                    HttpClient = httpClient
                };

                var client = new DropboxClient(AuthenticationData.AccessToken, config);
                return client;
            }
            catch (HttpException e)
            {
                Console.WriteLine("Exception reported from RPC layer");
                Console.WriteLine("    Status code: " + e.StatusCode);
                Console.WriteLine("    Message    : " + e.Message);
                if (e.RequestUri != null)
                {
                    Console.WriteLine("    Request uri: {0}", e.RequestUri);
                }
            }

            return null;
        }

        public Action Authenticate { get; set; }

        public void ParseRedirectResponse(Uri uri)
        {
            var result = DropboxOAuth2Helper.ParseTokenFragment(uri);
            if (result.State != this.oauth2State)
            {
                return;
            }

            AuthenticationData = new OAuthAuthenticationData(result.AccessToken, result.Uid);
        }

        /// <summary>
        /// Gets the dropbox access token.
        /// <para>
        /// This fetches the access token from the applications settings, if it is not found there
        /// (or if the user chooses to reset the settings) then the UI in <see cref="LoginForm"/> is
        /// displayed to authorize the user.
        /// </para>
        /// </summary>
        /// <returns>A valid access token or null.</returns>
        public void Login()
        {
            try
            {
                if (AuthenticationData != null)
                {
                    return;
                }

                Authenticate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Logout()
        {
            if (AuthenticationData != null)
            {
                AuthenticationData = null;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Run tests for user-level operations.
        /// </summary>
        /// <param name="client">The Dropbox client.</param>
        /// <returns>An asynchronous task.</returns>
        private async Task RunUserTests(DropboxClient client)
        {
            await GetCurrentAccount(client);

            var path = "/DotNetApi/Help";
            var folder = await CreateFolder(client, path);
            var list = await ListFolder(client, path);

            var firstFile = list.Entries.FirstOrDefault(i => i.IsFile);
            if (firstFile != null)
            {
                await Download(client, path, firstFile.AsFile);
            }

            await Upload(client, path, "Test.txt", "This is a text file");

            await ChunkUpload(client, path, "Binary");
        }

        /// <summary>
        /// Run tests for team-level operations.
        /// </summary>
        /// <param name="client">The Dropbox client.</param>
        /// <returns>An asynchronous task.</returns>
        private async Task RunTeamTests(DropboxTeamClient client)
        {
            var members = await client.Team.MembersListAsync();

            var member = members.Members.FirstOrDefault();

            if (member != null)
            {
                // A team client can perform action on a team member's behalf. To do this,
                // just pass in team member id in to AsMember function which returns a user client.
                // This client will operates on this team member's Dropbox.
                var userClient = client.AsMember(member.Profile.TeamMemberId);
                await RunUserTests(userClient);
            }
        }

        /// <summary>
        /// Initializes ssl certificate pinning.
        /// </summary>
        private void InitializeCertPinning()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var root = chain.ChainElements[chain.ChainElements.Count - 1];
                var publicKey = root.Certificate.GetPublicKeyString();

                return DropboxCertHelper.IsKnownRootCertPublicKey(publicKey);
            };
        }

        /// <summary>
        /// Gets information about the currently authorized account.
        /// <para>
        /// This demonstrates calling a simple rpc style api from the Users namespace.
        /// </para>
        /// </summary>
        /// <param name="client">The Dropbox client.</param>
        /// <returns>An asynchronous task.</returns>
        private async Task GetCurrentAccount(DropboxClient client)
        {
            var full = await client.Users.GetCurrentAccountAsync();

            Console.WriteLine("Account id    : {0}", full.AccountId);
            Console.WriteLine("Country       : {0}", full.Country);
            Console.WriteLine("Email         : {0}", full.Email);
            Console.WriteLine("Is paired     : {0}", full.IsPaired ? "Yes" : "No");
            Console.WriteLine("Locale        : {0}", full.Locale);
            Console.WriteLine("Name");
            Console.WriteLine("  Display  : {0}", full.Name.DisplayName);
            Console.WriteLine("  Familiar : {0}", full.Name.FamiliarName);
            Console.WriteLine("  Given    : {0}", full.Name.GivenName);
            Console.WriteLine("  Surname  : {0}", full.Name.Surname);
            Console.WriteLine("Referral link : {0}", full.ReferralLink);

            if (full.Team != null)
            {
                Console.WriteLine("Team");
                Console.WriteLine("  Id   : {0}", full.Team.Id);
                Console.WriteLine("  Name : {0}", full.Team.Name);
            }
            else
            {
                Console.WriteLine("Team - None");
            }
        }

        /// <summary>
        /// Creates the specified folder.
        /// </summary>
        /// <remarks>This demonstrates calling an rpc style api in the Files namespace.</remarks>
        /// <param name="path">The path of the folder to create.</param>
        /// <param name="client">The Dropbox client.</param>
        /// <returns>The result from the ListFolderAsync call.</returns>
        private async Task<FolderMetadata> CreateFolder(DropboxClient client, string path)
        {
            Console.WriteLine("--- Creating Folder ---");
            var folderArg = new CreateFolderArg(path);
            var folder = await client.Files.CreateFolderAsync(folderArg);

            Console.WriteLine("Folder: " + path + " created!");

            return folder;
        }

        /// <summary>
        /// Lists the items within a folder.
        /// </summary>
        /// <remarks>This demonstrates calling an rpc style api in the Files namespace.</remarks>
        /// <param name="path">The path to list.</param>
        /// <param name="client">The Dropbox client.</param>
        /// <returns>The result from the ListFolderAsync call.</returns>
        private async Task<ListFolderResult> ListFolder(DropboxClient client, string path)
        {
            Console.WriteLine("--- Files ---");
            var list = await client.Files.ListFolderAsync(path);

            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                Console.WriteLine("D  {0}/", item.Name);
            }

            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                var file = item.AsFile;

                Console.WriteLine("F{0,8} {1}",
                    file.Size,
                    item.Name);
            }

            if (list.HasMore)
            {
                Console.WriteLine("   ...");
            }
            return list;
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <remarks>This demonstrates calling a download style api in the Files namespace.</remarks>
        /// <param name="client">The Dropbox client.</param>
        /// <param name="folder">The folder path in which the file should be found.</param>
        /// <param name="file">The file to download within <paramref name="folder"/>.</param>
        /// <returns></returns>
        private async Task Download(DropboxClient client, string folder, FileMetadata file)
        {
            Console.WriteLine("Download file...");

            using (var response = await client.Files.DownloadAsync(folder + "/" + file.Name))
            {
                Console.WriteLine("Downloaded {0} Rev {1}", response.Response.Name, response.Response.Rev);
                Console.WriteLine("------------------------------");
                Console.WriteLine(await response.GetContentAsStringAsync());
                Console.WriteLine("------------------------------");
            }
        }

        /// <summary>
        /// Uploads given content to a file in Dropbox.
        /// </summary>
        /// <param name="client">The Dropbox client.</param>
        /// <param name="folder">The folder to upload the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The file content.</param>
        /// <returns></returns>
        private async Task Upload(DropboxClient client, string folder, string fileName, string fileContent)
        {
            Console.WriteLine("Upload file...");

            using (var stream = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(fileContent)))
            {
                var response = await client.Files.UploadAsync(folder + "/" + fileName, WriteMode.Overwrite.Instance, body: stream);

                Console.WriteLine("Uploaded Id {0} Rev {1}", response.Id, response.Rev);
            }
        }

        /// <summary>
        /// Uploads a big file in chunk. The is very helpful for uploading large file in slow network condition
        /// and also enable capability to track upload progerss.
        /// </summary>
        /// <param name="client">The Dropbox client.</param>
        /// <param name="folder">The folder to upload the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns></returns>
        private async Task ChunkUpload(DropboxClient client, string folder, string fileName)
        {
            Console.WriteLine("Chunk upload file...");
            // Chunk size is 128KB.
            const int chunkSize = 128 * 1024;

            // Create a random file of 1MB in size.
            var fileContent = new byte[1024 * 1024];        
            new Random().NextBytes(fileContent);

            using (var stream = new MemoryStream(fileContent))
            {
                int numChunks = (int)Math.Ceiling((double)stream.Length / chunkSize);

                byte[] buffer = new byte[chunkSize];
                string sessionId = null;

                for (var idx = 0; idx < numChunks; idx++)
                {
                    Console.WriteLine("Start uploading chunk {0}", idx);
                    var byteRead = stream.Read(buffer, 0, chunkSize);

                    using (MemoryStream memStream = new MemoryStream(buffer, 0, byteRead))
                    {
                        if (idx == 0)
                        {
                            var result = await client.Files.UploadSessionStartAsync(memStream);
                            sessionId = result.SessionId;
                        }

                        else
                        {
                            UploadSessionCursor cursor = new UploadSessionCursor(sessionId, (ulong)(chunkSize * idx));

                            if (idx == numChunks - 1)
                            {
                                await client.Files.UploadSessionFinishAsync(cursor, new CommitInfo(folder + "/" + fileName), memStream);
                            }

                            else
                            {
                                await client.Files.UploadSessionAppendAsync(cursor, memStream);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// List all members in the team.
        /// </summary>
        /// <param name="client">The Dropbox team client.</param>
        /// <returns>The result from the MembersListAsync call.</returns>
        private async Task<MembersListResult> ListTeamMembers(DropboxTeamClient client)
        {
            var members = await client.Team.MembersListAsync();

            foreach (var member in members.Members)
            {
                Console.WriteLine("Member id    : {0}", member.Profile.TeamMemberId);
                Console.WriteLine("Name         : {0}", member.Profile.Name);
                Console.WriteLine("Email        : {0}", member.Profile.Email);
            }

            return members;
        }
        
        #endregion
    }

    public class OAuthAuthenticationData : IOAuthAuthenticationData
    {
        public string AccessToken { get; }
        public string CurrentUserName { get; }

        public OAuthAuthenticationData(string accessToken, string currentUserName)
        {
            AccessToken = accessToken;
            CurrentUserName = currentUserName;
        }
    }
}
