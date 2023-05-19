using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Vit.Extensions.Linq_Extensions
{


    public static partial class IMutableEntityType_Extensions
    {
        #region GetEntityTableName        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string GetEntityTableName(this IMutableEntityType data)
        {             
            return data?.Relational().TableName;
        }
        #endregion        

    }
}