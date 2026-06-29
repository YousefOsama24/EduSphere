using EduSphere.Models;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace EduSphere.Data;

public static class DbSeeder
{

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        if (context.Centers.Any())
            return;
        #region Roles

        string[] roles =
        {
    "SuperAdmin",
    "CenterManager",
    "Teacher",
    "Student",
    "Parent"
};

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        #endregion

        #region Users

        List<ApplicationUser> users = new();

        string[] names =
        {
    "Yousef Osama",
    "Ahmed Mohamed",
    "Omar Ali",
    "Mahmoud Hassan",
    "Sara Ahmed",
    "Mona Mostafa",
    "Ali Ibrahim",
    "Nour Mohamed",
    "Heba Adel",
    "Mostafa Gamal"
};

        string[] emails =
        {
    "YousefOsama24@gmail.com",
    "ahmed@edusphere.com",
    "omar@edusphere.com",
    "mahmoud@edusphere.com",
    "sara@edusphere.com",
    "mona@edusphere.com",
    "ali@edusphere.com",
    "nour@edusphere.com",
    "heba@edusphere.com",
    "mostafa@edusphere.com"
};

        for (int i = 0; i < 10; i++)
        {
            var user = new ApplicationUser
            {
                FullName = names[i],
                UserName = emails[i],
                Email = emails[i],
                PhoneNumber = $"010100000{i + 1:D2}",
                EmailConfirmed = true,
                IsActive = true,
                ProfileImage = null,
                CreatedAt = DateTime.Now
            };

            await userManager.CreateAsync(user, "Admin@123");

            users.Add(user);
        }

        #endregion

        #region Assign Roles

        // 3 Super Admins
        await userManager.AddToRoleAsync(users[0], "SuperAdmin");
        await userManager.AddToRoleAsync(users[1], "SuperAdmin");
        await userManager.AddToRoleAsync(users[2], "SuperAdmin");

        // Center Managers
        await userManager.AddToRoleAsync(users[3], "CenterManager");
        await userManager.AddToRoleAsync(users[4], "CenterManager");

        // Teachers
        await userManager.AddToRoleAsync(users[5], "Teacher");
        await userManager.AddToRoleAsync(users[6], "Teacher");

        // Students
        await userManager.AddToRoleAsync(users[7], "Student");
        await userManager.AddToRoleAsync(users[8], "Student");

        // Parent
        await userManager.AddToRoleAsync(users[9], "Parent");

        #endregion

        #region Centers

        List<Center> centers = new();

        for (int i = 1; i <= 2; i++)
        {
            centers.Add(new Center
            {
                Name = $"EduSphere Center {i}",
                Description = $"Educational Center Number {i}",
                Address = $"Cairo Branch {i}",
                Phone = $"01111111{i:D3}",
                Email = $"center{i}@edusphere.com",
                LogoUrl = "",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            });
        }

        context.Centers.AddRange(centers);

        await context.SaveChangesAsync();

        #endregion

        #region Subscription Plans

        List<SubscriptionPlan> plans =
        [
            new SubscriptionPlan
    {
        Name="Free",
        Tier=PlanTier.Free,
        Price=0,
        DurationInMonths=1,
        MaxTeachers=1,
        MaxStudents=20,
        Features="Basic Features"
    },

    new SubscriptionPlan
    {
        Name="Basic",
        Tier=PlanTier.Basic,
        Price=499,
        DurationInMonths=1,
        MaxTeachers=10,
        MaxStudents=300,
        Features="Standard Features"
    },

    new SubscriptionPlan
    {
        Name="Premium",
        Tier=PlanTier.Premium,
        Price=999,
        DurationInMonths=1,
        MaxTeachers=100,
        MaxStudents=5000,
        Features="All Features"
    }
        ];

        context.SubscriptionPlans.AddRange(plans);

        await context.SaveChangesAsync();

        #endregion

        #region Subscriptions

        List<Subscription> subscriptions = new();

