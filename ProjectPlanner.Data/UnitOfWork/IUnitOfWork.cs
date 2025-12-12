using ProjectPlanner.Data.Repository.IRepository;

namespace ProjectPlanner.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IProjectRepository Project { get; }
        ITaskRepository Task { get; }
        IProjectTypeRepository ProjectType { get; }
        ITaskAttachmentRepository TaskAttachment { get; }
        ITaskLinkRepository TaskLink { get; }
        ITaskNoteRepository TaskNote { get; }

        void Save();
    }
}