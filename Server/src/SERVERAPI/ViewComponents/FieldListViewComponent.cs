﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SERVERAPI.Models;
using SERVERAPI.Models.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SERVERAPI.ViewComponents
{
    public class FieldList : ViewComponent
    {
        private IHostingEnvironment _env;
        private UserData _ud;

        public FieldList(IHostingEnvironment env, UserData ud)
        {
            _env = env;
            _ud = ud;
        }

        public async Task<IViewComponentResult> InvokeAsync(string cntl, string actn)
        {
            var itemsTask = GetFieldsAsync(cntl, actn);
            var items = await itemsTask;
            return View(items);
        }

        private Task<FieldListViewModel> GetFieldsAsync(string cntl, string actn)
        {
            FieldListViewModel fvm = new FieldListViewModel();
            fvm.actn = actn;
            fvm.cntl = cntl;
            fvm.fields = new List<Field>();

            List<Field> fldList = _ud.GetFields();

            foreach (var f in fldList)
            {
                Field nf = new Field();
                nf.fieldName = f.fieldName;
                nf.area = f.area;
                nf.comment = f.comment;
                nf.crops = f.crops;

                fvm.fields.Add(nf);
            }

            return Task.FromResult(fvm);
        }
    }

    public class FieldListViewModel
    {
        public string cntl { get; set; }
        public string actn { get; set; }
        public List<Field> fields { get; set; }
    }
}
