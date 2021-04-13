﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smartstore.Core.Data.Migrations;

namespace Smartstore.Web.Infrastructure.Installation
{
    /// <summary>
    /// Responsible for installing the application
    /// </summary>
    public partial interface IInstallationService
    {
        Task<InstallationResult> Install(InstallationModel model);

        string GetResource(string resourceName);

        InstallationLanguage GetCurrentLanguage();

        void SaveCurrentLanguage(string languageCode);

        IList<InstallationLanguage> GetInstallationLanguages();

        IEnumerable<InstallationAppLanguageMetadata> GetAppLanguages();

        Lazy<InvariantSeedData, InstallationAppLanguageMetadata> GetAppLanguage(string culture);
    }
}