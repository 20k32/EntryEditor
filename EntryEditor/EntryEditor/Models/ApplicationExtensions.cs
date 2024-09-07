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
        [ThreadStatic]
        private static StorageFolder localCacheFolder;
        
        public static async Task<StorageFolder> GetLocalCacheFolderAsync()
            => localCacheFolder ??= await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(DefaultPaths.FOLDER);
    }
}
