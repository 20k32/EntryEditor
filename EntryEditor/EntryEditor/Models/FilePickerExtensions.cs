using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace EntryEditor.Models
{
    internal static class FilePickerExtensions
    {
        private static readonly FileSavePicker fileSavePicker;
        private static readonly FileOpenPicker fileOpenPicker;

        static FilePickerExtensions()
        {
            fileSavePicker = new FileSavePicker()
            {
                DefaultFileExtension = ".xml",
                SuggestedFileName = "Entries",
                SuggestedStartLocation = PickerLocationId.Desktop,
                CommitButtonText = "Save"
            };

            var xmlPair = new KeyValuePair<string, string[]>("xml", new[] { ".xml" });
            var jsonPair = new KeyValuePair<string, string[]>("json", new[] { ".json" });

            fileSavePicker.FileTypeChoices.Add(xmlPair.Key, xmlPair.Value);
            fileSavePicker.FileTypeChoices.Add(jsonPair.Key, jsonPair.Value);

            fileOpenPicker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.Thumbnail,
                CommitButtonText = "Open"
            };

            fileOpenPicker.FileTypeFilter.Add(xmlPair.Value[0]);
            fileOpenPicker.FileTypeFilter.Add(jsonPair.Value[0]);
        }

        public static IAsyncOperation<StorageFile> ShowOpenAsync() => fileOpenPicker.PickSingleFileAsync();
        public static IAsyncOperation<StorageFile> ShowSaveAsync() => fileSavePicker.PickSaveFileAsync();
    }
}
