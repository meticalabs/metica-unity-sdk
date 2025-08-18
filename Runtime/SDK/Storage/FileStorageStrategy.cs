using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Metica.SDK.Storage
{
    /// <summary>
    /// File-based storage strategy.
    /// Stores and retrieves data from the file system using a Json formatter.
    /// </summary>
    public class FileStorageStrategy : IStorageStrategy
    {
        private readonly DirectoryInfo _dirInfo = null;

        /// <summary>
        /// <see cref="FileStorageStrategy"/> constructor.
        /// </summary>
        /// <param name="path">Path to a folder that will be used to store files where <see cref="key"/> is used as filename. If a path is not given a temporary folder will be used. </param>
        public FileStorageStrategy(string path = null)
        {
            // This should ensure that the path is well formed and a few other things and
            // throw meaningful exceptions if anything fails in the process.
            // See https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.-ctor?view=net-9.0#system-io-directoryinfo-ctor(system-string)
            var dirInfo = new DirectoryInfo(string.IsNullOrEmpty(path) ? Path.GetTempPath() : path);
            // DirectoryInfo.Create does nothing if dir already exists, creates it with needed subfolder if it doesn't.
            // Raises IOException if cannot be created due to malformed path, access issues and more.
            // https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.create?view=net-9.0#system-io-directoryinfo-create
            dirInfo.Create();
            _dirInfo = dirInfo;
            Log.Debug(() => $"{nameof(FileStorageStrategy)} folder set to {_dirInfo.FullName}");
        }

        public async Task SaveAsync<T>(string filename, T value)
        {
            string filePath = Path.Combine(_dirInfo.FullName, filename);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

            string jsonString = JsonConvert.SerializeObject(value, settings);

            await File.WriteAllTextAsync(filePath, jsonString);
        }

        public async Task<T> LoadAsync<T>(string filename)
        {
            string filePath = Path.Combine(_dirInfo.FullName, filename);
            if (!File.Exists(filePath))
                throw new FileLoadException($"The file {filename} could not be found.");

            string jsonString = await File.ReadAllTextAsync(filePath);

            return JsonConvert.DeserializeObject<T>(jsonString); // can raise JsonSerializationException
        }

        public bool Exists(string filename)
        {
            return File.Exists(Path.Combine(_dirInfo.FullName, filename));
        }

        public void Delete(string filename)
        {
            string filePath = Path.Combine(_dirInfo.FullName, filename);
            if (Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}