namespace Droniverse.Academy.Application.Common.Extensions;

public static class GuidCollectionExtensions
{
    public static List<Guid> ToDistinctValidIds(this IEnumerable<Guid>? ids)
    {
        return ids?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? [];
    }

    public static List<Guid> ToDistinctValidIds(this IEnumerable<Guid?>? ids)
    {
        return ids?
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList() ?? [];
    }
}
