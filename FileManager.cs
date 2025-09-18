using System.IO;
using SFile = System.IO.File;

namespace OpenGameSync.SDK
{
    /// <summary>
    /// Represents a file, wich can be stored localy but also remotely.
    /// </summary>
    public class File
    {
        //// <summary>
        /// The relative file path (based on the local and remote root directories).
        /// </summary>
        private string relativePath;

        /// <summary>
        /// The absolute local root directory where the file is stored.
        /// </summary>
        private string localPath;

        /// <summary>
        /// The binary content of the file (loaded from either local or remote).
        /// </summary>
        protected byte[] content;

        /// <summary>
        /// Gets the absolute path of the file in the local filesystem.
        /// </summary>
        protected string LocalPath
        {
            get
            {
                return Path.Join(localPath, relativePath);
            }
        }

        /// <summary>
        /// Returns the Fileinfo, if the file exists in the local storage.
        /// </summary>
        public FileInfo FileInfo
        {
            get
            {
                if (!SFile.Exists(LocalPath))
                {
                    return null;
                }
                return new FileInfo(LocalPath);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="File"/> class.
        /// </summary>
        /// <param name="localPath">The absolute path of the local root directory.</param>
        /// <param name="relativeFilePath">The relative path of the file.</param>
        public File(string localPath, string relativeFilePath)
        {
            this.localPath = localPath;
            this.relativePath = relativeFilePath;
        }

        /// <summary>
        /// Clears the <see cref="content"/> variable.
        /// </summary>
        public void Unload()
        {
            if (content != null)
                Array.Clear(content, 0, content.Length);
            content = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        /// <summary>
        /// Loads the content from a local file.
        /// </summary>
        /// <returns></returns>
        public async Task LoadLocalAsync()
        {
            // Logger.Log("Read local file");
            content = await SFile.ReadAllBytesAsync(LocalPath);
            // Logger.Log($"Local file conent: {content}");
        }

        /// <summary>
        /// Loads the content from the remote file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task LoadRemoteAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves content in a local file.
        /// </summary>
        /// <returns></returns>
        public async Task SaveLocalAsync()
        {
            await SFile.WriteAllBytesAsync(LocalPath, content);
        }

        /// <summary>
        /// Saves content to a remote file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task SaveRemoteAsync()
        {
            throw new NotImplementedException();
        }

    }
}