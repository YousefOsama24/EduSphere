using EduSphere.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduSphere.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Center> Centers { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Parent> Parents { get; set; }

        public DbSet<ParentStudent> ParentStudents { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Lecture> Lectures { get; set; }

        public DbSet<AttendanceSession> AttendanceSessions { get; set; }

        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        public DbSet<Exam> Exams { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Choice> Choices { get; set; }

        public DbSet<ExamAttempt> ExamAttempts { get; set; }

        public DbSet<StudentAnswer> StudentAnswers { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Teacher
            builder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Parent
            builder.Entity<Parent>()
                .HasOne(p => p.User)
                .WithOne(u => u.Parent)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ParentStudent
            builder.Entity<ParentStudent>()
                .HasOne(ps => ps.Parent)
                .WithMany(p => p.ParentStudents)
                .HasForeignKey(ps => ps.ParentId);

            builder.Entity<ParentStudent>()
                .HasOne(ps => ps.Student)
                .WithMany(s => s.ParentStudents)
                .HasForeignKey(ps => ps.StudentId);

            // Enrollment
            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Group)
                .WithMany(g => g.Enrollments)
                .HasForeignKey(e => e.GroupId);

            // StudentAnswer
            builder.Entity<StudentAnswer>()
                .HasOne(sa => sa.SelectedChoice)
                .WithMany()
                .HasForeignKey(sa => sa.SelectedChoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ConversationParticipant
            builder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.User)
                .WithMany(u => u.Conversations)
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Center -> Teacher
            builder.Entity<Teacher>()
                .HasOne(t => t.Center)
                .WithMany(c => c.Teachers)
                .HasForeignKey(t => t.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Center -> Student
            builder.Entity<Student>()
                .HasOne(s => s.Center)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Center -> Course
            builder.Entity<Course>()
                .HasOne(c => c.Center)
                .WithMany(cn => cn.Courses)
                .HasForeignKey(c => c.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Teacher -> Course
            builder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Teacher -> Group
            builder.Entity<Group>()
                .HasOne(g => g.Teacher)
                .WithMany(t => t.Groups)
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course -> Group
            builder.Entity<Group>()
                .HasOne(g => g.Course)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            builder.Entity<SubscriptionPlan>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<ExamAttempt>()
                .Property(e => e.Score)
                .HasPrecision(18, 2);

            builder.Entity<StudentAnswer>()
                .Property(a => a.MarksAwarded)
                .HasPrecision(18, 2);
            builder.Entity<StudentAnswer>()
    .HasOne(sa => sa.Question)
    .WithMany(q => q.StudentAnswers)
    .HasForeignKey(sa => sa.QuestionId)
    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}