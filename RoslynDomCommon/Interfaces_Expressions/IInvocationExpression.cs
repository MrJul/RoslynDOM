﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynDom.Common
{
   public interface  IInvocationExpression : IHasInvocationFeatures,IExpression 
   {
      RDomCollection<IArgument> Arguments { get; }
   }
}
