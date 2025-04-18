﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DLL.Models;

namespace PRL_Web.Controllers
{
    public class PaymentHistoriesController : Controller
    {
        private readonly BanHangDbContext _context;

        public PaymentHistoriesController(BanHangDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> IndexHis()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem lịch sử thanh toán.";
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login", "Users");
            }

            // Lấy danh sách lịch sử thanh toán của người dùng
            var paymentHistories = await _context.PaymentHistories
                .Include(ph => ph.Order)
                .Include(ph => ph.PaymentMethod)
                .Where(ph => ph.Order != null && ph.Order.UserId == user.UserId && ph.Status == 1) // Lọc trạng thái Success
                .OrderByDescending(ph => ph.ThoiGianTT) // Sắp xếp mới nhất trước
                .ToListAsync();

            return View(paymentHistories);
        }




        public async Task<IActionResult> Index()
        {
            var banHangDbContext = _context.PaymentHistories.Include(p => p.Order).Include(p => p.PaymentMethod);
            return View(await banHangDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentHistory = await _context.PaymentHistories
                .Include(p => p.Order)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (paymentHistory == null)
            {
                return NotFound();
            }

            return View(paymentHistory);
        }

        // GET: PaymentHistories/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "MoTa");
            return View();
        }

        // POST: PaymentHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentId,OrderId,PaymentMethodId,TongTien,ThoiGianTT,Status")] PaymentHistory paymentHistory)
        {
            if (ModelState.IsValid)
            {
                paymentHistory.PaymentId = Guid.NewGuid();
                _context.Add(paymentHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", paymentHistory.OrderId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "MoTa", paymentHistory.PaymentMethodId);
            return View(paymentHistory);
        }

        // GET: PaymentHistories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentHistory = await _context.PaymentHistories.FindAsync(id);
            if (paymentHistory == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", paymentHistory.OrderId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "MoTa", paymentHistory.PaymentMethodId);
            return View(paymentHistory);
        }

        // POST: PaymentHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("PaymentId,OrderId,PaymentMethodId,TongTien,ThoiGianTT,Status")] PaymentHistory paymentHistory)
        {
            if (id != paymentHistory.PaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentHistoryExists(paymentHistory.PaymentId))
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", paymentHistory.OrderId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "MoTa", paymentHistory.PaymentMethodId);
            return View(paymentHistory);
        }

        // GET: PaymentHistories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentHistory = await _context.PaymentHistories
                .Include(p => p.Order)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (paymentHistory == null)
            {
                return NotFound();
            }

            return View(paymentHistory);
        }

        // POST: PaymentHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var paymentHistory = await _context.PaymentHistories.FindAsync(id);
            if (paymentHistory != null)
            {
                _context.PaymentHistories.Remove(paymentHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentHistoryExists(Guid id)
        {
            return _context.PaymentHistories.Any(e => e.PaymentId == id);
        }
    }
}
