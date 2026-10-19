using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookParadise.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "quan_ly_ban_sach");

            migrationBuilder.CreateTable(
                name: "binh_luan",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_binh_luan = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_sach = table.Column<int>(type: "integer", nullable: false),
                    noi_dung = table.Column<string>(type: "text", nullable: false),
                    ngay_binh_luan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    trang_thai_ai = table.Column<string>(type: "text", nullable: false),
                    hien_thi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_binh_luan", x => x.ma_binh_luan);
                });

            migrationBuilder.CreateTable(
                name: "chi_tiet_don_hang",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_chi_tiet = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_don_hang = table.Column<int>(type: "integer", nullable: false),
                    ma_sach = table.Column<int>(type: "integer", nullable: false),
                    so_luong = table.Column<int>(type: "integer", nullable: false),
                    don_gia = table.Column<decimal>(type: "numeric", nullable: false),
                    trang_thai_ai = table.Column<string>(type: "text", nullable: false),
                    hien_thi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_tiet_don_hang", x => x.ma_chi_tiet);
                });

            migrationBuilder.CreateTable(
                name: "don_hang",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_don_hang = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ngay_dat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tong_tien = table.Column<decimal>(type: "numeric", nullable: false),
                    trang_thai_ai = table.Column<string>(type: "text", nullable: false),
                    hien_thi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_don_hang", x => x.ma_don_hang);
                });

            migrationBuilder.CreateTable(
                name: "khuyen_mai",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_km = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ten_km = table.Column<string>(type: "text", nullable: false),
                    phan_tram_giam = table.Column<decimal>(type: "numeric", nullable: false),
                    ngay_bat_dau = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ngay_ket_thuc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khuyen_mai", x => x.ma_km);
                });

            migrationBuilder.CreateTable(
                name: "loai_sach",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_loai = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ten_loai = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loai_sach", x => x.ma_loai);
                });

            migrationBuilder.CreateTable(
                name: "nguoi_dung",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ho_ten = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    mat_khau = table.Column<string>(type: "text", nullable: false),
                    vai_tro = table.Column<string>(type: "text", nullable: false),
                    trang_thai_ai = table.Column<string>(type: "text", nullable: false),
                    hien_thi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nguoi_dung", x => x.ma_nguoi_dung);
                });

            migrationBuilder.CreateTable(
                name: "sach_yeu_thich",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_yeu_thich = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_sach = table.Column<int>(type: "integer", nullable: false),
                    ngay_them = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sach_yeu_thich", x => x.ma_yeu_thich);
                });

            migrationBuilder.CreateTable(
                name: "sach",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_sach = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ten_sach = table.Column<string>(type: "text", nullable: false),
                    mo_ta = table.Column<string>(type: "text", nullable: false),
                    gia = table.Column<decimal>(type: "numeric", nullable: false),
                    so_luong = table.Column<int>(type: "integer", nullable: false),
                    trang_thai_ai = table.Column<string>(type: "text", nullable: false),
                    hien_thi = table.Column<bool>(type: "boolean", nullable: false),
                    ma_loai = table.Column<int>(type: "integer", nullable: true),
                    ma_km = table.Column<int>(type: "integer", nullable: true),
                    hien_thi_trang_sale = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sach", x => x.ma_sach);
                    table.ForeignKey(
                        name: "FK_sach_khuyen_mai_ma_km",
                        column: x => x.ma_km,
                        principalSchema: "quan_ly_ban_sach",
                        principalTable: "khuyen_mai",
                        principalColumn: "ma_km");
                    table.ForeignKey(
                        name: "FK_sach_loai_sach_ma_loai",
                        column: x => x.ma_loai,
                        principalSchema: "quan_ly_ban_sach",
                        principalTable: "loai_sach",
                        principalColumn: "ma_loai");
                });

            migrationBuilder.CreateTable(
                name: "bien_the",
                schema: "quan_ly_ban_sach",
                columns: table => new
                {
                    ma_bien_the = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_sach = table.Column<int>(type: "integer", nullable: false),
                    ten_bien_the = table.Column<string>(type: "text", nullable: false),
                    gia_cong_them = table.Column<decimal>(type: "numeric", nullable: false),
                    so_luong = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bien_the", x => x.ma_bien_the);
                    table.ForeignKey(
                        name: "FK_bien_the_sach_ma_sach",
                        column: x => x.ma_sach,
                        principalSchema: "quan_ly_ban_sach",
                        principalTable: "sach",
                        principalColumn: "ma_sach",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bien_the_ma_sach",
                schema: "quan_ly_ban_sach",
                table: "bien_the",
                column: "ma_sach");

            migrationBuilder.CreateIndex(
                name: "IX_sach_ma_km",
                schema: "quan_ly_ban_sach",
                table: "sach",
                column: "ma_km");

            migrationBuilder.CreateIndex(
                name: "IX_sach_ma_loai",
                schema: "quan_ly_ban_sach",
                table: "sach",
                column: "ma_loai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bien_the",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "binh_luan",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "chi_tiet_don_hang",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "don_hang",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "nguoi_dung",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "sach_yeu_thich",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "sach",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "khuyen_mai",
                schema: "quan_ly_ban_sach");

            migrationBuilder.DropTable(
                name: "loai_sach",
                schema: "quan_ly_ban_sach");
        }
    }
}
