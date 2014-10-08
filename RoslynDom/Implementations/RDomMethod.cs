using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RoslynDom.Common;
using System.Linq;
using System;
 using System.ComponentModel.DataAnnotations;
namespace RoslynDom
{
   public class RDomMethod : RDomBase<IMethod, IMethodSymbol>, IMethod
   {
      private RDomCollection<IParameter> _parameters;
      private RDomCollection<ITypeParameter> _typeParameters;
      private RDomCollection<IStatementCommentWhite> _statements;
      private AttributeCollection _attributes = new AttributeCollection();

      public RDomMethod(SyntaxNode rawItem, IDom parent, SemanticModel model)
         : base(rawItem, parent, model)
      { Initialize(); }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
     "CA1811:AvoidUncalledPrivateCode", Justification = "Called via Reflection")]
      internal RDomMethod(RDomMethod oldRDom)
           : base(oldRDom)
      {
         Initialize();
         Attributes.AddOrMoveAttributeRange(oldRDom.Attributes.Select(x => x.Copy()));
         var newParameters = RoslynDomUtilities.CopyMembers(oldRDom._parameters);
         Parameters.AddOrMoveRange(newParameters);
         var newTypeParameters = RoslynDomUtilities.CopyMembers(oldRDom._typeParameters);
         TypeParameters.AddOrMoveRange(newTypeParameters);
         var newStatements = RoslynDomUtilities.CopyMembers(oldRDom._statements);
         StatementsAll.AddOrMoveRange(newStatements);

         AccessModifier = oldRDom.AccessModifier;
         DeclaredAccessModifier = oldRDom.DeclaredAccessModifier;
         ReturnType = oldRDom.ReturnType;
         IsAbstract = oldRDom.IsAbstract;
         IsVirtual = oldRDom.IsVirtual;
         IsOverride = oldRDom.IsOverride;
         IsSealed = oldRDom.IsSealed;
         IsStatic = oldRDom.IsStatic;
         IsNew = oldRDom.IsNew;
         IsExtensionMethod = oldRDom.IsExtensionMethod;
      }

      private void Initialize()
      {
         _typeParameters = new RDomCollection<ITypeParameter>(this);
         _parameters = new RDomCollection<IParameter>(this);
         _statements = new RDomCollection<IStatementCommentWhite>(this);
      }

      public override IEnumerable<IDom> Children
      {
         get
         {
            var list = base.Children.ToList();
            list.AddRange(_statements);
            return list;
         }
      }

      public AttributeCollection Attributes
      { get { return _attributes; } }

      [Required]
      public string Name { get; set; }
      [Required]
      public IReferencedType ReturnType { get; set; }
      public AccessModifier AccessModifier { get; set; }
      public AccessModifier DeclaredAccessModifier { get; set; }
      public bool IsAbstract { get; set; }
      public bool IsVirtual { get; set; }
      public bool IsOverride { get; set; }
      public bool IsSealed { get; set; }
      public bool IsNew { get; set; }
      public bool IsStatic { get; set; }
      public bool IsExtensionMethod { get; set; }
      public IStructuredDocumentation StructuredDocumentation { get; set; }
      public string Description { get; set; }

      public RDomCollection<ITypeParameter> TypeParameters
      { get { return _typeParameters; } }

      public RDomCollection<IParameter> Parameters
      { get { return _parameters; } }

      public RDomCollection<IStatementCommentWhite> StatementsAll
      { get { return _statements; } }

      public IEnumerable<IStatement> Statements
      { get { return _statements.OfType<IStatement>().ToList(); } }

      public bool HasBlock
      {
         get { return true; }
         set { }
      }

      public MemberKind MemberKind
      { get { return MemberKind.Method; } }
   }
}
