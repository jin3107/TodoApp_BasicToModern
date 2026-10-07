using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todo.Models.Migrations
{
    /// <inheritdoc />
    public partial class BackfillTodoOwnershipToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE TodoItems ti
                INNER JOIN AspNetUsers u ON u.Email = ti.CreatedBy
                SET ti.CreatedBy = u.Id
                WHERE ti.CreatedBy IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE TodoLists tl
                INNER JOIN AspNetUsers u ON u.Email = tl.CreatedBy
                SET tl.CreatedBy = u.Id
                WHERE tl.CreatedBy IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE TodoItems ti
                INNER JOIN AspNetUsers u ON u.Id = ti.CreatedBy
                SET ti.CreatedBy = u.Email
                WHERE ti.CreatedBy IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE TodoLists tl
                INNER JOIN AspNetUsers u ON u.Id = tl.CreatedBy
                SET tl.CreatedBy = u.Email
                WHERE tl.CreatedBy IS NOT NULL;
            ");
        }
    }
}
