using Dapper;
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
    public class CoverTypeController : Controller
    {
        #region VARIABLES
        private readonly IUnitofWork _uow; 
        #endregion

        #region CTOR
        public CoverTypeController(IUnitofWork uow)
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
            //var allObj = _uow.cover_type.GetAll();
            var allCoverTypes = _uow.sp_call.List<CoverType>(ProjectConstant.Proc_CoverType_GetAll, null);
            return Json(new { data = allCoverTypes });
        } 

        public IActionResult Delete(int id)
        {
            //var data = _uow.cover_type.Get(id);
            //if (data == null)
            //{
            //    return Json(new { success = false, message = "Kapak bulunamadı" });
            //}
            //_uow.cover_type.Remove(data);
            //_uow.Save();
            //return Json(new { success = true, message = "Kapak silindi" });

            var param = new DynamicParameters();
            param.Add("@Id", id);
            var data = _uow.sp_call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, param);
            if (data == null)
            {
                return Json(new { success = false, message = "Kapak bulunamadı" });
            }
            _uow.sp_call.Execute(ProjectConstant.Proc_CoverType_Delete, param);
            _uow.Save();
            return Json(new { success = true, message = "Kapak silindi" });

        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            CoverType ct = new CoverType();
            if (id == null)
            {
                // This for create
                return View(ct);
            }

            //ct = _uow.cover_type.Get((int)id);
            //if (ct != null)
            //{
            //    return View(ct);
            //}

            //return NotFound();
            var param = new DynamicParameters();
            param.Add("@Id", (int)id);
            ct = _uow.sp_call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, param);
            if (ct != null) {
                return View(ct);
            }
            return NotFound();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {           
            if (ModelState.IsValid)
            {
                var param = new DynamicParameters();
                param.Add("@Name", coverType.Name);

                if (coverType.Id == 0)
                {
                    //_uow.cover_type.Add(coverType);

                    _uow.sp_call.Execute(ProjectConstant.Proc_CoverType_Create, param);
                }
                else
                {
                    //_uow.cover_type.Update(coverType);
                    param.Add("@Id", coverType.Id);
                    _uow.sp_call.Execute(ProjectConstant.Proc_CoverType_Update, param);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }
            return View(coverType);       
        }

    }
}
