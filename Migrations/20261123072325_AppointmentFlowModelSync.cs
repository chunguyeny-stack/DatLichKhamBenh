using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatLichKhamBenh.Migrations;

/// <inheritdoc />
public partial class _20261123072325_AppointmentFlowModelSync : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Doctors_AspNetUsers_UserId",
            table: "Doctors");

        migrationBuilder.DropForeignKey(
            name: "FK_Doctors_Specialties_SpecialtyId",
            table: "Doctors");

        migrationBuilder.DropForeignKey(
            name: "FK_Patients_AspNetUsers_UserId",
            table: "Patients");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Specialties",
            table: "Specialties");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Patients",
            table: "Patients");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Doctors",
            table: "Doctors");

        migrationBuilder.RenameTable(
            name: "Specialties",
            newName: "Specialty");

        migrationBuilder.RenameTable(
            name: "Patients",
            newName: "Patient");

        migrationBuilder.RenameTable(
            name: "Doctors",
            newName: "Doctor");

        migrationBuilder.RenameIndex(
            name: "IX_Patients_UserId",
            table: "Patient",
            newName: "IX_Patient_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_Doctors_UserId",
            table: "Doctor",
            newName: "IX_Doctor_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_Doctors_SpecialtyId",
            table: "Doctor",
            newName: "IX_Doctor_SpecialtyId");

        migrationBuilder.AddColumn<string>(
            name: "FullName",
            table: "AspNetUsers",
            type: "TEXT",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(
            "UPDATE AspNetUsers SET FullName = trim(COALESCE(MiddleName, '') || ' ' || COALESCE(FirstName, '')) WHERE FullName = '';");

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "AspNetUsers",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<string>(
            name: "CommonSymptoms",
            table: "Specialty",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "Patient",
            type: "TEXT",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AddColumn<DateTime>(
            name: "DateOfBirth",
            table: "Patient",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Patient",
            type: "TEXT",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FullName",
            table: "Patient",
            type: "TEXT",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PhoneNumber",
            table: "Patient",
            type: "TEXT",
            maxLength: 20,
            nullable: true);

        migrationBuilder.Sql(
            "UPDATE Patient SET FullName = COALESCE((SELECT FullName FROM AspNetUsers WHERE AspNetUsers.Id = Patient.UserId), FullName), PhoneNumber = COALESCE((SELECT PhoneNumber FROM AspNetUsers WHERE AspNetUsers.Id = Patient.UserId), PhoneNumber), Email = COALESCE((SELECT Email FROM AspNetUsers WHERE AspNetUsers.Id = Patient.UserId), Email), DateOfBirth = (SELECT DateOfBirth FROM AspNetUsers WHERE AspNetUsers.Id = Patient.UserId) WHERE UserId IS NOT NULL;");

        migrationBuilder.AlterColumn<int>(
            name: "YearsOfExperience",
            table: "Doctor",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "ConsultationFee",
            table: "Doctor",
            type: "decimal(18, 2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "FullName",
            table: "Doctor",
            type: "TEXT",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(
            "UPDATE Doctor SET FullName = COALESCE((SELECT FullName FROM AspNetUsers WHERE AspNetUsers.Id = Doctor.UserId), FullName);");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Specialty",
            table: "Specialty",
            column: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Patient",
            table: "Patient",
            column: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Doctor",
            table: "Doctor",
            column: "Id");

        migrationBuilder.CreateTable(
            name: "Appointment",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                DoctorId = table.Column<int>(type: "INTEGER", nullable: false),
                AppointmentDate = table.Column<DateTime>(type: "date", nullable: false),
                TimeSlot = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Symptoms = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Notes = table.Column<string>(type: "TEXT", nullable: true),
                ConsultationFee = table.Column<decimal>(type: "decimal(18, 2)", precision: 18, scale: 2, nullable: false),
                MedicineFee = table.Column<decimal>(type: "decimal(18, 2)", precision: 18, scale: 2, nullable: false),
                ServiceFee = table.Column<decimal>(type: "decimal(18, 2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Appointment", x => x.Id);
                table.ForeignKey(
                    name: "FK_Appointment_Doctor_DoctorId",
                    column: x => x.DoctorId,
                    principalTable: "Doctor",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Appointment_Patient_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patient",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DoctorSchedule",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                DoctorId = table.Column<int>(type: "INTEGER", nullable: false),
                WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                Shift = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                RoomName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TimeSlot = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                AbsenceReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DoctorSchedule", x => x.Id);
                table.ForeignKey(
                    name: "FK_DoctorSchedule_Doctor_DoctorId",
                    column: x => x.DoctorId,
                    principalTable: "Doctor",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Service",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                SpecialtyId = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18, 2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Service", x => x.Id);
                table.ForeignKey(
                    name: "FK_Service_Specialty_SpecialtyId",
                    column: x => x.SpecialtyId,
                    principalTable: "Specialty",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Appointment_DoctorId",
            table: "Appointment",
            column: "DoctorId");

        migrationBuilder.CreateIndex(
            name: "IX_Appointment_PatientId",
            table: "Appointment",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_DoctorSchedule_DoctorId_WorkDate_TimeSlot",
            table: "DoctorSchedule",
            columns: new[] { "DoctorId", "WorkDate", "TimeSlot" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Service_SpecialtyId",
            table: "Service",
            column: "SpecialtyId");

        migrationBuilder.AddForeignKey(
            name: "FK_Doctor_AspNetUsers_UserId",
            table: "Doctor",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Doctor_Specialty_SpecialtyId",
            table: "Doctor",
            column: "SpecialtyId",
            principalTable: "Specialty",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Patient_AspNetUsers_UserId",
            table: "Patient",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Doctor_AspNetUsers_UserId",
            table: "Doctor");

        migrationBuilder.DropForeignKey(
            name: "FK_Doctor_Specialty_SpecialtyId",
            table: "Doctor");

        migrationBuilder.DropForeignKey(
            name: "FK_Patient_AspNetUsers_UserId",
            table: "Patient");

        migrationBuilder.DropTable(
            name: "Appointment");

        migrationBuilder.DropTable(
            name: "DoctorSchedule");

        migrationBuilder.DropTable(
            name: "Service");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Specialty",
            table: "Specialty");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Patient",
            table: "Patient");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Doctor",
            table: "Doctor");

        migrationBuilder.DropColumn(
            name: "FullName",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "CommonSymptoms",
            table: "Specialty");

        migrationBuilder.DropColumn(
            name: "DateOfBirth",
            table: "Patient");

        migrationBuilder.DropColumn(
            name: "Email",
            table: "Patient");

        migrationBuilder.DropColumn(
            name: "FullName",
            table: "Patient");

        migrationBuilder.DropColumn(
            name: "PhoneNumber",
            table: "Patient");

        migrationBuilder.DropColumn(
            name: "ConsultationFee",
            table: "Doctor");

        migrationBuilder.DropColumn(
            name: "FullName",
            table: "Doctor");

        migrationBuilder.RenameTable(
            name: "Specialty",
            newName: "Specialties");

        migrationBuilder.RenameTable(
            name: "Patient",
            newName: "Patients");

        migrationBuilder.RenameTable(
            name: "Doctor",
            newName: "Doctors");

        migrationBuilder.RenameIndex(
            name: "IX_Patient_UserId",
            table: "Patients",
            newName: "IX_Patients_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_Doctor_UserId",
            table: "Doctors",
            newName: "IX_Doctors_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_Doctor_SpecialtyId",
            table: "Doctors",
            newName: "IX_Doctors_SpecialtyId");

        migrationBuilder.AddColumn<DateTime>(
            name: "DateOfBirth",
            table: "AspNetUsers",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "AspNetUsers",
            type: "TEXT",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LockoutReason",
            table: "AspNetUsers",
            type: "TEXT",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MiddleName",
            table: "AspNetUsers",
            type: "TEXT",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "Patients",
            type: "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "YearsOfExperience",
            table: "Doctors",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Specialties",
            table: "Specialties",
            column: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Patients",
            table: "Patients",
            column: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Doctors",
            table: "Doctors",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Doctors_AspNetUsers_UserId",
            table: "Doctors",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Doctors_Specialties_SpecialtyId",
            table: "Doctors",
            column: "SpecialtyId",
            principalTable: "Specialties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Patients_AspNetUsers_UserId",
            table: "Patients",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
