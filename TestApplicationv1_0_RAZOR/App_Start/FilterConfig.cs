using System.Web;
using System.Web.Mvc;

namespace TestApplicationv1_0_RAZOR
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
