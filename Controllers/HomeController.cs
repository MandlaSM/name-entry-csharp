using Microsoft.AspNetCore.Mvc;
using NameEntryApp.Data;
using NameEntryApp.Models;

namespace NameEntryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var people = _db.People.OrderByDescending(x => x.Id).ToList();
            return View(people);
        }

        [HttpPost]
        public IActionResult Index(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _db.People.Add(new Person { Name = name.Trim() });
                _db.SaveChanges();
            }

            var people = _db.People.OrderByDescending(x => x.Id).ToList();
            return View(people);
        }
    }
}
