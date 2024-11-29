namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noce : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Plantillas", "FechaModificacion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Plantillas", "FechaModificacion", c => c.DateTime());
        }
    }
}
