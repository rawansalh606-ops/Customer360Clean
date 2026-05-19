using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Customer360Clean.Data;
using Customer360Clean.Models;
using OfficeOpenXml;

namespace Customer360Clean.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ========== لوحة التحكم الرئيسية ==========
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalCustomers = await _db.Customers.CountAsync();
        ViewBag.ComplaintsCount = await _db.Tickets.CountAsync(t => t.TicketType == "شكوى");
        ViewBag.OpenTickets = await _db.Tickets.CountAsync(t => t.Status == "Open");
        ViewBag.ActiveCustomers = ViewBag.TotalCustomers;

        var complaintsList = await _db.Tickets
            .Include(t => t.Customer)
            .Where(t => t.TicketType == "شكوى")
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new ComplaintViewModel
            {
                CustomerName = t.Customer != null ? t.Customer.FullName : "غير معروف",
                CustomerPhone = t.Customer != null ? t.Customer.PhoneNumber : "",
                Description = t.Description,
                Status = t.Status,
                DaysOld = (DateTime.Now - t.CreatedAt).Days,
                PulseScore = new Random().Next(20, 90)
            })
            .ToListAsync();

        ViewBag.LatestComplaints = complaintsList;
        ViewBag.LastSearchedCustomerName = "محمد العلي";
        ViewBag.LastSearchedCustomerPhone = "0501234567";
        ViewBag.LastCustomerPulse = 72;
        ViewBag.LastCustomerNote = "عميل مستقر — لكن لديه شكوى مفتوحة منذ 12 يوم";

        var recentActivities = new List<ActivityItem>
        {
            new ActivityItem { Text = "شكوى جديدة من علي الشمري", TimeAgo = "منذ 20 دقيقة", DotColor = "#f87171" },
            new ActivityItem { Text = "تم إغلاق محضر فاطمة الحربي", TimeAgo = "منذ ساعة", DotColor = "#4ade80" },
            new ActivityItem { Text = "تقرير شهري مايو جاهز", TimeAgo = "منذ 3 ساعات", DotColor = "#93c5fd" }
        };
        ViewBag.RecentActivities = recentActivities;

        return View();
    }

    // ========== صفحة الشكاوي ==========
    public async Task<IActionResult> Complaints()
    {
        var complaints = await _db.Tickets
            .Include(t => t.Customer)
            .Where(t => t.TicketType == "شكوى")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.Complaints = complaints;

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
        }

        return View();
    }

    // ========== إضافة شكوى جديدة ==========
    [HttpPost]
    public async Task<IActionResult> AddComplaint(int customerId, string description)
    {
        var customer = await _db.Customers.FindAsync(customerId);
        if (customer == null)
        {
            TempData["SuccessMessage"] = "❌ رقم العميل غير موجود";
            return RedirectToAction("Complaints");
        }

        var newComplaint = new Ticket
        {
            CustomerId = customerId,
            TicketType = "شكوى",
            Status = "Open",
            Description = description,
            CreatedAt = DateTime.Now
        };

        _db.Tickets.Add(newComplaint);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "✅ تم إضافة الشكوى بنجاح!";

        return RedirectToAction("Complaints");
    }

    // ========== تصدير إلى Excel ==========
    public async Task<IActionResult> ExportToExcel()
    {
        var complaints = await _db.Tickets
            .Include(t => t.Customer)
            .Where(t => t.TicketType == "شكوى")
            .ToListAsync();

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("الشكاوي");

            worksheet.Cells[1, 1].Value = "العميل";
            worksheet.Cells[1, 2].Value = "رقم الجوال";
            worksheet.Cells[1, 3].Value = "الوصف";
            worksheet.Cells[1, 4].Value = "التاريخ";
            worksheet.Cells[1, 5].Value = "الحالة";

            int row = 2;
            foreach (var c in complaints)
            {
                worksheet.Cells[row, 1].Value = c.Customer?.FullName;
                worksheet.Cells[row, 2].Value = c.Customer?.PhoneNumber;
                worksheet.Cells[row, 3].Value = c.Description;
                worksheet.Cells[row, 4].Value = c.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = c.Status;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الشكاوي.xlsx");
        }
    }

    // ========== التقارير ==========
    public async Task<IActionResult> Reports()
    {
        var totalCustomers = await _db.Customers.CountAsync();
        var totalComplaints = await _db.Tickets.CountAsync(t => t.TicketType == "شكوى");
        var openTickets = await _db.Tickets.CountAsync(t => t.Status == "Open");
        var closedTickets = await _db.Tickets.CountAsync(t => t.Status == "Closed");

        ViewBag.TotalCustomers = totalCustomers;
        ViewBag.TotalComplaints = totalComplaints;
        ViewBag.OpenTickets = openTickets;
        ViewBag.ClosedTickets = closedTickets;

        return View();
    }

    // ========== العملاء المميزين ==========
    public async Task<IActionResult> VIPCustomers()
    {
        var vipCustomers = await _db.Customers
            .Where(c => _db.Tickets.Count(t => t.CustomerId == c.Id && t.TicketType == "شكوى") > 2)
            .ToListAsync();

        return View(vipCustomers);
    }

    // ========== البلاك ليست ==========
    public async Task<IActionResult> Blacklist()
    {
        var blacklist = await _db.Customers
            .Where(c => _db.Tickets.Count(t => t.CustomerId == c.Id && t.TicketType == "شكوى") >= 5)
            .ToListAsync();

        return View(blacklist);
    }

    // ========== تعديل تذكرة ==========
    [HttpGet]
    public async Task<IActionResult> EditTicket(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> EditTicket(Ticket ticket)
    {
        if (ModelState.IsValid)
        {
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "✅ تم التعديل بنجاح";
            return RedirectToAction("Complaints");
        }
        return View(ticket);
    }

    // ========== حذف تذكرة ==========
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket != null)
        {
            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "✅ تم الحذف بنجاح";
        }
        return RedirectToAction("Complaints");
    }
}

// ViewModels
public class ComplaintViewModel
{
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public int DaysOld { get; set; }
    public int PulseScore { get; set; }
}

public class ActivityItem
{
    public string Text { get; set; } = "";
    public string TimeAgo { get; set; } = "";
    public string DotColor { get; set; } = "";
}