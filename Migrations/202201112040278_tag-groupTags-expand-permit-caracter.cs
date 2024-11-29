namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class taggroupTagsexpandpermitcaracter : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GrupoTags", "Nombre", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Tags", "Nombre", c => c.String(nullable: false, maxLength: 200));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Tags", "Nombre", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.GrupoTags", "Nombre", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
