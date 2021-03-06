﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using SERVERAPI.ViewModels;
using SERVERAPI.Models.Impl;
using SERVERAPI.Models;
using static SERVERAPI.Models.StaticData;
using SERVERAPI.Utility;

namespace SERVERAPI.Controllers
{
    //[RedirectingAction]
    public class ManureController : Controller
    {
        public IHostingEnvironment _env { get; set; }
        public UserData _ud { get; set; }
        public Models.Impl.StaticData _sd { get; set; }

        public ManureController(IHostingEnvironment env, UserData ud, Models.Impl.StaticData sd)
        {
            _env = env;
            _ud = ud;
            _sd = sd;
        }
        // GET: /<controller>/
        public IActionResult Manure()
        {           
            return View();
        }
        public IActionResult CompostDetails(int? id, string target)
        {
            //Utility.CalculateNutrients calculateNutrients = new CalculateNutrients(_env, _ud, _sd);
            //NOrganicMineralizations nOrganicMineralizations = new NOrganicMineralizations();

            CompostDetailViewModel mvm = new CompostDetailViewModel();

            mvm.act = id == null ? "Add" : "Edit";
            mvm.url = _sd.GetExternalLink("labanalysisexplanation");


            if (id != null)
            {
                FarmManure fm = _ud.GetFarmManure(id.Value);
                mvm.selManOption = fm.manureId;

                if (!fm.customized)
                {
                    mvm.bookValue = true;
                    mvm.compost = false;
                    mvm.onlyCustom = false;
                }
                else
                {
                    mvm.bookValue = false;
                    mvm.compost = fm.manure_class == "Compost" ? true : false;
                    mvm.onlyCustom = (fm.manure_class == "Other" || fm.manure_class == "Compost") ? true : false;
                }
                mvm.manureName = fm.name;
                mvm.moisture = fm.moisture;
                mvm.nitrogen = fm.nitrogen.ToString("#0.00");
                mvm.ammonia = fm.ammonia.ToString("#0");
                mvm.phosphorous = fm.phosphorous.ToString("#0.00");
                mvm.potassium = fm.potassium.ToString("#0.00");
                mvm.nitrate = fm.nitrate.HasValue ? fm.nitrate.Value.ToString("#0") : "";
            }
            else
            {
                mvm.bookValue = true;
                mvm.manureName = "  ";
                mvm.moisture = "  ";
                mvm.nitrogen = "  ";
                mvm.ammonia = "  ";
                mvm.phosphorous = "  ";
                mvm.potassium = "  ";
                mvm.nitrate = "  ";
                mvm.compost = false;
                mvm.onlyCustom = false;
            }

            CompostDetailsSetup(ref mvm);

            return PartialView(mvm);
        }
        private void CompostDetailsSetup(ref CompostDetailViewModel cvm)
        {
            cvm.manOptions = new List<Models.StaticData.SelectListItem>();
            cvm.manOptions = _sd.GetManuresDll().ToList();

            return;
        }
        [HttpPost]
        public IActionResult CompostDetails(CompostDetailViewModel cvm)
        {
            decimal userNitrogen = 0;
            decimal userAmmonia = 0;
            decimal userPhosphorous = 0;
            decimal userPotassium = 0;
            decimal userMoisture = 0;
            decimal userNitrate = 0;
            Models.StaticData.Manure man;

            CompostDetailsSetup(ref cvm);

            try
            {

                if (cvm.buttonPressed == "ManureChange")
                {
                    ModelState.Clear();
                    cvm.buttonPressed = "";

                    if (cvm.selManOption != 0)
                    {
                        man = _sd.GetManure(cvm.selManOption.ToString());
                        if(man.manure_class == "Other" ||
                           man.manure_class == "Compost")
                        {
                            cvm.bookValue = false;
                            cvm.onlyCustom = true;
                            cvm.nitrogen = string.Empty;
                            cvm.moisture = string.Empty;
                            cvm.ammonia = string.Empty;
                            cvm.nitrate = string.Empty;
                            cvm.phosphorous = string.Empty;
                            cvm.potassium = string.Empty;
                            cvm.compost = man.manure_class == "Compost" ? true : false;
                            cvm.manureName = cvm.compost ? "Custom - " + man.name + " - " : "Custom - " + man.solid_liquid + " - ";
                        }
                        else
                        {
                            cvm.bookValue = true;
                            cvm.nitrogen = man.nitrogen.ToString();
                            cvm.moisture = man.moisture.ToString();
                            cvm.ammonia = man.ammonia.ToString();
                            cvm.nitrate = string.Empty;
                            cvm.phosphorous = man.phosphorous.ToString();
                            cvm.potassium = man.potassium.ToString();
                            cvm.manureName = man.name;
                        }
                    }
                    else
                    {
                        cvm.bookValue = true;
                        cvm.nitrogen = string.Empty;
                        cvm.moisture = string.Empty;
                        cvm.ammonia = string.Empty;
                        cvm.nitrate = string.Empty;
                        cvm.phosphorous = string.Empty;
                        cvm.potassium = string.Empty;
                        cvm.manureName = string.Empty;
                    }
                    return View(cvm);
                }
                if (cvm.buttonPressed == "TypeChange")
                {
                    ModelState.Clear();
                    cvm.buttonPressed = "";

                    if (cvm.selManOption != 0)
                    {
                        man = _sd.GetManure(cvm.selManOption.ToString());
                        cvm.onlyCustom = false;
                        if (cvm.bookValue)
                        {
                            cvm.moisture = cvm.bookValue ? man.moisture.ToString() : "";
                            cvm.nitrogen = man.nitrogen.ToString();
                            cvm.ammonia = man.ammonia.ToString();
                            cvm.nitrate = string.Empty;
                            cvm.phosphorous = man.phosphorous.ToString();
                            cvm.potassium = man.potassium.ToString();
                            cvm.manureName = man.name;
                        }
                        else
                        {
                            cvm.nitrogen = string.Empty;
                            cvm.moisture = string.Empty;
                            cvm.ammonia = string.Empty;
                            cvm.nitrate = string.Empty;
                            cvm.phosphorous = string.Empty;
                            cvm.potassium = string.Empty;
                            cvm.manureName = (!cvm.compost) ? "Custom - " + man.name + " - " : "Custom - " + man.solid_liquid + " - ";

                            cvm.moistureBook = man.moisture.ToString();
                            cvm.nitrogenBook = man.nitrogen.ToString();
                            cvm.ammoniaBook = man.ammonia.ToString();
                            cvm.nitrateBook = string.Empty;
                            cvm.phosphorousBook = man.phosphorous.ToString();
                            cvm.potassiumBook = man.potassium.ToString();
                        }
                    }
                    else
                    {
                        cvm.nitrogen = string.Empty;
                        cvm.moisture = string.Empty;
                        cvm.ammonia = string.Empty;
                        cvm.nitrate = string.Empty;
                        cvm.phosphorous = string.Empty;
                        cvm.potassium = string.Empty;
                        cvm.manureName = string.Empty;
                    }
                    return View(cvm);
                }

                if (ModelState.IsValid)
                {
                    man = _sd.GetManure(cvm.selManOption.ToString());

                    if (!cvm.bookValue)
                    {
                        if (string.IsNullOrEmpty(cvm.moisture))
                        {
                            ModelState.AddModelError("moisture", "Required.");
                        }
                        else
                        {
                            if (!Decimal.TryParse(cvm.moisture, out userMoisture))
                            {
                                ModelState.AddModelError("moisture", "Numbers only.");
                            }
                            else
                            {
                                if (userMoisture < 0 || userMoisture > 100)
                                {
                                    ModelState.AddModelError("moisture", "Invalid %.");
                                }
                                else
                                {
                                    if(man.solid_liquid.ToUpper() == "SOLID")
                                    {
                                        if(userMoisture > 80)
                                        {
                                            ModelState.AddModelError("moisture", "Invalid % for solid.");
                                        }
                                    }
                                    else
                                    {
                                        if(userMoisture <= 80)
                                        {
                                            ModelState.AddModelError("moisture", "Invalid % for liquid.");
                                        }
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(cvm.nitrogen))
                        {
                            ModelState.AddModelError("nitrogen", "Required.");
                        }
                        else
                        {
                            if (!Decimal.TryParse(cvm.nitrogen, out userNitrogen))
                            {
                                ModelState.AddModelError("nitrogen", "Numbers only.");
                            }
                            else
                            {
                                if (userNitrogen < 0 || userNitrogen > 100)
                                {
                                    ModelState.AddModelError("nitrogen", "Invalid %.");
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(cvm.ammonia))
                        {
                            ModelState.AddModelError("ammonia", "Required.");
                        }
                        else
                        {
                            if (!Decimal.TryParse(cvm.ammonia, out userAmmonia))
                            {
                                ModelState.AddModelError("ammonia", "Numbers only.");
                            }
                        }
                        if (string.IsNullOrEmpty(cvm.phosphorous))
                        {
                            ModelState.AddModelError("phosphorous", "Required.");
                        }
                        else
                        {
                            if (!Decimal.TryParse(cvm.phosphorous, out userPhosphorous))
                            {
                                ModelState.AddModelError("phosphorous", "Numbers only.");
                            }
                            else
                            {
                                if (userPhosphorous < 0 || userPhosphorous > 100)
                                {
                                    ModelState.AddModelError("phosphorous", "Invalid %.");
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(cvm.potassium))
                        {
                            ModelState.AddModelError("potassium", "Required.");
                        }
                        else
                        {
                            if (!Decimal.TryParse(cvm.potassium, out userPotassium))
                            {
                                ModelState.AddModelError("potassium", "Numbers only.");
                            }
                            else
                            {
                                if (userPotassium < 0 || userPotassium > 100)
                                {
                                    ModelState.AddModelError("potassium", "Invalid %.");
                                }
                            }
                        }
                        if (cvm.compost)
                        {
                            if (string.IsNullOrEmpty(cvm.nitrate))
                            {
                                ModelState.AddModelError("nitrate", "Required.");
                            }
                            else
                            {
                                if (!Decimal.TryParse(cvm.nitrate, out userNitrate))
                                {
                                    ModelState.AddModelError("nitrate", "Numbers only.");
                                }
                            }
                        }
                        if (_sd.GetManureByName(cvm.manureName) != null)
                        {
                            ModelState.AddModelError("manureName", "Description cannot match predefined entries.");
                        }
                    }

                    List<FarmManure> manures = _ud.GetFarmManures();
                    foreach(var m in manures)
                    {
                        if(m.customized &&
                           m.name == cvm.manureName &&
                           m.id != cvm.id)
                        {
                            ModelState.AddModelError("manureName", "Descriptions must be unique.");
                            break;
                        }
                    }


                    if (!ModelState.IsValid)
                        return View(cvm);

                    if (cvm.id == null)
                    {
                        FarmManure fm = new FarmManure();
                        if (cvm.bookValue)
                        {
                            fm.manureId = cvm.selManOption;
                            fm.customized = false;
                        }
                        else
                        {
                            man = _sd.GetManure(cvm.selManOption.ToString());

                            fm.customized = true;
                            fm.manureId = cvm.selManOption;
                            fm.ammonia = Convert.ToInt32(cvm.ammonia);
                            fm.dmid = man.dmid;
                            fm.manure_class = man.manure_class;
                            fm.moisture = cvm.moisture;
                            fm.name = cvm.manureName;
                            fm.nitrogen = Convert.ToDecimal(cvm.nitrogen);
                            fm.nminerizationid = man.nminerizationid;
                            fm.phosphorous = Convert.ToDecimal(cvm.phosphorous);
                            fm.potassium = Convert.ToDecimal(cvm.potassium);
                            fm.nitrate = cvm.compost ? Convert.ToDecimal(cvm.nitrate) : (decimal?)null;
                            fm.solid_liquid = man.solid_liquid;
                        }


                        _ud.AddFarmManure(fm);
                    }
                    else
                    {
                        FarmManure fm = _ud.GetFarmManure(cvm.id.Value);
                        if (cvm.bookValue)
                        {
                            fm = new FarmManure();
                            fm.id = cvm.id.Value;
                            fm.manureId = cvm.selManOption;
                            fm.customized = false;
                        }
                        else
                        {
                            man = _sd.GetManure(cvm.selManOption.ToString());

                            fm.customized = true;
                            fm.manureId = cvm.selManOption;
                            fm.ammonia = Convert.ToInt32(cvm.ammonia);
                            fm.dmid = man.dmid;
                            fm.manure_class = man.manure_class;
                            fm.moisture = cvm.moisture;
                            fm.name = cvm.manureName;
                            fm.nitrogen = Convert.ToDecimal(cvm.nitrogen);
                            fm.nminerizationid = man.nminerizationid;
                            fm.phosphorous = Convert.ToDecimal(cvm.phosphorous);
                            fm.potassium = Convert.ToDecimal(cvm.potassium);
                            fm.solid_liquid = man.solid_liquid;
                            fm.nitrate = cvm.compost ? Convert.ToDecimal(cvm.nitrate) : (decimal?)null;
                        }

                        _ud.UpdateFarmManure(fm);

                        ReCalculateManure(fm.id);
                    }

                    string url = Url.Action("RefreshCompostList", "Manure");
                    return Json(new { success = true, url = url, target = cvm.target });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unexpected system error.");
            }

            return PartialView(cvm);
        }
        private void ReCalculateManure(int id)
        {
            Utility.CalculateNutrients calculateNutrients = new CalculateNutrients(_env, _ud, _sd);
            NOrganicMineralizations nOrganicMineralizations = new NOrganicMineralizations();

            List<Field> flds = _ud.GetFields();

            foreach (var fld in flds)
            {
                List<NutrientManure> mans = _ud.GetFieldNutrientsManures(fld.fieldName);

                foreach (var nm in mans)
                {
                    if (id.ToString() == nm.manureId)
                    {
                        int regionid = _ud.FarmDetails().farmRegion.Value;
                        Region region = _sd.GetRegion(regionid);
                        nOrganicMineralizations = calculateNutrients.GetNMineralization(Convert.ToInt16(nm.manureId), region.locationid);

                        string avail = (nOrganicMineralizations.OrganicN_FirstYear * 100).ToString("###");

                        string nh4 = (calculateNutrients.GetAmmoniaRetention(Convert.ToInt16(nm.manureId), Convert.ToInt16(nm.applicationId)) * 100).ToString("###");

                        NutrientInputs nutrientInputs = new NutrientInputs();

                        calculateNutrients.manure = nm.manureId;
                        calculateNutrients.applicationSeason = nm.applicationId;
                        calculateNutrients.applicationRate = Convert.ToDecimal(nm.rate);
                        calculateNutrients.applicationRateUnits = nm.unitId;
                        calculateNutrients.ammoniaNRetentionPct = Convert.ToDecimal(nh4);
                        calculateNutrients.firstYearOrganicNAvailablityPct = Convert.ToDecimal(avail);

                        calculateNutrients.GetNutrientInputs(nutrientInputs);

                        nm.yrN = nutrientInputs.N_FirstYear;
                        nm.yrP2o5 = nutrientInputs.P2O5_FirstYear;
                        nm.yrK2o = nutrientInputs.K2O_FirstYear;
                        nm.ltN = nutrientInputs.N_LongTerm;
                        nm.ltP2o5 = nutrientInputs.P2O5_LongTerm;
                        nm.ltK2o = nutrientInputs.K2O_LongTerm;

                        _ud.UpdateFieldNutrientsManure(fld.fieldName, nm);
                    }
                }
            }
        }
        public ActionResult CompostDelete(int id, string target)
        {
            CompostDeleteViewModel dvm = new CompostDeleteViewModel();
            bool manureUsed = false;

            dvm.id = id;
            dvm.target = target;

            FarmManure nm = _ud.GetFarmManure(id);

            dvm.manureName = nm.name;

            // determine if the selected manure is currently being used on any of the fields
            List<Field> flds = _ud.GetFields();

            foreach (var fld in flds)
            {
                List<NutrientManure> mans = _ud.GetFieldNutrientsManures(fld.fieldName);

                foreach (var man in mans)
                {
                    if (id.ToString() == man.manureId)
                    {
                        manureUsed = true;
                    }
                }
            }

            if (manureUsed)
            {
                dvm.warning = _sd.GetUserPrompt("manuredeletewarning");
            }

            dvm.act = "Delete";

            return PartialView("CompostDelete", dvm);
        }
        [HttpPost]
        public ActionResult CompostDelete(CompostDeleteViewModel dvm)
        {
            if (ModelState.IsValid)
            {
                // first remove manure from all fields that had it applied
                if(!string.IsNullOrEmpty(dvm.warning))
                {
                    List<Field> flds = _ud.GetFields();

                    foreach (var fld in flds)
                    {
                        List<NutrientManure> mans = _ud.GetFieldNutrientsManures(fld.fieldName);

                        foreach (var man in mans)
                        {
                            if (dvm.id.ToString() == man.manureId)
                            {
                                _ud.DeleteFieldNutrientsManure(fld.fieldName, man.id);
                            }
                        }
                    }
                }

                // delete the actual manure
                _ud.DeleteFarmManure(dvm.id);

                string url = Url.Action("RefreshCompostList", "Manure");
                return Json(new { success = true, url = url, target = dvm.target });
            }
            return PartialView("CompostDelete", dvm);
        }
        public IActionResult RefreshCompostList()
        {
            return ViewComponent("Compost");
        }
    }
}
