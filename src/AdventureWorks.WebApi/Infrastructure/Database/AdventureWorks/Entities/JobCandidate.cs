using System;
using System.Collections.Generic;

namespace AdventureWorks.WebApi.Infrastructure.Database.AdventureWorks.Entities;

/// <summary>
/// R�sum�s submitted to Human Resources by job applicants.
/// </summary>
public partial class JobCandidate
{
    /// <summary>
    /// Primary key for JobCandidate records.
    /// </summary>
    public int JobCandidateId { get; set; }

    /// <summary>
    /// Employee identification number if applicant was hired. Foreign key to Employee.BusinessEntityID.
    /// </summary>
    public int? BusinessEntityId { get; set; }

    /// <summary>
    /// R�sum� in XML format.
    /// </summary>
    public string? Resume { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Employee? BusinessEntity { get; set; }
}
