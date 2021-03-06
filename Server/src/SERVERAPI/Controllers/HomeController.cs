﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using SERVERAPI.Models;
using SERVERAPI.Models.Impl;
using SERVERAPI.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Diagnostics;

namespace SERVERAPI.Controllers
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, string value)
        {
            session.SetString(key, value);
        }
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string x = context.HttpContext.Session.GetString("active");
            if (x == null)
            {
                context.HttpContext.Session.SetString("active", "active");
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index"
                }));
            }
            base.OnActionExecuting(context);
        }
    }
    ////[SessionTimeout]
    public class HomeController : Controller
    {
        public IHostingEnvironment _env { get; set; }
        public UserData _ud { get; set; }
        public Models.Impl.StaticData _sd { get; set; }
        public AppSettings _settings;

        public HomeController(IHostingEnvironment env, UserData ud, Models.Impl.StaticData sd)
        {
            _env = env;
            _ud = ud;
            _sd = sd;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public IActionResult About()
        {
            return Redirect(_sd.GetExternalLink("aboutmessage"));
        }
        public IActionResult Help()
        {
            return Redirect(_sd.GetExternalLink("helpmessage"));
        }

        public IActionResult Index()
        {
            IndexViewModel lvm = new IndexViewModel();
            FarmData fd = _ud.FarmData();
            if(fd != null && fd.unsaved)
            {
                lvm.unsavedData = true;
            }

            lvm.welcomeMsg = _sd.GetUserPrompt("welcome");
            lvm.disclaimerMsg = _sd.GetUserPrompt("disclaimer");
            lvm.staticDataVersionMsg = _sd.GetStaticDataVersion();


            ViewBag.Title = "NMP";
            //LoadStatic();

            return View(lvm);

        }
        [HttpPost]
        public IActionResult Index(LaunchViewModel lvm)
        {
            _ud.NewFarm();
            return RedirectToAction("Farm","Farm");
        }
        public IActionResult NewWarning()
        {
            NewWarningViewModel nvm = new NewWarningViewModel();
            nvm.msg = _sd.GetUserPrompt("fileoverwritewarning");
            return View(nvm);
        }
        [HttpPost]
        public IActionResult NewWarning(NewWarningViewModel nvm)
        {
                _ud.NewFarm();
                string url = Url.Action("Farm", "Farm");
                return Json(new { success = true, url = url });
        }

        [HttpGet]
        public IActionResult Print()
        {
            var userData = HttpContext.Session.GetObjectFromJson<FarmData>("FarmData");

            FileContentResult result = null;
            //JSONResponse result = null;
            var pdfHost = Environment.GetEnvironmentVariable("PDF_SERVICE_NAME");

            //string pdfHost = Configuration["PDF_SERVICE_NAME"];

            string targetUrl = pdfHost + "/api/PDF/BuildPDF";

            ViewBag.Service = targetUrl;

            // call the microservice
            try
            {
                PDFRequest req = new PDFRequest();

                HttpClient client = new HttpClient();

                string rawdata = "<!DOCTYPE html><html><head><title></title></head><body><div style='width: 100%; background-color:lightgreen'>Section 1</div><br><div style='page -break-after:always; '></div><div style='width: 100%; background-color:lightgreen'>Section 2</div></body></html>";

                req.html = rawdata;

                string payload = JsonConvert.SerializeObject(req);

                var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                request.Headers.Clear();
                // transfer over the request headers.
                foreach (var item in Request.Headers)
                {
                    string key = item.Key;
                    string value = item.Value;
                    request.Headers.Add(key, value);
                }

                Task<HttpResponseMessage> responseTask = client.SendAsync(request);
                responseTask.Wait();

                HttpResponseMessage response = responseTask.Result;

                ViewBag.StatusCode = response.StatusCode.ToString();

                if (response.StatusCode == HttpStatusCode.OK) // success
                {
                    var bytetask = response.Content.ReadAsByteArrayAsync();
                    bytetask.Wait();

                    result = new FileContentResult(bytetask.Result, "application/pdf");
                }
            }
            catch (Exception e)
            {
                result = null;
            }

            return result;
        }
        private async Task<JSONResponse> BuildReport(INodeServices nodeServices)
        {
            JSONResponse result = null;
            var options = new { format = "letter", orientation = "landscape" };

            var opts = new
            {
                orientation = "landscape",

            };

            string rawdata = "<!DOCTYPE html><html><head><meta charset='utf-8' /><title></title></head><body><div style='width: 100%; background-color:lightgreen'>Section 1</div><br><div style='page -break-after:always; '></div><div style='width: 100%; background-color:lightgreen'>Section 2</div></body></html>";

            // execute the Node.js component
            result = await nodeServices.InvokeAsync<JSONResponse>("pdf.js", rawdata, options);

            return result;
        }
        public void LoadStatic()
        {
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream("SERVERAPI.Data.static.json");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                string staticValues = reader.ReadToEnd();
                HttpContext.Session.Set("Static", Encoding.ASCII.GetBytes(staticValues));
            }
        }
        public IActionResult Download()
        {
            FarmData farmData = _ud.FarmData();
            farmData.unsaved = false;
            _ud.SaveFarmData(farmData);

            var fileName = farmData.farmDetails.year + " - " + farmData.farmDetails.farmName + ".nmp";
            byte[] fileBytes = Encoding.ASCII.GetBytes(HttpContext.Session.GetString("FarmData"));
            return File(fileBytes, "application/octet-stream", fileName);
        }
        public IActionResult FileLoad()
        {
            FileLoadViewModel lvm = new FileLoadViewModel();
            lvm.warningMsg = _sd.GetUserPrompt("fileoverwritewarning");
            FarmData farmData = _ud.FarmData();

            if(farmData != null && farmData.unsaved)
            {
                lvm.unsavedData = true;
            }

            return View(lvm);
        }
        [HttpPost]
        public IActionResult FileLoad(FileLoadViewModel lvm)
        {
            FarmData fd;

            if(lvm.unsavedData)
            {
                ModelState.Clear();
                lvm.unsavedData = false;
                return View(lvm);
            }

            string fileContents = "";

            if (Request.Form.Files.Count > 0)
            {
                if (Request.Form.Files.Count > 1)
                {
                    ModelState.AddModelError("", "Only one file may be selected.");
                }
                else
                {
                    try
                    {
                        foreach (var file in Request.Form.Files)
                        {
                            var fileBytes = new byte[file.Length];

                            file.OpenReadStream().Read(fileBytes, 0, (int)file.Length);
                            fileContents = System.Text.Encoding.Default.GetString(fileBytes);
                        }

                        try
                        {
                            fd = JsonConvert.DeserializeObject<FarmData>(fileContents);
                        }
                        catch(Exception ex)
                        {
                            ModelState.AddModelError("", "File does not appear to be a valid NMP data file.");
                            return View(lvm);
                        }

                        // Returns message that successfully uploaded  
                        _ud.SaveFarmData(fd);
                        HttpContext.Session.SetObject("Farm", _ud.FarmDetails().farmName + " " + _ud.FarmDetails().year);

                        string url = Url.Action("Farm", "Farm");
                        return Json(new { success = true, url = url });
                    }
                    catch (Exception ex)
                    {
                        return Json("Error occurred. Error details: " + ex.Message);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "A file has not been selected.");
            }
            return View(lvm);
        }
        [HttpGet]
        public IActionResult SoilTests()
        {
            var farmData = HttpContext.Session.GetObjectFromJson<FarmData>("FarmData");

            FarmViewModel fvm = new FarmViewModel();

            return View(fvm);
        }
        [HttpPost]
        public IActionResult SoilTests(FarmViewModel fvm)
        {

            return View(fvm);
        }
        [HttpGet]
        public IActionResult Manure()
        {
            var farmData = HttpContext.Session.GetObjectFromJson<FarmData>("FarmData");

            FarmViewModel fvm = new FarmViewModel();

            return View(fvm);
        }
        [HttpPost]
        public IActionResult Manure(FarmViewModel fvm)
        {

            return View(fvm);
        }

        [HttpGet]
        public IActionResult ValidateStaticData()
        {            
            ValidateStaticDataViewModel vvm = new ValidateStaticDataViewModel();
            Utility.ValidateStaticData validate = new Utility.ValidateStaticData(_sd);
            StringBuilder sb = new StringBuilder("");

            List<Utility.StaticDataValidationMessages>  messages = validate.PerformValidation();

            if (messages.Count > 0)
            {
                foreach (Utility.StaticDataValidationMessages message in messages)
                {
                    sb.Append(String.Format("Validate error: {0} value of {1} does not exist in {2}. <br/>", message.Child, message.LinkData, message.Parent));
                }
                vvm.staticDataErrors = sb.ToString();
            }
            else
            {
                vvm.staticDataErrors = "No Errors";
            }

            return View(vvm);
        }
    }
}
