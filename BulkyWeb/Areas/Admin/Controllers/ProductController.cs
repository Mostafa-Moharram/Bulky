using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.ProductRepository.GetAll(new string[] {"Category"}));
        }
        public IActionResult Upsert(int? id)
        {
            Product product = (id is null || id == 0) ? new() : _unitOfWork.ProductRepository.Get(product => product.Id == id);
            return View(new ProductVM()
            {
                Product = product,
                CategoryList = _unitOfWork.CategoryRepository
                    .GetAll().OrderBy(category => category.DisplayOrder)
                    .Select(category => {
                        var categoryListItem = new SelectListItem(category.Name, category.Id.ToString());
                        if (category.Id == product.Id)
                            categoryListItem.Selected = true;
                        return categoryListItem;
                        })
            });
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (!ModelState.IsValid)
                return View(productVM);
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file is not null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\products");
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    System.IO.File.Delete(Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\')));
                productVM.Product.ImageUrl = @"\images\products\" + fileName;
            }
            if (productVM.Product.Id == 0)
            {
                _unitOfWork.ProductRepository.Add(productVM.Product);
                TempData["success"] = "The product is created successfully!";
            } else
            {
                _unitOfWork.ProductRepository.Update(productVM.Product);
                TempData["success"] = "The product is updated successfully!";
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.ProductRepository.GetAll(new string[] { "Category" });
            return Json(new {data = productList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = _unitOfWork.ProductRepository.Get(product => product.Id == id);
            if (product is null)
                return Json(new { status = "failed", message = "The provided product Id doesn't exist." });
            if (!string.IsNullOrEmpty(product.ImageUrl))
                System.IO.File.Delete(Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\')));
            _unitOfWork.ProductRepository.Remove(product);
            _unitOfWork.Save();
            return Json(new { status = "success", message = "The product is deleted successfully." });
        }
        #endregion
    }
}
