using System.Data.Common;
using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Security;
using Aloe.Common.AloeCoreLib.Util;
using Microsoft.EntityFrameworkCore;

namespace AloeSsmixSample.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SsmixSource> SsmixSources { get; set; } = null!;

    public DbSet<MedicalEmbedding> MedicalEmbeddings { get; set; } = null!;
}
