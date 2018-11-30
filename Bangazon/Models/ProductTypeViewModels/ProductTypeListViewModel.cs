using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductTypeViewModels
{
    
    /*
        Author: Ricky Bruner
        Purpose: Viewmodel for the index view of ProductType, only currently meant to hold a list of GroupedProducts
    */
    public class ProductTypeListViewModel
    {

       public List<GroupedProducts> GroupedProducts { get; set; }

    }
}
