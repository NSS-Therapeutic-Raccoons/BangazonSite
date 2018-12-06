using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderPaymentTypesViewModel
    {
        public Order Order { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a Payment Type")]
        public int PaymentTypeId { get; set; }
        public List<SelectListItem> PaymentTypes { get; set; }
    }
}
