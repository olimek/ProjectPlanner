using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ProjectPlanner.Model
{
    public class SubTask : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }

        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _description = string.Empty;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private bool _isDone;

        public bool IsDone
        {
            get => _status == SubTaskStatus.Done;
            set => Status = value ? SubTaskStatus.Done : SubTaskStatus.None;
        }

        private SubTaskStatus _status;

        public SubTaskStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _tags = string.Empty;

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        private int _priority;

        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        private DateTime? _dueDate;

        public DateTime? DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        public int? ProjectId { get; set; }

        public Project? Project { get; set; }

        public List<TaskAttachment> Attachments { get; set; } = [];
        public List<TaskLink> Links { get; set; } = [];
        public List<TaskNote> Notes { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}