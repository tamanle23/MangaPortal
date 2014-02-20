using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BlueWind.CloudApi.Domain
{
    public class BlueWindDataContextInitializer:IDatabaseInitializer<BlueWindDataContext>
    {
        public void InitializeDatabase(BlueWindDataContext context)
        {

        }
    }
}