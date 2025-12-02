using GeneralModal.TagHelper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GeneralModal.Extensions
{
    public static class ModalFluentExtensions
    {
        public static GenericModalBuilder Eorc_Modal(this IHtmlHelper helper, string id)
        {
            return new GenericModalBuilder(id);
        }
    }
}
