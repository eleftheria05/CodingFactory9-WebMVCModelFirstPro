using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data;

public class SchoolMvc9proContext : DbContext
{

    public SchoolMvc9proContext(DbContextOptions<SchoolMvc9proContext> options)
        : base(options)
    {
    }

    public DbSet<Capability> Capabilities { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Student> Students { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Capability>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasIndex(e => e.Name, "IX_Capabilities_Name");
            entity.HasIndex(e => e.Name, "UQ_Capabilities_Name").IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.HasOne(d => d.Teacher)
                .WithMany(p => p.Courses)
                .HasForeignKey(d => d.TeacherId) 
                .HasConstraintName("FK_Courses_Teachers");

            // Τα πεδία του Composite Key ονομάζονται by default StudentsId και CoursesId
            entity.HasMany(d => d.Students).WithMany(p => p.Courses)
                    .UsingEntity("StudentsCourses");

            // Αν θέλουμε explicit ονομασίες πεδίων:
            //entity.HasMany(d => d.Students).WithMany(p => p.Courses)
            //    .UsingEntity("StudentsCourses",
            //        l => l.HasOne(typeof(Student)).WithMany().HasForeignKey("StudentId"),
            //        r => r.HasOne(typeof(Course)).WithMany().HasForeignKey("CourseId"));

            entity.HasIndex(e => e.Description, "IX_Courses_Description");
            entity.HasIndex(e => e.TeacherId, "IX_Courses_TeacherId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasMany(d => d.Capabilities).WithMany(p => p.Roles)
                .UsingEntity("RolesCapabilities", j =>
                {
                    j.HasIndex("CapabilitiesId")
                    .HasDatabaseName("IX_RolesCapabilities_CapabilityId");
                });
            entity.HasIndex(e => e.Name, "IX_Roles_Name");
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Am)
                .HasMaxLength(10)
                .HasColumnName("AM");
            entity.Property(e => e.Department).HasMaxLength(50);
            entity.Property(e => e.Institution).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_UserId");

            entity.HasIndex(e => e.Am, "IX_Students_AM").IsUnique();
            entity.HasIndex(e => e.Institution, "IX_Students_Institution");
            entity.HasIndex(e => e.UserId, "IX_Students_UserId").IsUnique();
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(e => e.Institution).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.UserId)
                //  Ισχύει ούτως ή άλλως από το EF. Αλλά δεν πειράζει να είναι explicit για readability.
                .OnDelete(DeleteBehavior.Cascade)   
                .HasConstraintName("FK_Teachers_UserId");

            entity.HasIndex(e => e.Institution, "IX_Teachers_Institution");
            entity.HasIndex(e => e.UserId, "IX_Teachers_UserId").IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Firstname).HasMaxLength(50);
            entity.Property(e => e.Lastname).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(60);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Users_RoleId");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();
            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");
            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();
        });
    }
}
