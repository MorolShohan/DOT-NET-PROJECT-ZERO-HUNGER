using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using Zero_Hunger.EF;
using Zero_Hunger.Models;

namespace Zero_Hunger.Controllers
{
    public class SignUpController : Controller
    {

        // GET: SignUp
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SignUpDTO obj)
        {
            if (ModelState.IsValid)
            {


                var db = new zero_hungerEntities1();

                var existingUser = db.SignUps.FirstOrDefault(e => e.Email == obj.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(obj);
                }
                var SignUpdata = new SignUp
                {
                    Name = obj.Name,
                    Email = obj.Email,
                    Password = obj.Password,
                    AccountType = obj.AccountType,
                    ResturentName = obj.ResturentName,
                    Address = obj.Address,

                };

                TempData["SignUpData"] = SignUpdata;



                Random random = new Random();
                int otp = random.Next(100000, 999999);
                Session["OTP"] = otp;


                MailMessage mm = new MailMessage("gardenaid29@gmail.com", obj.Email);


                mm.Subject = "OTP From Zero Hunger";
                mm.Body = "Your OTP is : " + otp;
                mm.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;

                NetworkCredential nc = new NetworkCredential("gardenaid29@gmail.com", "rjwhlucthgnjwmbm");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = nc;
                smtp.Send(mm);
                return RedirectToAction("otp");

            }

            return View(obj);
        }
        public ActionResult otp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult otp(string Otp, SignUpDTO obj)
        {
            int? storedOTP = Session["OTP"] as int?;

            if (Otp == storedOTP.ToString())
            {

                var signUpData = TempData["SignUpData"] as SignUp;


                var db = new zero_hungerEntities1();
                db.SignUps.Add(signUpData);
                db.SaveChanges();


                Session["OTP"] = null;


                return RedirectToAction("Index", "LogIn", new { area = "" });


            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect OTP";
            }



            return View();
        }


    }
}