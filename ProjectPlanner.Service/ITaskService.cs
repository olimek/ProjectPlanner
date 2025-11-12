using ProjectPlanner.Model;
using System.Collections.Generic;

namespace ProjectPlanner.Service
{
    public interface ITaskService
    {
        void AddTaskToProject(int projectId, string taskName, string description);

        void DeleteTask(int projectId, int taskId);

        void DeleteAllTasksInProject(int projectId);

        List<SubTask> GetTasksForProject(int projectId);
    }
}