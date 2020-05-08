using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Umbrella.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    {
        public static ApplicationRole Create()
        {
            return new ApplicationRole();
        }

        //public string Description { get; set; }
    }

    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public static ApplicationUserRole Create()
        {
            return new ApplicationUserRole();
        }
    }

    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public static ApplicationUserLogin Create()
        {
            return new ApplicationUserLogin();
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("ITConnectionString", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Como nuestra base de datos ya existe, no necesitamos inicializar la base de datos para el contexto actual.
            Database.SetInitializer<ApplicationDbContext>(null);
            // Hacemos una llamada al mapeo base de la clase que genera las llaves apropiadas para las clases identity
            base.OnModelCreating(modelBuilder);
        }

    }
}