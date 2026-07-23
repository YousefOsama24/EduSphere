using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSphere.Migrations
{
    public partial class InsertPlaceholderCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Courses)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Centers) AND EXISTS (SELECT 1 FROM dbo.Teachers)
    BEGIN
        DECLARE @centerId int = (SELECT TOP 1 CenterId FROM dbo.Centers ORDER BY CenterId);
        DECLARE @teacherId int = (SELECT TOP 1 TeacherId FROM dbo.Teachers ORDER BY TeacherId);

        INSERT INTO dbo.Courses (CenterId, TeacherId, Title, Description, Price, ThumbnailUrl, IsPublished, CreatedAt, UpdatedAt)
        VALUES (@centerId, @teacherId, 'Placeholder Course', 'Automatically created placeholder to satisfy FK constraints.', 0.00, NULL, 0, GETDATE(), GETDATE());
    END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM dbo.Courses WHERE Title = 'Placeholder Course' AND Description LIKE 'Automatically created placeholder%';
");
        }
    }
}
