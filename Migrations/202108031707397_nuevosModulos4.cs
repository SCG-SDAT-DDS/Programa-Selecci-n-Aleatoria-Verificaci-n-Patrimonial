namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nuevosModulos4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ImportacionMasivoes", "documentoError", c => c.String());
            AddColumn("dbo.ImportacionMasivoes", "status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportacionMasivoes", "status");
            DropColumn("dbo.ImportacionMasivoes", "documentoError");
        }
    }
}
