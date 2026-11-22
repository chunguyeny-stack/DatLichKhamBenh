using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatLichKhamBenh.Migrations;

/// <inheritdoc />
public partial class _20261122044358_GopCode_InitSqlite : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                MiddleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                LockoutReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ActorUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                TargetUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "chuyen_khoa",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ten_chuyen_khoa = table.Column<string>(type: "TEXT", nullable: false),
                mo_ta = table.Column<string>(type: "TEXT", nullable: true),
                trang_thai = table.Column<string>(type: "TEXT", nullable: false),
                ly_do_xoa = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_chuyen_khoa", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ho_so_benh_an",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                LichHenId = table.Column<int>(type: "INTEGER", nullable: false),
                KetQuaLamSang = table.Column<string>(type: "TEXT", nullable: true),
                ChanDoan = table.Column<string>(type: "TEXT", nullable: true),
                LoiDan = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ho_so_benh_an", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Specialties",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Specialties", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RoleId = table.Column<string>(type: "TEXT", nullable: false),
                ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                UserId = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                RoleId = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "benh_nhan",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_benh_nhan", x => x.id);
                table.ForeignKey(
                    name: "FK_benh_nhan_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmailVerificationTokens",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                Token = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                ExpiredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                IsUsed = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailVerificationTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmailVerificationTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Patients",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                HealthInsuranceNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                MedicalHistory = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                EmergencyContact = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Patients", x => x.Id);
                table.ForeignKey(
                    name: "FK_Patients_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "bac_si",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                nguoi_dung_id = table.Column<int>(type: "INTEGER", nullable: false),
                application_user_id = table.Column<string>(type: "TEXT", nullable: true),
                chuyen_khoa_id = table.Column<int>(type: "INTEGER", nullable: false),
                hoc_vi = table.Column<string>(type: "TEXT", nullable: true),
                kinh_nghiem = table.Column<string>(type: "TEXT", nullable: true),
                gia_kham = table.Column<decimal>(type: "TEXT", nullable: false),
                rating = table.Column<double>(type: "REAL", nullable: false),
                so_luong_danh_gia = table.Column<int>(type: "INTEGER", nullable: false),
                ho_ten = table.Column<string>(type: "TEXT", nullable: false),
                so_dien_thoai = table.Column<string>(type: "TEXT", nullable: true),
                email = table.Column<string>(type: "TEXT", nullable: true),
                hinh_anh = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_bac_si", x => x.id);
                table.ForeignKey(
                    name: "FK_bac_si_AspNetUsers_application_user_id",
                    column: x => x.application_user_id,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_bac_si_chuyen_khoa_chuyen_khoa_id",
                    column: x => x.chuyen_khoa_id,
                    principalTable: "chuyen_khoa",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "dich_vu",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                chuyen_khoa_id = table.Column<int>(type: "INTEGER", nullable: false),
                ten_dich_vu = table.Column<string>(type: "TEXT", nullable: false),
                gia_tien = table.Column<decimal>(type: "TEXT", nullable: false),
                mo_ta = table.Column<string>(type: "TEXT", nullable: false),
                dang_hoat_dong = table.Column<bool>(type: "INTEGER", nullable: false),
                trang_thai = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_dich_vu", x => x.id);
                table.ForeignKey(
                    name: "FK_dich_vu_chuyen_khoa_chuyen_khoa_id",
                    column: x => x.chuyen_khoa_id,
                    principalTable: "chuyen_khoa",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "don_thuoc",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                HoSoBenhAnId = table.Column<int>(type: "INTEGER", nullable: false),
                TenThuoc = table.Column<string>(type: "TEXT", nullable: true),
                LieuLuong = table.Column<string>(type: "TEXT", nullable: true),
                SoLuong = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_don_thuoc", x => x.Id);
                table.ForeignKey(
                    name: "FK_don_thuoc_ho_so_benh_an_HoSoBenhAnId",
                    column: x => x.HoSoBenhAnId,
                    principalTable: "ho_so_benh_an",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Doctors",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                SpecialtyId = table.Column<int>(type: "INTEGER", nullable: false),
                Degree = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                YearsOfExperience = table.Column<int>(type: "INTEGER", nullable: true),
                Workplace = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Doctors", x => x.Id);
                table.ForeignKey(
                    name: "FK_Doctors_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Doctors_Specialties_SpecialtyId",
                    column: x => x.SpecialtyId,
                    principalTable: "Specialties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "lich_hen",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                benh_nhan_id = table.Column<int>(type: "INTEGER", nullable: false),
                bac_si_id = table.Column<int>(type: "INTEGER", nullable: true),
                dich_vu_id = table.Column<int>(type: "INTEGER", nullable: false),
                ngay_kham = table.Column<DateTime>(type: "TEXT", nullable: false),
                khung_gio = table.Column<string>(type: "TEXT", nullable: false),
                trang_thai = table.Column<string>(type: "TEXT", nullable: false),
                ghi_chu = table.Column<string>(type: "TEXT", nullable: true),
                created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                trieu_chung_thuc_the = table.Column<string>(type: "TEXT", nullable: true),
                chan_doan = table.Column<string>(type: "TEXT", nullable: true),
                loi_dan = table.Column<string>(type: "TEXT", nullable: true),
                don_thuoc = table.Column<string>(type: "TEXT", nullable: true),
                thiet_bi_y_te = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_lich_hen", x => x.id);
                table.ForeignKey(
                    name: "FK_lich_hen_bac_si_bac_si_id",
                    column: x => x.bac_si_id,
                    principalTable: "bac_si",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "FK_lich_hen_benh_nhan_benh_nhan_id",
                    column: x => x.benh_nhan_id,
                    principalTable: "benh_nhan",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "lich_lam_viec",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                le_tan_ma_le_tan = table.Column<int>(type: "INTEGER", nullable: true),
                bac_si_id = table.Column<int>(type: "INTEGER", nullable: false),
                ngay_kham = table.Column<DateTime>(type: "TEXT", nullable: false),
                khung_gio = table.Column<string>(type: "TEXT", nullable: false),
                con_trong = table.Column<bool>(type: "INTEGER", nullable: false),
                ly_do_ban = table.Column<string>(type: "TEXT", nullable: false),
                trang_thai_chi_tiet = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_lich_lam_viec", x => x.id);
                table.ForeignKey(
                    name: "FK_lich_lam_viec_bac_si_bac_si_id",
                    column: x => x.bac_si_id,
                    principalTable: "bac_si",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_bac_si_application_user_id",
            table: "bac_si",
            column: "application_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_bac_si_chuyen_khoa_id",
            table: "bac_si",
            column: "chuyen_khoa_id");

        migrationBuilder.CreateIndex(
            name: "IX_benh_nhan_UserId",
            table: "benh_nhan",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_dich_vu_chuyen_khoa_id",
            table: "dich_vu",
            column: "chuyen_khoa_id");

        migrationBuilder.CreateIndex(
            name: "IX_Doctors_SpecialtyId",
            table: "Doctors",
            column: "SpecialtyId");

        migrationBuilder.CreateIndex(
            name: "IX_Doctors_UserId",
            table: "Doctors",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_don_thuoc_HoSoBenhAnId",
            table: "don_thuoc",
            column: "HoSoBenhAnId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailVerificationTokens_UserId",
            table: "EmailVerificationTokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_lich_hen_bac_si_id",
            table: "lich_hen",
            column: "bac_si_id");

        migrationBuilder.CreateIndex(
            name: "IX_lich_hen_benh_nhan_id",
            table: "lich_hen",
            column: "benh_nhan_id");

        migrationBuilder.CreateIndex(
            name: "IX_lich_lam_viec_bac_si_id",
            table: "lich_lam_viec",
            column: "bac_si_id");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_UserId",
            table: "Patients",
            column: "UserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "AuditLogs");

        migrationBuilder.DropTable(
            name: "dich_vu");

        migrationBuilder.DropTable(
            name: "Doctors");

        migrationBuilder.DropTable(
            name: "don_thuoc");

        migrationBuilder.DropTable(
            name: "EmailVerificationTokens");

        migrationBuilder.DropTable(
            name: "lich_hen");

        migrationBuilder.DropTable(
            name: "lich_lam_viec");

        migrationBuilder.DropTable(
            name: "Patients");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "Specialties");

        migrationBuilder.DropTable(
            name: "ho_so_benh_an");

        migrationBuilder.DropTable(
            name: "benh_nhan");

        migrationBuilder.DropTable(
            name: "bac_si");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "chuyen_khoa");
    }
}
