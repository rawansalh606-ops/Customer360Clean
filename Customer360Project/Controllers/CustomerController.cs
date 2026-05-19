using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Customer360Clean.Data;
using Customer360Clean.Models;

namespace Customer360Clean.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "الرجاء إدخال رقم الجوال";
                return View();
            }

            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == phone);

            if (customer == null)
            {
                ViewBag.Error = "لا يوجد عميل بهذا الرقم";
                return View();
            }

            var activities = await _db.Activities
                .Where(a => a.CustomerId == customer.Id)
                .OrderByDescending(a => a.LoginTime)
                .Take(10)
                .ToListAsync();

            var tickets = await _db.Tickets
                .Where(t => t.CustomerId == customer.Id && t.Status != "Closed")
                .ToListAsync();

            bool isVIP = tickets.Count > 2;

            var model = new Customer360ViewModel
            {
                Customer = customer,
                Activities = activities,
                Tickets = tickets,
                IsVIP = isVIP
            };

            return View("Customer360", model);
        }
    }
}
