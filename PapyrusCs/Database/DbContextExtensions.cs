using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PapyrusCs.Database
{
    public static class DbContextExtensions
    {
        public static void MyBulkInsert<T>(this DbContext context, IEnumerable<T> entities)
        {
            var type = typeof(T);
            var entityType = context.Model.FindEntityType(type);
        }

        

    }
}
