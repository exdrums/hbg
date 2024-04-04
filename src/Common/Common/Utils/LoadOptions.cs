using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Common.Utils;

[ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))]
public class DevExtremeLoadOptions : DataSourceLoadOptionsBase 
{
    
}

public class DataSourceLoadOptionsBinder : IModelBinder {

    public Task BindModelAsync(ModelBindingContext bindingContext) {
        var loadOptions = new DevExtremeLoadOptions();
        DataSourceLoadOptionsParser.Parse(loadOptions, key => bindingContext.ValueProvider.GetValue(key).FirstOrDefault());
        bindingContext.Result = ModelBindingResult.Success(loadOptions);
        return Task.CompletedTask;
    }

}
