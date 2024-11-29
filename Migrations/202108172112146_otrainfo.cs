namespace Transparencia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class otrainfo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TipoOtraInformacions",
                c => new
                    {
                        TipoOtraInformacionId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                        Descripcion = c.String(),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TipoOtraInformacionId);
            
            AddColumn("dbo.OtraInformacions", "OrganismoID", c => c.Int(nullable: false));
            AddColumn("dbo.OtraInformacions", "TipoOtraInformacionId", c => c.Int(nullable: false));
            AlterColumn("dbo.OtraInformacions", "Nombre", c => c.String(nullable: false));
            CreateIndex("dbo.OtraInformacions", "OrganismoID");
            CreateIndex("dbo.OtraInformacions", "TipoOtraInformacionId");
            AddForeignKey("dbo.OtraInformacions", "OrganismoID", "dbo.Organismoes", "OrganismoID", cascadeDelete: true);
            AddForeignKey("dbo.OtraInformacions", "TipoOtraInformacionId", "dbo.TipoOtraInformacions", "TipoOtraInformacionId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OtraInformacions", "TipoOtraInformacionId", "dbo.TipoOtraInformacions");
            DropForeignKey("dbo.OtraInformacions", "OrganismoID", "dbo.Organismoes");
            DropIndex("dbo.OtraInformacions", new[] { "TipoOtraInformacionId" });
            DropIndex("dbo.OtraInformacions", new[] { "OrganismoID" });
            AlterColumn("dbo.OtraInformacions", "Nombre", c => c.Int(nullable: false));
            DropColumn("dbo.OtraInformacions", "TipoOtraInformacionId");
            DropColumn("dbo.OtraInformacions", "OrganismoID");
            DropTable("dbo.TipoOtraInformacions");
        }
    }
}
