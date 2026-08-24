using InventorySystem.Infrastructure.Security;

namespace InventorySystem.UnitTests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.True(_hasher.Verify(hash, "correct-horse-battery-staple"));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.False(_hasher.Verify(hash, "wrong-password"));
    }

    [Fact]
    public void Hash_CalledTwiceForSamePassword_ProducesDifferentHashes()
    {
        // Different random salts each time, even for the same input password.
        var hash1 = _hasher.Hash("same-password");
        var hash2 = _hasher.Hash("same-password");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.Verify(hash1, "same-password"));
        Assert.True(_hasher.Verify(hash2, "same-password"));
    }

    [Fact]
    public void Verify_WithMalformedHash_ReturnsFalseInsteadOfThrowing()
    {
        Assert.False(_hasher.Verify("not-a-real-hash", "anything"));
    }
}
