using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductViewModels
{


    /*
        Author: Ricky Bruner
        Purpose: Holds the search query from the navbar search input, and a collection of products that contain that query string within its Title. This view model feed the Search.cshtml in Products.
    */

    public class ProductSearchViewModel
    {
        
        public string Search { get; set; }
        
        public ICollection<Product> Products { get; set; }
    }
}
