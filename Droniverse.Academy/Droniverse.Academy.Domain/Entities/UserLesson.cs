using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;

public class UserLesson
{
    public Guid UserLessonID { get; set; }
    public Lesson Lesson { get; set; }
    public Guid LessonID { get; set; }
    public Guid UserID { get; set; }
    public UserLessonStatus Status { get; private set; } = UserLessonStatus.INCOMPLETED;
    public float Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }

    public void SetProgress(float progress)
    {
        if (progress is < 0 or > 100)
            throw new DomainException("Progress phải nằm trong khoảng từ 0 đến 100.");

        Progress = progress;

        if (Status == UserLessonStatus.LOCKED)
            return;

        Status = Progress >= 100 ? UserLessonStatus.COMPLETED : UserLessonStatus.INCOMPLETED;
    }

    public void Complete()
    {
        Progress = 100;
        Status = UserLessonStatus.COMPLETED;
    }

    public void Lock()
    {
        if (Status == UserLessonStatus.COMPLETED)
            throw new DomainException("Cannot lock completed lesson.");

        Status = UserLessonStatus.LOCKED;
    }

    public void Unlock()
    {
        if (Status != UserLessonStatus.LOCKED)
            throw new DomainException($"Cannot unlock lesson from status {Status}");

        Status = Progress >= 100 ? UserLessonStatus.COMPLETED : UserLessonStatus.INCOMPLETED;
    }

    public void TransitionTo(UserLessonStatus targetStatus)
    {
        if (Status == targetStatus)
            return;

        switch (targetStatus)
        {
            case UserLessonStatus.INCOMPLETED:
                Unlock();
                break;
            case UserLessonStatus.COMPLETED:
                Complete();
                break;
            case UserLessonStatus.LOCKED:
                Lock();
                break;
            default:
                throw new DomainException($"Unsupported user lesson status {targetStatus}");
        }
    }
}
