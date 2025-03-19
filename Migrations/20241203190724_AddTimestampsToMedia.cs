﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poplensMediaApi.Migrations {
    /// <inheritdoc />
    public partial class AddTimestampsToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Media",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Media",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "Media");
        }
    }
}
