namespace Droniverse.Academy.Application.DTO.Request;

public class ReorderModulesRequestDTO
{
    public List<ReorderModuleItemDTO> Modules { get; set; } = [];
}

public class ReorderModuleItemDTO
{
    public Guid ModuleID { get; set; }
    public int ModuleNumber { get; set; }
}
