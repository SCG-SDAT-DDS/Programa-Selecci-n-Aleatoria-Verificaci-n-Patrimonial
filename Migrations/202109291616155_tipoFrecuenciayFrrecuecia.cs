namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tipoFrecuenciayFrrecuecia : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Frecuencias",
                c => new
                    {
                        FrecuenciaId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 200),
                        TipoFrecuenciaId = c.Int(nullable: false),
                        Orden = c.Int(nullable: false),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FrecuenciaId)
                .ForeignKey("dbo.TipoFrecuencias", t => t.TipoFrecuenciaId, cascadeDelete: true)
                .Index(t => t.TipoFrecuenciaId);
            
            CreateTable(
                "dbo.TipoFrecuencias",
                c => new
                    {
                        TipoFrecuenciaId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 200),
                        Orden = c.Int(nullable: false),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TipoFrecuenciaId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Frecuencias", "TipoFrecuenciaId", "dbo.TipoFrecuencias");
            DropIndex("dbo.Frecuencias", new[] { "TipoFrecuenciaId" });
            DropTable("dbo.TipoFrecuencias");
            DropTable("dbo.Frecuencias");
        }
    }
}
