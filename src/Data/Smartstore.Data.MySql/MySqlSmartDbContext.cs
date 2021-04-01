﻿using System;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Data;

namespace Smartstore.Data.SqlServer
{
    public class MySqlSmartDbContext : SmartDbContext
    {
        public MySqlSmartDbContext(DbContextOptions<MySqlSmartDbContext> options)
            : base(options)
        {
        }
    }
}