using EntryEditor.Models;
using System;
using System.Windows.Input;
using EntryEditor.Models.Commands;
using System.Linq;
using Windows.UI.Xaml;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;

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

        private bool CanAdd(object _) => !string.IsNullOrWhiteSpace(FirstName) 
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

        #region SaveChanges Command

        public Visibility ChangeEntryButtonsVisibility => EditRequested ? Visibility.Visible : Visibility.Collapsed;
        public IRelayCommand SaveChangesCommand { get; set; }

        private void SaveChanges(object _)
        {
            if (SelectedEntry is not null && EditRequested)
            {
                SelectedEntry.FirstName = this.FirstName;
                SelectedEntry.LastName = this.LastName;
            }
        }

        private bool CanSaveChanges(object _) => SelectedEntry is not null && Entries.Count > 0;

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
            }
        }

        #endregion
    }
}