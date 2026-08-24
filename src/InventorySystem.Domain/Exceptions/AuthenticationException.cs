namespace InventorySystem.Domain.Exceptions;

// Deliberately a distinct subtype (not just DomainException) so the API's
// exception middleware can map bad credentials to 401 rather than 400.
public class AuthenticationException : DomainException
{
    public AuthenticationException(string message) : base(message)
    {
    }
}
