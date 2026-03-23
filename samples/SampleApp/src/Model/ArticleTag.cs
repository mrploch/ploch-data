using Ploch.Data.Model.CommonTypes;

namespace Ploch.Data.SampleApp.Model;

public class ArticleTag : Tag
{
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
