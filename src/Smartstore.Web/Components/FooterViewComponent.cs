﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smartstore.Core.Checkout.Tax;
using Smartstore.Core.Common.Settings;
using Smartstore.Core.Content.Menus;
using Smartstore.Core.Identity;
using Smartstore.Core.Theming;
using Smartstore.Utilities;
using Smartstore.Web.Filters;
using Smartstore.Web.Models.Common;
using Smartstore.Web.Theming;

namespace Smartstore.Web.Components
{
    public class FooterViewComponent : SmartViewComponent
    {
        private readonly static string[] _hints = new string[] { "Shopsystem", "Onlineshop Software", "Shopsoftware", "E-Commerce Solution" };

        private readonly Lazy<IThemeRegistry> _themeRegistry;
        private readonly ThemeSettings _themeSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly TaxSettings _taxSettings;
        private readonly SocialSettings _socialSettings;

        public FooterViewComponent(
            Lazy<IThemeRegistry> themeRegistry, 
            ThemeSettings themeSettings,
            CustomerSettings customerSettings,
            TaxSettings taxSettings,
            SocialSettings socialSettings)
        {
            _themeRegistry = themeRegistry;
            _themeSettings = themeSettings;
            _customerSettings = customerSettings;
            _taxSettings = taxSettings;
            _socialSettings = socialSettings;
        }

        [GdprConsent]
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var store = Services.StoreContext.CurrentStore;
            var taxDisplayType = Services.WorkContext.GetTaxDisplayTypeFor(Services.WorkContext.CurrentCustomer, store.Id);
            var taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");
            var availableStoreThemes = !_themeSettings.AllowCustomerToSelectTheme
                ? new List<StoreThemeModel>()
                : _themeRegistry.Value.GetThemeManifests()
                    .Select(x =>
                    {
                        return new StoreThemeModel
                        {
                            Name = x.ThemeName,
                            Title = x.ThemeTitle
                        };
                    })
                    .ToList();

            var model = new FooterModel
            {
                StoreName = store.Name,
                ShowLegalInfo = _taxSettings.ShowLegalHintsInFooter,
                ShowThemeSelector = availableStoreThemes.Count > 1,
                HideNewsletterBlock = _customerSettings.HideNewsletterBlock
            };

            var shippingInfoUrl = await Url.TopicAsync("shippinginfo");
            if (shippingInfoUrl.HasValue())
            {
                model.LegalInfo = T("Tax.LegalInfoFooter", taxInfo, shippingInfoUrl);
            }
            else
            {
                model.LegalInfo = T("Tax.LegalInfoFooter2", taxInfo);
            }

            var hint = Services.Settings.GetSettingByKey("Rnd_SmCopyrightHint", string.Empty, store.Id);
            if (hint.IsEmpty())
            {
                hint = _hints[CommonHelper.GenerateRandomInteger(0, _hints.Length - 1)];

                await Services.Settings.ApplySettingAsync("Rnd_SmCopyrightHint", hint, store.Id);
                await Services.DbContext.SaveChangesAsync();
            }

            model.ShowSocialLinks = _socialSettings.ShowSocialLinksInFooter;
            model.FacebookLink = _socialSettings.FacebookLink;
            model.TwitterLink = _socialSettings.TwitterLink;
            model.PinterestLink = _socialSettings.PinterestLink;
            model.YoutubeLink = _socialSettings.YoutubeLink;
            model.InstagramLink = _socialSettings.InstagramLink;

            model.SmartStoreHint = $"<a href='https://www.smartstore.com/' class='sm-hint' target='_blank'><strong>{hint}</strong></a> by SmartStore AG &copy; {DateTime.Now.Year}";

            return View(model);
        }
    }
}