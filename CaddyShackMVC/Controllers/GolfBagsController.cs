using CaddyShackMVC.DataAccess;
using CaddyShackMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaddyShackMVC.Controllers
{
    public class GolfBagsController : Controller
    {
        private readonly CaddyShackContext _context;

        public GolfBagsController(CaddyShackContext context)
        {
            _context = context;
        }

        // GET: /GolfBags (ID, Player, and Capcity)
        public IActionResult Index()
        {
            var golfbags = _context.GolfBags;

            return View(golfbags);
        }

        // GET: /GolfBags/id (id, Player, Capcity and Clubs)
        [HttpGet]
        [Route("GolfBags/{id:int}")]
        public IActionResult Show(int id)
        {
            var golfbags = _context.GolfBags
                           .Include(g => g.Clubs) // Loads the Clubs for each GolfBag
                           .SingleOrDefault(g => g.Id == id); // Gets the clubs for just this id

            return View(golfbags);
        }

        // GET: /GolfBags/New (Make a new golfbag)
        public IActionResult New()
        {
            return View();
        }

        // I used the create method first but ran into issues where it was not being created
        // in my database or rederecting to my show so I went with Index since I was able
        // to get it functioning. I will study this code later to understand what happened
        [HttpPost]  // POST: /GolfBags then SHOW /GolfBags/id
        public IActionResult Index(GolfBag golfbag)
        { 
            _context.GolfBags.Add(golfbag);
            _context.SaveChanges();

            var newGolfBagID = golfbag.Id;

            return RedirectToAction("Show", new { id = newGolfBagID });
        }

        // GET: /GolfBags/:id/edit
        [Route("/GolfBags/{id:int}/edit")]
        public IActionResult Edit(int id)
        {
            var golfbags = _context.GolfBags.Find(id);

            return View(golfbags);
        }

        // PUT: /GolfBags/:id (update resource then show resource)
        [HttpPost]
        [Route("/GolfBags/{id:int}")]
        public IActionResult Update(GolfBag golfbag)
        {
            _context.GolfBags.Update(golfbag);
            _context.SaveChanges();

            return RedirectToAction("show", new { id = golfbag.Id });
        }

        // DELETE: /GolfBags/Delete/:id
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var golfbags = _context.GolfBags.Find(id);

            _context.GolfBags.Remove(golfbags);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
