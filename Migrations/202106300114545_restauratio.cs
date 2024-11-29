namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class restauratio : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plantillas", "FechaModificacion", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plantillas", "FechaModificacion");
        }
    }
}
