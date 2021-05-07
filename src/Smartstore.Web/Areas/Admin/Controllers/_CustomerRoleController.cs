﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dasync.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Catalog.Brands;
using Smartstore.Core.Catalog.Categories;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Content.Topics;
using Smartstore.Core.Data;
using Smartstore.Core.Identity;
using Smartstore.Core.Localization;
using Smartstore.Data;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling;

namespace Smartstore.Admin.Controllers
{
    //[AdminAuthorize]
    public class CustomerRoleController : AdminControllerBase
    {
        private readonly SmartDbContext _db;
        
        public CustomerRoleController(
            SmartDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// TODO: (mh) (core) Add documentation.
        /// </summary>
        public async Task<IActionResult> AllCustomerRoles(string label, string selectedIds, bool? includeSystemRoles)
        {
            var rolesQuery = await _db.CustomerRoles
                .ApplyStandardFilter(true)
                .ToListAsync();
            
            if (!(includeSystemRoles ?? true))
            {
                rolesQuery = rolesQuery.Where(x => x.IsSystemRole).ToList();
            }

            var rolesPager = new FastPager<CustomerRole>(rolesQuery.AsQueryable(), 500);
            var customerRoles = new List<CustomerRole>();
            var ids = selectedIds.ToIntArray();

            while (rolesPager.ReadNextPage(out var roles))
            {
                customerRoles.AddRange(roles);
            }

            var list = customerRoles
                .OrderBy(x => x.Name)
                .Select(x => new ChoiceListItem
                {
                    Id = x.Id.ToString(),
                    Text = x.Name,
                    Selected = ids.Contains(x.Id)
                })
                .ToList();

            if (label.HasValue())
            {
                list.Insert(0, new ChoiceListItem
                {
                    Id = "0",
                    Text = label,
                    Selected = false
                });
            }

            return new JsonResult(list);
        }
    }
}