        for (int i = 0; i < centers.Count; i++)
        {
            subscriptions.Add(new Subscription
            {
                CenterId = centers[i].CenterId,
                SubscriptionPlanId = plans[i % 3].SubscriptionPlanId,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(1),
                Status = SubscriptionStatus.Active
            });
        }

        context.Subscriptions.AddRange(subscriptions);

        await context.SaveChangesAsync();

        #endregion

        #region Payments

        List<Payment> payments = new();

        Random random = new();

        foreach (var subscription in subscriptions)
        {
            payments.Add(new Payment
            {
                SubscriptionId = subscription.SubscriptionId,
                Amount = random.Next(300, 1200),
                PaymentMethod = PaymentMethod.VodafoneCash,
                Status = PaymentStatus.Paid,
                TransactionReference = Guid.NewGuid().ToString(),
                PaymentDate = DateTime.Now.AddDays(-random.Next(1, 30))
            });
        }

        context.Payments.AddRange(payments);

        await context.SaveChangesAsync();

        #endregion
        #region Teachers

        List<Teacher> teachers = new()
{
    new Teacher
    {
        UserId = users[5].Id,
        CenterId = centers[0].CenterId,
        Specialization = "Mathematics",
        HireDate = DateTime.Now.AddYears(-3),
        IsActive = true
    },

    new Teacher
    {
        UserId = users[6].Id,
        CenterId = centers[1].CenterId,
        Specialization = "Physics",
        HireDate = DateTime.Now.AddYears(-2),
        IsActive = true
    }
};

        context.Teachers.AddRange(teachers);

        await context.SaveChangesAsync();

        #endregion

        #region Students

        List<Student> students = new()
{
    new Student
    {
        UserId = users[7].Id,
        CenterId = centers[0].CenterId,
        DateOfBirth = new DateTime(2008,5,10),
        AcademicLevel = "Grade 10",
        CreatedAt = DateTime.Now
    },

    new Student
    {
        UserId = users[8].Id,
        CenterId = centers[1].CenterId,
        DateOfBirth = new DateTime(2007,8,25),
        AcademicLevel = "Grade 11",
        CreatedAt = DateTime.Now
    }
};

        context.Students.AddRange(students);

        await context.SaveChangesAsync();

        #endregion

        #region Parents

        List<Parent> parents = new()
{
    new Parent
    {
        UserId = users[9].Id,
        Occupation = "Engineer"
    }
};

        context.Parents.AddRange(parents);

        await context.SaveChangesAsync();

        #endregion

        #region ParentStudent

        List<ParentStudent> parentStudents = new()
{
    new ParentStudent
    {
        ParentId = parents[0].ParentId,
        StudentId = students[0].StudentId,
        Relationship = ParentRelationship.Father
    },

    new ParentStudent
    {
        ParentId = parents[0].ParentId,
        StudentId = students[1].StudentId,
        Relationship = ParentRelationship.Father
    }
};

        context.ParentStudents.AddRange(parentStudents);

        await context.SaveChangesAsync();

        #endregion

        #region Courses

        List<Course> courses = new()
{
    new Course
    {
        CenterId = centers[0].CenterId,
        TeacherId = teachers[0].TeacherId,
        Title = "Mathematics Grade 10",
        Description = "Basic Mathematics",
        Price = 500,
        ThumbnailUrl = "",
        IsPublished = true,
        CreatedAt = DateTime.Now
    },

    new Course
    {
        CenterId = centers[1].CenterId,
        TeacherId = teachers[1].TeacherId,
        Title = "Physics Grade 11",
        Description = "Physics Fundamentals",
        Price = 650,
        ThumbnailUrl = "",
        IsPublished = true,
        CreatedAt = DateTime.Now
    }
};

        context.Courses.AddRange(courses);

        await context.SaveChangesAsync();

        #endregion

        #region Groups

