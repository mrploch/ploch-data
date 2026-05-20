using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Moq;
using Ploch.Common.AppServices.Security;
using Ploch.Data.GenericRepository.EFCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.Tests;

public class AuditEntityHandlerTests
{
    private readonly IOptions<RepositoriesConfiguration> _disabledConfig = Options.Create(new RepositoriesConfiguration { EnableAuditing = false });
    private readonly IOptions<RepositoriesConfiguration> _enabledConfig = Options.Create(new RepositoriesConfiguration { EnableAuditing = true });
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly Mock<IUserInfoProvider> _userInfoProviderMock = new();

    [Fact]
    public void HandleCreation_should_set_created_time_when_auditing_enabled()
    {
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);
        var entity = new AuditableEntity();
        var before = DateTimeOffset.UtcNow;

        handler.HandleCreation(entity);

        entity.CreatedTime.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void HandleCreation_should_set_created_by_when_auditing_enabled()
    {
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new(ClaimTypes.Name, "testuser"));
        var principal = new ClaimsPrincipal(identity);
        _userInfoProviderMock.Setup(p => p.GetCurrentUserInfo()).Returns(principal);

        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);
        var entity = new AuditableEntity();

        handler.HandleCreation(entity);

        entity.CreatedBy.Should().Be("testuser");
    }

    [Fact]
    public void HandleCreation_should_not_set_times_when_auditing_disabled()
    {
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _disabledConfig);
        var entity = new AuditableEntity();

        handler.HandleCreation(entity);

        entity.CreatedTime.Should().BeNull();
    }

    [Fact]
    public void HandleCreation_should_handle_entity_without_audit_interfaces()
    {
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);
        var entity = new NonAuditableEntity();

        var act = () => handler.HandleCreation(entity);

        act.Should().NotThrow();
    }

    [Fact]
    public void HandleModification_should_set_modified_time_when_auditing_enabled()
    {
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);
        var entity = new AuditableEntity();
        var before = DateTimeOffset.UtcNow;

        handler.HandleModification(entity);

        entity.ModifiedTime.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void HandleModification_should_set_modified_by_when_auditing_enabled()
    {
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new(ClaimTypes.Name, "modifier"));
        var principal = new ClaimsPrincipal(identity);
        _userInfoProviderMock.Setup(p => p.GetCurrentUserInfo()).Returns(principal);

        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);
        var entity = new AuditableEntity();

        handler.HandleModification(entity);

        entity.LastModifiedBy.Should().Be("modifier");
    }

    [Fact]
    public void HandleModification_should_not_set_times_when_auditing_disabled()
    {
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _disabledConfig);
        var entity = new AuditableEntity();

        handler.HandleModification(entity);

        entity.ModifiedTime.Should().BeNull();
    }

    [Fact]
    public void HandleAccess_should_return_false()
    {
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);

        handler.HandleAccess(new()).Should().BeFalse();
    }

    [Fact]
    public void HandleCreation_should_handle_null_user_info_provider_gracefully()
    {
        _userInfoProviderMock.Setup(p => p.GetCurrentUserInfo()).Returns((ClaimsPrincipal?)null);
        var handler = new AuditEntityHandler(_userInfoProviderMock.Object, _timeProvider, _enabledConfig);
        var entity = new AuditableEntity();

        var act = () => handler.HandleCreation(entity);

        act.Should().NotThrow();
        entity.CreatedBy.Should().BeNull();
    }

    private sealed class AuditableEntity : IHasCreatedTime, IHasCreatedBy, IHasModifiedTime, IHasModifiedBy
    {
        public DateTimeOffset? CreatedTime { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset? ModifiedTime { get; set; }

        public string? LastModifiedBy { get; set; }
    }

    [SuppressMessage("Design", "S2094", Justification = "Intentionally empty - tests handler with non-auditable entities")]
    private sealed class NonAuditableEntity
    { }
}
