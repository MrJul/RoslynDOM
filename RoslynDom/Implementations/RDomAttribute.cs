﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RoslynDom.Common;

namespace RoslynDom
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", 
        "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
                Justification ="Because this represents an attribute, it's an appropriate name")]
    public class RDomAttribute : RDomBase<IAttribute, ISymbol>, IAttribute
    {
        private List<IAttributeValue> _attributeValues = new List<IAttributeValue>();

        public RDomAttribute(SyntaxNode rawItem, IDom parent, SemanticModel model)
           : base(rawItem, parent, model)
        { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
          "CA1811:AvoidUncalledPrivateCode", Justification = "Called via Reflection")]
        internal RDomAttribute(RDomAttribute oldRDom)
            : base(oldRDom)
        {
            var newAttributeValues = RoslynDomUtilities.CopyMembers(oldRDom._attributeValues);
            foreach (var value in newAttributeValues)
            { AddOrMoveAttributeValue(value); }
        }

        public string Name { get; set; }

        public void RemoveAttributeValue(IAttributeValue attributeValue)
        { _attributeValues.Remove(attributeValue); }

        public void AddOrMoveAttributeValue(IAttributeValue attributeValue)
        { _attributeValues.Add(attributeValue); }

        public IEnumerable<IAttributeValue> AttributeValues
        { get { return _attributeValues; } }
    }
}
