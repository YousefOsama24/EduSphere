using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSphere.Migrations
{
    /// <inheritdoc />
    public partial class FixParentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ParentId to Students if it doesn't exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Students','ParentId') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD ParentId int NULL;
    -- initialize existing rows with 0 (or set to appropriate parent id after migration)
    UPDATE dbo.Students SET ParentId = 0 WHERE ParentId IS NULL;
END
");

            // Add CourseId to Enrollments as NULLABLE to avoid FK conflicts for existing data
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Enrollments','CourseId') IS NULL
BEGIN
    ALTER TABLE dbo.Enrollments ADD CourseId int NULL;
END
");

            // Populate CourseId for existing rows with a valid CourseId (if any exist)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM dbo.Courses)
BEGIN
    DECLARE @firstCourseId int = (SELECT TOP 1 CourseId FROM dbo.Courses ORDER BY CourseId);
    UPDATE dbo.Enrollments SET CourseId = @firstCourseId WHERE CourseId IS NULL OR CourseId = 0;
END
");

            // Create indexes if missing
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Students_ParentId' AND object_id = OBJECT_ID('dbo.Students'))
BEGIN
    CREATE INDEX IX_Students_ParentId ON dbo.Students (ParentId);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId' AND object_id = OBJECT_ID('dbo.Enrollments'))
BEGIN
    CREATE INDEX IX_Enrollments_CourseId ON dbo.Enrollments (CourseId);
END
");

            // Add foreign keys if they don't already exist
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Enrollments_Courses_CourseId')
BEGIN
    ALTER TABLE dbo.Enrollments ADD CONSTRAINT FK_Enrollments_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES dbo.Courses (CourseId) ON DELETE CASCADE;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Students_Parents_ParentId')
BEGIN
    ALTER TABLE dbo.Students ADD CONSTRAINT FK_Students_Parents_ParentId FOREIGN KEY (ParentId) REFERENCES dbo.Parents (ParentId);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys and indexes if they exist, then drop columns if present
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Enrollments_Courses_CourseId')
BEGIN
    ALTER TABLE dbo.Enrollments DROP CONSTRAINT FK_Enrollments_Courses_CourseId;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Students_Parents_ParentId')
BEGIN
    ALTER TABLE dbo.Students DROP CONSTRAINT FK_Students_Parents_ParentId;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId' AND object_id = OBJECT_ID('dbo.Enrollments'))
BEGIN
    DROP INDEX IX_Enrollments_CourseId ON dbo.Enrollments;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Students_ParentId' AND object_id = OBJECT_ID('dbo.Students'))
BEGIN
    DROP INDEX IX_Students_ParentId ON dbo.Students;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Students','ParentId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Students DROP COLUMN ParentId;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Enrollments','CourseId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Enrollments DROP COLUMN CourseId;
END
");
        }
    }
}
