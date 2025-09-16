namespace OpenGameSync.SDK
{
    public class SyncManager<T> where T : File
    {
        private string localPath;
        protected string LocalPath
        {
            get
            {
                return localPath;
            }
        }
        private List<T> remoteFiles = new List<T>();
        private List<T> localFiles = new List<T>();

        public SyncManager(string localPath)
        {
            this.localPath = localPath;
        }

        public virtual async Task FetchRemote()
        {
            throw new NotImplementedException();
        }
        public void FetchLocal()
        {
            var filePaths = GetFiles(localPath);
            localFiles.Clear();
            var loadFileTask = new List<Task>();
            foreach (var filePath in filePaths)
            {
                var file = InitLocalFile(localPath, filePath.Substring(localPath.Length));
                localFiles.Add(
                    file
                );
            }
        }

        protected virtual T InitLocalFile(string localPath, string absolutePath)
        {
            return (T)Activator.CreateInstance(typeof(T), localPath, absolutePath)!;
        }

        protected void ClearRemoteFiles()
        {
            remoteFiles.Clear();
        }

        protected void AddRemoteFile(T file)
        {
            remoteFiles.Add(file);
        }

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


        public async Task SyncToLocal()
        {
            var tasks = new List<Task>();
            foreach (var remoteFile in remoteFiles)
            {
                tasks.Add(DownloadToLocal(remoteFile));
            }
            await Task.WhenAll(tasks);
        }

        protected virtual async Task ClearRemote()
        {
            throw new NotImplementedException();
        }
        protected async Task UploadToRemote(T file)
        {
            await file.LoadLocalAsync();
            await file.SaveRemoteAsync();
        }

        protected async Task DownloadToLocal(T file)
        {
            await file.LoadRemoteAsync();
            await file.SaveLocalAsync();
        }



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