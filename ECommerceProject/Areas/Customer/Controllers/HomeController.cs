using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models;
using ECommerceProject.Models.DbModels;
using ECommerceProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerceProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _uow;

        public HomeController(ILogger<HomeController> logger, IUnitofWork uow)
        {
            _logger = logger;
            _uow = uow;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _uow.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var shoppingCount = _uow.ShoppingCard.GetAll(a => a.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(ProjectConstant.ShoppingCard, shoppingCount);
            }

            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var product = _uow.Product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category");
            ShoppingCard card = new ShoppingCard()
            {
                Product = product,
                ProductId = product.Id
            };

            return View(card);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCard cardObj)
        {
            cardObj.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cardObj.ApplicationUserId = claim.Value;

                ShoppingCard fromDb = _uow.ShoppingCard.GetFirstOrDefault(s => s.ApplicationUserId == cardObj.ApplicationUserId
                && s.ProductId == cardObj.ProductId, includeProperties: "Product");

                if (fromDb == null)  //Insert
                {
                    _uow.ShoppingCard.Add(cardObj);
                }

                else  //Update
                {
                    fromDb.Count += cardObj.Count;
                }
                _uow.Save();

                var shoppingCount = _uow.ShoppingCard.GetAll(a => a.ApplicationUserId == cardObj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(ProjectConstant.ShoppingCard, shoppingCount);

                return RedirectToAction("Details");
            }
            else
            {
                var product = _uow.Product.GetFirstOrDefault(p => p.Id == cardObj.ProductId, includeProperties: "Category");
                ShoppingCard card = new ShoppingCard()
                {
                    Product = product,
                    ProductId = product.Id
                };

                return View(card);
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