        List<Group> groups = new()
{
    new Group
    {
        CourseId = courses[0].CourseId,
        TeacherId = teachers[0].TeacherId,
        Name = "Math Group A",
        Capacity = 30,
        StartDate = DateTime.Now,
        EndDate = DateTime.Now.AddMonths(4)
    },

    new Group
    {
        CourseId = courses[1].CourseId,
        TeacherId = teachers[1].TeacherId,
        Name = "Physics Group A",
        Capacity = 25,
        StartDate = DateTime.Now,
        EndDate = DateTime.Now.AddMonths(4)
    }
};

        context.Groups.AddRange(groups);

        await context.SaveChangesAsync();

        #endregion

        #region Schedules

        List<Schedule> schedules = new()
{
    new Schedule
    {
        GroupId = groups[0].GroupId,
        Day = DayOfWeek.Sunday,
        StartTime = new TimeSpan(10, 0, 0),
        EndTime = new TimeSpan(12, 0, 0),
        Room = "A101"
    },

    new Schedule
    {
        GroupId = groups[0].GroupId,
        Day = DayOfWeek.Wednesday,
        StartTime = new TimeSpan(10, 0, 0),
        EndTime = new TimeSpan(12, 0, 0),
        Room = "A101"
    },

    new Schedule
    {
        GroupId = groups[1].GroupId,
        Day = DayOfWeek.Monday,
        StartTime = new TimeSpan(2, 0, 0),
        EndTime = new TimeSpan(4, 0, 0),
        Room = "B203"
    },

    new Schedule
    {
        GroupId = groups[1].GroupId,
        Day = DayOfWeek.Thursday,
        StartTime = new TimeSpan(2, 0, 0),
        EndTime = new TimeSpan(4, 0, 0),
        Room = "B203"
    }
};

        context.Schedules.AddRange(schedules);

        await context.SaveChangesAsync();

        #endregion

        #region Enrollments

        List<Enrollment> enrollments = new()
{
    new Enrollment
    {
        StudentId = students[0].StudentId,
        GroupId = groups[0].GroupId,
        EnrollmentDate = DateTime.Now,
        Status = EnrollmentStatus.Active
    },

    new Enrollment
    {
        StudentId = students[1].StudentId,
        GroupId = groups[1].GroupId,
        EnrollmentDate = DateTime.Now,
        Status = EnrollmentStatus.Active
    }
};

        context.Enrollments.AddRange(enrollments);

        await context.SaveChangesAsync();

        #endregion

        #region Lectures

        List<Lecture> lectures = new();

        foreach (var course in courses)
        {
            for (int i = 1; i <= 5; i++)
            {
                lectures.Add(new Lecture
                {
                    CourseId = course.CourseId,
                    Title = $"Lecture {i}",
                    Description = $"Lecture {i} Description",
                    VideoUrl = $"https://youtube.com/video{i}",
                    PdfUrl = $"Lecture{i}.pdf",
                    IsPreview = i == 1,
                    CreatedAt = DateTime.Now
                });
            }
        }

        context.Lectures.AddRange(lectures);

        await context.SaveChangesAsync();

        #endregion

        #region Attendance Sessions

        List<AttendanceSession> sessions = new();

        foreach (var group in groups)
        {
            sessions.Add(new AttendanceSession
            {
                GroupId = group.GroupId,
                TeacherId = group.TeacherId,
                Title = "Week 1",
                SessionDate = DateTime.Now.AddDays(-7)
            });

            sessions.Add(new AttendanceSession
            {
                GroupId = group.GroupId,
                TeacherId = group.TeacherId,
                Title = "Week 2",
                SessionDate = DateTime.Now
            });
        }

        context.AttendanceSessions.AddRange(sessions);

        await context.SaveChangesAsync();

        #endregion

        #region Attendance Records


        string[] notes =
        {
    "Late due to traffic.",
    "Medical excuse.",
    "Family emergency.",
    "Arrived after class started.",
    null!
};

        List<AttendanceRecord> records = new();

