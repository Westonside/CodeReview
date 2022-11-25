using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace UserService.WebApi.Extensions;

public class EmptyStringAllowedModelMetadataProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Property)
        {
            context.DisplayMetadata.ConvertEmptyStringToNull = false;
        }
    }
}
