using System;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
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
                .With(x => x.IsHidden, false)
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Name, "Other Resident")
                .With(x => x.IsHidden, false)
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Name, "Test Resident2")
                .With(x => x.IsHidden, false)
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
                .With(x => x.IsHidden, false)
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Name, "other Resident")
                .With(x => x.IsHidden, false)
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Name, "test Resident2")
                .With(x => x.IsHidden, false)
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
                .With(x => x.IsHidden, false)
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Email, "OtherEmail@hackney.gov.uk")
                .With(x => x.IsHidden, false)
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Email, "TestEmail2@hackney.gov.uk")
                .With(x => x.IsHidden, false)
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
                .With(x => x.IsHidden, false)
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "567")
                .With(x => x.IsHidden, false)
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "1234")
                .With(x => x.IsHidden, false)
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
                .With(x => x.IsHidden, false)
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.PhoneNumber, "567")
                .With(x => x.IsHidden, false)
                .Create();
            var resident3 = _fixture.Build<Resident>()
                .With(x => x.Email, "TestEmail@hackney.gov.uk")
                .With(x => x.IsHidden, false)
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
        public void FindResidentsByGroupId()
        {
            // Arrange
            var team = "A Team";
            var groupId = new Guid("935f283d-cd81-468d-a96f-cad90d41e60d");
            var resident = _fixture.Build<Resident>()
                .With(x => x.IsHidden, false)
                .Create();
            var residentGroupId1 = _fixture.Build<ResidentsTeamGroupId>()
                .With(x => x.GroupId, groupId)
                .With(x => x.Team, team)
                .With(x => x.ResidentId, resident.Id)
                .Without(x => x.Resident)
                .Create();
            var residentGroupId2 = _fixture.Build<ResidentsTeamGroupId>()
                .With(x => x.GroupId, groupId)
                .With(x => x.Team, "Another Team")
                .With(x => x.ResidentId, resident.Id)
                .Without(x => x.Resident)
                .Create();

            DatabaseContext.ResidentsTeamGroupId.Add(residentGroupId1);
            DatabaseContext.ResidentsTeamGroupId.Add(residentGroupId2);
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            var expected = new Resident()
            {
                Id = resident.Id,
                CreatedAt = resident.CreatedAt,
                Email = resident.Email,
                IsHidden = false,
                Name = resident.Name
            };


            var request = new ResidentSearchQuery { Team = team, GroupId = groupId };

            // Act
            var found = _classUnderTest.FindResidentByGroupId(request);

            // Assert
            found.Id.Should().Be(expected.Id);
            found.Name.Should().Be(expected.Name);
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

            _classUnderTest.AddResidentGroupId(residentId, team, null);

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
        public void AddResidentGroupIdAddsNewEntryWithGroupIdProvided()
        {
            var residentId = Guid.NewGuid();
            var providedGroupId = Guid.NewGuid();
            var team = "some team";
            // Add resident for FK constraint
            var resident = _fixture.Create<Resident>();
            resident.Id = residentId;
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            _classUnderTest.AddResidentGroupId(residentId, team, providedGroupId);

            var query = DatabaseContext.ResidentsTeamGroupId.Where(x => x.ResidentId == residentId && x.Team == team);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Resident.Id.Should().Be(residentId);
            foundRecord.Team.Should().Be(team);
            foundRecord.GroupId.Should().Be(providedGroupId);
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
            result.Should().Be(Guid.Empty);
        }

        [Test]
        public void GetAllResidentIdsAndGroupIdsByFirstCharacter()
        {
            var currentDate = new DateTime();
            var residentOne = _fixture.Create<Resident>();
            var groupIdOne = new Guid("38703a76-3af6-48f5-aa1b-188679400136");
            var residentTwo = _fixture.Create<Resident>();
            var groupIdTwo = new Guid("48703a76-3af6-48f5-aa1b-188679400136");
            var residentThree = _fixture.Create<Resident>();
            var groupIdThree = new Guid("58703a76-3af6-48f5-aa1b-188679400136");

            var guidCharacters = groupIdOne.ToString().Substring(0, 2);

            var entryOne = new ResidentsTeamGroupId() { Resident = residentOne, GroupId = groupIdOne, CreatedAt = currentDate.AddHours(1) };
            var entryTwo = new ResidentsTeamGroupId() { Resident = residentTwo, GroupId = groupIdTwo, CreatedAt = currentDate.AddHours(2) };
            var entryThree = new ResidentsTeamGroupId() { Resident = residentThree, GroupId = groupIdThree, CreatedAt = currentDate.AddHours(3) };

            DatabaseContext.ResidentsTeamGroupId.Add(entryOne);
            DatabaseContext.ResidentsTeamGroupId.Add(entryTwo);
            DatabaseContext.ResidentsTeamGroupId.Add(entryThree);
            DatabaseContext.SaveChanges();

            var result = _classUnderTest.GetAllResidentIdsAndGroupIdsByFirstCharacter(guidCharacters);

            result.Should().HaveCount(1);
            result[0].GroupId.Should().Be(groupIdOne);
        }

        [Test]
        public void CanFindResidentTeamGroupIdByResidentIdAndTeam()
        {
            var residentId = new Guid("682a799b-36c3-47c5-85dc-2d65df0cdad7");
            var resident = _fixture.Create<Resident>();
            resident.Id = residentId;
            var team = "some team";
            var residentTeamGroupId = _fixture.Build<ResidentsTeamGroupId>()
                .With(x => x.Resident, resident)
                .Create();
            residentTeamGroupId.ResidentId = resident.Id;
            residentTeamGroupId.Team = team;
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);
            DatabaseContext.SaveChanges();

            var found = _classUnderTest.FindResidentTeamGroupIdByResidentIdAndTeam(residentId, team);
            found.Should().Be(residentTeamGroupId);
        }

        [Test]
        public void FindResidentTeamGroupIdByResidentIdAndTeamReturnsNull()
        {
            var resident = _fixture.Create<Resident>();
            var team = "some team";
            var residentTeamGroupId = _fixture.Build<ResidentsTeamGroupId>()
                .With(x => x.Resident, resident)
                .Create();
            residentTeamGroupId.ResidentId = resident.Id;
            residentTeamGroupId.Team = team;
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);
            DatabaseContext.SaveChanges();

            var found = _classUnderTest.FindResidentTeamGroupIdByResidentIdAndTeam(Guid.NewGuid(), team);
            found.Should().Be(null);
        }
        [Test]
        public void HideResidentHidesAResident()
        {
            var resident = _fixture.Create<Resident>();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            _classUnderTest.HideResident(resident.Id);

            var query = DatabaseContext.Residents.Where(x => x.Id == resident.Id && !x.IsHidden);

            query.Count()
                .Should()
                .Be(0);
        }
    }
}
