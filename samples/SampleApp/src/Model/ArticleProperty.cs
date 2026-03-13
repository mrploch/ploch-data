using Ploch.Data.Model.CommonTypes;

namespace Ploch.Data.SampleApp.Model;

public class ArticleProperty : StringProperty
{
    public int ArticleId { get; set; }

    public virtual Article Article { get; set; } = null!;
}
