using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web;
using UserManagement.Web.Dtos;

namespace UserManagement.Data.Tests
{
    public class UserAuditsControllerTests
    {
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly UserAuditsController _controller;

        public UserAuditsControllerTests()
        {
            _mockAuditService = new Mock<IAuditService>();
            _controller = new UserAuditsController(_mockAuditService.Object);
        }

        [Fact]
        public async Task GetAllAudits_WhenAuditsExist_ReturnsOkWithPagedResult()
        {
            var audits = new List<UserAudit>
            {
                new UserAudit { Id = 1, UserId = 1, Action = AuditAction.Created }
            };

            _mockAuditService.Setup(s => s.GetAllUserAudits(1, 10))
                .ReturnsAsync((audits, audits.Count));

            var result = await _controller.GetAllAudits();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var pagedResult = ok.Value as PagedResult<UserAuditDto>;
            pagedResult.Should().NotBeNull();
            pagedResult!.Items.Should().HaveCount(1);
            pagedResult.Items.First().Action.Should().Be("Created");

            _mockAuditService.Verify(s => s.GetAllUserAudits(1, 10), Times.Once);
        }

        [Fact]
        public async Task GetAllAudits_WhenNoAuditsExist_ReturnsOK()
        {
            _mockAuditService.Setup(s => s.GetAllUserAudits(1, 10))
                .ReturnsAsync((new List<UserAudit>(), 0));

            var result = await _controller.GetAllAudits();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserAudits_WhenUserIdInvalid_ReturnsBadRequest()
        {
            var result = await _controller.GetUserAudits(0);

            result.Should().BeOfType<BadRequestObjectResult>();
            _mockAuditService.Verify(s => s.GetAllUserAuditsById(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUserAudits_WhenAuditsExist_ReturnsOkWithPagedResult()
        {
            var audits = new List<UserAudit>
            {
                new UserAudit { Id = 1, UserId = 1, Action = AuditAction.Updated }
            };

            _mockAuditService.Setup(s => s.GetAllUserAuditsById(1, 1, 10))
                .ReturnsAsync((audits, audits.Count));

            var result = await _controller.GetUserAudits(1);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var pagedResult = ok.Value as PagedResult<UserAuditDto>;
            pagedResult.Should().NotBeNull();
            pagedResult!.Items.Should().HaveCount(1);
            pagedResult.Items.First().Action.Should().Be("Updated");

            _mockAuditService.Verify(s => s.GetAllUserAuditsById(1, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetUserAuditsById_WhenNoAuditsExist_ReturnsOk()
        {
            _mockAuditService.Setup(s => s.GetAllUserAuditsById(1, 1, 10))
                .ReturnsAsync((new List<UserAudit>(), 0));

            var result = await _controller.GetUserAudits(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
