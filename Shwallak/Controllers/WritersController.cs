﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Shwallak.Models;

namespace Shwallak.Controllers
{
    public class WritersController : Controller
    {
        private OurDB db = new OurDB();

        // GET: Writers
        public ActionResult Index()
        {
            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            var query = from wri in db.Writers
                        join sub in db.Subscribers on wri.Email equals sub.Email
                        select new { id = wri.WriterID.ToString() };

            foreach(var wri in query)
            {
                ViewData[wri.id] = "yes";
            }
            return View(db.Writers.ToList());
        }

        // GET: Writers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            if(Session["type"] == null || !Session["type"].Equals("admin"))
            {
                if (Session["type"] == null || !Session["type"].Equals("writer"))
                    return RedirectToAction("Index", "Home");
                else if(Session["id"]==null || !Session["id"].Equals(id))
                    return RedirectToAction("Index", "Home");
            }
            Writer writer = db.Writers.Find(id);
            if (writer == null)
            {
                return HttpNotFound();
            }
            ViewBag.Address = RemoveWhitespace(writer.Address);

            var query = from wri in db.Writers
                        join art in db.Articles on wri.Year equals art.Year
                        where wri.WriterID == writer.WriterID
                        select new { wri.WriterID };

            ViewBag.number = query.Count();
            return View(writer);
        }

        // GET: Writers/Create
        public ActionResult Create()
        {
            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Writers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WriterID,FullName,Gender,Email,Year,Password,Address,Age")] Writer writer)
        {

            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                foreach (Writer wri in db.Writers.ToList())
                {
                    if (wri.FullName.Equals(writer.FullName))
                    {
                        ViewBag.messege = "this name is taken";
                        return View(writer);
                    }
                    if (wri.Email.Equals(writer.Email))
                    {
                        ViewBag.messege = "this email is allready in use";
                        return View(writer);
                    }
                }
                db.Writers.Add(writer);
                db.SaveChanges();
                return RedirectToAction("Index", "Writers");
            }

            return View(writer);
        }

