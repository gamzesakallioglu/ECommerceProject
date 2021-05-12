using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using ECommerceProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =ProjectConstant.Role_Admin+","+ ProjectConstant.Role_Employee)]
    public class CategoryController : Controller
    {
        #region VARIABLES
        private readonly IUnitofWork _uow; 
        #endregion

        #region CTOR
        public CategoryController(IUnitofWork uow)
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
            var allObj = _uow.category.GetAll();
            return Json(new { data = allObj });
        } 

        public IActionResult Delete(int id)
        {
            var data = _uow.category.Get(id);
            if (data == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı" });
            }
            _uow.category.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Kategori silindi" });

        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Category cat = new Category();
            if (id == null)
            {
                // This for create
                return View(cat);
            }

            cat = _uow.category.Get((int)id);
            if (cat != null)
            {
                return View(cat);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _uow.category.Add(category);
                }
                else
                {
                    _uow.category.Update(category);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }
            return View(category);       
        }

    }
}
