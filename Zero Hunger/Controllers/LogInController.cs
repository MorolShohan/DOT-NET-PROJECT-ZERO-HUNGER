using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Zero_Hunger.EF;
using Zero_Hunger.Models;

namespace Zero_Hunger.Controllers
{
    public class LogInController : Controller
    {
        private readonly zero_hungerEntities1 _context;

        public LogInController()
        {
            _context = new zero_hungerEntities1();
        }


        public ActionResult Index()
        {
            if (Session["Email"] != null)
            {
                string userEmail = Session["Email"].ToString();
                SignUp User = _context.SignUps.FirstOrDefault(u => u.Email == userEmail);
                if (User.AccountType == 1)
                {
                    return RedirectToAction("RestaurantDashboard");
                }
                if (User.AccountType == 2)
                {
                    return RedirectToAction("AdminDashboard");
                }
                else
                {
                    return RedirectToAction("EmployeefDashboard");
                }
            }

            return View();
        }
        [HttpPost]
        public ActionResult Index(LogInDTO loginModel, SignUp obj)
        {
            if (ModelState.IsValid)
            {

                using (var db = new zero_hungerEntities1())
                {

                    var user = db.SignUps.FirstOrDefault(u => u.Email == obj.Email && u.Password == obj.Password);

                    if (user != null)
                    {
                        if (user.AccountType == 1)
                        {
                            Session["Email"] = user.Email;
                            return RedirectToAction("RestaurantDashboard");
                        }
                        else if (user.AccountType == 2)
                        {
                            Session["Email"] = user.Email;
                            return RedirectToAction("AdminDashboard");
                        }
                        else
                        {
                            Session["Email"] = user.Email;
                            return RedirectToAction("EmployeefDashboard");
                        }
                    }
                    else
                    { ModelState.AddModelError("", "Invalid email or password. Please try again."); }
                }
            }
            return View(loginModel);
        }

        public ActionResult AdminDashboard()
        {
            if (Session["Email"] == null)
            {

                return RedirectToAction("Index", "LogIn");
            }
            string userEmail = Session["Email"].ToString();
            SignUp restaurantUser = _context.SignUps.FirstOrDefault(u => u.Email == userEmail);
            int AccountType = Convert.ToInt32(restaurantUser.AccountType);

            if (AccountType != 2)
            {
                return RedirectToAction("Index", "LogIn");
            }


            // Retrieve unique ProductIds and corresponding quantities from the Request entity
            var productQuantities = _context.Requests
                .Where(r => r.ProductId != null)
                .GroupBy(r => r.ProductId)
                .Select(group => new { ProductId = group.Key.Value, Quantity = group.Sum(r => r.Amount) })
                .ToList();

            // Retrieve Product details for each ProductId
            var productDetails = new List<ProductWithQuntity>();
            foreach (var item in productQuantities)
            {
                var product = _context.Products
                    .FirstOrDefault(p => p.ProductId == item.ProductId);

                if (product != null)
                {

                    var restaurantId = _context.Requests.FirstOrDefault(r => r.ProductId == item.ProductId)?.RestaurantId;

                    var restaurantName = string.Empty;
                    if (restaurantId.HasValue)
                    {
                        var restaurant = _context.SignUps.FirstOrDefault(s => s.UserId == restaurantId.Value);
                        if (restaurant != null)
                        {
                            restaurantName = restaurant.ResturentName;
                        }
                    }

                    var productWithQuantity = new ProductWithQuntity
                    {
                        Product = product,
                        Quantity = Convert.ToInt32(item.Quantity),
                        RestaurantName = restaurantName
                    };

                    productDetails.Add(productWithQuantity);
                }
            }
            var users = _context.SignUps.Where(u => u.AccountType == 3).ToList();
            ViewBag.EmployeeList = users;
            ViewBag.Admin = restaurantUser.Name;


            return View(productDetails);
        }

        [HttpPost]
        public ActionResult AcceptRequest(int productId, int amount, int employeeId, string restaurantName)
        {


            var request = new RequestForEmployee
            {
                ProductId = productId,
                Amount = amount,
                EmployeeId = employeeId,
                RestaurantName = restaurantName
            };

            _context.RequestForEmployees.Add(request);
            _context.SaveChanges();

            var requestToDelete = _context.Requests.FirstOrDefault(r => r.ProductId == productId && r.Amount == amount);
            if (requestToDelete != null)
            {
                _context.Requests.Remove(requestToDelete);
                _context.SaveChanges();
            }

            var notification = new Notificationn
            {
                RequestType = 2,
                ProductId = productId,
                Amount = amount,
                Status = 1,
                Restaurant = restaurantName
            };

            _context.Notificationns.Add(notification);
            _context.SaveChanges();


            return RedirectToAction("RestaurantDashboard");
        }

