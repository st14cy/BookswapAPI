using BookswapAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookswapAPI.Migrations
{
    /// <summary>
    /// Удаляет колонку Surname из таблицы Sellers (свойство убрано из модели Seller).
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260924120000_RemoveSellerSurname")]
    public partial class RemoveSellerSurname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Колонку могли уже удалить вручную, поэтому IF EXISTS
            migrationBuilder.Sql(
                "ALTER TABLE \"Sellers\" DROP COLUMN IF EXISTS \"Surname\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Sellers\" ADD COLUMN IF NOT EXISTS \"Surname\" character varying(100) NOT NULL DEFAULT '';");
        }
    }
}
