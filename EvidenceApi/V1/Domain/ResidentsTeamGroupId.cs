using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Domain;

[Table("residents_team_group_id")]
public class ResidentsTeamGroupId : IEntity
{
    [Required]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("resident_id")]
    [ForeignKey("Resident")]
    public Guid ResidentId { get; set; }

    [ForeignKey("ResidentId")]
    public virtual Resident Resident { get; set; }

    [Column("team")]
    public string Team { get; set; }

    [Column("group_id")]
    public Guid? GroupId { get; set; } = null;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

}
