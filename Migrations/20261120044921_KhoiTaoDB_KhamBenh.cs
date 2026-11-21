using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediSlot.Migrations;

/// <inheritdoc />
public partial class _20261120044921_KhoiTaoDB_KhamBenh : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "HoSoBenhAns",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                LichHenId = table.Column<int>(type: "int", nullable: false),
                KetQuaLamSang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChanDoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LoiDan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NgayKham = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HoSoBenhAns", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DonThuocs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                HoSoBenhAnId = table.Column<int>(type: "int", nullable: false),
                TenThuoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LieuLuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SoLuong = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DonThuocs", x => x.Id);
                table.ForeignKey(
                    name: "FK_DonThuocs_HoSoBenhAns_HoSoBenhAnId",
                    column: x => x.HoSoBenhAnId,
                    principalTable: "HoSoBenhAns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DonThuocs_HoSoBenhAnId",
            table: "DonThuocs",
            column: "HoSoBenhAnId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DonThuocs");

        migrationBuilder.DropTable(
            name: "HoSoBenhAns");
    }
}
