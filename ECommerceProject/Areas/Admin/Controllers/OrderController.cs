using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using ECommerceProject.Models.ViewModels;
using ECommerceProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerceProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitofWork _uow;
        [BindProperty]
        public OrderDetailsVM OrderDetailsVM { get; set; }
        public OrderController(IUnitofWork uow)
        {
            _uow = uow;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderDetailsVM = new OrderDetailsVM()
            {
                OrderHeader = _uow.OrderHeader.GetFirstOrDefault(o => o.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _uow.OrderDetail.GetAll(o => o.OrderId == id, includeProperties: "Product")
            };
            return View(OrderDetailsVM);
        }

        [Authorize(ProjectConstant.Role_Admin + "," + ProjectConstant.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            orderHeader.OrderStatus = ProjectConstant.statusInProcess;
            _uow.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(ProjectConstant.Role_Admin + "," + ProjectConstant.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(o => o.Id == OrderDetailsVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderDetailsVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailsVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = ProjectConstant.statusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            return RedirectToAction("Index");
        }

        [Authorize(ProjectConstant.Role_Admin + "," + ProjectConstant.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _uow.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            if (orderHeader.PaymentStatus == ProjectConstant.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                orderHeader.OrderStatus = ProjectConstant.statusRefund;
                orderHeader.PaymentStatus = ProjectConstant.statusRefund;
            }
            else
            {
                orderHeader.OrderStatus = ProjectConstant.statusCancelled;
                orderHeader.PaymentStatus = ProjectConstant.statusCancelled;
            }

            _uow.Save();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(ProjectConstant.Role_Admin) || User.IsInRole(ProjectConstant.Role_Employee))
                orderHeaderList = _uow.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            else
                orderHeaderList = _uow.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == ProjectConstant.PaymentStatusDelayed);
                    break;

                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(o =>
                                        o.OrderStatus == ProjectConstant.StatusApproved
                                        || o.OrderStatus == ProjectConstant.statusInProcess
                                        || o.OrderStatus == ProjectConstant.StatusPending);
                    break;

                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == ProjectConstant.statusShipped);
                    break;

                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == ProjectConstant.statusCancelled
                                                            || o.OrderStatus == ProjectConstant.statusRefund
                                                            || o.PaymentStatus == ProjectConstant.PaymentStatusRejected);
                    break;

                default:
                    break;
            }


            return Json(new { data = orderHeaderList });
        }

    }
}
