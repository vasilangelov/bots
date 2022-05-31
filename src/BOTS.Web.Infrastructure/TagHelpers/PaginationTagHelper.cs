namespace BOTS.Web.Infrastructure.TagHelpers
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement("pagination", TagStructure = TagStructure.WithoutEndTag)]
    public class PaginationTagHelper : TagHelper
    {
        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }

        public int Pages { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "nav";


        }

        private TagBuilder CreateULTagBuilder()
        {
            var ul = new TagBuilder("ul");



            return ul;
        }
    }
}
/*
<nav aria-label="...">
  <ul class="pagination">
    <li class="page-item"><a class="page-link" href="#">1</a></li>
    <li class="page-item active" aria-current="page">
      <span class="page-link">2</span>
    </li>
    <li class="page-item"><a class="page-link" href="#">3</a></li>
    <li class="page-item">
      <a class="page-link" href="#">Next</a>
    </li>
  </ul>
</nav>
 */