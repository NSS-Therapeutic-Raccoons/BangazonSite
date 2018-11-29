using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductTypeViewModels
{
    public class ProductListViewModel
    {
        public ProductType ProductType { get; set; }

        [Display(Name = "List of Products")]
        public List<Product> ProductList { get; set; }

    }
}
