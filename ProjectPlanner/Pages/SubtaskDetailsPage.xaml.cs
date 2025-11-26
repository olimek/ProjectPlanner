using System;
using ProjectPlanner.Model;
using ProjectPlanner.Service;

namespace ProjectPlanner.Pages
{
    public partial class SubtaskDetailsPage : ContentPage
    {
        private readonly IProjectService? _projectService;

        public SubtaskDetailsPage()
        {
            InitializeComponent();
            BindingContext = new SubTask();
        }

        public SubtaskDetailsPage(SubTask subtask) : this()
        {
            if (subtask != null)
            {
                BindingContext = subtask;
            }
        }

        public SubtaskDetailsPage(SubTask subtask, IProjectService projectService) : this(subtask)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }
    }
}