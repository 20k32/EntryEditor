using EntryEditor.Models;
using System;
using EntryEditor.Models.Commands;
using System.Linq;
using System.IO;
using Windows.Storage;
using EntryEditor.Models.Serialization;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;
using EntryEditor.Models.Serialization.DTOs;

namespace EntryEditor.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        private readonly ISerializer<IEnumerable<EntryDTO>> xmlSerializer;
        private readonly ISerializer<IEnumerable<EntryDTO>> jsonSerializer;

        private bool firstLoad;

        public NotifyOnCompleteAddingCollection<EntryReactive> Entries { get; }

        public MainWindowViewModel()
        {
            firstLoad = true;
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

        #region Add Command
        public IRelayCommand AddCommand { get; }

        private void Add(object _)
        {
            var newEntry = new EntryReactive();
            newEntry.InitCredentialsIfAllowed(FirstName, LastName);
            Entries.Add(newEntry);
        }

        private bool CanAdd(object _) => !string.IsNullOrWhiteSpace(FirstName)
                                         && !string.IsNullOrWhiteSpace(LastName);

        #endregion

        #region Edit Command

        public IRelayCommand EditCommand { get; }

        private void Edit(object entry)
        {
            var entryToEdit = (EntryReactive)entry;
            entryToEdit.CanEdit = true;
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
                currentEntry.EndEdit();
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
            currentEntry.CancelEdit();
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
                    await FileIO.WriteTextAsync(file, string.Empty);
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

        private string firstName = string.Empty;

        public string FirstName
        {
            get => firstName;
            set
            {
                if (firstName != value)
                {
                    firstName = value;
                    NotifyOfPropertyChange();
                    AddCommand.NotifyCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Last Name filed

        private string lastName = string.Empty;

        public string LastName
        {
            get => lastName;
            set
            {
                if(lastName != value)
                {
                    lastName = value;
                    NotifyOfPropertyChange();
                    AddCommand.NotifyCanExecuteChanged();
                }
            }
        }

        #endregion

        public override async Task OnLoadingAsync()
        {
            if (firstLoad)
            {
                try
                {
                    var serializer = GetSerializerByFileExtension();
                    EntryReactive.SetSerializer(serializer);

                    var appFolder = await ApplicationExtensions.GetOrCreateLocalCacheFolderAsync();
                    var file = (StorageFile)await appFolder.TryGetItemAsync(DefaultPaths.FileName);

                    if (file is null)
                    {
                        file = await appFolder.CreateFileAsync(DefaultPaths.FileName);
                        EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Enumerable.Empty<EntryReactive>());
                    }
                    else
                    {
                        EntryReactive.Deserialize(await file.OpenStreamForReadAsync(), Entries.AddRange);
                    }

                    firstLoad = false;
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
        }

        public override async Task OnClosingAsync()
        {
            var appFolder = await ApplicationExtensions.GetOrCreateLocalCacheFolderAsync();
            var file = (StorageFile)await appFolder.TryGetItemAsync(DefaultPaths.FileName);

            if (file is null)
            {
                file = await appFolder.CreateFileAsync(DefaultPaths.FileName);
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