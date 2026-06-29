namespace EduSphere.Models
{
    public enum ParentRelationship
    {
        Father,
        Mother,
        Guardian
    }

    public class ParentStudent
        {
            public int ParentStudentId { get; set; }

            public int ParentId { get; set; }

            public Parent? Parent { get; set; }

            public int StudentId { get; set; }

            public Student? Student { get; set; }

            public ParentRelationship Relationship { get; set; } = ParentRelationship.Father;
        }
    }
