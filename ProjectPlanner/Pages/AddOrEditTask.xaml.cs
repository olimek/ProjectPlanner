using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages;

public partial class AddOrEditTask : ContentPage
{
    private readonly IProjectService _projectService;
    private readonly Project _project;
    private SubTask? _currentTask;
    private readonly int? _taskId;

    public ObservableCollection<TaskAttachment> Attachments { get; } = new();
    public ObservableCollection<TaskLink> Links { get; } = new();
    public ObservableCollection<TaskNote> Notes { get; } = new();

    public AddOrEditTask(Project project, IProjectService projectService, int? taskId = null)
    {
        InitializeComponent();
        _project = project;
        _projectService = projectService;
        _taskId = taskId;
        BindingContext = this;

        // Subscribe to collection changes to update empty labels
        Attachments.CollectionChanged += (s, e) => UpdateEmptyLabels();
        Links.CollectionChanged += (s, e) => UpdateEmptyLabels();
        Notes.CollectionChanged += (s, e) => UpdateEmptyLabels();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private void UpdateEmptyLabels()
    {
        lbl_no_attachments.IsVisible = Attachments.Count == 0;
        lbl_no_links.IsVisible = Links.Count == 0;
        lbl_no_notes.IsVisible = Notes.Count == 0;
    }

    private void LoadData()
    {
        lbl_project_name.Text = _project.Name?.ToUpper() ?? "UNKNOWN PROJECT";

        Attachments.Clear();
        Links.Clear();
        Notes.Clear();

        // Set default values
        picker_priority.SelectedIndex = 0;

        if (_taskId.HasValue)
        {
            _currentTask = _projectService.GetTaskWithDetails(_taskId.Value);
        }

        if (_currentTask != null)
        {
            entry_task_name.Text = _currentTask.Name ?? string.Empty;
            entry_task_description.Text = _currentTask.Description ?? string.Empty;
            entry_tags.Text = _currentTask.Tags ?? string.Empty;
            picker_priority.SelectedIndex = _currentTask.Priority;
            Title = "EDIT TASK";

            // Load existing resources
            foreach (var attachment in _currentTask.Attachments)
                Attachments.Add(attachment);

            foreach (var link in _currentTask.Links)
                Links.Add(link);

            foreach (var note in _currentTask.Notes)
                Notes.Add(note);
        }
        else
        {
            Title = "NEW TASK";
        }

        UpdateEmptyLabels();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var nameInput = entry_task_name.Text?.Trim();
        var descInput = entry_task_description.Text?.Trim();
        var tagsInput = entry_tags.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nameInput))
        {
            await DisplayAlert("Error", "Task name is required.", "OK");
            return;
        }

        var safeName = nameInput!;
        var safeDescription = descInput ?? string.Empty;
        var safeTags = tagsInput ?? string.Empty;
        var priority = picker_priority.SelectedIndex >= 0 ? picker_priority.SelectedIndex : 0;

        if (_currentTask != null)
        {
            // Update existing task
            _currentTask.Name = safeName;
            _currentTask.Description = safeDescription;
            _currentTask.Tags = safeTags;
            _currentTask.Priority = priority;
            _projectService.UpdateTask(_currentTask);

            // Sync attachments - remove deleted ones
            var existingAttachments = _projectService.GetAttachmentsForTask(_currentTask.Id);
            foreach (var existing in existingAttachments)
            {
                if (!Attachments.Any(a => a.Id == existing.Id))
                    _projectService.RemoveAttachment(existing);
            }
            // Add new ones
            foreach (var attachment in Attachments.Where(a => a.Id == 0))
            {
                attachment.SubTaskId = _currentTask.Id;
                _projectService.AddAttachment(attachment);
            }

            // Sync links
            var existingLinks = _projectService.GetLinksForTask(_currentTask.Id);
            foreach (var existing in existingLinks)
            {
                if (!Links.Any(l => l.Id == existing.Id))
                    _projectService.RemoveLink(existing);
            }
            foreach (var link in Links.Where(l => l.Id == 0))
            {
                link.SubTaskId = _currentTask.Id;
                _projectService.AddLink(link);
            }

            // Sync notes
            var existingNotes = _projectService.GetNotesForTask(_currentTask.Id);
            foreach (var existing in existingNotes)
            {
                if (!Notes.Any(n => n.Id == existing.Id))
                    _projectService.RemoveNote(existing);
            }
            foreach (var note in Notes.Where(n => n.Id == 0))
            {
                note.SubTaskId = _currentTask.Id;
                _projectService.AddNote(note);
            }
        }
        else
        {
            // Create new task
            var newTask = _projectService.AddTaskToProject(_project, safeName, safeDescription);
            
            // Update with additional fields
            newTask.Tags = safeTags;
            newTask.Priority = priority;
            _projectService.UpdateTask(newTask);

            // Add all resources to new task
            foreach (var attachment in Attachments)
            {
                attachment.SubTaskId = newTask.Id;
                _projectService.AddAttachment(attachment);
            }

            foreach (var link in Links)
            {
                link.SubTaskId = newTask.Id;
                _projectService.AddLink(link);
            }

            foreach (var note in Notes)
            {
                note.SubTaskId = newTask.Id;
                _projectService.AddNote(note);
            }
        }

