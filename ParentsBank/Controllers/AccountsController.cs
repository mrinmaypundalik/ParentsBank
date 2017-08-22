using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ParentsBank.Models;

namespace ParentsBank.Controllers
{

    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        [Authorize]
        public ActionResult Index()
        {

            if (User.IsInRole("Recepient"))
            {
                var acc = db.Accounts.Where(a => a.Recipient == User.Identity.Name).ToList();
                foreach (Account account in acc)
                {
                    account.YearToDateInterest();
                }
                return View(acc);
            }

            var acct = db.Accounts.Where(a => a.Owner == User.Identity.Name).ToList();
            if (acct != null)
            {
                foreach (Account account in acct)
                {
                    account.YearToDateInterest();
                }
            }
            return View(acct);

            // return View();
        }

        // GET: Accounts/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (User.IsInRole("Owner"))
            {
             var account = db.Accounts.Where(a => a.Owner == User.Identity.Name && a.ID == id);
                if (account == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Transactions = db.Transactions.Where(t => t.AccountID == id).OrderBy(t=>t.TransactionDate);
                ViewBag.WishLists = db.WishLists.Where(w => w.AccountID == id);
                ViewBag.Accounts = account;
                return View();
            }
            else if (User.IsInRole("Recepient"))
            {
                var account =  db.Accounts.Where(a => a.Recipient == User.Identity.Name && a.ID == id);
                if (account == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Transactions = db.Transactions.Where(t => t.AccountID == id);
                ViewBag.WishLists = db.WishLists.Where(w => w.AccountID == id);
                ViewBag.Accounts = account;
                return View();
            }

            return View(db.Accounts.Find(id));
        }

        // GET: Accounts/Create
        [Authorize(Roles = "Owner")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles ="Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Recipient,Name,Balance,Description,Interest")] Account account)
        {
            if (ModelState.IsValid)
            {
                int countDuplicateRecipients = db.Accounts.Count(a => a.Recipient == account.Recipient);
                int isRecepientAnOwner = db.Accounts.Count(a => a.Owner == account.Recipient);
                int isOwnerAnRecepient = db.Accounts.Count(a => a.Recipient == account.Owner);
                if(account.Balance<0)
                    ModelState.AddModelError("Balance", "Balance Cannot be Negative");
                else if (account.Owner==account.Recipient)
                    ModelState.AddModelError("Recipient", "Owner and Recepient cannot have same email address");
                else if (countDuplicateRecipients > 0)
                    ModelState.AddModelError("Recipient", "Email ID already exists");
                else if (isRecepientAnOwner > 0)
                    ModelState.AddModelError("Recipient", "A recipient cannot be an owner of another account");
                else if (isOwnerAnRecepient > 0)
                    ModelState.AddModelError("Owner", "An Owner cannot be a recepient");

                /**Create user with the role of an Recepient*/
                ApplicationUserManager mgr = new ApplicationUserManager(
                        new Microsoft.AspNet.Identity.EntityFramework.UserStore<Models.ApplicationUser>(db));

                Models.ApplicationUser existingUser = db.Users.FirstOrDefault(x => x.Email == account.Recipient);

                //If the user already exsists in the Database, throw an error
                if (existingUser != null) {
                    ModelState.AddModelError("Recipient", "The Recepient account already exists");
                    return View(account);
                }

                Models.ApplicationUser au = new Models.ApplicationUser { Email = account.Recipient, UserName = account.Recipient };

                //set default password
                var result = mgr.CreateAsync(au, "Password123").Result;

                //set Recepient role on the new user created
                Microsoft.AspNet.Identity.UserManagerExtensions.AddToRoles(mgr, au.Id, "Recepient");

                if (ModelState.IsValid)
                {
                    account.Owner = User.Identity.Name;
                    db.Accounts.Add(account);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(account);
            }

            return View(account);
        }

        // GET: Accounts/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }            
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles ="Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Owner,Recipient,Name,Balance,Description,Interest")] Account account)
        {
            account.YearToDateInterest();
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        [Authorize(Roles ="Owner")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts/Delete/5
        [Authorize(Roles = "Owner")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Recepient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Buy(int id)
        {
            Account account = db.Accounts.FirstOrDefault(a => a.Recipient == User.Identity.Name);
            if (ModelState.IsValid)
            {
                WishList list = db.WishLists.Find(id);
                list.Purchased = true;
                Transaction transaction = new Transaction();
                transaction.AccountID = list.AccountID;
                transaction.TransactionDate = DateTime.Now;
                transaction.Note = "Debit for " + list.Description;
                transaction.Amount = list.Cost * -1;
                float bal = account.Balance + transaction.Amount;
                account.Balance = bal;
                db.Entry(account).State = EntityState.Modified;
                db.Entry(list).State = EntityState.Modified;
                db.Transactions.Add(transaction);
                db.SaveChanges();
                // return RedirectToAction("Index");
            }
            //ViewBag.AccountID = new SelectList(db.Accounts, "ID", "Owner", transaction.AccountID);
            //return View(wishList);
            //Account account = db.Accounts.FirstOrDefault(a => a.Recipient == User.Identity.Name);
            return RedirectToAction("Details", new {id=account.ID });
        }

    }
}
