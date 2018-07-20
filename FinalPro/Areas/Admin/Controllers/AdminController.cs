using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinalPro.Models;

namespace FinalPro.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        libraffEntities db = new libraffEntities();

        // GET: Admin/Admin
        public ActionResult Index()
        {
            if (Session["loggedadmin"] != null)
            {
                return View();
            }

            return RedirectToAction("Login");
        }

        public ActionResult AddAuthor()
        {
            if (Session["loggedadmin"] != null)
            {
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult AddAuthor(string authorN)
        {
            if (Session["loggedadmin"] != null)
            {
                if (authorN == "")
                {
                    ViewBag.error = "Please fill the box.";
                    return View();
                }
                Author author = db.Authors.FirstOrDefault(au => au.Name == authorN);
                if (author != null)
                {
                    ViewBag.error = "This author is already in database.";
                    return View();
                }

                db.Authors.Add(new Author
                {
                    Name = authorN,
                });
                db.SaveChanges();
                ViewBag.succ = "Added author to database successfully.";
                return View();
            }

            return RedirectToAction("Login");
        }

        [ActionName("Login")]
        public ActionResult LoginGet()
        {
            if (Session["loggedadmin"] == null)
            {
                return View();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Login")]
        public ActionResult LoginPost(string username, string password)
        {
            Moderator moderator = db.Moderators.FirstOrDefault(rd => rd.Username == username && rd.Password == password);
            if (moderator == null)
            {
                ViewBag.error = "Username or password is wrong.";
                return View();
            }
            Session["loggedadmin"] = moderator;
            return RedirectToAction("Index", "Admin");
        }

        public ActionResult AddBook()
        {
            if (Session["loggedadmin"] != null)
            {
                ViewBag.Authors = db.Authors.ToList();
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult AddBook(string name, string author, string description, int quantity)
        {
            if (Session["loggedadmin"] != null)
            {
                ViewBag.Authors = db.Authors.ToList();
                if (name != "" && author != "Select Author ..." && description != "" && quantity > 0)
                {
                    if (!db.Books.Any(bk => bk.Name == name && bk.Author.Name == author))
                    {
                        db.Books.Add(new Book
                        {
                            Name = name,
                            AuthorID = db.Authors.First(au => au.Name == author).ID,
                            Descript = description,
                            Cover = "satre.jpg",
                            Pdf = "pablo.pdf",
                            Quantity = quantity,
                            Available = quantity,
                            Status = 1,                           
                        });
                        db.SaveChanges();
                        ViewBag.succ = "Added book to database successfully.";
                        return View();
                    }
                    ViewBag.error = "This book is already in database.";
                    return View();
                }
                ViewBag.error = "Please fill all the boxes.";
                return View();
            }
            return RedirectToAction("Login");
        }

        public ActionResult Delete()
        {
            if (Session["loggedadmin"] != null)
            {
                ViewBag.Books = db.Books.Where(bk => bk.Status == 1).ToList();
                return View();
            }

            return RedirectToAction("Login");
        }

        public ActionResult DeleteBook(int bkid)
        {
            if (Session["loggedadmin"] != null)
            {
                db.Books.First(bk => bk.ID == bkid).Status = 0;
                db.SaveChanges();
                return RedirectToAction("Delete");
            }
            return RedirectToAction("Login");
        }
    }
}