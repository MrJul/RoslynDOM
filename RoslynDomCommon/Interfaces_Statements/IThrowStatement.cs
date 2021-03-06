﻿using System.Collections.Generic;
using RoslynDom.Common;

namespace RoslynDom.Common
{
    public interface IThrowStatement : IStatement, IDom<IThrowStatement >
    {
        IExpression  ExceptionExpression { get; set; }
    }
}
