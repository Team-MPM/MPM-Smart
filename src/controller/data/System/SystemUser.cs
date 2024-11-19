using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.System;

public class SystemUser : IdentityUser
{
    public int UserProfileId { get; set; }
    public UserProfileEntity? UserProfile { get; set; }

}



public class SystemUserEntityConfiguration : IEntityTypeConfiguration<SystemUser>
{
    public void Configure(EntityTypeBuilder<SystemUser> builder)
    {
        builder.HasOne(e => e.UserProfile).WithOne().HasForeignKey<SystemUser>(e => e.UserProfileId);
    }
}