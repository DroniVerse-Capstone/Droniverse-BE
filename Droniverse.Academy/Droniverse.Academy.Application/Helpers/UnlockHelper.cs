namespace Droniverse.Academy.Application.Helpers;

public static class UnlockHelper
{
    public static bool IsModuleLocked(bool previousModuleCompleted)
    {
        return !previousModuleCompleted;
    }

    public static bool IsLessonLocked(bool moduleLocked, bool previousLessonCompleted)
    {
        if (moduleLocked)
            return true;

        return !previousLessonCompleted;
    }
}
