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
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.Devices.Bluetooth.Advertisement;

namespace EntryEditor.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        private DataGridWrapper wrapGrid;
        private EntryReactive previousEdited;
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

        public void SetPartGrid(DataGridWrapper value)
        {
            this.wrapGrid = value;
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
            EntryReactive current = null;
            if (entry is EntryReactive reactive)
            {
                current = reactive;
            }
            if (previousEdited != current)
            {
                if (previousEdited is not null)
                {
                    previousEdited.EndEdit();
                }

                previousEdited = current;
            }

            SelectedEntry = current;
            wrapGrid?.BeginEdit();
        }

        #endregion

        #region Selected Entry

        private EntryReactive selectedEntry;
        
        public EntryReactive SelectedEntry
        {
            get => selectedEntry;
            set
            {
                selectedEntry = value;
                NotifyOfPropertyChange();
            }
        } 

        #endregion

        #region Delete Entry Command

        public IRelayCommand DeleteEntryCommand { get; }

        private async void DeleteEntry(object entry)
        {            
            var uiCommand = await MessageDialogExtensions.ShowMessageAsync("Do you wish to delete entry?",
                    MessageDialogExtensions.YesDialogButtonName, MessageDialogExtensions.NoDialogButtonName);

            if (uiCommand.Label == MessageDialogExtensions.YesDialogButtonName)
            {
                Entries.Remove((EntryReactive)entry);
            }

            wrapGrid?.EndEdit();
        }

        #endregion

        #region Save Changes Command
        public IRelayCommand SaveChangesCommand { get; }

        private void SaveChanges(object entry)
        {
            wrapGrid?.EndEdit();
        }

        #endregion

        #region Cancel Changes Command

        public IRelayCommand CancelChangesCommand { get; }

        private void CancelChanges(object entry)
        {
            var reactive = ((EntryReactive)entry);
            if (reactive.CanEdit)
            {
                reactive.CancelEdit();
                wrapGrid?.EndEdit();
            }
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
                    EntryReactive.SerializeAndDispose(await file.OpenStreamForWriteAsync(), Entries, Entries.Count);
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
                    var stream = await file.OpenStreamForReadAsync();
                    EntryReactive.DeserializeAndDispose(stream, Entries.AddRange);
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

                        var stream = await file.OpenStreamForWriteAsync();
                        EntryReactive.SerializeAndDispose(stream, Enumerable.Empty<EntryReactive>());
                    }
                    else
                    {
                        var stream = await file.OpenStreamForReadAsync();
                        EntryReactive.DeserializeAndDispose(stream, Entries.AddRange);
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

            var stream = await file.OpenStreamForWriteAsync();
            EntryReactive.SerializeAndDispose(stream, Entries, Entries.Count);
        }
    }
}