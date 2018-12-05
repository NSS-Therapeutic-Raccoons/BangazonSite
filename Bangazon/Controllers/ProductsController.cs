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

		public ProductsController(ApplicationDbContext context,
						  UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
			_context = context;
		}

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

		/*
        Author: Mark Hale
        Purpose: This method takes a product and adds it to the user's order.
        */
		[Authorize]
		public async Task<IActionResult> Purchase([FromRoute] int id)
		{
			// Find the product requested
			Product productToAdd = await _context.Product.SingleOrDefaultAsync(p => p.ProductId == id);

			// Get the current user
			var user = await GetCurrentUserAsync();

			// See if the user has an open order
			var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);


			// If no order, create one, else add to existing order
			Order activeOrder;
			if (openOrder == null)
			{
				activeOrder = new Order();
				activeOrder.UserId = user.Id;
				activeOrder.PaymentTypeId = null;
				_context.Add(activeOrder);
			}
			else
			{
				activeOrder = openOrder;
			}

			//Update the OrderProduct table so that the product is officially added to the order
			OrderProduct productToAddToOrder = new OrderProduct();
			productToAddToOrder.ProductId = id;
			productToAddToOrder.OrderId = activeOrder.OrderId;
			_context.Add(productToAddToOrder);

			/*Subtract from the product's listed quantity when the order has been confirmed to avoid maximum suckiness Take the product that we found above, subtract 1 from the quantity field, update the context, save the changes to complete the update.*/
			productToAdd.Quantity = productToAdd.Quantity - 1;
			_context.Update(productToAdd);
			await _context.SaveChangesAsync();

			return RedirectToAction("Details", "Orders", new { id = activeOrder.OrderId });
		}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveCartProduct(int id)
        {
            
            Product product = await _context.Product
                                            .Where(p => p.ProductId == id)
                                            .SingleOrDefaultAsync();

            ApplicationUser user = await GetCurrentUserAsync();
            
            Order order = await _context.Order
                                        .Include(o => o.OrderProducts)
                                        .Where(o => o.UserId == user.Id && o.DateCompleted == null)
                                        .SingleOrDefaultAsync();

            OrderProduct orderProduct = order.OrderProducts
                                             .Where(op => op.OrderId == order.OrderId && op.ProductId == product.ProductId)
                                                      .Take(1).SingleOrDefault();


            _context.OrderProduct.Remove(orderProduct);

            order.OrderProducts.Remove(orderProduct);

            bool cancelOrder = false;

            if (order.OrderProducts.Count == 0) 
            {
                _context.Order.Remove(order);
                cancelOrder = true;
            }

            await _context.SaveChangesAsync();

            if (cancelOrder == true)
            {
                return RedirectToAction("Details", "Orders");
            }
            else 
            { 
                return RedirectToAction("Details", "Orders", new { id = order.OrderId });
            }

        }


        /*
        Author:     Taylor Gulley/Ricky Bruner
        Purpose:    Added the creation of the product type list if the ModelState is not valid
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
            productTypeListOptions.Insert(0, new SelectListItem
            {
                Text = "Choose a Category",
                Value = "0"
            });
            createProduct.ProductTypes = productTypeListOptions;
            
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
 
