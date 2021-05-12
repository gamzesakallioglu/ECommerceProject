using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using ECommerceProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin + "," + ProjectConstant.Role_Employee)]
    public class CompanyController : Controller
    {
        #region VARIABLES
        private readonly IUnitofWork _uow; 
        #endregion

        #region CTOR
        public CompanyController(IUnitofWork uow)
        {
            _uow = uow;
        } 
        #endregion

        #region ACTIONS
        public IActionResult Index()
        {
            return View();
        } 
        #endregion

        #region API CALLS
        public IActionResult GetAll()
        {
            var allObj = _uow.Company.GetAll();
            return Json(new { data = allObj });
        } 

        public IActionResult Delete(int id)
        {
            var data = _uow.Company.Get(id);
            if (data == null)
            {
                return Json(new { success = false, message = "Şirket bulunamadı" });
            }
            _uow.Company.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Şirket silindi" });

        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Company com = new Company();
            if (id == null)
            {
                // This for create
                return View(com);
            }

            com = _uow.Company.Get((int)id);
            if (com != null)
            {
                return View(com);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _uow.Company.Add(company);
                }
                else
                {
                    _uow.Company.Update(company);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }
            return View(company);       
        }

    }
}
