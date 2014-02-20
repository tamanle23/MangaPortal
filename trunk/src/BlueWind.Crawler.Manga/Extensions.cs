using BlueWind.Common;
using BlueWind.Crawler.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Text;

namespace BlueWind.Crawler.Manga
{
    public static class Extensions
    {
        public static ObjectContext ObjContext(this DbContext context)
        {
            return ((IObjectContextAdapter)context).ObjectContext;
        }
        public static bool Save(this DbContext context)
        {
            try
            {
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.BeginWrite(ex);
                return false;
            }
            finally
            {
                context.ObjContext().AcceptAllChanges();
            }
        }
    }
}
