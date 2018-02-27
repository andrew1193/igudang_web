using System.Web.Http.ModelBinding;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.CountriesParameters
{
    [ModelBinder(typeof(ParametersModelBinder<CountriesParametersModel>))]
    public class CountriesParametersModel : BaseCountriesParametersModel
    {
        
    }
}
