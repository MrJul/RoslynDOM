﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynDom.Common;

namespace RoslynDom
{
    /// <summary>
    /// Base class for Roslyn Dom navigation tree
    /// </summary>
    /// <remarks>
    /// Initialize must be called near end of the constructor. Existing RDom impelementations all do this.
    /// </remarks>
    public abstract class RDomBase : IRoslynDom
    {
        private PublicAnnotationList _publicAnnotations = new PublicAnnotationList();

        protected RDomBase(params PublicAnnotation[] publicAnnotations)
        {
            _publicAnnotations.Add(publicAnnotations);
        }

        protected RDomBase(IDom oldIDom)
        {
            var oldRDom = (RDomBase)oldIDom;
            Name = oldIDom.Name;
            _publicAnnotations.AddCopy(oldRDom._publicAnnotations);
        }

        /// <summary>
        /// Must at least set the Name property
        /// </summary>
        protected virtual void Initialize() { }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Return type is object, not SyntaxNode to match interface
        /// </remarks>
        public abstract object RawItem { get; }

        /// <summary>
        /// NOTE: This documentation has not been updated to reflect changes due to @beefarino's input
        /// <para>
        /// The name is the local name. The best name 
        /// is the name you are most likely to use inside the assembly and 
        /// outside the current type. This name is also legal inside the type.
        ///  </para><para>
        ///    - namespace: The full namespace, regardless of declaration nesting
        ///  </para><para>
        ///    - nested types: The type name plus the outer type name
        ///  </para><para>
        ///    - static members: The member name plus the containing type name
        ///  </para><para>
        ///    - root and usings: empty string
        ///  </para><para>
        ///    - other symbols: The symbol name
        /// </para>
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///  <para>
        /// Names are complicated. These are the distinctions for the RoslynDom
        ///  <para>
        ///    - Name: The local name, and same as the semantic tree symbol name
        ///  </para><para>
        ///    - OuterName: Described above
        ///  </para><para>
        ///    - QualifiedName: The name, nesting types, and namespace
        ///  </para><para>
        /// Part of the driver for this is removing differences based on namespace
        /// nesting and other layout details that are irrelevant to meaning. However, 
        /// it seemed unfriendly to have a name that differed from the Roslyn symbol name, 
        /// so namespaces are the immediate name, not the current namespace name. Thus
        /// Name on namespaces should be used with caution. Either use the Namespace property 
        /// on another item, or use the OuterName in almost all cases.
        ///  </para><para>
        /// NOTE: Naming for generics is not yet included. 
        ///  </para> 
        /// </remarks>
        public abstract string OuterName { get; }

        public abstract ISymbol Symbol { get; }

        public abstract object RequestValue(string propertyName);

        internal abstract ISymbol GetSymbol(SyntaxNode node);

        /// <summary>
        /// For a discussion of names <see cref="OuterName"/>
        /// </summary>
        /// <returns>The string name, same as Roslyn symbol's name</returns>
        public string Name { get; set; }

        public bool SameIntent<TLocal>(TLocal other)
               where TLocal : class
        {
            return SameIntent(other, true);
        }

           public bool SameIntent<TLocal>(TLocal other, bool includePublicAnnotations)
            where TLocal : class
        {
            return SameIntentInternal(other, includePublicAnnotations);
        }

       internal abstract bool SameIntentInternal<TLocal>(TLocal other, bool includePublicAnnotations)
                        where TLocal : class;

        public PublicAnnotationList PublicAnnotations
        { get { return _publicAnnotations; } }

        protected bool CheckPublicAnnotations(IDom other, bool includePublicAnnotations)
        {
            if (includePublicAnnotations)
            { if (!this.PublicAnnotations.SameIntent(other.PublicAnnotations)) return false; }
            return true;
        }


    }

