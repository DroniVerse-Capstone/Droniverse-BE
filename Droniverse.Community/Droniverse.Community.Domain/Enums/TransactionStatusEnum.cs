using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum TransactionStatusEnum : byte
    {
        PENDING = 0, //ĐANG CHỜ XỬ LÝ
        SUCCESS = 1, //HOÀN THÀNH
        FAILED = 2, //THẤT BẠI
    }
}
