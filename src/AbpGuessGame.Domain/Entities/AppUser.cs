using System;
using Volo.Abp.Identity;

public class AppUser : IdentityUser
{
    public int? BestGuessCount { get; set; }

    protected AppUser()
    {
    }

    public AppUser(Guid id, string userName, string email, Guid? tenantId = null) : base(id, userName, email, tenantId)
    {
    }
}