        foreach (var session in sessions)
        {
            var groupStudents = enrollments
                .Where(e => e.GroupId == session.GroupId);

            foreach (var enrollment in groupStudents)
            {
                records.Add(new AttendanceRecord
                {
                    AttendanceSessionId = session.AttendanceSessionId,
                    StudentId = enrollment.StudentId,
                    Status = (AttendanceStatus)random.Next(0, 3),
                    Notes = notes[random.Next(notes.Length)]
                });
            }
        }

        if (!context.AttendanceRecords.Any())
        {
            context.AttendanceRecords.AddRange(records);
            await context.SaveChangesAsync();
        }
        #endregion

        #region Exams

        List<Exam> exams = new();

        foreach (var course in courses)
        {
            exams.Add(new Exam
            {
                CourseId = course.CourseId,
                Title = $"{course.Title} Midterm",
                Description = "Midterm Examination",
                TotalMarks = 50,
                DurationMinutes = 60,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(5).AddHours(1)
            });

            exams.Add(new Exam
            {
                CourseId = course.CourseId,
                Title = $"{course.Title} Final",
                Description = "Final Examination",
                TotalMarks = 100,
                DurationMinutes = 120,
                StartDate = DateTime.Now.AddDays(20),
                EndDate = DateTime.Now.AddDays(20).AddHours(2)
            });
        }

        context.Exams.AddRange(exams);

        await context.SaveChangesAsync();

        #endregion

        #region Questions


        List<Question> questions = new();

        foreach (var exam in exams)
        {
            for (int i = 1; i <= 10; i++)
            {
                questions.Add(new Question
                {
                    ExamId = exam.ExamId,
                    Content = $"Question {i} for {exam.Title}",
                    Type = i <= 8
                        ? QuestionType.MCQ
                        : QuestionType.Essay,
                    Marks = i <= 8 ? 5 : 10
                });
            }
        }

        context.Questions.AddRange(questions);

        await context.SaveChangesAsync();

        #endregion

        #region Choices

        List<Choice> choices = new();

        foreach (var question in questions.Where(q => q.Type == QuestionType.MCQ))
        {
            choices.Add(new Choice
            {
                QuestionId = question.QuestionId,
                Content = "Option A",
                IsCorrect = true
            });

            choices.Add(new Choice
            {
                QuestionId = question.QuestionId,
                Content = "Option B",
                IsCorrect = false
            });

            choices.Add(new Choice
            {
                QuestionId = question.QuestionId,
                Content = "Option C",
                IsCorrect = false
            });

            choices.Add(new Choice
            {
                QuestionId = question.QuestionId,
                Content = "Option D",
                IsCorrect = false
            });
        }

        context.Choices.AddRange(choices);

        await context.SaveChangesAsync();

        #endregion

        #region Exam Attempts

        List<ExamAttempt> attempts = new();

        foreach (var student in students)
        {
            foreach (var exam in exams)
            {
                attempts.Add(new ExamAttempt
                {
                    ExamId = exam.ExamId,
                    StudentId = student.StudentId,
                    StartTime = exam.StartDate,
                    SubmitTime = exam.StartDate.AddMinutes(exam.DurationMinutes),
                    Score = random.Next(20, exam.TotalMarks + 1),
                    Status = ExamAttemptStatus.Submitted
                });
            }
        }

        context.ExamAttempts.AddRange(attempts);

        await context.SaveChangesAsync();

        #endregion

        #region Student Answers

        List<StudentAnswer> answers = new();

        foreach (var attempt in attempts)
        {
            var examQuestions = questions
                .Where(q => q.ExamId == attempt.ExamId);

            foreach (var question in examQuestions)
            {
                if (question.Type == QuestionType.MCQ)
                {
                    var questionChoices = choices
                        .Where(c => c.QuestionId == question.QuestionId)
                        .ToList();

                    var selectedChoice =
                        questionChoices[random.Next(questionChoices.Count)];

                    answers.Add(new StudentAnswer
                    {
                        ExamAttemptId = attempt.ExamAttemptId,
                        QuestionId = question.QuestionId,
                        SelectedChoiceId = selectedChoice.ChoiceId,
                        EssayAnswer = null,
                        MarksAwarded = selectedChoice.IsCorrect
                            ? question.Marks
                            : 0
                    });
                }
                else
                {
                    answers.Add(new StudentAnswer
                    {
                        ExamAttemptId = attempt.ExamAttemptId,
                        QuestionId = question.QuestionId,
                        SelectedChoiceId = null,
                        EssayAnswer =
                            "This is a sample essay answer generated by the Seeder.",
                        MarksAwarded = random.Next(5, question.Marks + 1)
                    });
                }
            }
        }

