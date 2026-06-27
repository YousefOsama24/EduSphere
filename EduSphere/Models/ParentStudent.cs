namespace EduSphere.Models
{

   
        public class ParentStudent
        {
            public int ParentStudentId { get; set; }

            public int ParentId { get; set; }

            public Parent? Parent { get; set; }

            public int StudentId { get; set; }

            public Student? Student { get; set; }

            public string Relationship { get; set; } = string.Empty;
        }
    }
