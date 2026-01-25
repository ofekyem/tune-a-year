using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Server.Models.Game;
using Server.Models.Game.Timeline;
using Server.Models.Music;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialUnifiedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<MatchConfiguration>(type: "jsonb", nullable: false),
                    CurrentPlayerIndex = table.Column<int>(type: "integer", nullable: false),
                    PlayedSongIds = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentActiveSong = table.Column<Song>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    ExternalPlaylistId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Artist = table.Column<string>(type: "text", nullable: false),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: false),
                    SpotifyId = table.Column<string>(type: "text", nullable: false),
                    PreviewUrl = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Tokens = table.Column<int>(type: "integer", nullable: false),
                    Timeline = table.Column<List<TimelineCard>>(type: "jsonb", nullable: false),
                    BaseGameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ConnectionId = table.Column<string>(type: "text", nullable: true),
                    IsHost = table.Column<bool>(type: "boolean", nullable: true),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_GameSessions_BaseGameSessionId",
                        column: x => x.BaseGameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_RoomCode",
                table: "GameSessions",
                column: "RoomCode");

            migrationBuilder.CreateIndex(
                name: "IX_Players_BaseGameSessionId",
                table: "Players",
                column: "BaseGameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Language",
                table: "Songs",
                column: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "GameSessions");
        }
    }
}
