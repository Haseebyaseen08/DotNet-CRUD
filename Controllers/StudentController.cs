using FinalTry;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StudentForm.Controllers
{
    public class StudentController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        // GET: Student
        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var con = new FormEntities();
            var std = con.Students;
            var result = std.ToDataSourceResult(request, student => new
            {
                StudentID = student.StudentID,
                Username = student.Username,
                Name = student.Name,
                Email = student.Email,
                Phone = student.Phone,
                Address = student.Address,
                Province = new Province()
                {
                    ProvinceCode = student.Province.ProvinceCode,
                    Name = student.Province.Name
                },
                City = new City()
                {
                    CityCode = student.City.CityCode,
                    Name = student.City.Name
                }
            }
            );
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View(new Student());
        }

        [HttpPost]
        public ActionResult Create(Student std)
        {
            if (ModelState.IsValid)
            {
                if (std.ImageFile != null)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(std.ImageFile.FileName);
                    string[] ext = { ".jpg", ".png", ".jpeg" };
                    string extenstion = System.IO.Path.GetExtension(std.ImageFile.FileName);
                    var ext_find = Array.Find(ext, name => name == extenstion);
                    if (ext_find == null)
                    {
                        ModelState.AddModelError("Picture", "Image Extention must be .png, .jpg or .jpeg");
                        return View(std);
                    }
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extenstion;
                    std.Picture = "~/Image/" + fileName;
                    fileName = System.IO.Path.Combine(Server.MapPath("~/Image/"), fileName);
                    std.ImageFile.SaveAs(fileName);
                }

                using (var context = new FormEntities())
                {
                    var checkUser = (from s in context.Students
                                     where s.Username == std.Username
                                     select s).FirstOrDefault();
                    if (checkUser != null)
                    {
                        ModelState.AddModelError("Username", "User already exist. Try new name.");
                        return View(std);
                    }
                    else
                    {
                        context.Students.Add(std);
                        context.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            else
                return View(std);
        }

        public ActionResult Province_list()
        {
            var con = new FormEntities();

            return Json(con.Provinces.Select(c => new { ProvinceID = c.ProvinceCode, ProvinceName = c.Name }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult City_list(string id)
        {
            var con = new FormEntities();
            if (id != null)
            {
                var cities = from s in con.Cities
                             where s.ProvinceID == id
                             select s;
                return Json(cities.Select(c => new { CityID = c.CityCode, CityName = c.Name }), JsonRequestBehavior.AllowGet);
            }

            else
                return Json(con.Cities.Select(s => new { CityID = s.CityCode, CityName = s.Name }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Student_Detail(int id)
        {
            var con = new FormEntities();
            var std = (from s in con.Students
                       where s.StudentID == id
                       select s).FirstOrDefault();
            return View(std);
        }

        [HttpPost]
        public ActionResult Student_Detail(Student st)
        {
            if (ModelState.IsValid)
            {
                using (var context = new FormEntities())
                {
                    var std = context.Students.SingleOrDefault(b => b.StudentID == st.StudentID);
                    if (st.ImageFile != null)
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(st.ImageFile.FileName);
                        string[] ext = { ".jpg", ".png", ".jpeg" };
                        string extenstion = System.IO.Path.GetExtension(st.ImageFile.FileName);
                        var ext_find = Array.Find(ext, name => name == extenstion);
                        if (ext_find == null)
                        {
                            ModelState.AddModelError("Picture", "Image Extention must be .png, .jpg or .jpeg");
                            return View(st);
                        }
                        if (std.Picture != null)
                        {
                            var fullPath = Request.MapPath(std.Picture);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extenstion;
                        std.Picture = "~/Image/" + fileName;
                        fileName = System.IO.Path.Combine(Server.MapPath("~/Image/"), fileName);
                        st.ImageFile.SaveAs(fileName);
                    }

                    var checkUser = (from s in context.Students
                                     where s.Username == st.Username
                                     select s).ToList();
                    if (checkUser.Count > 0 && st.Username!=std.Username)
                    {
                        ModelState.AddModelError("Username", "User already exist. Try new name.");
                        return View(st);
                    }

                    std.Username = st.Username;
                    std.Name = st.Name;
                    std.Phone = st.Phone;
                    std.Address = st.Address;
                    std.Email = st.Email;
                    std.ProvinceID = st.ProvinceID;
                    std.CityID = st.CityID;
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(st);
        }

        public ActionResult Student_info(int id)
        {
            var context = new FormEntities();
            var std = context.Students.SingleOrDefault(b => b.StudentID == id);
            return View(std);
        }

        public ActionResult ManualPages()
        {
            return View();
        }
        public ActionResult pageResult([DataSourceRequest] DataSourceRequest request,int pageNumber,int perPageResult)
        {
            using (var context = new FormEntities())
            {

                var recordSize = new ObjectParameter("RecordSize", typeof(int));
                var std = context.Students_get(perPageResult, pageNumber, recordSize).ToList();
                ViewBag.TotalRecord = recordSize.Value;
                //Console.WriteLine($"Show result of page:{pageNumber}/{Convert.ToInt32(totalPages.Value)}");
                DataSourceResult result=std.ToDataSourceResult(request, student => new
                {
                    StudentID = student.StudentID,
                    Username = student.Username,
                    Name = student.Name,
                    Email = student.Email,
                    Phone = student.Phone,
                    Address = student.Address,
                }
            );
                return Json(result, JsonRequestBehavior.AllowGet);
            }
           
        }
    }
}