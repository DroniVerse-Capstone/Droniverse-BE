namespace Droniverse.Academy.Application.Helpers;

public static class ProgressHelper
{
    public static float CalculateProgress(int completed, int total)
    {
        if (total <= 0)
            return 0;

        var progress = (float)completed / total * 100f;
        return MathF.Round(progress, 2);
    }
}
