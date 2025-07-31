using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chats96.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomPinAndExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoomPin",
                table: "ChatRooms",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "ChatRooms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomTitle",
                table: "ChatRooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ChatRooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPersistent",
                table: "ChatRooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomPin",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "RoomTitle",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "IsPersistent",
                table: "ChatRooms");
        }
    }
}
