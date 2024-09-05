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

namespace EntryEditor.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        private bool _firstLoad;

        private bool _editRequested;
        private bool EditRequested
        {
            get => _editRequested;
            set
            {
                _editRequested = value;
                NotifyOfPropertyChange(nameof(ChangeEntryButtonsVisibility));
                NotifyOfPropertyChange(nameof(EditButtonVisibility));
                AddCommand?.NotifyCanExecuteChanged();
            }
        }

        public NotifyOnCompleteAddingCollection<EntryReactive> Entries { get; }

        public MainWindowViewModel()
        {
            _firstLoad = true;
            Entries = new();
            EditRequested = false;
            AddCommand = new RelayCommand(Add, CanAdd);
            EditCommand = new RelayCommand(Edit, CanEdit);
            DeleteEntryCommand = new RelayCommand(DeleteEntry);
            SaveChangesCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            CancelChangesCommand = new RelayCommand(CancelChanges);
            SaveStateCommand = new RelayCommand(SaveState);
            LoadStateCommand = new RelayCommand(LoadState);
        }

        private async Task SaveStateAsync()
        {
            var documentsFolder = await KnownFolders.GetFolderForUserAsync(null, KnownFolderId.DocumentsLibrary);
            var appFolder = await documentsFolder.TryGetItemAsync(DefaultPaths.FOLDER) as StorageFolder;

            if (appFolder is null)
            {
                appFolder = await documentsFolder.CreateFolderAsync(DefaultPaths.FOLDER);
            }

            var file = await appFolder.TryGetItemAsync(DefaultPaths.FILE_NAME) as StorageFile;

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

        private async Task LoadStateAsync()
        {
            StorageFile file = null;
            try
            {
                var documentsFolder = await KnownFolders.GetFolderForUserAsync(null, KnownFolderId.DocumentsLibrary);
                var appFolder = await documentsFolder.GetFolderAsync(DefaultPaths.FOLDER);
                file = await appFolder.GetFileAsync(DefaultPaths.FILE_NAME);

                if (Entries.Count > 0)
                {
                    Entries.Clear();
                }

                EntryReactive.Deserialize(await file.OpenStreamForReadAsync(), Entries.AddRange);
            }
            catch (UnauthorizedAccessException ex)
            {
                await MessageDialogExtensions.ShowMessage(ex.Message, MessageDialogExtensions.OkDialogButtonName);
            }
            catch (FileNotFoundException ex)
            {
                await MessageDialogExtensions.ShowMessage(ex.Message, MessageDialogExtensions.OkDialogButtonName);
            }
            catch (SerializationException)
            {
                await MessageDialogExtensions.ShowMessage("Saving file is corrupted. Click 'Ok' to delete it.",
                    MessageDialogExtensions.OkDialogButtonName);
                await file.DeleteAsync();
            }
        }

        #region Add Command
        public IRelayCommand AddCommand { get; }

        private void Add(object _)
        {
            var newEntry = new EntryReactive()
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
            };

            Entries.Add(newEntry);
            SelectedEntry = null;

            if (Entries.Count == 1)
            {
                EditCommand.NotifyCanExecuteChanged();
                SaveChangesCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanAdd(object _) => !EditRequested
            && !string.IsNullOrWhiteSpace(FirstName)
            && !string.IsNullOrWhiteSpace(LastName);

        #endregion

        #region Edit Command

        public Visibility EditButtonVisibility => EditRequested ? Visibility.Collapsed : Visibility.Visible;

        public IRelayCommand EditCommand { get; }

        private void Edit(object _)
        {
            if (SelectedEntry is null)
            {
                return;
            }

            FirstName = SelectedEntry.FirstName;
            LastName = SelectedEntry.LastName;
            EditRequested = true;
        }

        private bool CanEdit(object _) => SelectedEntry is not null && Entries.Count > 0;

        #endregion

        #region Delete entry command

        public IRelayCommand DeleteEntryCommand { get; }

        private async void DeleteEntry(object entryId)
        {
            if (entryId is Guid entryGuidId)
            {
                var uiCommand = await MessageDialogExtensions.ShowMessage("Do you wish to delete entry?", 
                    MessageDialogExtensions.YesDialogButtonName, MessageDialogExtensions.NoDialogButtonName);

                if (uiCommand.Label == MessageDialogExtensions.NoDialogButtonName)
                {
                    return;
                }

                var entryToDelete = Entries.First(entry => entry.Equals(entryId));
                Entries.Remove(entryToDelete);

                if (Entries.Count == 0)
                {
                    EditCommand.NotifyCanExecuteChanged();
                    SaveChangesCommand.NotifyCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Save Changes Command

        public Visibility ChangeEntryButtonsVisibility => EditRequested ? Visibility.Visible : Visibility.Collapsed;
        public IRelayCommand SaveChangesCommand { get; }

        private void SaveChanges(object _)
        {
            SelectedEntry.FirstName = FirstName;
            SelectedEntry.LastName = LastName;
        }

        private bool CanSaveChanges(object _) => SelectedEntry is not null && Entries.Count > 0;

        #endregion

        #region Cancel Changes Command

        public IRelayCommand CancelChangesCommand { get; }

        private void CancelChanges(object _)
        {
            EditRequested = false;
        }

        #endregion

        #region Save State Command

        public IRelayCommand SaveStateCommand { get; }
        private async void SaveState(object _)
        {
            await SaveStateAsync();
        }

        #endregion

        #region Load State Command

        public IRelayCommand LoadStateCommand { get; }

        private async void LoadState(object _)
        {
            await LoadStateAsync();
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
                DeleteEntryCommand.NotifyCanExecuteChanged();
                SaveChangesCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();

                if (EditRequested && SelectedEntry is not null)
                {
                    FirstName = SelectedEntry.FirstName;
                    LastName = SelectedEntry.LastName;
                }
            }
        }

        #endregion

        public override async void OnLoading(object sender, LeavingBackgroundEventArgs e)
        {
            if (!_firstLoad)
            {
                return;
            }
            try
            {
                var documentsFolder = await KnownFolders.GetFolderForUserAsync(null, KnownFolderId.DocumentsLibrary);
                var appFolder = await documentsFolder.TryGetItemAsync(DefaultPaths.FOLDER) as StorageFolder;
                if (appFolder is null)
                {
                    appFolder = await documentsFolder.CreateFolderAsync(DefaultPaths.FOLDER);
                }

                var file = await appFolder.TryGetItemAsync(DefaultPaths.FILE_NAME) as StorageFile;

                if (file is null)
                {
                    file = await appFolder.CreateFileAsync(DefaultPaths.FILE_NAME);
                    EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Enumerable.Empty<EntryReactive>());
                }

                await LoadStateAsync();
                _firstLoad = false;
            }
            catch (UnauthorizedAccessException)
            {
                await MessageDialogExtensions.ShowMessage("Please, give me access to Documents folder",
                    MessageDialogExtensions.OkDialogButtonName);
            }
        }

        public override async void OnClosing(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SaveStateAsync();
            deferral.Complete();
        }
    }
}