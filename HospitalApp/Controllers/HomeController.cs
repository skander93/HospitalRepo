using HospitalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HospitalApp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using System.Data.Entity;


namespace HospitalApp.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetData()
        {
            
            using (DBModels db = new DBModels())  
            {
                db.Configuration.LazyLoadingEnabled = false;

                var surgeriesList = (from p in db.Surgeries
                                     from e in db.Surgeons
                                     where p.fileId == e.fileId && p.surgeonId == e.id
                              select new
                              {
                                  id = p.pk,
                                  entryDate = p.entryDate,
                                  exitDate = p.exitDate,
                                  plannedDuration = p.plannedDuration,
                                  status = p.status,
                                  firstName = e.firstName,
                                  lastName = e.lastName,
                                  idSurgeon = e.pk,
                                  fileId = p.fileId
                              }).ToList();

                //List<Surgeons> surgonsList = db.Surgeons.ToList<Surgeons>();
                return Json(new { data = surgeriesList }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ImportDataSurgeons()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportDataSurgeons(List<Surgeons> surgonsList)
        {
            bool status = false;
            if (ModelState.IsValid)
            {
                using (DBModels db = new DBModels())  
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    foreach (var i in surgonsList)
                    {
                        db.Surgeons.Add(i);
                    }              
                    db.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }

        [HttpPost]
        public ActionResult ImportDataSurgeries(List<Surgeries> surgeriesList)
        {
            bool status = false;
            if (ModelState.IsValid)
            {
                using (DBModels db = new DBModels())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    foreach (var i in surgeriesList)
                    {
                        db.Surgeries.Add(i);
                    }
                    db.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }  

        //

        [HttpPost]
        public ActionResult ImportFile(File file)
        {
            bool status2 = false;
            if (ModelState.IsValid)
            {
                using (DBModels db = new DBModels())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    db.File.Add(file);
                    
                    db.SaveChanges();
                    status2 = true;
                }
            }
            return new JsonResult { Data = new { status2 = status2 , file = file } };
        }

        [HttpPost]
        public ActionResult GetFile(File file)
        {
            bool status = false;
            using (DBModels db = new DBModels())
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<File> files = db.File.Where(s => s.name == file.name).ToList();
                if (files.Count() >= 1) { status = true; }
                if (files.Count() == 0) { status = false; }
                return Json(new { data = files, status = status }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFiltredData()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult GetFiltredDataResult(string jsonReceiverInCsharpObjecName)
        {
            string lastName = null;
            string firstName = null;
             dynamic dynData =JsonConvert.DeserializeObject<ExpandoObject>
               (jsonReceiverInCsharpObjecName, new ExpandoObjectConverter());

            foreach (KeyValuePair<string, object> transItem in dynData )
            {
               if (transItem.Key == "lastName") {
                lastName = Convert.ToString(transItem.Value);

               }
               else if (transItem.Key == "firstName")
               {
                  firstName = Convert.ToString(transItem.Value);
               }
             }

       
                DBModels db = new DBModels();
                db.Configuration.LazyLoadingEnabled = false;
                var surgeriesList = (from p in db.Surgeries
                                     from e in db.Surgeons
                                     where p.fileId == e.fileId && p.surgeonId == e.id
                                     select new
                                     {
                                         id = p.pk,
                                         entryDate = p.entryDate,
                                         exitDate = p.exitDate,
                                         plannedDuration = p.plannedDuration,
                                         status = p.status,
                                         firstName = e.firstName,
                                         lastName = e.lastName,
                                         idSurgeon = e.pk,
                                         fileId = p.fileId
                                     });
                if (!String.IsNullOrEmpty(lastName))
                {
                    surgeriesList = surgeriesList.Where(s => s.lastName.Contains(lastName)
                                           || s.firstName.Contains(firstName));
                }             
            
            return Json(new { data = surgeriesList.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMostEfficientSurgeons()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetMostEfficientSurgeonsData()
        {

            using (DBModels db = new DBModels())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var surgeriesList = (from p in db.Surgeries
                                     from e in db.Surgeons
                                     where p.fileId == e.fileId && p.surgeonId == e.id && (DbFunctions.DiffMinutes(p.exitDate, p.entryDate) < p.plannedDuration) && p.status != "canceled"
                                     group p by new { e.firstName, e.lastName } into g
                                     let count = g.Count()
                                     orderby count descending
                                      select new
                                     {                                        
                                         firstName = g.Key.firstName,
                                         lastName = g.Key.lastName,
                                         count = count,
                                     }).ToList();
                //List<Surgeons> surgonsList = db.Surgeons.ToList<Surgeons>();
                return Json(new { data = surgeriesList }, JsonRequestBehavior.AllowGet);
            }
        }

	}

}
