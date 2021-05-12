using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using ECommerceProject.Models.ViewModels;
using ECommerceProject.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ECommerceProject.Areas.Customer
{
    [Area("Customer")]
    public class CardController : Controller
    {
        private readonly IUnitofWork _uow;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        public CardController(IUnitofWork uow, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _uow = uow;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        [BindProperty]
        public ShoppingCardVM ShoppingCardVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCardVM = new ShoppingCardVM()
            {
                OrderHeader = new OrderHeader(),
                ListCard = _uow.ShoppingCard.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            ShoppingCardVM.OrderHeader.OrderTotal = 0;
            ShoppingCardVM.OrderHeader.ApplicationUser =
                _uow.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");

            foreach (var item in ShoppingCardVM.ListCard)
            {
                item.Price = ProjectConstant.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCardVM.OrderHeader.OrderTotal += (item.Price * item.Count);
                if (item.Product.Description != null)
                {
                    item.Product.Description = ProjectConstant.ConvertToRawHtml(item.Product.Description);
                    if (item.Product.Description.Length > 50)
                    {
                        item.Product.Description = item.Product.Description.Substring(0, 49) + "...";
                    }
                }

            }

            return View(ShoppingCardVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _uow.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (user == null)
                ModelState.AddModelError(string.Empty, "Doğrulama maili boş");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Hesabınızı doğrulamak için <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya tıklayınız</a>.");

            ModelState.AddModelError(string.Empty, "Doğrulama maili gönderildi, lütfen mail kutunuzu kontrol ediniz.");
            return RedirectToAction("Index");
        }

        public IActionResult Plus(int cardId)
        {
            var card = _uow.ShoppingCard.GetFirstOrDefault(s => s.Id == cardId, includeProperties: "Product");
            if (card == null)
                return RedirectToAction("Index");

            card.Count += 1;
            card.Price = ProjectConstant.GetPriceBasedOnQuantity(card.Count, card.Product.Price, card.Product.Price50, card.Product.Price100);

            _uow.Save();

            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cardId)
        {
            var card = _uow.ShoppingCard.GetFirstOrDefault(s => s.Id == cardId, includeProperties: "Product");
            if (card == null)
                return RedirectToAction("Index");

            if (card.Count == 1)
            {
                var cnt = _uow.ShoppingCard.GetAll(s => s.ApplicationUserId == card.ApplicationUserId).ToList().Count;
                _uow.ShoppingCard.Remove(card);
                _uow.Save();
                HttpContext.Session.SetInt32(ProjectConstant.ShoppingCard, cnt - 1);
            }
            else
            {
                card.Count -= 1;
                card.Price = ProjectConstant.GetPriceBasedOnQuantity(card.Count, card.Product.Price, card.Product.Price50, card.Product.Price100);

                _uow.Save();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cardId)
        {
            var card = _uow.ShoppingCard.GetFirstOrDefault(s => s.Id == cardId, includeProperties: "Product");
            if (card == null)
                return RedirectToAction("Index");

            var cnt = _uow.ShoppingCard.GetAll(s => s.ApplicationUserId == card.ApplicationUserId).ToList().Count;
            _uow.ShoppingCard.Remove(card);
            _uow.Save();
            HttpContext.Session.SetInt32(ProjectConstant.ShoppingCard, cnt - 1);
            return RedirectToAction("Index");

        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCardVM = new ShoppingCardVM()
            {
                OrderHeader = new OrderHeader(),
                ListCard = _uow.ShoppingCard.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            ShoppingCardVM.OrderHeader.ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");
            ShoppingCardVM.OrderHeader.OrderTotal = 0;

            foreach (var item in ShoppingCardVM.ListCard)
            {
                item.Price = ProjectConstant.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCardVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            ShoppingCardVM.OrderHeader.Name = ShoppingCardVM.OrderHeader.ApplicationUser.Name;
            ShoppingCardVM.OrderHeader.PhoneNumber = ShoppingCardVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCardVM.OrderHeader.Address = ShoppingCardVM.OrderHeader.ApplicationUser.Address;
            ShoppingCardVM.OrderHeader.City = ShoppingCardVM.OrderHeader.ApplicationUser.City;
            ShoppingCardVM.OrderHeader.State = ShoppingCardVM.OrderHeader.ApplicationUser.State;
            ShoppingCardVM.OrderHeader.PostCode = ShoppingCardVM.OrderHeader.ApplicationUser.PostCode;

            return View(ShoppingCardVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCardVM.OrderHeader.ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(a => a.Id == claim.Value, includeProperties: "Company");

            ShoppingCardVM.ListCard = _uow.ShoppingCard.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");

            ShoppingCardVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusPending;
            ShoppingCardVM.OrderHeader.OrderStatus = ProjectConstant.StatusPending;
            ShoppingCardVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCardVM.OrderHeader.OrderDate = DateTime.Now;

            _uow.OrderHeader.Add(ShoppingCardVM.OrderHeader);
            _uow.Save();

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            foreach (var orderDetail in ShoppingCardVM.ListCard)
            {
                orderDetail.Price = ProjectConstant.GetPriceBasedOnQuantity(orderDetail.Count, orderDetail.Product.Price, orderDetail.Product.Price50, orderDetail.Product.Price100);

                OrderDetails oDetails = new OrderDetails()
                {
                    ProductId = orderDetail.ProductId,
                    OrderId = ShoppingCardVM.OrderHeader.Id,
                    Price = orderDetail.Price,
                    Count = orderDetail.Count
                };
                ShoppingCardVM.OrderHeader.OrderTotal += oDetails.Count * oDetails.Price;
                _uow.OrderDetail.Add(oDetails);
            }
            _uow.ShoppingCard.RemoveRange(ShoppingCardVM.ListCard);
            _uow.Save();
            HttpContext.Session.SetInt32(ProjectConstant.ShoppingCard, 0);

            if (stripeToken == null)
            {
                ShoppingCardVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCardVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusDelayed;
                ShoppingCardVM.OrderHeader.OrderStatus = ProjectConstant.StatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCardVM.OrderHeader.OrderTotal * 100),
                    Currency = "try",
                    Description = "Siparis no:" + ShoppingCardVM.OrderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.BalanceTransactionId == null)
                    ShoppingCardVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusRejected;
                else
                    ShoppingCardVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCardVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusApproved;
                    ShoppingCardVM.OrderHeader.OrderStatus = ProjectConstant.StatusApproved;
                    ShoppingCardVM.OrderHeader.PaymentDate = DateTime.Now;
                }
            }
            _uow.Save();
            return RedirectToAction("OrderConfirmation", "Card", new { id = ShoppingCardVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
