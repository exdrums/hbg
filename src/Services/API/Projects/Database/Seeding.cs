using System.Collections.Generic;
using System.Linq;
using API.Common;
using Projects.Models;

public static class ProjectsDbSetSeeding 
{
    public static void SeedProjectsDb(this ProjectsDbContext context, AppSettings settings) {
        if (settings.EnableSeeding != true) return;

        if(!context.Projects.Any()) 
        {
            var proj1 = new Project 
            { 
                Name = "Seeding project 1", 
                Articles = new List<Article>
                {
                    new Article{ Name = "First Article of Project 1" },
                    new Article{ Name = "Second Article of Project 1" },
                    new Article{ Name = "Third Article of Project 1" },
                },
                Plans = new List<Plan> 
                {
                    new Plan { Name = "First Plan of Project 1" },
                    new Plan { Name = "Second Plan of Project 1" },
                    new Plan { Name = "Third Plan of Project 1" },
                }
            };
            var proj2 = new Project 
            {
                Name = "Seeding project 2",
                Articles = new List<Article>
                {
                    new Article{ Name = "First Article of Project 2" },
                    new Article{ Name = "Second Article of Project 2" },
                    new Article{ Name = "Third Article of Project 2" },
                },
                Plans = new List<Plan> 
                {
                    new Plan { Name = "First Plan of Project 2" },
                    new Plan { Name = "Second Plan of Project 2" },
                    new Plan { Name = "Third Plan of Project 2" },
                }
            };
            var proj3 = new Project 
            {
                Name = "Seeding project 3",
                Articles = new List<Article>
                {
                    new Article{ Name = "First Article of Project 3" },
                    new Article{ Name = "Second Article of Project 3" },
                    new Article{ Name = "Third Article of Project 3" },
                },
                Plans = new List<Plan> 
                {
                    new Plan { Name = "First Plan of Project 3" },
                    new Plan { Name = "Second Plan of Project 3" },
                    new Plan { Name = "Third Plan of Project 3" },
                }
            };
            context.Projects.AddRange(proj1, proj2, proj3);
        }

        context.SaveChanges();
    }
}