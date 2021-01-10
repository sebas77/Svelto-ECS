using System;

namespace Svelto.ECS
{
    public class ECSException : Exception
    {
        public ECSException(string message):base("<color=red>".FastConcat(message, "</color>"))
        {}
        
        public ECSException(string message, Exception innerE):base("<color=red>".FastConcat(message, "</color>"), innerE)
        {}
        
        public ECSException(string message, Type entityComponentType, Type type) :
            base(message.FastConcat(" entity view: '", entityComponentType.Name, "', field: '", type.Name))
        {
        }

        public ECSException(string message, Type entityComponentType) :
            base(message.FastConcat(" entity view: ", entityComponentType.Name))
        {
        }
    }
}