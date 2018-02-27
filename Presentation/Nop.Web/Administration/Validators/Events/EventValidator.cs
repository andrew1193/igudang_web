using FluentValidation;
using FluentValidation.Results;
using Nop.Admin.Models.Events;
using Nop.Core.Domain.Events;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Events
{
    public partial class EventValidator : BaseNopValidator<EventModel>
    {
        public EventValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Events.Events.Fields.Name.Required"));
            RuleFor(x => x.PageSizeOptions).Must(ValidatorUtilities.PageSizeOptionsValidator).WithMessage(localizationService.GetResource("Admin.Catalog.Events.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
            Custom(x =>
            {
                if (!x.AllowCustomersToSelectPageSize && x.PageSize <= 0)
                    return new ValidationFailure("PageSize", localizationService.GetResource("Admin.Events.Events.Fields.PageSize.Positive"));

                return null;
            });

            SetDatabaseValidationRules<Event>(dbContext);
        }
    }
}