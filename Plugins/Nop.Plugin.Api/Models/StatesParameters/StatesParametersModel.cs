using System.Web.Http.ModelBinding;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.StatesParameters
{
    [ModelBinder(typeof(ParametersModelBinder<StatesParametersModel>))]
    public class StatesParametersModel : BaseStatesParametersModel
    {
        // Nothing special here, created just for clarity.
    }
}
