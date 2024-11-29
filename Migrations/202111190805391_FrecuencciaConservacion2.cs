namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FrecuencciaConservacion2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plantillas", "DescripcionFrecuencciaConservacion", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plantillas", "DescripcionFrecuencciaConservacion");
        }
    }
}
