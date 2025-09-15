using System.IO;
using SFile = System.IO.File;

namespace OpenGameSync.SDK
{
    /// <summary>
    /// 
    /// </summary>
    public class File
    {
        private string relativePath;
        private string localPath;

        protected byte[] content;
        protected string LocalPath
        {
            get
            {
                return Path.Join(localPath, relativePath);
            }
        }

        public FileInfo FileInfo
        {
            get
            {
                return new FileInfo(LocalPath);
            }
        }

        public File(string localPath, string relativeFilePath)
        {
            this.localPath = localPath;
            this.relativePath = relativeFilePath;
        }

        public void Unload()
        {
            if (content != null)
                Array.Clear(content, 0, content.Length);
            content = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        public async Task LoadLocalAsync()
        {
            // Logger.Log("Read local file");
            content = await SFile.ReadAllBytesAsync(LocalPath);
            // Logger.Log($"Local file conent: {content}");
        }

        public virtual async Task LoadRemoteAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SaveLocalAsync()
        {
            await SFile.WriteAllBytesAsync(LocalPath, content);
        }

        public virtual Task SaveRemoteAsync()
        {
            throw new NotImplementedException();
        }

    }
}