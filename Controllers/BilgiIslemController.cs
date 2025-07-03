using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MZDNETWORK.Attributes;

namespace MZDNETWORK.Controllers
{
    [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem")]
    public class BilgiIslemController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult YemekYukle()
        {
            var photoDirectoryMerkez = HttpContext.Server.MapPath("~/UploadPhotosMerkez");
            var photosMerkez = Directory.GetFiles(photoDirectoryMerkez).Select(Path.GetFileName).ToList();

            var photoDirectoryYerleske = HttpContext.Server.MapPath("~/UploadPhotosYerleske");
            var photosYerleske = Directory.GetFiles(photoDirectoryYerleske).Select(Path.GetFileName).ToList();

            ViewBag.PhotosMerkez = photosMerkez;
            ViewBag.PhotosYerleske = photosYerleske;

            return View();
        }

        [HttpGet]
        public ActionResult MolaYukle()
        {
            var photoDirectory = HttpContext.Server.MapPath("~/UploadPhotosMola");
            if (!Directory.Exists(photoDirectory))
                Directory.CreateDirectory(photoDirectory);

            var photos = Directory.GetFiles(photoDirectory).Select(Path.GetFileName).ToList();
            return View(photos);
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem.FoodPhoto", Action = "Manage")]
        public ActionResult UploadPhotoMerkez(HttpPostedFileBase photo)
        {
            if (photo != null && photo.ContentLength > 0)
            {
                var fileName = Path.GetFileName(photo.FileName);
                var path = Path.Combine(HttpContext.Server.MapPath("~/UploadPhotosMerkez"), fileName);
                try
                {
                    photo.SaveAs(path);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Fotoğraf yüklenirken bir hata oluştu.";
                }
            }
            return RedirectToAction("YemekYukle", "BilgiIslem");
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem.FoodPhoto", Action = "Manage")]
        public ActionResult UploadPhotoYerleske(HttpPostedFileBase photo)
        {
            if (photo != null && photo.ContentLength > 0)
            {
                var fileName = Path.GetFileName(photo.FileName);
                var path = Path.Combine(HttpContext.Server.MapPath("~/UploadPhotosYerleske"), fileName);
                try
                {
                    photo.SaveAs(path);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Fotoğraf yüklenirken bir hata oluştu.";
                }
            }
            return RedirectToAction("YemekYukle", "BilgiIslem");
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem.FoodPhoto", Action = "Manage")]
        public ActionResult DeleteYemekMerkez(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var path = Path.Combine(HttpContext.Server.MapPath("~/UploadPhotosMerkez"), fileName);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        TempData["Message"] = "Dosya başarıyla silindi.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Dosya silinirken bir hata oluştu: {ex.Message}";
                    }
                }
                else
                {
                    TempData["Error"] = "Dosya bulunamadı.";
                }
            }
            else
            {
                TempData["Error"] = "Geçersiz dosya adı.";
            }
            return RedirectToAction("YemekYukle", "BilgiIslem");
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem.FoodPhoto", Action = "Manage")]
        public ActionResult DeleteYemekYerleske(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var path = Path.Combine(HttpContext.Server.MapPath("~/UploadPhotosYerleske"), fileName);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        TempData["Message"] = "Dosya başarıyla silindi.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Dosya silinirken bir hata oluştu: {ex.Message}";
                    }
                }
                else
                {
                    TempData["Error"] = "Dosya bulunamadı.";
                }
            }
            else
            {
                TempData["Error"] = "Geçersiz dosya adı.";
            }
            return RedirectToAction("YemekYukle", "BilgiIslem");
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem.BreakPhoto", Action = "Manage")]
        public ActionResult UploadMolaPhoto(HttpPostedFileBase photo)
        {
            if (photo != null && photo.ContentLength > 0)
            {
                var fileName = Path.GetFileName(photo.FileName);
                var path = Path.Combine(HttpContext.Server.MapPath("~/UploadPhotosMola"), fileName);
                try
                {
                    photo.SaveAs(path);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Fotoğraf yüklenirken bir hata oluştu.";
                }
            }
            return RedirectToAction("MolaYukle", "BilgiIslem");
        }

        [HttpPost]
        [DynamicAuthorize(Permission = "InformationTechnology.BilgiIslem.BreakPhoto", Action = "Manage")]
        public ActionResult DeleteMola(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var path = Path.Combine(HttpContext.Server.MapPath("~/UploadPhotosMola"), fileName);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        TempData["Message"] = "Dosya başarıyla silindi.";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Dosya silinirken bir hata oluştu: {ex.Message}";
                    }
                }
                else
                {
                    TempData["Error"] = "Dosya bulunamadı.";
                }
            }
            else
            {
                TempData["Error"] = "Geçersiz dosya adı.";
            }
            return RedirectToAction("MolaYukle", "BilgiIslem");
        }
    }
}