using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class SubtaskDetailsPage : ContentPage
    {
        private readonly IProjectService? _projectService;

        private SubTask _subtask;
        private bool _isHandlingToggle = false;
        private bool _isEditMode;

        public SubtaskDetailsPage()
        {
            InitializeComponent();
            _subtask = new SubTask();
            _projectService = null;
            BindingContext = _subtask;
        }

        public SubtaskDetailsPage(SubTask subtask, IProjectService projectService)
        {
            InitializeComponent();
            _subtask = subtask;
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            BindingContext = _subtask;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReloadPage();
        }

        private void ReloadPage()
        {
            if (_projectService == null || _subtask == null || _subtask.Id == 0 || !_subtask.ProjectId.HasValue)
            {
                SetEditMode(false);
                return;
            }

            var refreshedTask = _projectService.GetTaskWithDetails(_subtask.Id);

            if (refreshedTask != null)
            {
                _subtask = refreshedTask;
                BindingContext = _subtask;
            }

            UpdateDisplayLabels();
            LoadResources();
            SetEditMode(false);
        }

        private void UpdateDisplayLabels()
        {
            string[] priorityNames = { "NONE", "LOW", "MEDIUM", "HIGH", "URGENT", "CRITICAL" };
            lbl_priority.Text = _subtask.Priority >= 0 && _subtask.Priority < priorityNames.Length
                ? priorityNames[_subtask.Priority]
                : "NONE";

            lbl_due_date.Text = _subtask.DueDate.HasValue
                ? _subtask.DueDate.Value.ToString("dd MMM yyyy")
                : "NOT SET";

            lbl_tags.Text = string.IsNullOrWhiteSpace(_subtask.Tags) ? "NO TAGS" : _subtask.Tags.ToUpper();

            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
            if (_subtask.IsDone)
            {
                switch_status.IsToggled = true;
                lbl_status_text.Text = "COMPLETED";
                lbl_status_text.TextColor = (Color)Application.Current!.Resources["NeonAccent"];
            }
            else
            {
                switch_status.IsToggled = false;
                lbl_status_text.Text = "MARK AS COMPLETE";
                lbl_status_text.TextColor = (Color)Application.Current!.Resources["TextSecondary"];
            }
        }

        private void OnStatusSwitchToggled(object? sender, ToggledEventArgs e)
        {
            if (_isHandlingToggle) return;
            if (_projectService == null) return;

            _isHandlingToggle = true;

            try
            {
                _subtask.IsDone = e.Value;
                _projectService.UpdateTask(_subtask);

                if (_subtask.IsDone)
                {
                    lbl_status_text.Text = "COMPLETED";
                    lbl_status_text.TextColor = (Color)Application.Current!.Resources["NeonAccent"];
                }
                else
                {
                    lbl_status_text.Text = "MARK AS COMPLETE";
                    lbl_status_text.TextColor = (Color)Application.Current!.Resources["TextSecondary"];
                }
            }
            finally
            {
                _isHandlingToggle = false;
            }
        }

        private void LoadResources()
        {
            if (_projectService == null || _subtask == null) return;

            attachments_list.Children.Clear();
            var attachments = _projectService.GetAttachmentsForTask(_subtask.Id);
            lbl_no_attachments.IsVisible = attachments.Count == 0;

            foreach (var attachment in attachments)
            {
                var itemView = CreateAttachmentView(attachment);
                attachments_list.Children.Add(itemView);
            }

            links_list.Children.Clear();
            var links = _projectService.GetLinksForTask(_subtask.Id);
            lbl_no_links.IsVisible = links.Count == 0;

            foreach (var link in links)
            {
                var itemView = CreateLinkView(link);
                links_list.Children.Add(itemView);
            }

            notes_list.Children.Clear();
            var notes = _projectService.GetNotesForTask(_subtask.Id);
            lbl_no_notes.IsVisible = notes.Count == 0;

            foreach (var note in notes)
            {
                var itemView = CreateNoteView(note);
                notes_list.Children.Add(itemView);
            }
        }

        private View CreateAttachmentView(TaskAttachment attachment)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 12
            };

            var stack = new VerticalStackLayout { Spacing = 2 };
            
            var fileNameLabel = new Label
            {
                Text = attachment.FileName,
                FontFamily = "TechFont",
                FontSize = 15,
                TextColor = (Color)Application.Current!.Resources["NeonAccent"],
                TextDecorations = TextDecorations.Underline
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
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
                    await DisplayAlert("Error", "Could not open file.", "OK");
                }
            };
            fileNameLabel.GestureRecognizers.Add(tapGesture);

            stack.Children.Add(fileNameLabel);
            stack.Children.Add(new Label
            {
                Text = attachment.FilePath,
                FontFamily = "TechFont",
                FontSize = 12,
                TextColor = (Color)Application.Current!.Resources["TextSecondary"]
            });

            var removeBtn = new Button
            {
                Text = "REMOVE",
                FontFamily = "HeaderFont",
                FontSize = 12,
                HeightRequest = 32,
                BackgroundColor = Colors.Transparent,
                BorderColor = (Color)Application.Current!.Resources["Magenta"],
                BorderWidth = 1,
                TextColor = (Color)Application.Current!.Resources["Magenta"],
                CommandParameter = attachment
            };
            removeBtn.Clicked += OnRemoveAttachmentClicked;

            grid.Add(stack, 0, 0);
            grid.Add(removeBtn, 1, 0);

            return grid;
        }

        private View CreateLinkView(TaskLink link)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 12
            };

            var stack = new VerticalStackLayout { Spacing = 2 };

            var titleLabel = new Label
            {
                Text = link.Title,
                FontFamily = "TechFont",
                FontSize = 15,
                TextColor = (Color)Application.Current!.Resources["NeonAccent"],
                TextDecorations = TextDecorations.Underline
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                try
                {
                    await Launcher.OpenAsync(new Uri(link.Url));
                }
                catch
                {
                    await DisplayAlert("Error", "Could not open link.", "OK");
                }
            };
            titleLabel.GestureRecognizers.Add(tapGesture);

            stack.Children.Add(titleLabel);
            stack.Children.Add(new Label
            {
                Text = link.Url,
                FontFamily = "TechFont",
                FontSize = 12,
                TextColor = (Color)Application.Current!.Resources["TextSecondary"]
            });

            var removeBtn = new Button
            {
                Text = "REMOVE",
                FontFamily = "HeaderFont",
                FontSize = 12,
                HeightRequest = 32,
                BackgroundColor = Colors.Transparent,
                BorderColor = (Color)Application.Current!.Resources["Magenta"],
                BorderWidth = 1,
                TextColor = (Color)Application.Current!.Resources["Magenta"],
                CommandParameter = link
            };
            removeBtn.Clicked += OnRemoveLinkClicked;

            grid.Add(stack, 0, 0);
            grid.Add(removeBtn, 1, 0);

            return grid;
        }

        private View CreateNoteView(TaskNote note)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 12
            };

            var stack = new VerticalStackLayout { Spacing = 2 };
            stack.Children.Add(new Label
            {
                Text = note.Content,
                FontFamily = "TechFont",
                FontSize = 15,
                TextColor = (Color)Application.Current!.Resources["TextPrimary"],
                LineBreakMode = LineBreakMode.WordWrap
            });
            stack.Children.Add(new Label
            {
                Text = $"CREATED {note.CreatedAt:dd MMM yyyy HH:mm}",
                FontFamily = "TechFont",
                FontSize = 12,
                TextColor = (Color)Application.Current!.Resources["TextSecondary"]
            });

            var removeBtn = new Button
            {
                Text = "REMOVE",
                FontFamily = "HeaderFont",
                FontSize = 12,
                HeightRequest = 32,
                BackgroundColor = Colors.Transparent,
                BorderColor = (Color)Application.Current!.Resources["Magenta"],
                BorderWidth = 1,
                TextColor = (Color)Application.Current!.Resources["Magenta"],
                CommandParameter = note
            };
            removeBtn.Clicked += OnRemoveNoteClicked;

            grid.Add(stack, 0, 0);
            grid.Add(removeBtn, 1, 0);

            return grid;
        }

        private void SetEditMode(bool enable)
        {
            _isEditMode = enable;

            view_mode_content.IsVisible = !enable;
            edit_mode_content.IsVisible = enable;
            btn_edit.IsVisible = !enable;
            btn_delete.IsVisible = !enable;
            btn_save.IsVisible = enable;
            btn_cancel.IsVisible = enable;

            if (enable)
            {
                entry_task_name.Text = _subtask?.Name ?? string.Empty;
                editor_description.Text = _subtask?.Description ?? string.Empty;
                entry_tags.Text = _subtask?.Tags ?? string.Empty;
                picker_priority.SelectedIndex = _subtask?.Priority ?? 0;
                if (_subtask?.DueDate.HasValue == true)
                {
                    picker_due_date.Date = _subtask.DueDate.Value;
                }
                else
                {
                    picker_due_date.Date = DateTime.Today;
                }
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            SetEditMode(!_isEditMode);
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_projectService == null)
            {
                await DisplayAlert("Error", "Project service is not available.", "OK");
                return;
            }

            var updatedName = entry_task_name.Text?.Trim();
            var updatedDescription = editor_description.Text?.Trim();
            var updatedTags = entry_tags.Text?.Trim();

            if (string.IsNullOrWhiteSpace(updatedName))
            {
                await DisplayAlert("Error", "Task name is required.", "OK");
                return;
            }

            _subtask.Name = updatedName;
            _subtask.Description = updatedDescription ?? string.Empty;
            _subtask.Tags = updatedTags ?? string.Empty;
            _subtask.Priority = picker_priority.SelectedIndex >= 0 ? picker_priority.SelectedIndex : 0;
            _subtask.DueDate = picker_due_date.Date;

            _projectService.UpdateTask(_subtask);

            SetEditMode(false);
            ReloadPage();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (_projectService == null) return;

            bool confirm = await DisplayAlert("DELETE",
                "Are you sure you want to delete this task?",
                "DELETE", "CANCEL");

            if (confirm)
            {
                _projectService.DeleteTask(_subtask);
                await Navigation.PopAsync();
            }
        }

        private async void OnAddAttachmentClicked(object sender, EventArgs e)
        {
            if (_projectService == null) return;

            var action = await DisplayActionSheet(
                "ADD ATTACHMENT",
                "CANCEL",
                null,
                "?? PICK FILE",
                "??? PICK FROM GALLERY",
                "?? TAKE PHOTO");

            if (action == null || action == "CANCEL") return;

            try
            {
                TaskAttachment? attachment = null;

                if (action == "?? PICK FILE")
                {
                    var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "SELECT FILE" });
                    if (result != null)
                    {
                        attachment = new TaskAttachment
                        {
                            FileName = result.FileName,
                            FilePath = result.FullPath,
                            FileType = GetFileTypeDisplay(result.FileName),
                            FileSize = new FileInfo(result.FullPath).Length,
                            UploadedAt = DateTime.Now,
                            SubTaskId = _subtask.Id
                        };
                    }
                }
                else if (action == "??? PICK FROM GALLERY")
                {
                    var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "SELECT PHOTO" });
                    if (result != null)
                    {
                        attachment = new TaskAttachment
                        {
                            FileName = result.FileName,
                            FilePath = result.FullPath,
                            FileType = GetFileTypeDisplay(result.FileName),
                            FileSize = new FileInfo(result.FullPath).Length,
                            UploadedAt = DateTime.Now,
                            SubTaskId = _subtask.Id
                        };
                    }
                }
                else if (action == "?? TAKE PHOTO")
                {
                    if (!MediaPicker.Default.IsCaptureSupported)
                    {
                        await DisplayAlert("Error", "Camera not supported.", "OK");
                        return;
                    }

                    var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "TAKE PHOTO" });
                    if (result != null)
                    {
                        attachment = new TaskAttachment
                        {
                            FileName = result.FileName,
                            FilePath = result.FullPath,
                            FileType = GetFileTypeDisplay(result.FileName),
                            FileSize = new FileInfo(result.FullPath).Length,
                            UploadedAt = DateTime.Now,
                            SubTaskId = _subtask.Id
                        };
                    }
                }

                if (attachment != null)
                {
                    _projectService.AddAttachment(attachment);
                    LoadResources();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not add attachment: {ex.Message}", "OK");
            }
        }

        private void OnRemoveAttachmentClicked(object? sender, EventArgs e)
        {
            if (_projectService == null) return;
            if ((sender as Button)?.CommandParameter is TaskAttachment attachment)
            {
                _projectService.RemoveAttachment(attachment);
                LoadResources();
            }
        }

        private async void OnAddLinkClicked(object sender, EventArgs e)
        {
            if (_projectService == null) return;

            var urlInput = await DisplayPromptAsync("ADD LINK", "Enter URL:", placeholder: "https://...", keyboard: Keyboard.Url);
            if (string.IsNullOrWhiteSpace(urlInput)) return;

            var titleInput = await DisplayPromptAsync("ADD LINK", "Enter label (optional):", placeholder: "Design doc");

            var trimmedUrl = urlInput.Trim();
            var link = new TaskLink
            {
                Url = trimmedUrl,
                Title = string.IsNullOrWhiteSpace(titleInput) ? trimmedUrl : titleInput.Trim(),
                AddedAt = DateTime.Now,
                SubTaskId = _subtask.Id
            };

            _projectService.AddLink(link);
            LoadResources();
        }

        private void OnRemoveLinkClicked(object? sender, EventArgs e)
        {
            if (_projectService == null) return;
            if ((sender as Button)?.CommandParameter is TaskLink link)
            {
                _projectService.RemoveLink(link);
                LoadResources();
            }
        }

        private async void OnAddNoteClicked(object sender, EventArgs e)
        {
            if (_projectService == null) return;

            var noteInput = await DisplayPromptAsync("ADD NOTE", "Enter note content:", placeholder: "Build hint...", maxLength: 500);
            if (string.IsNullOrWhiteSpace(noteInput)) return;

            var note = new TaskNote
            {
                Content = noteInput.Trim(),
                CreatedAt = DateTime.Now,
                SubTaskId = _subtask.Id
            };

            _projectService.AddNote(note);
            LoadResources();
        }

        private void OnRemoveNoteClicked(object? sender, EventArgs e)
        {
            if (_projectService == null) return;
            if ((sender as Button)?.CommandParameter is TaskNote note)
            {
                _projectService.RemoveNote(note);
                LoadResources();
            }
        }

        private static string GetFileTypeDisplay(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "FILE";
            var extension = System.IO.Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(extension)) return "FILE";
            var cleaned = extension.Trim('.');
            return string.IsNullOrWhiteSpace(cleaned) ? "FILE" : cleaned.ToUpperInvariant();
        }
    }
}
