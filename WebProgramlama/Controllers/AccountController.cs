using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebProgramlama.Data;
using WebProgramlama.Models;
using Zakuska_AI.Data;

namespace WebProgramlama.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private IdentityContext _Context;
        stringSQL strSQL = new stringSQL();



        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _Context = context;
            _configuration = configuration;
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
                        var token = GenerateJwtToken(userMail);

                        return RedirectToAction("Index", "Kullanici", new { token });
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

        private string GenerateJwtToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),  // Email claim ekleniyor
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
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

