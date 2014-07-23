﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RoslynDom.Common;
using System.Reflection;

namespace RoslynDom.Common
{
    public static class RoslynDomSymbolUtilities
    {

        public static IEnumerable<ITypeParameter> TypeParametersFrom(this INamedTypeSymbol rDomType)
        {
            return TypeParametersFrom(rDomType.TypeParameters);
        }

        public static IEnumerable<ITypeParameter> TypeParametersFrom(this IMethodSymbol rDomType)
        {
            return TypeParametersFrom(rDomType.TypeParameters);
        }

        private static IEnumerable<ITypeParameter> TypeParametersFrom(IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            var retList = new List<ITypeParameter>();
            foreach (var param in typeParameters)
            {
                retList.Add(new RDomTypeParameter(param.DeclaringSyntaxReferences, param));
            }
            return retList;
        }

        public static IEnumerable<IReferencedType> ImpementedInterfacesFrom(this IHasImplementedInterfaces rDomType, bool all)
        {
            var symbol = ((IRoslynDom)rDomType).Symbol as INamedTypeSymbol;
            var retList = new List<IReferencedType>();
            IEnumerable<INamedTypeSymbol> interfaces;
            if (all) { interfaces = symbol.AllInterfaces; }
            else { interfaces = symbol.Interfaces; }
            foreach (var inter in interfaces)
            {
                retList.Add(new RDomReferencedType(inter.DeclaringSyntaxReferences, inter));

            }
            return retList;

        }

    }
}