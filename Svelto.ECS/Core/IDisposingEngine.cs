using System;

namespace Svelto.ECS
{
    public interface IDisposingEngine: IDisposable
    {
        bool isDisposing { set; }
    }
}