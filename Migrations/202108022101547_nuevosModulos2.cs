namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nuevosModulos2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ImportacionMasivoes", "Fecha", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportacionMasivoes", "Fecha");
        }
    }
}
