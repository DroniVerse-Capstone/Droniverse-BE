using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public class SearchRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Trang hiện tại phải lớn hơn hoặc bằng 1.")]
        [DefaultValue(1)]
        public int CurrentPage { get; set; } = 1;

        [Range(4, 20, ErrorMessage = "Kích thước trang phải từ 5 đến 20.")]
        [DefaultValue(5)]
        public int PageSize { get; set; } = 5;
    }
}
