using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models.OrderViewModels;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Bangazon.Controllers
{
	public class OrdersController : Controller
	{
		private readonly ApplicationDbContext _context;

		private readonly UserManager<ApplicationUser> _userManager;

		private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

		public OrdersController(ApplicationDbContext context,
					UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
			_context = context;
		}

		// GET: Orders
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Order.Include(o => o.PaymentType).Include(o => o.User);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Orders/Details/5
		//GET: Orders/Details
		/*
        Author: Mark Hale
        Purpose: Takes the current user if they are logged in and displays their open order, if no open order is available it sends them directly to the viewmodel view.
        */
		[Authorize]
		public async Task<IActionResult> Details(int? id)
		{
			OrderDetailViewModel viewModel = new OrderDetailViewModel();
			OrderLineItem lineItems = new OrderLineItem();
			var user = await GetCurrentUserAsync();

			/*
			Author: Mark Hale
			Purpose: Looks to see if an OrderId was supplied in the URL, if not then it looks to see if the current user has an open order. If there is an open order, then it uses the open order's order id as the id value. If there isn't an open order, then it returns the OrderDetailviewModel with nothing in it so that the order in the viewmodel will be null. This causes the default message "there are no items in the cart" to appear on the screen instead of the actual order.
			*/

			if (id == null)
			{
				var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);
				if (openOrder == null)
				{

					return View(viewModel);
				}
				if (openOrder != null)
				{
					id = openOrder.OrderId;
				}

			}
			/*
			Author: Mark Hale
			Purpose: Collects the open order using the provided OrderId from above and selects that we want from it using the include method. The orderProduct variable is used to actually pull in the data that the order variable uses to fill out it's product data in OrderProducts.
			*/
			var order = await _context.Order
				.Include(o => o.PaymentType)
				.Include(o => o.OrderProducts)
				.Where(o => o.User.Id == user.Id)
				.SingleOrDefaultAsync(o => o.OrderId == id);

			var orderProduct = await _context.OrderProduct
				.Include(p => p.Product)
				.ToListAsync();

			/*
			Author: Mark Hale
			Purpose: This section creates a list of OrderLineItems to add to when we loop over all of the orderProducts gathered above. The conditional checks to make sure that the OrderId for the singleOrderProduct is equal to the OrderId that was obtained above. This prevents the user from seeing all of the items in every order in the database, including other user's order items.
			*/

			List<OrderLineItem> lineItemsToadd = new List<OrderLineItem>();
			viewModel.Order = order;
			foreach (OrderProduct singleOrderProduct in orderProduct)
			{
				if (singleOrderProduct.OrderId == order.OrderId)
				{
					OrderLineItem newLineItem = new OrderLineItem();
					newLineItem.Product = singleOrderProduct.Product;
					newLineItem.Cost = singleOrderProduct.Product.Price;
					newLineItem.Units = 1;
					lineItemsToadd.Add(newLineItem);
				}
			}

			/*
        Author: Mark Hale
        Purpose: This line changes the completed lineItemsList in an enumerable since the viewModel contains an IEnumberable of LineItems and not a list.
        */

			viewModel.LineItems = lineItemsToadd.AsEnumerable();
			return View(viewModel);
		}

		// GET: Orders/Create
		public IActionResult Create()
		{
			ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber");
			ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
			return View();
		}

		// POST: Orders/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
		{
			if (ModelState.IsValid)
			{
				_context.Add(order);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
			ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
			return View(order);
		}

		// GET: Orders/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.Order.FindAsync(id);
			if (order == null)
			{
				return NotFound();
			}
			ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
			ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
			return View(order);
		}

		// POST: Orders/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
		{
			if (id != order.OrderId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(order);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!OrderExists(order.OrderId))
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
			ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
			ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
			return View(order);
		}

		// GET: Orders/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.Order
				.Include(o => o.PaymentType)
				.Include(o => o.User)
				.FirstOrDefaultAsync(m => m.OrderId == id);
			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		// POST: Orders/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var order = await _context.Order.FindAsync(id);
			_context.Order.Remove(order);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool OrderExists(int id)
		{
			return _context.Order.Any(e => e.OrderId == id);
		}
	}
}
