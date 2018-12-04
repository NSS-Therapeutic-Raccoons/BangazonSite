using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.Models.ProductViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace Bangazon.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        /*
        Author:     Daniel Figueroa
        Purpose:    GetCurrentUSerAsync. Method calls to get the user.
        */
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }


        /*
            Author: Ricky Bruner
            Purpose: Method to accept the search input from the navbar, get products related to that query string, and return a view to the user based on that data.
        */
        // GET: Search Products
        [Authorize]
        public async Task<IActionResult> Search(string search)
        {
            ProductSearchViewModel viewmodel = new ProductSearchViewModel();

            viewmodel.Search = search;

            viewmodel.Products = await _context.Product
                                    .Where(p => p.Title.Contains(search))
                                    .ToListAsync();

            return View(viewmodel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        /*
        Author:     Daniel Figueroa
        Purpose:    Route is Authorized so only a logged in user can view.
        */
        /*
        Author:     Taylor Gulley
        Purpose:    Added Insert into Product Type Options for default of Choose a Category
        */
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var productTypes = await _context.ProductType.ToListAsync();

            var productTypeListOptions = new List<SelectListItem>();

            foreach (ProductType pt in productTypes)
            {
                productTypeListOptions.Add(new SelectListItem
                {
                    Value = pt.ProductTypeId.ToString(),
                    Text = pt.Label
                });
            }

            ProductCreateViewModel createViewModel = new ProductCreateViewModel();

            productTypeListOptions.Insert(0, new SelectListItem
            {
                Text = "Choose a Category",
                Value = "0"
            });
            createViewModel.ProductTypes = productTypeListOptions;
            return View(createViewModel);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*
        Author:     Daniel Figueroa
        Purpose:    Route is Authorized so only a logged in user can view. Creates a new product and asks Jeeves (calls the DB) to add the product to the table. The user & UserId instance is removed from the model, and after
                    the ModelState is valid, it makes the GetCurrentUserAsync call to put the user back into the model and UserId is reassigned.
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ProductCreateViewModel createProduct)
        {
            ModelState.Remove("Product.User");
            ModelState.Remove("Product.UserId");
            if (ModelState.IsValid)
            {
                createProduct.Product.User = await GetCurrentUserAsync();
                createProduct.Product.UserId = createProduct.Product.User.Id;
                _context.Add(createProduct.Product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = createProduct.Product.ProductId.ToString() });
            }

            return View(createProduct);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,ProductTypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
