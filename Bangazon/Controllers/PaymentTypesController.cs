using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Bangazon.Views
{
	public class PaymentTypesController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public PaymentTypesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
			_context = context;
		}

		private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

		// GET: PaymentTypes
		[Authorize]
		public async Task<IActionResult> Index()
		{
			var user = await GetCurrentUserAsync();
			var applicationDbContext = _context.PaymentType
			.Include(p => p.User)
			.Where(p => p.UserId == user.Id);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: PaymentTypes/Create
		[Authorize]
		public IActionResult Create()
		{
			ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
			return View();
		}

		// POST: PaymentTypes/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("PaymentTypeId,DateCreated,Description,AccountNumber,UserId")] PaymentType paymentType)
		{
			ModelState.Remove("Product.User");
			ModelState.Remove("Product.UserId");
			if (ModelState.IsValid)
			{
				paymentType.User = await GetCurrentUserAsync();
				paymentType.UserId = paymentType.User.Id;
				_context.Add(paymentType);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", paymentType.UserId);
			return View(paymentType);
		}

		

		// GET: PaymentTypes/Delete/5
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			var user = await GetCurrentUserAsync();

			var paymentType = await _context.PaymentType
				.Include(p => p.User)
				.Include(p => p.Orders)
				.FirstOrDefaultAsync(m => m.PaymentTypeId == id && m.UserId == user.Id);
			if (paymentType.Orders != null)
			{
				foreach (Order order in paymentType.Orders)
				{
					_context.Remove(order);
				}
			}
			_context.PaymentType.Remove(paymentType);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "PaymentTypes");
		}

		private bool PaymentTypeExists(int id)
		{
			return _context.PaymentType.Any(e => e.PaymentTypeId == id);
		}
	}
}
