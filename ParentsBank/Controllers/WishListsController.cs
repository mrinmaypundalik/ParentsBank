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
    public class WishListsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: WishLists
        [Authorize]
        public ActionResult Index()
        {

            //
            if (User.IsInRole("Recepient"))
            {
                return View(db.WishLists.Where(a => a.Account.Recipient == User.Identity.Name).ToList());
            }

            return View(db.WishLists.Where(a => a.Account.Owner == User.Identity.Name).ToList());

        }

        // GET: WishLists/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WishList wishList = db.WishLists.Find(id);
            if (wishList == null)
            {
                return HttpNotFound();
            }
            return View(wishList);
        }

        // GET: WishLists/Create
        [Authorize(Roles = "Recepient")]
        public ActionResult Create()
        {
            ViewBag.AccountID = new SelectList(db.Accounts, "ID", "Recipient");
            return View();
        }

        // POST: WishLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Recepient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Cost,Description,Link")] WishList wishList)
        {
            if (ModelState.IsValid)
            {
                wishList.AccountID = db.Accounts.FirstOrDefault(a => a.Recipient == User.Identity.Name).ID;
                db.WishLists.Add(wishList);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountID = new SelectList(db.Accounts, "ID", "Recipient", wishList.AccountID);
            return View(wishList);
        }

        // GET: WishLists/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WishList wishList = db.WishLists.Find(id);
            if (wishList == null)
            {
                return HttpNotFound();
            }
            ViewBag.AccountID = new SelectList(db.Accounts, "ID", "Recipient", wishList.AccountID);
            return View(wishList);
        }

        // POST: WishLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Recepient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Cost,Description,Link")] WishList wishList)
        {
            if (ModelState.IsValid)
            {
                wishList.AccountID = db.Accounts.FirstOrDefault(a => a.Recipient == User.Identity.Name).ID;
                db.Entry(wishList).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountID = new SelectList(db.Accounts, "ID", "Recipient", wishList.AccountID);
            return View(wishList);
        }

        // GET: WishLists/Delete/5
        [Authorize(Roles = "Recepient")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WishList wishList = db.WishLists.Find(id);
            if (wishList == null)
            {
                return HttpNotFound();
            }
            return View(wishList);
        }

        // POST: WishLists/Delete/5
        [Authorize(Roles = "Recepient")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WishList wishList = db.WishLists.Find(id);
            db.WishLists.Remove(wishList);
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

        public void test()
        {

        }
    }
}
