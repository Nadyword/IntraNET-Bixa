using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bixa.Backend.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV23P : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoTramite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoTramite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ci = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUserRol = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByCi = table.Column<string>(type: "nvarchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("AK_Users_Ci", x => x.Ci);
                    table.ForeignKey(
                        name: "FK_Users_ModifiedUser",
                        column: x => x.ModifiedByCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_UserRols_IdUserRol",
                        column: x => x.IdUserRol,
                        principalTable: "UserRol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCi = table.Column<string>(type: "nvarchar(15)", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByCi = table.Column<string>(type: "nvarchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_ModifiedUser",
                        column: x => x.ModifiedByCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserCi",
                        column: x => x.UserCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoporteChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    RespondidoPorCi = table.Column<string>(type: "nvarchar(15)", nullable: true),
                    UsersId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByCi = table.Column<string>(type: "nvarchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoporteChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoporteChat_ModifiedUser",
                        column: x => x.ModifiedByCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoporteChats_Users_RespondidoPorCi",
                        column: x => x.RespondidoPorCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoporteChats_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tramites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoTramiteId = table.Column<int>(type: "int", nullable: false),
                    UserCi = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByCi = table.Column<string>(type: "nvarchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tramites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tramite_ModifiedUser",
                        column: x => x.ModifiedByCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tramites_TipoTramite_TipoTramiteId",
                        column: x => x.TipoTramiteId,
                        principalTable: "TipoTramite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tramites_Users_UserCi",
                        column: x => x.UserCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Aprobaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TramiteId = table.Column<int>(type: "int", nullable: false),
                    AprobadorCi = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByCi = table.Column<string>(type: "nvarchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aprobaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aprobacion_ModifiedUser",
                        column: x => x.ModifiedByCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aprobaciones_Tramites_TramiteId",
                        column: x => x.TramiteId,
                        principalTable: "Tramites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aprobaciones_Users_AprobadorCi",
                        column: x => x.AprobadorCi,
                        principalTable: "Users",
                        principalColumn: "Ci",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesVacaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TramiteId = table.Column<int>(type: "int", nullable: false),
                    Desde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hasta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiasTotales = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesVacaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesVacaciones_Tramites_TramiteId",
                        column: x => x.TramiteId,
                        principalTable: "Tramites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TipoTramite",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Anticipo de Utilidades" },
                    { 2, "Prestaciones Sociales" },
                    { 3, "Préstamo Prestaciones" },
                    { 4, "Vacaciones" },
                    { 5, "Dia Especial" }
                });

            migrationBuilder.InsertData(
                table: "UserRol",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Supervisor" },
                    { 3, "Empleado" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Ci", "CreatedAt", "FirstName", "IdUserRol", "IsActive", "LastLogin", "LastName", "ModifiedByCi", "PasswordHash", "RefreshToken", "RefreshTokenDate", "UpdatedAt" },
                values: new object[] { 74, "10.486.165", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MAGLENY", 1, true, null, "MATHEUS", null, "6n6SwrZf48UTEsrxT17QQ53bIe3xGLq+mqTYJiK9OpPiI3IlMy+B+n9ZVsFF9u15", null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Aprobaciones_AprobadorCi",
                table: "Aprobaciones",
                column: "AprobadorCi");

            migrationBuilder.CreateIndex(
                name: "IX_Aprobaciones_ModifiedByCi",
                table: "Aprobaciones",
                column: "ModifiedByCi");

            migrationBuilder.CreateIndex(
                name: "IX_Aprobaciones_TramiteId",
                table: "Aprobaciones",
                column: "TramiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ModifiedByCi",
                table: "Notifications",
                column: "ModifiedByCi");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserCi",
                table: "Notifications",
                column: "UserCi");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesVacaciones_TramiteId",
                table: "SolicitudesVacaciones",
                column: "TramiteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoporteChat_RespondidoPorCi",
                table: "SoporteChats",
                column: "RespondidoPorCi");

            migrationBuilder.CreateIndex(
                name: "IX_SoporteChat_UserCi",
                table: "SoporteChats",
                column: "UserCi");

            migrationBuilder.CreateIndex(
                name: "IX_SoporteChats_ModifiedByCi",
                table: "SoporteChats",
                column: "ModifiedByCi");

            migrationBuilder.CreateIndex(
                name: "IX_SoporteChats_UsersId",
                table: "SoporteChats",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_ModifiedByCi",
                table: "Tramites",
                column: "ModifiedByCi");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_TipoTramiteId",
                table: "Tramites",
                column: "TipoTramiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_UserCi",
                table: "Tramites",
                column: "UserCi");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Ci",
                table: "Users",
                column: "Ci",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdUserRol",
                table: "Users",
                column: "IdUserRol");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ModifiedByCi",
                table: "Users",
                column: "ModifiedByCi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aprobaciones");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SolicitudesVacaciones");

            migrationBuilder.DropTable(
                name: "SoporteChats");

            migrationBuilder.DropTable(
                name: "Tramites");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TipoTramite");

            migrationBuilder.DropTable(
                name: "UserRol");
        }
    }
}
