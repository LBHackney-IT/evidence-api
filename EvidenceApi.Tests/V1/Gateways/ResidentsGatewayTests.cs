using System;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ResidentsGatewayTests : DatabaseTests
    {
        private readonly IFixture _fixture = new Fixture();
        private ResidentsGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ResidentsGateway(DatabaseContext);
        }

        [Test]
        public void FindOrCreateReturnsTheResidentWhenItAlreadyExists()
        {
            var request = _fixture.Create<Resident>();

            DatabaseContext.Residents.Add(request);
            DatabaseContext.SaveChanges();

            _classUnderTest.FindOrCreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
        }

        [Test]
        public void FindOrCreateCreatesAResidentWhenOneDoesNotExist()
        {
            var request = _fixture.Create<Resident>();

            _classUnderTest.FindOrCreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
            foundRecord.Name.Should().Be(request.Name);
        }

        [Test]
        public void FindReturnsAResidentWhenOneExists()
        {
            var entity = _fixture.Create<Resident>();
            DatabaseContext.Residents.Add(entity);
            DatabaseContext.SaveChanges();


            var foundRecord = _classUnderTest.FindResident(entity.Id);

            foundRecord.Id.Should().Be(entity.Id);
            foundRecord.Email.Should().Be(entity.Email);
            foundRecord.PhoneNumber.Should().Be(entity.PhoneNumber);
            foundRecord.Name.Should().Be(entity.Name);
        }

        [Test]
        public void FindReturnsAResidentWithoutPhoneNumber()
        {
            var entity = _fixture.Build<Resident>()
                .Without(x => x.PhoneNumber)
                .Create();
            DatabaseContext.Residents.Add(entity);
            DatabaseContext.SaveChanges();


            var foundRecord = _classUnderTest.FindResident(entity.Id);

            foundRecord.Id.Should().Be(entity.Id);
            foundRecord.Email.Should().Be(entity.Email);
            foundRecord.PhoneNumber.Should().BeNull();
            foundRecord.Name.Should().Be(entity.Name);
        }

        [Test]
        public void FindReturnsAResidentWithoutEmail()
        {
            var entity = _fixture.Build<Resident>()
                .Without(x => x.Email)
                .Create();
            DatabaseContext.Residents.Add(entity);
            DatabaseContext.SaveChanges();


            var foundRecord = _classUnderTest.FindResident(entity.Id);

            foundRecord.Id.Should().Be(entity.Id);
            foundRecord.Email.Should().BeNull();
            foundRecord.PhoneNumber.Should().Be(entity.PhoneNumber);
            foundRecord.Name.Should().Be(entity.Name);
        }

        [Test]
        public void FindResidentsByName()
        {
            // Arrange
            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Name, "Test Resident")
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Name, "Other Resident")
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Name, "Test Resident2")
                .Create();
            DatabaseContext.Residents.Add(resident1);
            DatabaseContext.Residents.Add(resident2);
            DatabaseContext.Residents.Add(resident3);
            DatabaseContext.SaveChanges();

            // Act
            var result = _classUnderTest.FindResidents("Test");

            // Assert
            result.Count.Should().Be(2);
            var resultResident1 = result.Find(r => r.Name == "Test Resident");
            resultResident1.Should().NotBeNull();
            var resultResident2 = result.Find(r => r.Name == "Test Resident2");
            resultResident2.Should().NotBeNull();
        }

        [Test]
        public void FindResidentsByNameIsCaseInsensitive()
        {
            // Arrange
            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Name, "test Resident")
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Name, "other Resident")
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Name, "test Resident2")
                .Create();
            DatabaseContext.Residents.Add(resident1);
            DatabaseContext.Residents.Add(resident2);
            DatabaseContext.Residents.Add(resident3);
            DatabaseContext.SaveChanges();

            // Act
            var result = _classUnderTest.FindResidents("TEST");

            // Assert
            result.Count.Should().Be(2);
            var resultResident1 = result.Find(r => r.Name == "test Resident");
            resultResident1.Should().NotBeNull();
            var resultResident2 = result.Find(r => r.Name == "test Resident2");
            resultResident2.Should().NotBeNull();
        }

        [Test]
        public void FindResidentsByEmail()
        {
            // Arrange
            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Email, "TestEmail@hackney.gov.uk")
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Email, "OtherEmail@hackney.gov.uk")
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Email, "TestEmail2@hackney.gov.uk")
                .Create();
            DatabaseContext.Residents.Add(resident1);
            DatabaseContext.Residents.Add(resident2);
            DatabaseContext.Residents.Add(resident3);
            DatabaseContext.SaveChanges();

            // Act
            var result = _classUnderTest.FindResidents("Test");

            // Assert
            result.Count.Should().Be(2);
            var resultResident1 = result.Find(r => r.Email == "TestEmail@hackney.gov.uk");
            resultResident1.Should().NotBeNull();
            var resultResident2 = result.Find(r => r.Email == "TestEmail@hackney.gov.uk");
            resultResident2.Should().NotBeNull();
        }

        [Test]
        public void FindResidentsByPhoneNumber()
        {
            // Arrange
            var resident1 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "123")
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "567")
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "1234")
                .Create();
            DatabaseContext.Residents.Add(resident1);
            DatabaseContext.Residents.Add(resident2);
            DatabaseContext.Residents.Add(resident3);
            DatabaseContext.SaveChanges();

            // Act
            var result = _classUnderTest.FindResidents("123");

            // Assert
            result.Count.Should().Be(2);
            var resultResident1 = result.Find(r => r.PhoneNumber == "123");
            resultResident1.Should().NotBeNull();
            var resultResident2 = result.Find(r => r.PhoneNumber == "1234");
            resultResident2.Should().NotBeNull();
        }

        [Test]
        public void FindResidentsByBothNameAndEmail()
        {
            // Arrange
            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Name, "Test Resident")
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "567")
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Email, "TestEmail@hackney.gov.uk")
                .Create();
            DatabaseContext.Residents.Add(resident1);
            DatabaseContext.Residents.Add(resident2);
            DatabaseContext.Residents.Add(resident3);
            DatabaseContext.SaveChanges();

            // Act
            var result = _classUnderTest.FindResidents("Test");

            // Assert
            result.Count.Should().Be(2);
            var resultResident1 = result.Find(r => r.Name == "Test Resident");
            resultResident1.Should().NotBeNull();
            var resultResident2 = result.Find(r => r.Email == "TestEmail@hackney.gov.uk");
            resultResident2.Should().NotBeNull();
        }

        [Test]
        public void CreateResidentCreatesAResident()
        {
            var request = _fixture.Create<Resident>();

            _classUnderTest.CreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
            foundRecord.Name.Should().Be(request.Name);
        }

        [Test]
        public void AddResidentGroupIdAddsNewEntry()
        {
            var residentId = Guid.NewGuid();
            var team = "some team";
            // Add resident for FK constraint
            var resident = _fixture.Create<Resident>();
            resident.Id = residentId;
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            _classUnderTest.AddResidentGroupId(residentId, team);

            var query = DatabaseContext.ResidentsTeamGroupId.Where(x => x.ResidentId == residentId && x.Team == team);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Resident.Id.Should().Be(residentId);
            foundRecord.Team.Should().Be(team);
        }

        [Test]
        public void FindsGroupIdByResidentIdAndTeamWhenGroupIdExists()
        {
            var residentId = Guid.NewGuid();
            var resident = _fixture.Create<Resident>();
            resident.Id = residentId;
            var team = "some team";
            var groupId = Guid.NewGuid();
            DatabaseContext.Residents.Add(resident);

            var residentTeamGroupId = _fixture.Create<ResidentsTeamGroupId>();
            residentTeamGroupId.ResidentId = residentId;
            residentTeamGroupId.Team = team;
            residentTeamGroupId.GroupId = groupId;
            residentTeamGroupId.Resident = resident;

            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);
            DatabaseContext.SaveChanges();

            var result = _classUnderTest.FindGroupIdByResidentIdAndTeam(residentId, team);
            result.Should().Be(groupId);
        }

        [Test]
        public void FindGroupIdByResidentIdAndTeamReturnsNullWhenNoRecordFound()
        {
            var residentId = Guid.NewGuid();
            var resident = _fixture.Create<Resident>();
            resident.Id = residentId;
            var team = "some team";
            DatabaseContext.Residents.Add(resident);

            var residentTeamGroupId = _fixture.Build<ResidentsTeamGroupId>()
                .Without(x => x.GroupId)
                .Create();
            residentTeamGroupId.ResidentId = residentId;
            residentTeamGroupId.Team = team;
            residentTeamGroupId.Resident = resident;

            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);
            DatabaseContext.SaveChanges();

            var result = _classUnderTest.FindGroupIdByResidentIdAndTeam(residentId, team);
            result.Should().BeNull();
        }

        [Test]
        public void GetAllResidentIdsAndGroupIdsByFirstCharacter()
        {
            var currentDate = new DateTime();
            var residentOne = _fixture.Create<Resident>();
            var groupIdOne = Guid.NewGuid();
            var residentTwo = _fixture.Create<Resident>();
            var groupIdTwo = Guid.NewGuid();
            var residentThree = _fixture.Create<Resident>();
            var groupIdThree = Guid.NewGuid();

            var guidCharacter = groupIdOne.ToString().First();

            var entryOne = new ResidentsTeamGroupId() { Resident = residentOne, GroupId = groupIdOne, CreatedAt = currentDate.AddHours(1) };
            var entryTwo = new ResidentsTeamGroupId() { Resident = residentTwo, GroupId = groupIdTwo, CreatedAt = currentDate.AddHours(2) };
            var entryThree = new ResidentsTeamGroupId() { Resident = residentThree, GroupId = groupIdThree, CreatedAt = currentDate.AddHours(3) };

            DatabaseContext.ResidentsTeamGroupId.Add(entryOne);
            DatabaseContext.ResidentsTeamGroupId.Add(entryTwo);
            DatabaseContext.ResidentsTeamGroupId.Add(entryThree);
            DatabaseContext.SaveChanges();

            var result = _classUnderTest.GetAllResidentIdsAndGroupIdsByFirstCharacter(guidCharacter);

            result.Should().HaveCount(1);
            result[0].GroupId.Should().Be(groupIdOne);
        }
    }
}
