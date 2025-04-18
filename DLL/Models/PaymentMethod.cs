﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class PaymentMethod
    {
        [Key]
        public Guid PaymentMethodId { get; set; }

        public string TenPhuongThuc { get; set; }

        public string MoTa { get; set; }
        // Mối quan hệ  1 - n với PaymentHistories
        public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
    }
}
