using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemetryPlugin.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MetricName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CounterMetricEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    MetricId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStampStartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeStampEndUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterMetricEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterMetricEntries_Metrics_MetricId",
                        column: x => x.MetricId,
                        principalTable: "Metrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GaugeMetricEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    MetricId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStampStartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeStampEndUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GaugeMetricEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GaugeMetricEntries_Metrics_MetricId",
                        column: x => x.MetricId,
                        principalTable: "Metrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistogramBucketMetricEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Count = table.Column<long>(type: "INTEGER", nullable: false),
                    Bucket = table.Column<double>(type: "REAL", nullable: false),
                    MetricId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStampStartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeStampEndUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistogramBucketMetricEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistogramBucketMetricEntries_Metrics_MetricId",
                        column: x => x.MetricId,
                        principalTable: "Metrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistogramSumMetricEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Count = table.Column<long>(type: "INTEGER", nullable: false),
                    Sum = table.Column<double>(type: "REAL", nullable: false),
                    MetricId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStampStartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeStampEndUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistogramSumMetricEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistogramSumMetricEntries_Metrics_MetricId",
                        column: x => x.MetricId,
                        principalTable: "Metrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounterMetricEntries_MetricId",
                table: "CounterMetricEntries",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_GaugeMetricEntries_MetricId",
                table: "GaugeMetricEntries",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_HistogramBucketMetricEntries_MetricId",
                table: "HistogramBucketMetricEntries",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_HistogramSumMetricEntries_MetricId",
                table: "HistogramSumMetricEntries",
                column: "MetricId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounterMetricEntries");

            migrationBuilder.DropTable(
                name: "GaugeMetricEntries");

            migrationBuilder.DropTable(
                name: "HistogramBucketMetricEntries");

            migrationBuilder.DropTable(
                name: "HistogramSumMetricEntries");

            migrationBuilder.DropTable(
                name: "Metrics");
        }
    }
}
