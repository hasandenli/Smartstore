﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Catalog.Categories;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Data;
using Smartstore.Core.Domain.Catalog;
using Smartstore.Core.Security;
using Smartstore.Core.Stores;
using Smartstore.Web.Controllers;
using Smartstore.Web.Models.Catalog;

namespace Smartstore.Web.Components
{
    public class HomeBestSellersViewComponent : SmartViewComponent
    {
        private readonly SmartDbContext _db;
        private readonly CatalogHelper _catalogHelper;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly CatalogSettings _catalogSettings;

        public HomeBestSellersViewComponent(
            SmartDbContext db, 
            CatalogHelper catalogHelper,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            CatalogSettings catalogSettings)
        {
            _db = db;
            _catalogHelper = catalogHelper;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize = null)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
            {
                return Empty();
            }

            // TODO: (mh) (core) This query is only for testing purposes. Use bestsellers from IOrderReportService once ready.

            // Load products
            var products = await _db.Products
                .AsNoTracking()
                .ApplyStandardFilter(false)
                .Where(x => !x.ShowOnHomePage && x.MainPictureId > 0)
                .OrderByDescending(x => x.Id)
                .Skip(100)
                .Take(_catalogSettings.NumberOfBestsellersOnHomepage)
                .ToListAsync();

            // ACL and store mapping
            products = await products
                .WhereAsync(async c => (await _aclService.AuthorizeAsync(c)) && (await _storeMappingService.AuthorizeAsync(c)))
                .AsyncToList();

            var viewMode = _catalogSettings.UseSmallProductBoxOnHomePage ? ProductSummaryViewMode.Mini : ProductSummaryViewMode.Grid;

            var settings = _catalogHelper.GetBestFitProductSummaryMappingSettings(viewMode, x =>
            {
                x.ThumbnailSize = productThumbPictureSize;
            });

            var model = await _catalogHelper.MapProductSummaryModelAsync(products, settings);
            model.GridColumnSpan = GridColumnSpan.Max6Cols;

            return View(model);
        }
    }
}