        // GET: Writers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["type"] == null || !Session["type"].Equals("writer"))
                return RedirectToAction("Index", "Home");
            else if(Session["id"] == null || !Session["id"].Equals(id))
                return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            else if (Session["id"] == null || !Session["id"].Equals(id))
            {
                return RedirectToAction("Index", "Home");
            }
            Writer writer = db.Writers.Find(id);
            if (writer == null)
            {
                return HttpNotFound();
            }
            return View(writer);
        }

        // POST: Writers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "WriterID,FullName,Gender,Email,Year,Password,Address,Age")] Writer writer)
        {
            if (Session["type"] == null || !Session["type"].Equals("writer"))
                return RedirectToAction("Index", "Home");
            else if (Session["id"] == null || !Session["id"].Equals(writer.WriterID))
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                
                foreach (Writer wri in db.Writers.AsNoTracking().ToList())
                {
                    if (wri.WriterID == writer.WriterID)
                        continue;
                    if (wri.FullName.Equals(writer.FullName))
                    {
                        ViewBag.messege = "this name is taken";
                        return View(writer);
                    }
                    if (wri.Email.Equals(writer.Email))
                    {
                        ViewBag.messege = "this email is allready in use";
                        return View(writer);
                    }
                }
                
                db.Entry(writer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details/"+writer.WriterID);
            }
            return View(writer);
        }

        // GET: Writers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            Writer writer = db.Writers.Find(id);
            if (writer == null)
            {
                return HttpNotFound();
            }
            return View(writer);
        }

        // POST: Writers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            Writer writer = db.Writers.Find(id);
            db.Writers.Remove(writer);
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

        public ActionResult Search()
        {
            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            return View();
        }

        public ActionResult Results(string name, string email, int? year)
        {
            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            List<Writer> results = new List<Writer>();
            List<Writer> temp = new List<Writer>();

            if ((name == null || name.Equals("")) && (email == null || email.Equals("")) && year == null)
            {
                return RedirectToAction("Search");
            }

            foreach (Writer writer in db.Writers.ToList())
            {
                results.Add(writer);
            }


            if (name != null && !name.Equals(""))
            {
                temp.AddRange(results);
                foreach (Writer writer in temp)
                {
                    if (writer.FullName == null)
                        results.Remove(writer);
                    else if (!writer.FullName.ToLower().Contains(name.ToLower()))
                        results.Remove(writer);
                }
                temp.Clear();
            }

            if (email != null && !email.Equals(""))
            {
                temp.AddRange(results);
                foreach (Writer writer in temp)
                {
                    if (writer.Email == null)
                        results.Remove(writer);
                    else if (!writer.Email.ToLower().Contains(email.ToLower()))
                        results.Remove(writer);
                }
                temp.Clear();
            }

            if (year != null)
            {
                temp.AddRange(results);
                foreach (Writer writer in temp)
                {
                    if (writer.Year != year)
                        results.Remove(writer);
                }
                temp.Clear();
            }

            return View(results);
        }

        private int CompareGenderMale(Writer x, Writer y)
        {
            if (x.Gender.ToString().Equals(y.Gender))
                return 0;
            if (x.Gender.ToString().Equals("Male"))
                return -1;
            else if (x.Gender.ToString().Equals("Female"))
            {
                if (y.Gender.ToString().Equals("Male"))
                    return 1;
                else
                    return -1;
            }
            else if (x.Gender.ToString().Equals("Other"))
                return 1;
            return 1;
        }

        private int CompareGenderFemale(Writer x, Writer y)
        {
            if (x.Gender.ToString().Equals(y.Gender))
                return 0;
            if (x.Gender.ToString().Equals("Female"))
                return -1;
            else if (x.Gender.ToString().Equals("Male"))
            {
                if (y.Gender.ToString().Equals("Female"))
                    return 1;
                else
                    return -1;
            }
            else if (x.Gender.ToString().Equals("Other"))
                return 1;
            return 1;
        }

        public ActionResult Sort(string sortBy, int? gender)
        {
            if (Session["type"] == null || !Session["type"].Equals("admin"))
                return RedirectToAction("Index", "Home");

            if (sortBy == null)
                return RedirectToAction("Index");
            List<Writer> list = new List<Writer>();
            foreach (Writer w in db.Writers)
                list.Add(w);
            if (sortBy.Equals("year"))
                list.Sort((x, y) => x.Year - y.Year);
            else if (sortBy.Equals("fullname"))
                list.Sort((x, y) => string.Compare(x.FullName, y.FullName));
            else if (sortBy.Equals("gender"))
                if (gender == 1)
                    list.Sort((x, y) => CompareGenderMale(x, y));
                else
                    list.Sort((x, y) => CompareGenderFemale(x, y));
            return View(list);
        }
    
        public ActionResult MyArea(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session["type"] == null || !Session["type"].Equals("writer"))
                return RedirectToAction("Index", "Home");
            else if (Session["id"] == null || !Session["id"].Equals(id))
                return RedirectToAction("Index", "Home");

            List<Writer> writers = new List<Writer>();
            List<Writer> results = new List<Writer>();

            writers.AddRange(db.Writers.Include(x => x.Articles));
            results.AddRange(writers.Where(x => x.WriterID == id));
            if (results.Count != 1)
            {
                return HttpNotFound();
            }

            return View(results.First());
        }

        public ActionResult ChangePassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session["type"] == null || !Session["type"].Equals("writer"))
                return RedirectToAction("Index", "Home");
            else if (Session["id"] == null || !Session["id"].Equals(id))
                return RedirectToAction("Index", "Home");

            Writer writer = db.Writers.Find(id);
            if (writer == null)
            {
                return HttpNotFound();
            }
            return View(writer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "WriterID,FullName,Gender,Email,Year,Password,Address,Age")] Writer writer)
        {
            if (Session["type"] == null || !Session["type"].Equals("writer"))
                return RedirectToAction("Index", "Home");
            else if (Session["id"] == null || !Session["id"].Equals(writer.WriterID))
                return RedirectToAction("Index", "Home");


            if (ModelState.IsValid)
            {
                db.Entry(writer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details/" + writer.WriterID);
            }

          
            return View(writer);
        }

        private string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }
}
