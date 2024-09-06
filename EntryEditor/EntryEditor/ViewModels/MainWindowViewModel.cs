using EntryEditor.Models;
using System;
using EntryEditor.Models.Commands;
using System.Linq;
using Windows.UI.Xaml;
using System.IO;
using Windows.Storage;
using EntryEditor.Models.Serialization;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using EntryEditor.Models.Serialization.DTOs;

namespace EntryEditor.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        private readonly ISerializer<IEnumerable<EntryDTO>> xmlSerializer;
        private readonly ISerializer<IEnumerable<EntryDTO>> jsonSerializer;

        private bool _firstLoad;

        public NotifyOnCompleteAddingCollection<EntryReactive> Entries { get; }

        public MainWindowViewModel()
        {
            _firstLoad = true;
            Entries = new();
            AddCommand = new RelayCommand(Add, CanAdd);
            EditCommand = new RelayCommand(Edit);
            DeleteEntryCommand = new RelayCommand(DeleteEntry);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            CancelChangesCommand = new RelayCommand(CancelChanges);
            SaveStateCommand = new RelayCommand(SaveState);
            LoadStateCommand = new RelayCommand(LoadState);
            xmlSerializer = new XmlSerializer();
            jsonSerializer = new JsonSerializer();
        }

        private ISerializer<IEnumerable<EntryDTO>> GetSerializerByFileExtension(string extension = null) => extension switch
        {
            ".xml" => xmlSerializer,
            ".json" => jsonSerializer,
            _ => jsonSerializer
        };

        private async Task SaveStateAsync(StorageFolder appFolder)
        {
            if (appFolder is null)
            {
                appFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(DefaultPaths.FOLDER);
            }

            var file = (StorageFile)await appFolder.TryGetItemAsync(DefaultPaths.FILE_NAME);

            if (file is null)
            {
                file = await appFolder.CreateFileAsync(DefaultPaths.FILE_NAME);
            }
            else
            {
                await FileIO.WriteTextAsync(file, string.Empty);
            }

            EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Entries, Entries.Count);
        }

        #region Add Command
        public IRelayCommand AddCommand { get; }

        private void Add(object _)
        {
            var newEntry = new EntryReactive();
            newEntry.InitCredentials(FirstName, LastName);
            Entries.Add(newEntry);
        }

        private bool CanAdd(object _) => !string.IsNullOrWhiteSpace(FirstName)
                                         && !string.IsNullOrWhiteSpace(LastName);

        #endregion

        #region Edit Command

        public IRelayCommand EditCommand { get; }

        private void Edit(object id)
        {
            var entryToEdit = Entries.First(entry => entry.Equals(id));
            entryToEdit.AllowEditing = true;
        }

        #endregion

        #region Delete entry command

        public IRelayCommand DeleteEntryCommand { get; }

        private async void DeleteEntry(object entry)
        {
            var uiCommand = await MessageDialogExtensions.ShowMessageAsync("Do you wish to delete entry?",
                    MessageDialogExtensions.YesDialogButtonName, MessageDialogExtensions.NoDialogButtonName);

            if (uiCommand.Label == MessageDialogExtensions.YesDialogButtonName)
            {
                Entries.Remove((EntryReactive)entry);
            }
        }

        #endregion

        #region Save Changes Command
        public IRelayCommand SaveChangesCommand { get; }

        private async void SaveChanges(object entry)
        {
            string dialogResultLabel = MessageDialogExtensions.YesDialogButtonName;

            if (!CanSaveChanges(entry))
            {
               var dialogResult = await MessageDialogExtensions.ShowMessageAsync("Do you really want to leave fields empty?", 
                    MessageDialogExtensions.YesDialogButtonName, 
                    MessageDialogExtensions.NoDialogButtonName);

                dialogResultLabel = dialogResult.Label;
            }

            if(dialogResultLabel == MessageDialogExtensions.YesDialogButtonName)
            {
                var currentEntry = ((EntryReactive)entry);
                currentEntry.CommitChanges();
                currentEntry.AllowEditing = false;
            }
        }

        private bool CanSaveChanges(object entry)
        {
            bool result = false;

            if(entry is not null)
            {
                var currentEntry = (EntryReactive)entry;

                result = !string.IsNullOrEmpty(currentEntry.FirstName)
                        && !string.IsNullOrEmpty(currentEntry.LastName);
            }

            return result;
        }

        #endregion

        #region Cancel Changes Command

        public IRelayCommand CancelChangesCommand { get; }

        private void CancelChanges(object entry)
        {
            var currentEntry = ((EntryReactive)entry);
            currentEntry.RollBackChanges();
            currentEntry.AllowEditing = false;
        }

        #endregion

        #region Save State Command

        public IRelayCommand SaveStateCommand { get; }
        private async void SaveState(object _)
        {
            var file = await FilePickerExtensions.ShowSaveAsync();
            try
            {
                if(file is not null)
                {
                    var serializer = GetSerializerByFileExtension(file.FileType);
                    EntryReactive.SetSerializer(serializer);
                    EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Entries, Entries.Count);
                }
            }
            catch(SerializationException ex)
            {
                await MessageDialogExtensions.ShowMessageAsync(ex.Message, MessageDialogExtensions.OkDialogButtonName);
            }
        }

        #endregion

        #region Load State Command

        public IRelayCommand LoadStateCommand { get; }

        private async void LoadState(object _)
        {
            var file = await FilePickerExtensions.ShowOpenAsync();
            try
            {
                if(file is not null)
                {
                    if (Entries.Count > 0)
                    {
                        Entries.Clear();
                    }

                    var serializer = GetSerializerByFileExtension(file.FileType);
                    EntryReactive.SetSerializer(serializer);
                    EntryReactive.Deserialize(await file.OpenStreamForReadAsync(), Entries.AddRange);
                }
            }
            catch (SerializationException ex)
            {
                await MessageDialogExtensions.ShowMessageAsync(ex.Message, MessageDialogExtensions.OkDialogButtonName);
            }
        }

        #endregion

        #region First Name field

        private string _firstName = string.Empty;

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                NotifyOfPropertyChange();
                AddCommand.NotifyCanExecuteChanged();
            }
        }

        #endregion

        #region Last Name filed

        private string _lastName = string.Empty;

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                NotifyOfPropertyChange();
                AddCommand.NotifyCanExecuteChanged();
            }
        }

        #endregion

        #region Selected Entry

        private EntryReactive _selectedEntry = null;

        public EntryReactive SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        public override async Task OnLoading()
        {
            if (!_firstLoad)
            {
                return;
            }

            try
            {
                var appFolder = (StorageFolder) await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(DefaultPaths.FOLDER);
                if (appFolder is null)
                {
                    appFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(DefaultPaths.FOLDER);
                }

                var file = await appFolder.TryGetItemAsync(DefaultPaths.FILE_NAME) as StorageFile;

                if (file is null)
                {
                    file = await appFolder.CreateFileAsync(DefaultPaths.FILE_NAME);
                    EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Enumerable.Empty<EntryReactive>());
                }
                else
                {
                    var serializer = GetSerializerByFileExtension();
                    EntryReactive.SetSerializer(serializer);
                    EntryReactive.Deserialize(await file.OpenStreamForReadAsync(), Entries.AddRange);
                }

                _firstLoad = false;
            }
            catch (UnauthorizedAccessException)
            {
                await MessageDialogExtensions.ShowMessageAsync("Please, give me access to Local cache folder",
                    MessageDialogExtensions.OkDialogButtonName);
            }
            catch (SerializationException)
            {
                await MessageDialogExtensions.ShowMessageAsync("Saving file is corrupted, it will be rewritten when you exit program.",
                    MessageDialogExtensions.OkDialogButtonName);
            }
        }

        public override async Task OnClosing()
        {
            var appFolder = await ApplicationExtensions.GetLocalCacheFolderAsync();

            if (appFolder is null)
            {
                appFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(DefaultPaths.FOLDER);
            }

            var file = (StorageFile)await appFolder.TryGetItemAsync(DefaultPaths.FILE_NAME);

            if (file is null)
            {
                file = await appFolder.CreateFileAsync(DefaultPaths.FILE_NAME);
            }
            else
            {
                await FileIO.WriteTextAsync(file, string.Empty);
            }

            var serializer = GetSerializerByFileExtension();
            EntryReactive.SetSerializer(serializer);
            EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Entries, Entries.Count);
        }
    }
}