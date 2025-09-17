namespace OpenGameSync.SDK
{
    /// <summary>
    /// Manages both remote and local saved files required for synchronization.<br/>
    /// Because of custom file classes and remote syncing, it is recommended 
    /// to inherit from <c>SyncManager</c> and provide a custom implementation.
    /// </summary>
    /// <typeparam name="T">
    /// The type of file being managed. Custom file classes can be used, 
    /// but must inherit from <see cref="File"/>.</typeparam>

    public class SyncManager<T> where T : File
    {
        /// <summary>
        /// The absolute local root directory where the file is stored.
        /// </summary>
        private string localPath;
        /// <summary>
        /// The absolute local root directory where the file is stored.
        /// </summary>
        protected string LocalPath
        {
            get
            {
                return localPath;
            }
        }
        /// <summary>
        /// A list of remote files. <br>
        /// Usally updated with <see cref="FetchRemote"/>.
        /// </summary>
        private List<T> remoteFiles = new List<T>();
        /// <summary>
        /// A list of local files. <br>
        /// Usally updated with <see cref="FetchLocal"/> .
        /// </summary>
        private List<T> localFiles = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncManager"/> class. 
        /// </summary>
        /// <param name="localPath">The absolute path of the local root directory.</param>
        public SyncManager(string localPath)
        {
            this.localPath = localPath;
        }

        /// <summary>
        /// Fetches the remote storage. <br>
        /// It updates the <see cref="remoteFiles"/> list.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task FetchRemote()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Fetches the local storage (at <see cref="localPath"/>). <br>
        /// It updates the <see cref="localFiles"/> list.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void FetchLocal()
        {
            var filePaths = GetFiles(localPath);
            localFiles.Clear();
            foreach (var filePath in filePaths)
            {
                var file = InitLocalFile(localPath, filePath.Substring(localPath.Length));
                localFiles.Add(
                    file
                );
            }
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/>. 
        /// <typeparamref name="T"/> must be derived from <see cref="File"/>. 
        /// If your custom file class defines a different constructor signature, override this method.
        /// </summary>
        /// <param name="localPath">The absolute path of the local root directory.</param>
        /// <param name="absolutePath">The absolute path of the file.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>

        protected virtual T InitLocalFile(string localPath, string relativePath)
        {
            return (T)Activator.CreateInstance(typeof(T), localPath, relativePath)!;
        }

        /// <summary>
        /// Clears the <see cref="remoteFiles"/> list.
        /// </summary>
        protected void ClearRemoteFiles()
        {
            remoteFiles.Clear();
        }


        /// <summary>
        /// Adds an file to the <see cref="remoteFiles"/> list.
        /// </summary>
        /// <param name="file"></param>
        protected void AddRemoteFile(T file)
        {
            remoteFiles.Add(file);
        }

        /// <summary>
        /// Synchronizes the local files to the remote storage.
        /// </summary>
        /// <remarks>
        /// Before calling this method, you must call <c>FetchLocal</c> to populate the local file list. 
        /// This method assumes that the file list has already been generated.
        /// </remarks>
        /// <returns>
        /// </returns>

        public async Task SyncToRemote()
        {
            await ClearRemote();
            var tasks = new List<Task>();
            foreach (var file in localFiles)
            {
                tasks.Add(UploadToRemote(file));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Synchronizes the remote files to the local storage.
        /// </summary>
        /// <remarks>
        /// Before calling this method, you must call <c>FetchRemote</c> to populate the remote file list. 
        /// This method assumes that the file list has already been generated.
        /// </remarks>
        /// <returns>
        /// </returns>
        public async Task SyncToLocal()
        {
            var tasks = new List<Task>();
            foreach (var remoteFile in remoteFiles)
            {
                tasks.Add(DownloadToLocal(remoteFile));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Deletes the files in the remote storage. (Needs to be overridden)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task ClearRemote()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Uploads an file from local to remote.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected async Task UploadToRemote(T file)
        {
            await file.LoadLocalAsync();
            await file.SaveRemoteAsync();
        }
        /// <summary>
        /// Downloads an file from remote local.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected async Task DownloadToLocal(T file)
        {
            await file.LoadRemoteAsync();
            await file.SaveLocalAsync();
        }


        /// <summary>
        /// Lists all files in a directory on the local storage.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string[] GetFiles(string path)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path));
            var directories = Directory.GetDirectories(path);
            foreach (var subDir in directories)
            {
                files.AddRange(GetFiles(subDir));
            }
            string[] raw = files.ToArray();
            files.Clear();
            files = null;
            return raw;
        }



    }
}