using System.ComponentModel.DataAnnotations;
using MimeKit;

namespace SmartLocate.Infrastructure.Contracts;

public record EmailAddress(string Name, [EmailAddress] string Address)
{
    public static implicit operator MailboxAddress(EmailAddress address) => new(address.Name, address.Address);
    
    public MailboxAddress ToMailboxAddress() => this;

    public override string ToString()
    {
        return $"{Name} <{Address}>";
    }
}