        if (Navigation != null)
        {
            await Navigation.PopAsync();
            return;
        }

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (Navigation != null)
        {
            await Navigation.PopAsync();
            return;
        }

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnAddAttachmentClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "ADD ATTACHMENT",
            "CANCEL",
            null,
            "📁 PICK FILE",
            "🖼️ PICK FROM GALLERY",
            "📷 TAKE PHOTO");

        if (action == null || action == "CANCEL") return;

        try
        {
            TaskAttachment? attachment = null;

            if (action == "📁 PICK FILE")
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "SELECT FILE"
                });

                if (result != null)
                {
                    attachment = new TaskAttachment
                    {
                        FileName = result.FileName,
                        FilePath = result.FullPath,
                        FileType = GetFileTypeDisplay(result.FileName),
                        FileSize = new FileInfo(result.FullPath).Length,
                        UploadedAt = DateTime.Now
                    };
                }
            }
            else if (action == "🖼️ PICK FROM GALLERY")
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "SELECT PHOTO"
                });

                if (result != null)
                {
                    attachment = new TaskAttachment
                    {
                        FileName = result.FileName,
                        FilePath = result.FullPath,
                        FileType = GetFileTypeDisplay(result.FileName),
                        FileSize = new FileInfo(result.FullPath).Length,
                        UploadedAt = DateTime.Now
                    };
                }
            }
            else if (action == "📷 TAKE PHOTO")
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlert("Error", "Camera not supported on this device.", "OK");
                    return;
                }

                var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "TAKE PHOTO"
                });

                if (result != null)
                {
                    attachment = new TaskAttachment
                    {
                        FileName = result.FileName,
                        FilePath = result.FullPath,
                        FileType = GetFileTypeDisplay(result.FileName),
                        FileSize = new FileInfo(result.FullPath).Length,
                        UploadedAt = DateTime.Now
                    };
                }
            }

            if (attachment != null)
            {
                Attachments.Add(attachment);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not add attachment: {ex.Message}", "OK");
        }
    }

    private void OnRemoveAttachmentClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is TaskAttachment attachment)
        {
            Attachments.Remove(attachment);
        }
    }

    private async void OnOpenAttachmentClicked(object sender, EventArgs e)
    {
        if ((sender as View)?.BindingContext is TaskAttachment attachment)
        {
            await OpenAttachment(attachment);
        }
    }

    private async Task OpenAttachment(TaskAttachment attachment)
    {
        try
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(attachment.FilePath)
            });
        }
        catch
        {
            await DisplayAlert("Error", "Could not open file. It may have been moved or deleted.", "OK");
        }
    }

    private async void OnAddLinkClicked(object sender, EventArgs e)
    {
        var urlInput = await DisplayPromptAsync("ADD LINK", "Enter URL:", placeholder: "https://...", keyboard: Keyboard.Url);
        if (string.IsNullOrWhiteSpace(urlInput)) return;

        var titleInput = await DisplayPromptAsync("ADD LINK", "Enter label (optional):", placeholder: "Design doc");

        var trimmedUrl = urlInput.Trim();
        var link = new TaskLink
        {
            Url = trimmedUrl,
            Title = string.IsNullOrWhiteSpace(titleInput) ? trimmedUrl : titleInput.Trim(),
            AddedAt = DateTime.Now
        };

        Links.Add(link);
    }

    private void OnRemoveLinkClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is TaskLink link)
        {
            Links.Remove(link);
        }
    }

    private async void OnOpenLinkClicked(object sender, EventArgs e)
    {
        if ((sender as View)?.BindingContext is TaskLink link)
        {
            try
            {
                await Launcher.OpenAsync(new Uri(link.Url));
            }
            catch
            {
                await DisplayAlert("Error", "Could not open link.", "OK");
            }
        }
    }

    private async void OnAddNoteClicked(object sender, EventArgs e)
    {
        var noteInput = await DisplayPromptAsync("ADD NOTE", "Enter note content:", placeholder: "Add build hint", maxLength: 500);
        if (string.IsNullOrWhiteSpace(noteInput)) return;

        var note = new TaskNote
        {
            Content = noteInput.Trim(),
            CreatedAt = DateTime.Now
        };

        Notes.Add(note);
    }

    private void OnRemoveNoteClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is TaskNote note)
        {
            Notes.Remove(note);
        }
    }

    private static string GetFileTypeDisplay(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "FILE";
        }

        var extension = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "FILE";
        }

        var cleaned = extension.Trim('.');
        return string.IsNullOrWhiteSpace(cleaned) ? "FILE" : cleaned.ToUpperInvariant();
    }
}