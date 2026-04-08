using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs;

public class AddClubCourseRequestDto
{
    public Guid CourseId { get; set; }
    public int TotalQuantity { get; set; }
    public ClubCourseProfit ProfitType { get; set; } = ClubCourseProfit.PROFIT;
}

