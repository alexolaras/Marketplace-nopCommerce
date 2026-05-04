using FluentValidation;
using Nop.Plugin.Misc.InfigoMarketplace.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.InfigoMarketplace.Validators;

public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.BaseUrl)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.Fields.BaseUrl.Required"));

        RuleFor(model => model.HttpTimeoutSeconds)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.Fields.HttpTimeoutSeconds.Invalid"));
    }
}
