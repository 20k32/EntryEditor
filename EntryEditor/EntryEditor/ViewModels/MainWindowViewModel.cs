using EntryEditor.Models;
using System;
using System.Windows.Input;
using EntryEditor.Models.Commands;
using System.Linq;
using Windows.UI.Xaml;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace EntryEditor.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
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

        public NotifyOnCompleteAddingCollection<EntryReactive> Entries { get; set; }

        public MainWindowViewModel()
        {
            
            EditRequested = false;
            Entries = new NotifyOnCompleteAddingCollection<EntryReactive>();
            AddCommand = new RelayCommand(Add, CanAdd);
            EditCommand = new RelayCommand(Edit, CanEdit);
            DeleteEntryCommand = new RelayCommand(DeleteEntry);
            SaveChangesCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            CancelChangesCommand = new RelayCommand(CancelChanges);
        }

        #region Add Command
        public IRelayCommand AddCommand { get; set; }

        private void Add(object _)
        {
            if (EditRequested)
            {
                // todo: notify user that he is in editing mode;
                return;
            }

            var newEntry = new EntryReactive()
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
            };

            Entries.Add(newEntry);
            SelectedEntry = null;

            if(Entries.Count == 1)
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

        public IRelayCommand EditCommand { get; set; }

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

        public bool DeleteButtonVisibility => false;

        public IRelayCommand DeleteEntryCommand { get; set; }

        private async void DeleteEntry(object entryId)
        {
            if (entryId is Guid entryGuidId)
            {
                var message = new MessageDialog("Do you wish to delete this entry?");
                message.Commands.Add(new UICommand("Yes", null));
                message.Commands.Add(new UICommand("No", null));

                var uiCommand = await message.ShowAsync();

                if(uiCommand.Label == "No")
                {
                    return;
                }

                var entryToDelete = Entries.First(entry => entry.Equals(entryId));
                Entries.Remove(entryToDelete);

                if(Entries.Count == 0)
                {
                    EditCommand.NotifyCanExecuteChanged();
                    SaveChangesCommand.NotifyCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Save Changes Command

        public Visibility ChangeEntryButtonsVisibility => EditRequested ? Visibility.Visible : Visibility.Collapsed;
        public IRelayCommand SaveChangesCommand { get; set; }

        private async void SaveChanges(object _)
        {
            try
            {
                var documentsFolder = await KnownFolders.GetFolderForUserAsync(null, KnownFolderId.DocumentsLibrary);
                var appFolder = await documentsFolder.TryGetItemAsync("EntryEditor/") as StorageFolder;
                if (appFolder is null)
                {
                    await documentsFolder.CreateFolderAsync("EntryEditor/");
                }
      
                var file = await appFolder.TryGetItemAsync("test.xml") as StorageFile;

                if(file is null)
                {
                    file = await appFolder.CreateFileAsync("test.xml");
                }
                else
                {
                    await FileIO.WriteTextAsync(file, string.Empty);
                }
                

                EntryReactive.Serialize(await file.OpenStreamForWriteAsync(), Entries, Entries.Count);
            }
            catch(UnauthorizedAccessException ex)
            {
                var s = ex.Message;
            }
            catch (FileNotFoundException ex)
            {
                var s = ex.Message;
            }
            
        }

        private bool CanSaveChanges(object _) => Entries.Count > 0;

        #endregion

        #region Load Changes Command

        public IRelayCommand LoadChangesCommand { get; set; }

        private void LoadChanges()
        {
            if(Entries.Count > 0)
            {
                Entries.Clear();
            }

            EntryReactive.Deserialize(File.OpenRead("test.xml"), Entries.Add);
        }

        #endregion

        #region CancelChanges Command

        public IRelayCommand CancelChangesCommand { get; set; }

        private void CancelChanges(object _)
        {
            EditRequested = false;
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

                if (EditRequested)
                {
                    FirstName = SelectedEntry.FirstName;
                    LastName = SelectedEntry.LastName;
                }
            }
        }

        #endregion
    }
}