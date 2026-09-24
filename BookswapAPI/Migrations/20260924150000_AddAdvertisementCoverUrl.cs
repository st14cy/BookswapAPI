using BookswapAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookswapAPI.Migrations
{
    /// <summary>
    /// Добавляет колонку CoverUrl (ссылка на обложку книги) в таблицу Advertisements.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260924150000_AddAdvertisementCoverUrl")]
    public partial class AddAdvertisementCoverUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IF NOT EXISTS — на случай, если колонку уже добавили вручную
            migrationBuilder.Sql(
                "ALTER TABLE \"Advertisements\" ADD COLUMN IF NOT EXISTS \"CoverUrl\" character varying(500) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Advertisements\" DROP COLUMN IF EXISTS \"CoverUrl\";");
        }
    }
}
