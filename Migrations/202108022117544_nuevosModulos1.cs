namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nuevosModulos1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ImportacionMasivoes", "documento", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportacionMasivoes", "documento");
        }
    }
}