        [HttpPost]
        public ActionResult RejectRequestFromAdmin(int productId, int amount, string restaurantName)
        {
            var notification = new Notificationn
            {
                RequestType = 2,
                ProductId = productId,
                Amount = amount,
                Status = 2,
                Restaurant = restaurantName
            };
            _context.Notificationns.Add(notification);
            _context.SaveChanges();

            var requestToDelete = _context.Requests.FirstOrDefault(r => r.ProductId == productId && r.Amount == amount);
            if (requestToDelete != null)
            {
                _context.Requests.Remove(requestToDelete);
                _context.SaveChanges();
            }


            return RedirectToAction("AdminDashboard");
        }




        public ActionResult RestaurantDashboard()
        {
            if (Session["Email"] == null)
            {

                return RedirectToAction("Index", "LogIn");
            }

            string use = Session["Email"].ToString();
            SignUp r = _context.SignUps.FirstOrDefault(u => u.Email == use);
            int AccountType = Convert.ToInt32(r.AccountType);

            if (AccountType != 1)
            {
                return RedirectToAction("Index", "LogIn");
            }

            string userEmail = Session["Email"].ToString();
            SignUp restaurantUser = _context.SignUps.FirstOrDefault(u => u.Email == userEmail);
            int userId = restaurantUser.UserId;
            ViewBag.RestaurantName = restaurantUser.ResturentName;
            ViewBag.RestaurantId = userId;


            List<Product> products = _context.Products.Where(p => p.RestaurentId == userId).ToList();


            return View(products);
        }

        [HttpPost]
        public ActionResult DeleteProduct(int productId)
        {

            Product productToDelete = _context.Products.Find(productId);

            if (productToDelete != null)
            {
                _context.Products.Remove(productToDelete);
                _context.SaveChanges();
            }
            return RedirectToAction("RestaurantDashboard");
        }

        [HttpPost]
        public ActionResult SendRequest(int productId, int amount, int restaurantId)
        {

            var request = new Request
            {
                ProductId = productId,
                Amount = amount,
                RestaurantId = restaurantId
            };

            _context.Requests.Add(request);
            _context.SaveChanges();
            return RedirectToAction("RestaurantDashboard");
        }

        public ActionResult EmployeefDashboard()
        {
            if (Session["Email"] == null)
            {

                return RedirectToAction("Index", "LogIn");
            }

            string use = Session["Email"].ToString();
            SignUp r = _context.SignUps.FirstOrDefault(u => u.Email == use);
            int AccountType = Convert.ToInt32(r.AccountType);

            if (AccountType != 3)
            {
                return RedirectToAction("Index", "LogIn");
            }

            int loggedInEmployeeId = r.UserId;

            var productQuantities = _context.RequestForEmployees
                .Where(e => e.EmployeeId == loggedInEmployeeId && e.ProductId != null)
                .GroupBy(e => e.ProductId)
                .Select(group => new { ProductId = group.Key.Value, Quantity = group.Sum(e => e.Amount) })
                .ToList();

            var productDetails = new List<ProductWithQuntity>();
            foreach (var item in productQuantities)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);

                if (product != null)
                {
                    var requestForEmployee = _context.RequestForEmployees.FirstOrDefault(t => t.ProductId == item.ProductId);

                    if (requestForEmployee != null)
                    {
                        var productWithQuantity = new ProductWithQuntity
                        {
                            Product = product,
                            Quantity = Convert.ToInt32(item.Quantity),
                            RestaurantName = requestForEmployee.RestaurantName
                        };

                        productDetails.Add(productWithQuantity);
                    }
                }
            }

            ViewBag.Employee = r.Name;


            return View(productDetails);
        }

        [HttpPost]
        public ActionResult AcceptRequestForEmployee(int productId, int amount, string RestaurantName)
        {

            var notification = new Notificationn
            {
                RequestType = 1,
                ProductId = productId,
                Amount = amount,
                Status = 1,
                Restaurant = RestaurantName
            };

            _context.Notificationns.Add(notification);
            _context.SaveChanges();

            var requestToDelete = _context.RequestForEmployees.FirstOrDefault(r => r.ProductId == productId);
            if (requestToDelete != null)
            {
                _context.RequestForEmployees.Remove(requestToDelete);
                _context.SaveChanges();
            }


            return RedirectToAction("EmpolyeeDashboard");


        }

        [HttpPost]
        public ActionResult RejectRequestForEmployee(int productId, int amount, string restaurantName)
        {
            var notification = new Notificationn
            {
                RequestType = 1,
                ProductId = productId,
                Amount = amount,
                Status = 2,
                Restaurant = restaurantName
            };

            _context.Notificationns.Add(notification);
            _context.SaveChanges();

            var requestToDelete = _context.RequestForEmployees.FirstOrDefault(r => r.ProductId == productId);
            if (requestToDelete != null)
            {
                _context.RequestForEmployees.Remove(requestToDelete);
                _context.SaveChanges();
            }

            return RedirectToAction("EmployeeDashboard");
        }



        public ActionResult LogOut()
        {
            Session["Email"] = null;

            return RedirectToAction("RestaurantDashboard");
        }




    }
}