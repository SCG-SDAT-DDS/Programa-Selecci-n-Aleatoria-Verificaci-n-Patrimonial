namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tipoFrecuenciayFrrecueciaClave : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Frecuencias", "Clave", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Frecuencias", "Clave");
        }
    }
}
