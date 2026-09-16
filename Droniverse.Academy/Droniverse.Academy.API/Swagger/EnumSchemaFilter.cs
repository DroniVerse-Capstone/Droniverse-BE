using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Droniverse.Academy.API.Swagger;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!enumType.IsEnum)
        {
            return;
        }

        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType).Cast<object>().ToArray();

        var detailLines = new List<string>(names.Length);
        var enumNames = new OpenApiArray();
        var enumDescriptions = new OpenApiArray();

        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var value = Convert.ToInt64(values[i]);
            var description = GetMemberDescription(enumType, name);

            var line = string.IsNullOrWhiteSpace(description)
                ? $"- {name} = {value}"
                : $"- {name} = {value}: {description}";

            detailLines.Add(line);
            enumNames.Add(new OpenApiString(name));
            enumDescriptions.Add(new OpenApiString(description ?? string.Empty));
        }

        var enumDetail = "Allowed enum values:\n" + string.Join("\n", detailLines);
        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? enumDetail
            : $"{schema.Description}\n\n{enumDetail}";

        schema.Extensions["x-enumNames"] = enumNames;
        schema.Extensions["x-enumDescriptions"] = enumDescriptions;
    }

    private static string? GetMemberDescription(Type enumType, string memberName)
    {
        var member = enumType.GetMember(memberName, BindingFlags.Public | BindingFlags.Static).FirstOrDefault();
        if (member == null)
        {
            return null;
        }

        var descriptionAttribute = member.GetCustomAttribute<DescriptionAttribute>();
        if (!string.IsNullOrWhiteSpace(descriptionAttribute?.Description))
        {
            return descriptionAttribute.Description;
        }

        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrWhiteSpace(displayAttribute?.Description))
        {
            return displayAttribute.Description;
        }

        if (!string.IsNullOrWhiteSpace(displayAttribute?.Name))
        {
            return displayAttribute.Name;
        }

        return null;
    }
}
