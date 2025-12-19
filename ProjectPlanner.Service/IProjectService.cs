using ProjectPlanner.Model;

namespace ProjectPlanner.Service
{
    public interface IProjectService
    {
        void AddProject(string name);

        Project AddProject(string name, string? description, ProjectType? type);

        void DeleteProject(Project project);

        void AddTaskToProject(int projectId, string taskName, string? description = null);

        SubTask AddTaskToProject(Project project, string name, string? description = null);

        List<SubTask> GetTasksForProject(int projectId);

        SubTask? GetTaskById(int taskId);

        SubTask? GetTaskWithDetails(int taskId);

        void UpdateTask(SubTask task);

        void DeleteAllProjects();

        List<Project> GetAllProjects();

        void DeleteTask(SubTask task);

        Project ConvertSubTaskToProject(SubTask task);

        void UpdateProject(int projectId, string name, string? description = null, string? projectType = null);

        void UpdateProject(Project project);

        Project GetProjectByID(int projectId);

        // Attachments
        void AddAttachment(TaskAttachment attachment);
        void RemoveAttachment(TaskAttachment attachment);
        List<TaskAttachment> GetAttachmentsForTask(int taskId);

        // Links
        void AddLink(TaskLink link);
        void RemoveLink(TaskLink link);
        List<TaskLink> GetLinksForTask(int taskId);

        // Notes
        void AddNote(TaskNote note);
        void RemoveNote(TaskNote note);
        List<TaskNote> GetNotesForTask(int taskId);
    }
}