        context.StudentAnswers.AddRange(answers);

        await context.SaveChangesAsync();

        #endregion

        #region Notifications

        List<Notification> notifications = new();

        foreach (var user in users)
        {
            notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = "Welcome to EduSphere",
                Message = "Your account has been successfully created.",
                Type = NotificationType.Announcement,
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = "New Course Available",
                Message = "A new course has been added to your center.",
                Type = NotificationType.Course,
                IsRead = random.Next(2) == 0,
                CreatedAt = DateTime.Now.AddMinutes(-20)
            });

            notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = "Upcoming Exam",
                Message = "Don't forget your exam tomorrow.",
                Type = NotificationType.Exam,
                IsRead = random.Next(2) == 0,
                CreatedAt = DateTime.Now.AddHours(-5)
            });
        }

        context.Notifications.AddRange(notifications);

        await context.SaveChangesAsync();

        #endregion

        #region Conversations

        List<Conversation> conversations = new();

        for (int i = 0; i < 5; i++)
        {
            conversations.Add(new Conversation
            {
                CreatedAt = DateTime.Now.AddDays(-i)
            });
        }

        context.Conversations.AddRange(conversations);

        await context.SaveChangesAsync();

        #endregion

        #region Conversation Participants

        List<ConversationParticipant> participants = new();

        participants.Add(new ConversationParticipant
        {
            ConversationId = conversations[0].ConversationId,
            UserId = users[5].Id
        });

        participants.Add(new ConversationParticipant
        {
            ConversationId = conversations[0].ConversationId,
            UserId = users[7].Id
        });

        participants.Add(new ConversationParticipant
        {
            ConversationId = conversations[1].ConversationId,
            UserId = users[6].Id
        });

        participants.Add(new ConversationParticipant
        {
            ConversationId = conversations[1].ConversationId,
            UserId = users[8].Id
        });

        participants.Add(new ConversationParticipant
        {
            ConversationId = conversations[2].ConversationId,
            UserId = users[3].Id
        });

        participants.Add(new ConversationParticipant
        {
            ConversationId = conversations[2].ConversationId,
            UserId = users[5].Id
        });

        context.ConversationParticipants.AddRange(participants);

        await context.SaveChangesAsync();

        #endregion

        #region Messages

        string[] sampleMessages =
        {
    "Hello Teacher.",
    "Good morning.",
    "Today's lecture was great.",
    "Can you explain question 5?",
    "Thank you.",
    "Please submit your assignment.",
    "See you tomorrow.",
    "Excellent work.",
    "Don't forget the exam.",
    "Keep studying."
};

        List<Message> messages = new();

        foreach (var conversation in conversations)
        {
            var conversationUsers = participants
                .Where(p => p.ConversationId == conversation.ConversationId)
                .Select(p => p.UserId)
                .ToList();

            if (conversationUsers.Count < 2)
                continue;

            for (int i = 0; i < 10; i++)
            {
                messages.Add(new Message
                {
                    ConversationId = conversation.ConversationId,
                    SenderUserId = conversationUsers[random.Next(conversationUsers.Count)],
                    Content = sampleMessages[random.Next(sampleMessages.Length)],
                    SentAt = DateTime.Now.AddMinutes(-random.Next(500)),
                    IsRead = random.Next(2) == 0
                });
            }
        }

        context.Messages.AddRange(messages);

        await context.SaveChangesAsync();

        #endregion





    }
}