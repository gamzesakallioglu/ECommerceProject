using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using ECommerceProject.Models.ViewModels;
using ECommerceProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin + "," + ProjectConstant.Role_Employee)]
    public class ProductController : Controller
    {
        #region VARIABLES
        private readonly IUnitofWork _uow;
        private readonly IWebHostEnvironment _hostEnvironment;
        #endregion

        #region CTOR
        public ProductController(IUnitofWork uow, IWebHostEnvironment hostEnvironment)
        {
            _uow = uow;
            _hostEnvironment = hostEnvironment;
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
            var allObj = _uow.Product.GetAll(includeProperties: "Category");
            return Json(new { data = allObj });
        } 

        public IActionResult Delete(int id)
        {
            var data = _uow.Product.Get(id);
            
            if (data == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }
            string webRootPath = _hostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, data.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _uow.Product.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Ürün silindi" });

        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
           
            ProductVM productVM = new ProductVM() {
                Product = new Product(),
                CategoryList = _uow.category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.CategoryName,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _uow.cover_type.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                
            };

            if (id == null)
            {
                return View(productVM);
            }

            productVM.Product = _uow.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extenstion = Path.GetExtension(files[0].FileName);

                    if(productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }

                    }
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extenstion;

                }
                else
                {
                    if(productVM.Product.Id != 0)
                    {
                        var productData = _uow.Product.Get(productVM.Product.Id);
                        productVM.Product.ImageUrl = productData.ImageUrl;
                    }
                }



                if (productVM.Product.Id == 0)
                {
                    _uow.Product.Add(productVM.Product);
                }
                else
                {
                    _uow.Product.Update(productVM.Product);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _uow.category.GetAll().Select(a => new SelectListItem
                {
                    Text = a.CategoryName,
                    Value = a.Id.ToString()

                });
                productVM.CoverTypeList = _uow.cover_type.GetAll().Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()

                });
                if(productVM.Product.Id != 0)
                {
                    productVM.Product = _uow.Product.Get(productVM.Product.Id);
                }
            }
            return View(productVM);       
        }

    }
}

/*Category cat = new Category();
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

           return NotFound();*/