    public abstract class RDomBase<T> : RDomBase, IDom<T>
            where T : class, IDom<T>
    {
        protected RDomBase(params PublicAnnotation[] publicAnnotations)
           : base(publicAnnotations)
        { }

        protected RDomBase(T oldRDom)
             : base(oldRDom)
        { }

        protected static IEnumerable<T> CopyMembers(IEnumerable<T> members)
        {
            var ret = new List<T>();
            if (members != null)
            {
                foreach (var member in members)
                {
                    ret.Add(member.Copy());
                }
            }
            return ret;
        }

        public virtual T Copy()
        {
            var type = this.GetType();
            var constructor = type.GetTypeInfo()
                .DeclaredConstructors
                .Where(x => x.GetParameters().Count() == 1
                && typeof(T).IsAssignableFrom(x.GetParameters().First().ParameterType))
                .FirstOrDefault();
            if (constructor == null) throw new InvalidOperationException("Missing constructor for clone");
            var newItem = constructor.Invoke(new object[] { this });
            return (T)newItem;
        }

        internal override bool SameIntentInternal<TLocal>(TLocal other, bool includePublicAnnotations)
        {
            if (other == null) { return false; }
            if (!typeof(T).IsAssignableFrom(typeof(TLocal))) { return false; }
            var otherAsT = other as T;
            if (!CheckSameIntent(otherAsT, includePublicAnnotations)) { return false; }
            return true;
        }

        /// <summary>
        /// Derived classes should override this to determine intent
        /// </summary>
        /// <param name="other"></param>
        /// <param name="includePublicAnnotations"></param>
        /// <returns></returns>
        protected virtual bool CheckSameIntent(T other, bool includePublicAnnotations)
        {
            var otherItem = other as RDomBase;
            if (!base.CheckPublicAnnotations(otherItem, includePublicAnnotations)) return false;
            return true;
        }


        //public bool SameIntent(T other)
        //{
        //    return SameIntent(other, true);
        //}

        // public virtual bool SameIntent(T other, bool includePublicAnnotations)
        //{
        //    var otherItem = other as RDomBase;
        //    if (!base.CheckSameIntent(otherItem, includePublicAnnotations)) return false;
        //    return true;
        //}

        protected bool CheckSameIntentChildList<TChild>(IEnumerable<TChild> thisList,
                IEnumerable<TChild> otherList)
             where TChild : class, IDom<TChild>
        {
            return CheckSameIntentChildList(thisList, otherList, null);
        }
        protected bool CheckSameIntentChildList<TChild>(IEnumerable<TChild> thisList,
             IEnumerable<TChild> otherList, Func<TChild, TChild, bool> compareDelegate)
                where TChild : class, IDom<TChild>
        {
            if (thisList == null) return (otherList == null);
            if (otherList == null) return false;
            if (thisList.Count() != otherList.Count()) return false;
            compareDelegate = compareDelegate ?? ((x, y) => x.Name == y.Name);
            if (thisList == null) return false; // can't happen, suppresse FxCop error
            foreach (var item in thisList)
            {
                var otherItem = otherList.Where(x => compareDelegate(x, item)).FirstOrDefault();
                if (otherItem == null) return false;
                if (!item.SameIntent(otherItem)) return false;
            }
            return true;
        }

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public abstract class RDomBase<T, TSyntax, TSymbol> : RDomBase<T>, IRoslynDom<T, TSyntax, TSymbol>
            where TSyntax : SyntaxNode
            where TSymbol : ISymbol
            where T : class, IDom<T>
    {
        private TSyntax _originalRawSyntax;
        private TSyntax _rawSyntax;
        private TSymbol _symbol;
        private IEnumerable<IAttribute> _attributes;

        protected RDomBase(TSyntax rawItem, params PublicAnnotation[] publicAnnotations)
            : base(publicAnnotations)
        {
            _rawSyntax = rawItem;
            _originalRawSyntax = rawItem;
        }

        protected RDomBase(T oldIDom)
             : base(oldIDom)
        {
            var oldRDom = oldIDom as RDomBase<T, TSyntax, TSymbol>;
            _rawSyntax = oldRDom._rawSyntax;
            _originalRawSyntax = oldRDom._originalRawSyntax;
            if (oldRDom._attributes != null)
            { _attributes = RDomBase<IAttribute>.CopyMembers(oldRDom._attributes.Cast<RDomAttribute>()); }
            Name = oldRDom.Name;
            //_symbol = default(TSymbol); // this should be reset, this line is to remind us
        }

        protected override void Initialize()
        {
            if (TypedSymbol != null)
            {
                Name = TypedSymbol.Name;
            }
            if (this is IHasAttributes)
            {

            }
        }

        //public override T Copy()
        //{
        //    var newVal = base.Copy() ;
        //    var newRDom = newVal as RDomBase;
        //    return newVal;
        //}

        protected override bool CheckSameIntent(T other, bool includePublicAnnotations)
        {
            if (!base.CheckSameIntent(other, includePublicAnnotations)) return false;
            var rDomOther = other as RDomBase<T, TSyntax, TSymbol>;
            if (rDomOther == null) return false;
            if (Name != rDomOther.Name) return false;
            if (!CheckSameIntentNamespaceNames(rDomOther)) return false;
            if (!CheckSameIntentAccessModifier(other)) return false;
            if (!CheckSameIntentAttributes(rDomOther)) return false;
            if (!CheckSameIntentAccessModifier(other)) return false;
            if (!CheckSameIntentStaticModfier(other)) return false;
            if (!CheckSameIntentReturnType(other)) return false;
            if (!CheckSameIntentPropertyOrMethod(other)) return false;

            return true;
        }

        private bool CheckSameIntentNamespaceNames(RDomBase<T, TSyntax, TSymbol> rDomOther)
        {
            if (this is IHasNamespace)
            {
                if (OuterName != rDomOther.OuterName) return false;
                if (GetNamespace() != rDomOther.GetNamespace()) return false;
                if (GetQualifiedName() != rDomOther.GetQualifiedName()) return false;
            }
            return true;
        }

        private bool CheckSameIntentAttributes(RDomBase<T, TSyntax, TSymbol> rDomOther)
        {
            if (this is IHasAttributes)
            {
                var attributes = this.GetAttributes();
                var otherAttributes = rDomOther.GetAttributes();
                if (attributes != null || otherAttributes != null)
                {
                    if (attributes == null && otherAttributes != null) return false;
                    if (attributes != null && otherAttributes == null) return false;
                    if (attributes.Count() != otherAttributes.Count()) return false;
                    foreach (var attribute in attributes)
                    {
                        // TODO: Consider multiple attributes of the same name and values/attribute type
                        var otherAttribute = otherAttributes.Where(x => x.Name == attribute.Name).FirstOrDefault();
                        if (otherAttribute == null) return false;
                        if (!attribute.SameIntent(otherAttribute)) return false;
                    }
                }
            }
            return true;
        }

        private bool CheckSameIntentAccessModifier(T other)
        {
            var item = this as IHasAccessModifier;
            if (item != null)
            {
                var otherItem = other as IHasAccessModifier;
                if (item.AccessModifier != otherItem.AccessModifier) return false;
            }
            return true;
        }

        private bool CheckSameIntentStaticModfier(T other)
        {
            var item = this as ICanBeStatic;
            if (item != null)
            {
                var otherItem = other as ICanBeStatic;
                if (item.IsStatic != otherItem.IsStatic) return false;
            }
            return true;
        }

        private bool CheckSameIntentReturnType(T other)
        {
            var item = this as IHasReturnType;
            if (item != null)
            {
                var otherItem = other as IHasReturnType;
                if (!item.ReturnType.SameIntent(otherItem.ReturnType)) return false;
            }
            return true;
        }

        private bool CheckSameIntentPropertyOrMethod(T other)
        {
            var item = this as IPropertyOrMethod;
            if (item != null)
            {
                var otherItem = other as IPropertyOrMethod;
                if (item.IsAbstract != otherItem.IsAbstract) return false;
                if (item.IsOverride != otherItem.IsOverride) return false;
                if (item.IsSealed != otherItem.IsSealed) return false;
                if (item.IsVirtual != otherItem.IsVirtual) return false;
            }
            return true;
        }

        public TSyntax TypedSyntax
        { get { return _rawSyntax; } }

        protected TSyntax OriginalTypedSyntax
        { get { return _originalRawSyntax; } }

        public override object RawItem
        { get { return _rawSyntax; } }

        public override ISymbol Symbol
        { get { return TypedSymbol; } }

        public virtual TSymbol TypedSymbol
        {
            get
            {
                if (_symbol == null)
                { _symbol = (TSymbol)GetSymbol(TypedSyntax); }
                return _symbol;
            }
        }

        public override string OuterName
        {
            get
            {
                // namespace overrides this
                var typeName = GetContainingTypeName(Symbol.ContainingType);
                return (string.IsNullOrWhiteSpace(typeName) ? "" : typeName + ".") +
                       Name;
            }
        }

        internal virtual string GetQualifiedName()
        {
            var namespaceName = RoslynDomUtilities.GetContainingNamespaceName(Symbol.ContainingNamespace);
            var typeName = GetContainingTypeName(Symbol.ContainingType);
            namespaceName = string.IsNullOrWhiteSpace(namespaceName) ? "" : namespaceName + ".";
            typeName = string.IsNullOrWhiteSpace(typeName) ? "" : typeName + ".";
            return namespaceName + typeName + Name;
        }

        internal virtual string GetNamespace()
        { return RoslynDomUtilities.GetContainingNamespaceName(Symbol.ContainingNamespace); }

        private static string GetContainingTypeName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null) return "";
            var parentName = GetContainingTypeName(typeSymbol.ContainingType);
            return (string.IsNullOrWhiteSpace(parentName) ? "" : parentName + ".") +
                typeSymbol.Name;
        }

