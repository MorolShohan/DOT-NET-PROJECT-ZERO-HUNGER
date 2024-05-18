using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Zero_Hunger.EF;
using Zero_Hunger.Models;

namespace Zero_Hunger.Controllers
{
    public class DashboardController : Controller
    {
        private readonly zero_hungerEntities1 _context;

        public DashboardController()
        {
            _context = new zero_hungerEntities1();
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddProduct()
        {
            if (Session["Email"] == null)
            {

                return RedirectToAction("Index", "LogIn");
            }

            string userEmail = Session["Email"].ToString();
            SignUp restaurantUser = _context.SignUps.FirstOrDefault(u => u.Email == userEmail);
            int AccountType = Convert.ToInt32(restaurantUser.AccountType);

            if (AccountType != 1)
            {

                return RedirectToAction("Index", "LogIn");
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(ProductDTO p)
        {


            if (ModelState.IsValid)
            {
                string userEmail = Session["Email"].ToString();
                SignUp restaurantUser = _context.SignUps.FirstOrDefault(u => u.Email == userEmail);
                int userId = restaurantUser.UserId;

                string fileName = p.PhotoFile.FileName;
                fileName = DateTime.Now.ToString("yymms") + fileName;
                fileName = Path.Combine(Server.MapPath("~/Image/"), fileName);
                p.PhotoFile.SaveAs(fileName);

                Product newProduct = new Product
                {
                    RestaurentId = userId,

                    Name = p.Name,
                    Description = p.Description,
                    PhotoPath = DateTime.Now.ToString("yymms") + p.PhotoFile.FileName,
                };

                using (var db = new zero_hungerEntities1())
                {
                    db.Products.Add(newProduct);
                    db.SaveChanges();
                }

                return RedirectToAction("RestaurantDashboard", "LogIn");
            }
            return View(p);
        }

        public ActionResult AdminNotifications()
        {
            var adminNotifications = _context.Notificationns.Where(n => n.RequestType == 1).ToList();
            var productIds = adminNotifications.Select(n => n.ProductId).Distinct().ToList();
            var productNames = _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionary(p => (int)p.ProductId, p => p.Name);

            ViewBag.ProductNames = productNames;
            return View(adminNotifications);
        }

        public ActionResult RestaurantOwnerNotifications(string restaurantName)
        {
            var restaurantNotifications = _context.Notificationns.Where(n => n.Restaurant == restaurantName).ToList();
            var productIds = restaurantNotifications.Select(n => n.ProductId).Distinct().ToList();
            var productNames = _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionary(p => (int)p.ProductId, p => p.Name);

            ViewBag.ProductNames = productNames;

            return View(restaurantNotifications);
        }

    }
}