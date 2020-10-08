using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Core
{
    public static class EntityQueryExtensions
    {
#if NEVER
        public static ref T GetSingleton<T>(this EntityQuery<T> query) where T : struct
        {
            foreach (var entity in query)
            {
                // Need to have direct reference from query for this to work
                return ref entity.GetComponent<T>();
            }
        }
#endif
    }
}
