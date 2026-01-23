using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeGameSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameSessions_BaseGameSessionId",
                table: "Players");

            migrationBuilder.AlterColumn<Guid>(
                name: "BaseGameSessionId",
                table: "Players",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameSessions_BaseGameSessionId",
                table: "Players",
                column: "BaseGameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameSessions_BaseGameSessionId",
                table: "Players");

            migrationBuilder.AlterColumn<Guid>(
                name: "BaseGameSessionId",
                table: "Players",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameSessions_BaseGameSessionId",
                table: "Players",
                column: "BaseGameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id");
        }
    }
}
