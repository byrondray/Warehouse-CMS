using Warehouse_CMS.Models;
using Xunit;

namespace Warehouse_CMS.Tests
{
    public class RoleConstantsTests
    {
        [Theory]
        [InlineData("Admin")]
        [InlineData("admin")] // case-insensitive
        [InlineData("Manager")]
        [InlineData("MANAGER")]
        public void PrivilegedRoles_AreRejectedForSelfAssignment(string role)
        {
            Assert.True(RoleConstants.IsPrivileged(role));
        }

        [Theory]
        [InlineData("Sales Associate")]
        [InlineData("Warehouse Staff")]
        [InlineData("")]
        [InlineData(null)]
        public void NonPrivilegedRoles_AreAllowed(string? role)
        {
            Assert.False(RoleConstants.IsPrivileged(role));
        }
    }
}
