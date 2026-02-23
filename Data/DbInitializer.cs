using myapp.Models;
using System;
using System.Linq;

namespace myapp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any master data.
            if (context.MasterDataCombinations.Any())
            {
                return;   // DB has been seeded with master data
            }

            // --- Master Data --- //
            var masterData = new MasterDataCombination[]
            {
                new MasterDataCombination { DepartmentName = "HR", SectionName = "Recruitment", PlantName = "Main Office" },
                new MasterDataCombination { DepartmentName = "HR", SectionName = "Payroll", PlantName = "Main Office" },
                new MasterDataCombination { DepartmentName = "IT", SectionName = "Support", PlantName = "Main Office" },
                new MasterDataCombination { DepartmentName = "IT", SectionName = "Development", PlantName = "Main Office" },
                new MasterDataCombination { DepartmentName = "Production", SectionName = "Assembly Line 1", PlantName = "Factory A" },
                new MasterDataCombination { DepartmentName = "Production", SectionName = "Assembly Line 2", PlantName = "Factory B" },
            };
            context.MasterDataCombinations.AddRange(masterData);
            context.SaveChanges();

            // User seeding has been removed from here. 
            // It will be handled by a dedicated IdentityDataInitializer.
        }
    }
}
