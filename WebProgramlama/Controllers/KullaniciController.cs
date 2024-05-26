using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _configuration;
        readonly static stringSQL strSQL = new stringSQL();
        static int id = 0;
        static string name = "name";
        static string surname = "surname";
        static string mail2 = "email";

        public KullaniciController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Account");
            }

            var principal = ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized();
            }

            var mail = principal.FindFirstValue(ClaimTypes.Email);
            if (mail == null)
            {
                mail = mail2;
            }

            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(strSQL.SQLString);
            var context = new IdentityContext(optionsBuilder.Options);

            var kullanici = context.Users.FirstOrDefault(k => k.Email == mail);
            if (kullanici == null)
            {
                return NotFound();
            }

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

            ViewBag.Token = token;
            return View(suankiKullanici);
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
        public IActionResult Validate(string token)
        {
            ViewBag.Token = token;
            var mail = TempData["mail"] as string;
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(strSQL.SQLString);
            var context = new IdentityContext(optionsBuilder.Options);

            var kullanici = context.Users.FirstOrDefault(x => x.Email == mail);
            if (kullanici == null)
            {
                return NotFound();
            }

            return View(kullanici);
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
