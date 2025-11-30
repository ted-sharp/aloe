using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;

namespace AloeSsmixSample.Data;

/// <summary>
/// SSMIXソース（ssmix_sources）
/// </summary>
public class SsmixSourceDto
{
    public Guid SourceId { get; set; }

    public int PtId { get; set; }

    public string SourceFile { get; set; } = String.Empty;

    public string SourceKey { get; set; } = String.Empty;

    public string SectionType { get; set; } = String.Empty;

    public string ContentHash { get; set; } = String.Empty;

    public string Content { get; set; } = String.Empty;
}
