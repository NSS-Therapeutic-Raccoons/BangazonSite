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

			/*
			Author: Mark Hale
			Purpose: Collects the open order using the provided OrderId from above and selects that we want from it using the include method. The orderProduct variable is used to actually pull in the data that the order variable uses to fill out it's product data in OrderProducts.
			*/
				var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);
				if (openOrder == null)
				{

					return View(viewModel);
				}
				if (openOrder != null)
				{
					id = openOrder.OrderId;
				}

			var order = await _context.Order
				.Include(o => o.PaymentType)
				.Include(o => o.OrderProducts)
				.ThenInclude(op => op.Product)
				.SingleOrDefaultAsync(o => o.OrderId == id && user.Id == o.User.Id);
			/*
			Author: Mark Hale
			Purpose: This section creates a list of OrderLineItems to add to when we loop over all of the orderProducts gathered above. The conditional checks to make sure that the OrderId for the singleOrderProduct is equal to the OrderId that was obtained above. This prevents the user from seeing all of the items in every order in the database, including other user's order items.
			*/

			List<OrderLineItem> lineItemsToadd = new List<OrderLineItem>();
			viewModel.Order = order;
			foreach (OrderProduct singleOrderProduct in order.OrderProducts)
			{
				OrderLineItem newLineItem = new OrderLineItem();
				newLineItem.Product = singleOrderProduct.Product;
				newLineItem.Cost = singleOrderProduct.Product.Price;
				newLineItem.Units = 1;
                newLineItem.OrderProductId = singleOrderProduct.OrderProductId;
				lineItemsToadd.Add(newLineItem);
			}
			viewModel.LineItems = lineItemsToadd;
			return View(viewModel);
		}

        /*
            * Author: Ricky Bruner
            * Purpose: Deletes an order if a user decides to cancel it.
        */
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id)
        {
            Order order = await _context.Order
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .SingleOrDefaultAsync(o => o.OrderId == id);

            foreach (OrderProduct op in order.OrderProducts) 
            {
                op.Product.Quantity = op.Product.Quantity + 1;
                _context.Product.Update(op.Product);
                _context.OrderProduct.Remove(op);
            }

            _context.Order.Remove(order);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details");

        }

        /*
            Author: Taylor Gulley
            Purpose: To retrieve the payments types associated with the current user to attach to an order.
        */
        // Get
        [Authorize]
        public async Task<IActionResult> ChoosePaymentType(int id)
        {
            var user = await GetCurrentUserAsync();

            var paymentTypes = await _context.PaymentType.Where(pt => pt.User == user).ToListAsync();
            var order = await _context.Order.SingleOrDefaultAsync(o => o.OrderId == id);

            var paymentTypeListOptions = new List<SelectListItem>();

            foreach (PaymentType pt in paymentTypes)
            {
                paymentTypeListOptions.Add(new SelectListItem
                {
                    Value = pt.PaymentTypeId.ToString(),
                    Text = pt.Description
                });
            }

            OrderPaymentTypesViewModel choosePaymentTypeViewModel = new OrderPaymentTypesViewModel();

            paymentTypeListOptions.Insert(0, new SelectListItem
            {
                Text = "Choose a Payment Type",
                Value = "0"
            });

            choosePaymentTypeViewModel.Order = order;
            choosePaymentTypeViewModel.PaymentTypes = paymentTypeListOptions;
            return View(choosePaymentTypeViewModel);
        }

        /*
            Author: Taylor Gulley
            Purpose: To attach the chosen Payment TypeId to the order. Also if the user doesn't choose a payment type they will be prompted to choose one.
        */
        // Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChoosePaymentType(OrderPaymentTypesViewModel choosePaymentTypeViewModel, int id)
        {
            if (ModelState.IsValid)
            {
                Order order = await _context.Order.SingleOrDefaultAsync(o => o.OrderId == id);
                order.DateCompleted = DateTime.Today;
                order.PaymentTypeId = choosePaymentTypeViewModel.PaymentTypeId;
                _context.Order.Update(order);
                await _context.SaveChangesAsync();
                return View("ThankYou");
            }
            var user = await GetCurrentUserAsync();

            var paymentTypes = await _context.PaymentType.Where(pt => pt.User == user).ToListAsync();
            var viewOrder = await _context.Order.SingleOrDefaultAsync(o => o.OrderId == id);

            var paymentTypeListOptions = new List<SelectListItem>();

            foreach (PaymentType pt in paymentTypes)
            {
                paymentTypeListOptions.Add(new SelectListItem
                {
                    Value = pt.PaymentTypeId.ToString(),
                    Text = pt.Description
                });
            }

            OrderPaymentTypesViewModel viewModel = new OrderPaymentTypesViewModel();

            paymentTypeListOptions.Insert(0, new SelectListItem
            {
                Text = "Choose a Payment Type",
                Value = "0"
            });

            viewModel.Order = viewOrder;
            viewModel.PaymentTypes = paymentTypeListOptions;
            return View(viewModel);
        }

    }
}
