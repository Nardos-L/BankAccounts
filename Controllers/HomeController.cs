using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private int? uid
        {
            get
            {
                return HttpContext.Session.GetInt32("UserId");
            }
        }

        private bool isLoggedIn
        {
            get
            {
                return uid != null;
            }
        }

        private BankAccountsContext db;
        public HomeController(BankAccountsContext context)
        {
            db = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost("/register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                // If any user already exists with email.
                if (db.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "is taken.");
                }
            }

            // If we added an error above.
            if (ModelState.IsValid == false)
            {
                // Show form again to display errors.
                return View("Index");
            }

            PasswordHasher<User> hasher = new PasswordHasher<User>();
            newUser.Password = hasher.HashPassword(newUser, newUser.Password);

            db.Users.Add(newUser);
            db.SaveChanges();

            HttpContext.Session.SetInt32("UserId", newUser.UserId);
            HttpContext.Session.SetString("FullName", newUser.FullName());

            ViewBag.LoggedUser = uid;

            return Redirect($"account/{newUser.UserId}");
            //return RedirectToAction("Account");
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginUser loginUser)
        {
            if (ModelState.IsValid == false)
            {
                // Show form again to display errors.
                return View("Login");
            }

            User dbUser = db.Users.FirstOrDefault(user => user.Email == loginUser.EmailLogin);

            
            if (dbUser == null)
            {
                /* 
                Normally we want to be ambiguous with these error messages to
                not reveal too much info. E.g., if we say password is incorrect
                a hacker may now know the email existed.
                */
                ModelState.AddModelError("EmailLogin", "email not found.");
                return View("Login"); // Display errors.
            }

            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();

            // Right click PasswordVerificationResult and go to definition for more info.
            PasswordVerificationResult pwCompareResult = hasher.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.PasswordLogin);

            if (pwCompareResult == 0)
            {
                ModelState.AddModelError("EmailLogin", "incorrect credentials.");
                return View("Login"); // Display Errors.
            }

            HttpContext.Session.SetInt32("UserId", dbUser.UserId);
            HttpContext.Session.SetString("FullName", dbUser.FullName());

            return Redirect($"account/{dbUser.UserId}");
            
        }

        [HttpPost("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet("/account/{userID}")]
        public IActionResult Account(int userId)
        {
            if (!isLoggedIn)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.LoggedUser = db.Users.FirstOrDefault(u => u.UserId == uid);

            List<Transaction> allTransactions = db.Transactions
                .Where(t => t.UserId == uid)
                .OrderByDescending( t => t.CreatedAt)
                .ToList();
            ViewBag.Transactions = allTransactions;

            decimal balance = 0;
            foreach(var t in allTransactions){
                Console.WriteLine($"{t.UserId} {uid}");

                balance += t.Amount;
            }
            ViewBag.Balance = balance;
            return View("Account");
        } 
        

        [HttpPost("/transactions")]
        public IActionResult Transactions(Transaction transaction)
        {
            if (ModelState.IsValid == false)
            {
                // Show form again to display errors.
                return Redirect($"account/{(int)uid}");
            }
            
            List<Transaction> allTransactions = db.Transactions
                .Where(t => t.UserId == uid)
                .OrderByDescending( t => t.CreatedAt)
                .ToList();

            decimal balance = 0;
            foreach(var t in allTransactions){
                balance += t.Amount;
            }

            decimal total = balance + transaction.Amount;
            if(transaction.Amount < 0)
            {
                if(total <= 0)
                {
                    ModelState.AddModelError("Amount", "You don't have enough amount to withdraw");
                    return Redirect($"account/{(int)uid}");
                }
            }

            transaction.UserId = (int)uid;
            db.Add(transaction);
            db.SaveChanges();
            return Redirect($"account/{(int)uid}");
        }
    }
}
