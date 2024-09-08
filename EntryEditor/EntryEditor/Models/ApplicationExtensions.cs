using EntryEditor.Models.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace EntryEditor.Models
{
    internal static class ApplicationExtensions
    {
        private static StorageFolder localCacheFolder;
        
        public static async Task<StorageFolder> GetOrCreateLocalCacheFolderAsync()
        {
            if (localCacheFolder is null)
            {
                localCacheFolder = (StorageFolder)await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(DefaultPaths.Folder);

                if (localCacheFolder is null)
                {
                    localCacheFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(DefaultPaths.Folder);
                }
            }
            
            return localCacheFolder;
        }
    }
}
