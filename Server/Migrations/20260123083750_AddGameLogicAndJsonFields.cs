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
    public partial class AddGameLogicAndJsonFields : Migration
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
                    PlayedSongIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    CurrentActiveSongId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    SessionPlaylist = table.Column<List<Song>>(type: "jsonb", nullable: true),
                    ExternalPlaylistId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Songs_CurrentActiveSongId",
                        column: x => x.CurrentActiveSongId,
                        principalTable: "Songs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Tokens = table.Column<int>(type: "integer", nullable: false),
                    Timeline = table.Column<List<TimelineCard>>(type: "jsonb", nullable: false),
                    BaseGameSessionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_CurrentActiveSongId",
                table: "GameSessions",
                column: "CurrentActiveSongId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_BaseGameSessionId",
                table: "Players",
                column: "BaseGameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "GameSessions");
        }
    }
}
