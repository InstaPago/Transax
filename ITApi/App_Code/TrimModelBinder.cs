using System.Web.Mvc;

namespace ITApi
{
    /// <summary>
    /// Model Binder personalizado que elimina los espacios en blanco de todos 
    /// los inputs tipo string de la aplicacion
    /// </summary>
    /// <remarks>
    /// Source: https://stackoverflow.com/a/1734025
    /// </remarks>
    public class TrimStringModelBinder : System.Web.Mvc.DefaultModelBinder
    {
        protected override void SetProperty(ControllerContext controllerContext,
          System.Web.Mvc.ModelBindingContext bindingContext,
          System.ComponentModel.PropertyDescriptor propertyDescriptor, object value)
        {
            if (propertyDescriptor.PropertyType == typeof(string))
            {
                var stringValue = (string)value;
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    value = stringValue.Trim();
                }
                else
                {
                    value = null;
                }
            }

            base.SetProperty(controllerContext, bindingContext,
                                propertyDescriptor, value);
        }
    }

}