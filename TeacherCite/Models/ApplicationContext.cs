using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public static async Task CreateTeacherAccount(UserManager<User> appManager,RoleManager<IdentityRole> appRoles, IConfiguration config)
        {
           
            string FirstName = config["Data:Admin:FirstName"];
            string LastName = config["Data:Admin:LastName"];
            string UserName = config["Data:Admin:Username"];
            string Email = config["Data:Admin:Email"];
            string Password = config["Data:Admin:Password"];
            string PhoneNumber = config["Data:Admin:PhoneNumber"];
            string Roles = config["Data:Admin:Roles"];

            var result = await appManager.FindByEmailAsync(Email);
            if (result==null)
            {
                if (await appRoles.FindByNameAsync("Student")==null)
                {
                    await appRoles.CreateAsync(new IdentityRole("Student"));
                }
                if (await appRoles.FindByNameAsync("Admin") == null)
                {
                    await appRoles.CreateAsync(new IdentityRole("Admin"));
                }
                if (await appRoles.FindByNameAsync("Teacher") == null)
                {
                    await appRoles.CreateAsync(new IdentityRole("Teacher"));
                }
                var teacher = new TeacherUser()
                {
                    Email=Email,
                    FirstName=FirstName,
                    LastName=LastName,
                    PhoneNumber=PhoneNumber,
                    UserName=UserName

                };
                
                await appManager.CreateAsync(teacher,Password);
                await appManager.AddToRoleAsync(teacher,Roles);
            }

        }
        public DbSet<TeacherUser> Teachers { get; set; }

       public DbSet<AppFile> Files { get; set; }

        public DbSet<DocFile> DocFiles { get; set; }

        public DbSet<Answer> Answers { get; set; }

        public DbSet<TaskFile> TaskFiles { get; set; }

        public DbSet<StTask> Tasks { get; set; }

        public DbSet<DocPage> Documentation { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Lecture> Lectures { get; set; }

        public DbSet<Visit> Visits { get; set; }

        public DbSet<News> News { get; set; }

        public DbSet<StudentUser> Students { get; set; }
    }
}
