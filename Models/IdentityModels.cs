using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Transparencia.Models
{
    // Para agregar datos de perfil del usuario, agregue más propiedades a su clase ApplicationUser. Visite https://go.microsoft.com/fwlink/?LinkID=317594 para obtener más información.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tenga en cuenta que el valor de authenticationType debe coincidir con el definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Agregar aquí notificaciones personalizadas de usuario
            return userIdentity;
        }
        public string NombreCompleto { get; set; }
        public bool Activo { get; set; }

        [Display(Name = "recuperar contraseña clave")]
        public string claveRecuperarPassword { get; set; }

        [Display(Name = "Organimos")]
        public int? OrganismoID { get; set; }
        
        public virtual Organismo Organismo { get; set; }

        public int? UnidadAdministrativaId { get; set; } 

        public virtual UnidadAdministrativa UnidadAdministrativa { get; set; }

    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }
        public ApplicationRole(string name, string description, DateTime fechaDesde, DateTime fechaHasta, bool activo, string nombreOrganismo="") : base(name)
        {
            this.Descripcion = description;
            this.FechaDesde = fechaDesde;
            this.FechaHasta = fechaHasta;
            this.Activo = activo;
            this.NombreOrganismo = nombreOrganismo;
        }

        [Display(Name = "Descripción")]
        public virtual string Descripcion { get; set; }

        [Display(Name = "Desde")]
        public virtual DateTime? FechaDesde { get; set; }

        [Display(Name = "Hasta")]
        public virtual DateTime? FechaHasta { get; set; }

        public virtual bool Activo { get; set; }

        [NotMapped]
        public string NombreOrganismo { get; set; }

        [NotMapped]
        public string NombreUnidadAdministrativa { get; set; }


    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogoValor>().HasMany(m => m.RelatedCatalogoValor).WithMany(p => p.RelatedCatalogoValor2)
                .Map(w => w.ToTable("CatalagoValor_related").MapLeftKey("CatalogoValorId").MapRightKey("RelatedCatalogoValorId"));
            modelBuilder.Entity<Plantilla>()
                   .Property(x => x.Porcentage)
                   .HasPrecision(18, 3);
            base.OnModelCreating(modelBuilder);
        }
        public static ApplicationDbContext Create()=> new ApplicationDbContext();

        public DbSet<Tag> Tags { get; set; }

        public DbSet<GrupoTag> GrupoTags { get; set; }

        public DbSet<ListarGrupoTag> ListarGrupoTag { get; set; }

        public DbSet<Catalogo> Catalogoes { get; set; }

        public DbSet<CatalogoValor> CatalogoValores { get; set; }

        public DbSet<TipoBitacora> TipoBitacoras { get; set; }

        public DbSet<TipoOrganismo> TipoOrganismos { get; set; }

        public DbSet<Representante> Representantes { get; set; }

        public DbSet<Pais> Paises { get; set; }

        public DbSet<Estado> Estados { get; set; }

        public DbSet<Ciudad> Ciudades { get; set; }

        public DbSet<Organismo> Organismos { get; set; }

        public DbSet<UnidadAdministrativa> UnidadesAdministrativas { get; set; }

        public DbSet<Ley> Leyes { get; set; }

        public DbSet<Articulo> Articulos { get; set; }

        public DbSet<Fraccion> Fracciones { get; set; }

        public DbSet<TipoLey> TipoLeyes { get; set; }

        public DbSet<Extension> Extensiones { get; set; }

        public DbSet<GrupoExtension> GrupoExtesiones { get; set; }

        public DbSet<DataTypeM> DataTypeMs { get; set; }

        public DbSet<TipoInput> TipoInputs { get; set; }

        public DbSet<InputConfig> InputConfigs { get; set; }

        public DbSet<TipoEstatus> TipoEstatus { get; set; }

        public DbSet<Plantilla> Plantillas { get; set; }

        public DbSet<PlantillaFraccion> PlantillaFraccions { get; set; }

        public DbSet<Campo> Campos { get; set; }
        
        public DbSet<PlantillaOrganismos> PlantillaOrganismos { get; set; }

        public DbSet<OrganismosFraccion> OrganismosFraccion { get; set; }

        public DbSet<PlantillaUnidadAdministrativa> PlantillaUnidadAdministrativa { get; set; }

        public DbSet<Periodo> Periodos { get; set; }

        public DbSet<CampoCatalogo> CampoCatalogo { get; set; }

        public DbSet<BitIniSesion> BitIniSesion { get; set; }

        public DbSet<Bitacora> Bitacora { get; set; }
        public DbSet<OtraInformacion> OtraInformacion { get; set; }
        public DbSet<ImportacionMasivo> ImportacionMasivo { get; set; }
        public DbSet<TipoOtraInformacion> TipoOtraInformacion { get; set; }

        public DbSet<TipoFrecuencia> TipoFrecuencia { get; set; }

        public DbSet<Frecuencia> Frecuencia { get; set; }
        public DbSet<PlantillaHistory> PlantillaHistory { get; set; }

    }
}