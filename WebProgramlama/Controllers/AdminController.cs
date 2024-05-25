using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebProgramlama.Data;
using WebProgramlama.Models;
using Zakuska_AI.Data;

namespace WebProgramlama.Controllers
{
    public class AdminController : Controller
    {
        stringSQL strSQL = new stringSQL();
        private UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private IdentityContext _context;

        public AdminController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Index()
        {
            adminHtmlModel adminHtmlModel = new adminHtmlModel();
            adminHtmlModel.users = _context.Users.ToList();
            adminHtmlModel.products = _context.Products.ToList();
            return View(adminHtmlModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.role = "validate"; 
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kullanıcı başarıyla onaylandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> notApproveUser(string mail)
        {
            var user = await _userManager.FindByEmailAsync(mail);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Kullanıcının başarıyla onayı başarıyla kaldırıldı.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                }
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult UpdateProduct(int id, string productName, int amount)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.ProductName = productName;
                product.Amount = amount;
                _context.SaveChanges();
            }
            return RedirectToAction("Index"); 
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index"); 
        }

    }
}
