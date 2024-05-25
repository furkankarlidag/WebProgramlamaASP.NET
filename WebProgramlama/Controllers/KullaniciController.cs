using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using WebProgramlama.Data;
using WebProgramlama.Models;
using Zakuska_AI.Data;

namespace WebProgramlama.Controllers
{
    public class KullaniciController : Controller
    {
        private UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private IdentityContext _context;
        readonly static stringSQL strSQL = new stringSQL();
        static int id = 0;
        static string name = "name";
        static string surname = "surname";
        static string mail2 = "email";

        public KullaniciController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var mail = TempData["mail"] as string;
            if(mail == null) {
                mail = mail2;
            }
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(strSQL.SQLString);
            var context = new IdentityContext(optionsBuilder.Options);

            var kullanici = context.Users
                      .FirstOrDefault(k => k.Email == mail);
            id = kullanici.Id;
            name = kullanici.FirstName;
            surname = kullanici.LastName;
            mail2 = kullanici.Email;
            User suankiKullanici = new User
            {
                FirstName = kullanici.FirstName,
                LastName = kullanici.LastName,
                Email = kullanici.Email,
            };
            return View(suankiKullanici);
        }
        public IActionResult Validate()
        {
            var mail = TempData["mail"] as string;
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(strSQL.SQLString);
            var context = new IdentityContext(optionsBuilder.Options);
            var searchedUser = context.Users.FirstOrDefault(x => x.Email == mail);

            return View(searchedUser);
        }

        [HttpPost]
        public async Task<IActionResult> BilgiGuncelleme(User model)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(strSQL.SQLString);
            var context = new IdentityContext(optionsBuilder.Options);
            var kullaniciBul = context.Users.FirstOrDefault(k => k.Email == model.Email);

            if (kullaniciBul != null)
            {

                kullaniciBul.FirstName = model.FirstName;
                kullaniciBul.LastName = model.LastName;
                kullaniciBul.Email = model.Email;
                context.SaveChanges();
            }
            TempData["mail"] = mail2;
            return RedirectToAction("Index");
        }

        public IActionResult AddProduct()
        {
            htmlProductModel htmlProductModel = new htmlProductModel();
            htmlProductModel.OwnerID = id;
            htmlProductModel.OwnerSurname = surname;
            htmlProductModel.OwnerName = name;
            return View(htmlProductModel);
        }
        [HttpPost]
        public async Task<IActionResult> AddProductAsync(string ProductName,int Quantity)
        {
            _context.Products.Add(new Product
            {
                ProductName = ProductName,
                ProductOwnerID = id,
                Amount = Quantity,
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";

            return RedirectToAction("Products"); 
        }

        public IActionResult Products()
        {
            listProducts list = new listProducts();
            list.Products = _context.Products.ToList();
            return View(list);
        }
        [HttpPost]
        public IActionResult UpdateProduct(int id, string name, int quantity)
        {
            // Ürünü veritabanında bul ve güncelle
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.ProductName = name;
                product.Amount = quantity;
                _context.SaveChanges();
            }

            // Güncelleme işleminden sonra kullanıcıyı aynı sayfaya yönlendir
            return RedirectToAction("Products");
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
            return RedirectToAction("Products");
        }
    }
}
