﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SERVERAPI.Models;
using SERVERAPI.Controllers;
using SERVERAPI.Models.Impl;
using Microsoft.AspNetCore.Hosting;

namespace SERVERAPI.ViewComponents
{
    public class Fields : ViewComponent
    {
        private IHostingEnvironment _env;
        private UserData _ud;

        public Fields(IHostingEnvironment env, UserData ud)
        {
            _env = env;
            _ud = ud;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await GetFieldsAsync());
        }

        private Task<FieldsViewModel> GetFieldsAsync()
        {
            FieldsViewModel fvm = new FieldsViewModel();
            fvm.fields = new List<Field>();

            List<Field> fldList = _ud.GetFields();

            foreach (var f in fldList)
            {
                Field nf = new Field();
                nf.fieldName = f.fieldName;
                nf.area = f.area;
                nf.comment = f.comment;
                fvm.fields.Add(nf);
            }

            return Task.FromResult(fvm);
        }
    }

    public class FieldsViewModel
    {
        public List<Field> fields { get; set; }
    }
}
