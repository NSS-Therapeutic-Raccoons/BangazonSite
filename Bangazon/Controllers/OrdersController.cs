﻿using System;
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
				.SingleOrDefaultAsync(o => o.OrderId == id);

			var orderProduct = await _context.OrderProduct
				.Include(p => p.Product)
				.Where(p => p.OrderId == order.OrderId)
				.ToListAsync();

			/*
			Author: Mark Hale
			Purpose: This section creates a list of OrderLineItems to add to when we loop over all of the orderProducts gathered above. The conditional checks to make sure that the OrderId for the singleOrderProduct is equal to the OrderId that was obtained above. This prevents the user from seeing all of the items in every order in the database, including other user's order items.
			*/

			List<OrderLineItem> lineItemsToadd = new List<OrderLineItem>();
			viewModel.Order = order;
			foreach (OrderProduct singleOrderProduct in orderProduct)
			{
				OrderLineItem newLineItem = new OrderLineItem();
				newLineItem.Product = singleOrderProduct.Product;
				newLineItem.Cost = singleOrderProduct.Product.Price;
				newLineItem.Units = 1;
				lineItemsToadd.Add(newLineItem);
			}
			viewModel.LineItems = lineItemsToadd;
			return View(viewModel);
		}
	}
}
