namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FrecuencciaConservacion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plantillas", "FrecuencciaConservacion", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plantillas", "FrecuencciaConservacion");
        }
    }
}
