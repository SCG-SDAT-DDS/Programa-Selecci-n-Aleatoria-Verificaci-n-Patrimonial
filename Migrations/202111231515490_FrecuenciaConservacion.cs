namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FrecuenciaConservacion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plantillas", "FrecuenciaConservacion", c => c.Int(nullable: false));
            AddColumn("dbo.Plantillas", "DescripcionFrecuenciaConservacion", c => c.String());
            DropColumn("dbo.Plantillas", "FrecuencciaConservacion");
            DropColumn("dbo.Plantillas", "DescripcionFrecuencciaConservacion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Plantillas", "DescripcionFrecuencciaConservacion", c => c.String());
            AddColumn("dbo.Plantillas", "FrecuencciaConservacion", c => c.Int(nullable: false));
            DropColumn("dbo.Plantillas", "DescripcionFrecuenciaConservacion");
            DropColumn("dbo.Plantillas", "FrecuenciaConservacion");
        }
    }
}
