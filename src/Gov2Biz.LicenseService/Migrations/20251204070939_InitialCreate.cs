using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gov2Biz.LicenseService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicenseApplications_Tenants_TenantId",
                table: "LicenseApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Licenses_Tenants_TenantId",
                table: "Licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Agencies_AgencyId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AgencyId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Licenses_TenantId",
                table: "Licenses");

            migrationBuilder.DropIndex(
                name: "IX_LicenseApplications_TenantId",
                table: "LicenseApplications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "LicenseApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploaderId = table.Column<int>(type: "int", nullable: false),
                    LicenseApplicationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_LicenseApplications_LicenseApplicationId",
                        column: x => x.LicenseApplicationId,
                        principalTable: "LicenseApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    EntityReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    PayerId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_LicenseApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "LicenseApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_PayerId",
                        column: x => x.PayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Agencies",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsActive", "Name", "TenantId", "UpdatedAt" },
                values: new object[,]
                {
                    { "CONSTRUCTION", "CONSTRUCTION", new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5280), "Construction contractor licensing", true, "Construction Board", "tenant-001", null },
                    { "FINANCE", "FINANCE", new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5280), "Financial services licensing", true, "Financial Services Authority", "tenant-002", null },
                    { "HEALTH", "HEALTH", new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5280), "Healthcare professional licensing", true, "Department of Health", "tenant-001", null }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "ConnectionString", "CreatedAt", "Domain", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { "tenant-001", null, new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5210), "agency1.gov", true, "Government Agency 1", null },
                    { "tenant-002", null, new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5210), "agency2.gov", true, "Government Agency 2", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AgencyId", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Role", "TenantId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "HEALTH", new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5300), "admin", "Admin", true, "User", "plain:admin123", "Administrator", "tenant-001", null },
                    { 2, "HEALTH", new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5300), "staff", "Staff", true, "User", "plain:staff123", "AgencyStaff", "tenant-001", null },
                    { 3, null, new DateTime(2025, 12, 4, 7, 9, 39, 159, DateTimeKind.Utc).AddTicks(5300), "applicant", "Applicant", true, "User", "plain:applicant123", "Applicant", "tenant-002", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_LicenseApplicationId",
                table: "Documents",
                column: "LicenseApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploaderId",
                table: "Documents",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ApplicationId",
                table: "Payments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PayerId",
                table: "Payments",
                column: "PayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId_TenantId",
                table: "Payments",
                columns: new[] { "TransactionId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DeleteData(
                table: "Agencies",
                keyColumn: "Id",
                keyValue: "CONSTRUCTION");

            migrationBuilder.DeleteData(
                table: "Agencies",
                keyColumn: "Id",
                keyValue: "FINANCE");

            migrationBuilder.DeleteData(
                table: "Agencies",
                keyColumn: "Id",
                keyValue: "HEALTH");

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: "tenant-001");

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: "tenant-002");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "LicenseApplications",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AgencyId",
                table: "Users",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_TenantId",
                table: "Licenses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseApplications_TenantId",
                table: "LicenseApplications",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicenseApplications_Tenants_TenantId",
                table: "LicenseApplications",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Licenses_Tenants_TenantId",
                table: "Licenses",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Agencies_AgencyId",
                table: "Users",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
