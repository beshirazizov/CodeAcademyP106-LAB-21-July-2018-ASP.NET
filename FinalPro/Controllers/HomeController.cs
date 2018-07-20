using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinalPro.Models;

namespace FinalPro.Controllers
{
    public class HomeController : Controller
    {
        libraffEntities db = new libraffEntities();
        public ActionResult Index(int skip = 0, int take = 9)
        {
            ViewBag.Pages = db.Books.Where(bk=> bk.Status == 1).Count();
            return View(db.Books.Where(bk => bk.Status == 1).OrderBy(bk => bk.ID).Skip(skip).Take(take).ToList());
        }

        [ActionName("Login")]
        public ActionResult LoginGet()
        {
            if (Session["loggeduser"] == null)
            {
                return View();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Login")]
        public ActionResult LoginPost(string email, string password)
        {
            Reader reader = db.Readers.FirstOrDefault(rd => rd.Email == email && rd.Password == password);
            if (reader == null)
            {
                ViewBag.error = "Email or password is wrong.";
                return View();
            }
            Session["loggeduser"] = reader;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session["loggeduser"] = null;

            return RedirectToAction("Index");
        }

        public ActionResult Signup()
        {
            if (Session["loggeduser"] == null)
            {
                return View();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Signup(string email, string password, string name, string confirm)
        {
            if (email != "" && name != "" && password != "" && confirm != "")
            {
                if (password.Length >= 8)
                {
                    if (password == confirm)
                    {
                        if (!db.Readers.Any(rd => rd.Email == email))
                        {
                            Reader rd = new Reader
                            {
                                Name = name,
                                Email = email,
                                Password = password,
                            };
                            db.Readers.Add(rd);
                            db.SaveChanges();
                            Session["loggeduser"] = rd;
                            return RedirectToAction("Index");
                        }
                        ViewBag.Error = "This email is alredy taken.";
                        return View();
                    }
                    ViewBag.Error = "Passwords don't match.";
                    return View();
                }
                ViewBag.Error = "Password must be at least 8 chars long.";
                return View();
            }
            ViewBag.Error = "Please fill all the boxes.";
            return View();
        }

        public ActionResult MyBooks()
        {
            if (Session["loggeduser"] != null)
            {
                Reader rd = Session["loggeduser"] as Reader;
                return View(db.BookToReaders.Where(btr => btr.ReaderID == rd.ID).ToList());
            }

            return RedirectToAction("Index");
        }

        public ActionResult Addbook(int bookID)
        {
            if (Session["loggeduser"] != null)
            {
                if (db.Books.Any(bk => bk.ID == bookID))
                {
                    Reader rd = Session["loggeduser"] as Reader;
                    if (!db.BookToReaders.Any(bktr => bktr.BoodID == bookID && bktr.ReaderID == rd.ID))
                    {
                        if (db.Books.First(bk => bk.ID == bookID).Available > 0)
                        {
                            if (db.BookToReaders.Where(bktr => bktr.ReaderID == rd.ID).ToList().Count <= 4)
                            {
                                db.Books.First(bk => bk.ID == bookID).Available--;
                                db.SaveChanges();

                                db.BookToReaders.Add(new BookToReader
                                {
                                    BoodID = bookID,
                                    ReaderID = rd.ID,
                                });
                                db.SaveChanges();
                                return RedirectToAction("MyBooks");
                            }
                            TempData["error"] = "You already have 5 books. Return a book to get new one.";
                            return RedirectToAction("MyBooks");
                        }
                        return RedirectToAction("Index");
                    }
                    TempData["error"] = "You already have this book.";
                    return RedirectToAction("MyBooks");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public ActionResult Read(int bookID)
        {
            if (Session["loggeduser"] != null)
            {
            Reader rd = Session["loggeduser"] as Reader;
                if (db.BookToReaders.Any(btr => btr.BoodID == bookID && btr.ReaderID == rd.ID ))
                {
                    return File("~/Public/pdfs/" + db.Books.First(bk => bk.ID == bookID).Pdf,"application/pdf");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public ActionResult Return(int bookID)
        {
            if (Session["loggeduser"] != null)
            {
                Reader rd = Session["loggeduser"] as Reader;
                BookToReader btr = db.BookToReaders.FirstOrDefault(bt => bt.BoodID == bookID && bt.ReaderID == rd.ID);
                if (btr != null)
                {
                    btr.Book.Available++;
                    db.SaveChanges();

                    db.BookToReaders.Remove(btr);
                    db.SaveChanges();

                    return RedirectToAction("MyBooks");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}