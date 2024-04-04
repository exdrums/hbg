using System.Linq;
using API.Common;

public static class ProjectsDbSetSeeding 
{
    public static void SeedProjectsDb(this ProjectsDbContext context, AppSettings settings) {
        if (settings.EnableSeeding != true) return;

        if(!context.Projects.Any()) 
        {
            var proj1 = new Project { Name = "Seeding project 1" };
            var proj2 = new Project { Name = "Seeding project 2" };
            var proj3 = new Project { Name = "Seeding project 3" };
            context.Projects.AddRange(proj1, proj2, proj3);
        }

        context.SaveChanges();
    }
}