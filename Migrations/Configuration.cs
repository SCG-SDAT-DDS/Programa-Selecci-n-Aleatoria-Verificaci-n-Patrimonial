namespace Transparencia.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Transparencia.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Transparencia.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Transparencia.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var roleStore = new RoleStore<ApplicationRole>(context);
            var roleManager = new RoleManager<ApplicationRole>(roleStore);

            roleManager.Create(new ApplicationRole() { Name = "Administrador", Descripcion = "Administrador del Sistema", Activo = true });
            // Initialize default user
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            ApplicationUser admin = new ApplicationUser();
            admin.Email = "jesus.reyes@sonora.gob.mx";
            admin.UserName = "Administrador";
            admin.Activo = true;

            userManager.Create(admin, "abcd2019-");
            userManager.AddToRole(admin.Id, "Administrador");

            base.Seed(context);
        }
    }
    public class MyDBInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);
        }
    }
}
