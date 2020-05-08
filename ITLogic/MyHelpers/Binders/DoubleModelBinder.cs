using System;
using System.Globalization;
using System.Web.Mvc;

namespace MyHelpers.Binders
{
    public class DoubleModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);
            //valueResult.RawValue.ToString().Replace(",", ".");

            ModelState modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            try
            {
                actualValue = Convert.ToDouble(valueResult.AttemptedValue,
                    System.Globalization.CultureInfo.CurrentCulture);
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }
    }
}