        private SemanticModel GetModel()
        {
            var tree = TypedSyntax.SyntaxTree;
            var compilation = CSharpCompilation.Create("MyCompilation",
                                           options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                                           syntaxTrees: new[] { tree },
                                           references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);
            return model;

        }

        private MetadataReference mscorlib;
        private MetadataReference Mscorlib
        {
            get
            {
                if (mscorlib == null)
                {
                    mscorlib = new MetadataFileReference(typeof(object).Assembly.Location);
                }

                return mscorlib;
            }
        }

        internal override ISymbol GetSymbol(SyntaxNode node)
        {
            var model = GetModel();
            var symbol = (TSymbol)model.GetDeclaredSymbol(node);
            return symbol;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected IEnumerable<IAttribute> GetAttributes()
        {
            if (_attributes == null)
            {
                _attributes = RDomAttribute.MakeAttributes(this, Symbol, TypedSyntax);
            }
            return _attributes;
        }

        protected Microsoft.CodeAnalysis.TypeInfo GetTypeInfo(SyntaxNode node)
        {
            var model = GetModel();
            var tyypeInfo = model.GetTypeInfo(node);
            return tyypeInfo;
        }

        /// <summary>
        /// Fallback for getting requested values. 
        /// <br/>
        /// For special values (those that don't just return a property) override
        /// this method, return the approparite value, and olny call this base method when needed
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override object RequestValue(string propertyName)
        {
            if (ReflectionUtilities.CanGetProperty(this, propertyName))
            {
                var value = ReflectionUtilities.GetPropertyValue(this, propertyName);
                return value;
            }
            return null;
        }


    }

    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //public abstract class RDomSyntaxNodeBase<T, TSyntax, TSymbol> : RDomBase<T, TSyntax, TSymbol>
    //    where TSyntax : SyntaxNode
    //    where TSymbol : ISymbol
    //    where T : IDom<T>
    //{
    //    // TODO: Consider why this isn't collapsed into the RDomBase<T>
    //    //private TSyntax _rawItem;

    //    internal RDomSyntaxNodeBase(
    //        T oldRDom)
    //        : base(oldRDom)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected RDomSyntaxNodeBase(TSyntax rawItem,
    //                   params PublicAnnotation[] publicAnnotations)
    //             : base(rawItem, publicAnnotations)
    //    {
    //        //_rawItem = rawItem;
    //    }
    //}
}
