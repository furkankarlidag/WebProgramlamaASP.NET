using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProgramlama.Data;
using WebProgramlama.Models;
using Zakuska_AI.Data;

namespace WebProgramlama.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private IdentityContext _Context;
        stringSQL strSQL = new stringSQL();



        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _Context = context;
        }
        public IActionResult SignIn()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInUser user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            else
            {

                var userMail = await _userManager.FindByEmailAsync(user.Email);

                if (userMail == null)
                {
                    ModelState.AddModelError("", "Bu Email ile daha önce hesap oluşturulmamış!!");
                    return View(user);
                }
                var result = await _signInManager.PasswordSignInAsync(userMail, user.Password, true, true);
                if (result.Succeeded)
                {

                    var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
                    optionsBuilder.UseSqlServer(strSQL.SQLString);
                    var context = new IdentityContext(optionsBuilder.Options);
                    var searchedUser = context.Users.FirstOrDefault(x => x.Email == user.Email);
                    if (searchedUser.role == "not validate")
                    {
                        TempData["mail"] = searchedUser.Email;
                        return RedirectToAction("Validate", "Kullanici");
                    }
                    else if (searchedUser.role == "admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else 
                    {
                        TempData["mail"] = searchedUser.Email;
                        ViewData["SuccessMessage"] = "Giriş başarılı!";

                        return RedirectToAction("Index", "Kullanici");
                    }
                        
                    
                }
                else
                {
                    ModelState.AddModelError("", "Girilen email veya parola yanlış");
                    return View(user);
                }

            }
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpUser user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            AppUser appUser = new AppUser
            {
                Email = user.Email,
                UserName = user.Email 
            };

            var result = await _userManager.CreateAsync(appUser, user.Password);
            if (result.Succeeded)
            {
                _Context.Users.Add(new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    
                    role = "not validate",
                });

                await _Context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı.";
                return RedirectToAction("SignIn", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

    }
}

