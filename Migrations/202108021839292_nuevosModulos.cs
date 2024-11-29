namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nuevosModulos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImportacionMasivoes",
                c => new
                    {
                        ImportacionMasivoId = c.Int(nullable: false, identity: true),
                        PlanntillaId = c.Int(nullable: false),
                        OrganismoId = c.Int(nullable: false),
                        PeriodoId = c.Int(nullable: false),
                        sysFrecuencia = c.Int(nullable: false),
                        sysNumFrecuencia = c.Int(nullable: false),
                        UsuarioId = c.String(nullable: false),
                        Remplazar = c.Boolean(nullable: false),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ImportacionMasivoId);
            
            CreateTable(
                "dbo.OtraInformacions",
                c => new
                    {
                        OtraInformacionId = c.Int(nullable: false, identity: true),
                        Nombre = c.Int(nullable: false),
                        URL = c.String(nullable: false),
                        Notas = c.String(),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.OtraInformacionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OtraInformacions");
            DropTable("dbo.ImportacionMasivoes");
        }
    }
}
