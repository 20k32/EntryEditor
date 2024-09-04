using EntryEditor.Models;
using System;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EntryEditor.Models.Commands;
using Windows.UI.Xaml.Input;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;

namespace EntryEditor.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        private bool _editRequested;
        public NotifyOnCompleteAddingCollection<EntryReactive> Entries { get; set; }
        
        public MainWindowViewModel() 
        {
            _editRequested = false;
            Entries = new NotifyOnCompleteAddingCollection<EntryReactive>();
            AddCommand = new RelayCommand(Add, CanAdd);
            EditCommand = new RelayCommand(Edit);
            DeleteEntryCommand = new RelayCommand(DeleteEntry);
        }

        #region Add Command
        private bool _canAdd;
        public IRelayCommand AddCommand { get; set; }

        internal void Add(object _)
        {
            if(SelectedEntry is null)
            {
                var newEntry = new EntryReactive()
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                };

                Entries.Add(newEntry);
            }
            else if(SelectedEntry is not null && _editRequested)
            {
                SelectedEntry.FirstName = this.FirstName;
                SelectedEntry.LastName = this.LastName;
                _editRequested = false;
            }
        }

        internal bool CanAdd(object _)
        {
            return true;
        }

        #endregion

        #region Edit Command


        public ICommand EditCommand { get; set; }

        internal void Edit(object _)
        {
            if(SelectedEntry is null)
            {
                return;
            }

            FirstName = SelectedEntry.FirstName;
            LastName = SelectedEntry.LastName;
            _editRequested = true;
        }

        #endregion

        #region Delete entry command

        public IRelayCommand DeleteEntryCommand { get; set; }

        public void DeleteEntry(object entryId)
        {
            if(entryId is null)
            {
                return;
            }

            if(entryId is Guid entryGuidId)
            {
                var entryToDelete = Entries.First(entry => entry.Id == entryGuidId);
                Entries.Remove(entryToDelete);
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
    }
}