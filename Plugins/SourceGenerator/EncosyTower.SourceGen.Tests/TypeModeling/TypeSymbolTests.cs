using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EncosyTower.SourceGen.TypeModeling;

namespace EncosyTower.SourceGen.Tests.TypeModeling;

[TestClass]
public class TypeSymbolTests
{
    private const string SOURCE = """
        using System;

        namespace TestNs
        {
            [Obsolete]
            public sealed class MyClass : IMyInterface
            {
                public static readonly int StaticField = 1;
                public readonly string ReadOnlyField = "x";
                private int _private = 0;

                public int Value { get; init; }
                public string Name { get; }
                public double ReadWrite { get; set; }

                public MyClass() { }

                public void DoWork(int count, string label) { }
                public static int Add(int a, int b) => a + b;
                private void Hidden() { }

                public static MyClass operator +(MyClass a, MyClass b) => a;
            }

            public readonly struct MyStruct
            {
                public const int MaxValue = 100;
                public int X { get; init; }
            }

            public record MyRecord(int Id, string Tag);

            public static class MyStaticClass { }

            public interface IMyInterface { }
        }
        """;

    // ─── TypeSymbol identity ───────────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_Name()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.AreEqual("MyClass", sym.Name);
    }

    [TestMethod]
    public void TypeSymbol_FullName_ContainsGlobalPrefix()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        StringAssert.StartsWith(sym.FullName, "global::");
        StringAssert.Contains(sym.FullName, "TestNs.MyClass");
    }

    [TestMethod]
    public void TypeSymbol_Namespace()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.AreEqual("TestNs", sym.Namespace);
    }

    [TestMethod]
    public void TypeSymbol_Accessibility_Public()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.AreEqual(Accessibility.Public, sym.Accessibility);
    }

    [TestMethod]
    public void TypeSymbol_TypeKind_Class()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.AreEqual(TypeKind.Class, sym.TypeKind);
    }

    // ─── Modifiers ────────────────────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_IsSealed_True_ForSealedClass()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.IsTrue(sym.IsSealed);
    }

    [TestMethod]
    public void TypeSymbol_IsStatic_True_ForStaticClass()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyStaticClass").ToTypeSymbol();
        Assert.IsTrue(sym.IsStatic);
    }

    [TestMethod]
    public void TypeSymbol_IsReadOnly_True_ForReadOnlyStruct()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyStruct").ToTypeSymbol();
        Assert.IsTrue(sym.IsReadOnly);
    }

    [TestMethod]
    public void TypeSymbol_IsRecord_True_ForRecord()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyRecord").ToTypeSymbol();
        Assert.IsTrue(sym.IsRecord);
    }

    // ─── Fields ───────────────────────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_Fields_Count()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        int count = 0;
        foreach (var _ in sym.Fields) count++;
        // StaticField, ReadOnlyField, _private + 3 compiler backing fields for auto-properties
        // Layer 1 TypeSymbol is a raw wrapper with no filtering
        Assert.AreEqual(6, count);
    }

    [TestMethod]
    public void TypeSymbol_Fields_NamesContainExpected()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var names = new List<string>();
        foreach (var f in sym.Fields) names.Add(f.Name);
        CollectionAssert.Contains(names, "StaticField");
        CollectionAssert.Contains(names, "ReadOnlyField");
        CollectionAssert.Contains(names, "_private");
    }

    // ─── Properties ───────────────────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_Properties_Count()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        int count = 0;
        foreach (var _ in sym.Properties) count++;
        Assert.AreEqual(3, count); // Value, Name, ReadWrite
    }

    [TestMethod]
    public void TypeSymbol_Properties_NamesContainExpected()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var names = new List<string>();
        foreach (var p in sym.Properties) names.Add(p.Name);
        CollectionAssert.Contains(names, "Value");
        CollectionAssert.Contains(names, "Name");
        CollectionAssert.Contains(names, "ReadWrite");
    }

    // ─── Methods (only Ordinary) ──────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_Methods_ExcludesOperators()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var names = new List<string>();
        foreach (var m in sym.Methods) names.Add(m.Name);
        // operator+ must NOT appear
        Assert.IsFalse(names.Any(n => n.StartsWith("op_")),
            "Expected operators to be excluded, but found: " + string.Join(", ", names.Where(n => n.StartsWith("op_"))));
    }

    [TestMethod]
    public void TypeSymbol_Methods_ContainsOrdinaryMethods()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var names = new List<string>();
        foreach (var m in sym.Methods) names.Add(m.Name);
        CollectionAssert.Contains(names, "DoWork");
        CollectionAssert.Contains(names, "Add");
        CollectionAssert.Contains(names, "Hidden");
    }

    // ─── Attributes ───────────────────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_HasAttribute_True_WhenPresent()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.IsTrue(sym.HasAttribute("global::System.ObsoleteAttribute"));
    }

    [TestMethod]
    public void TypeSymbol_HasAttribute_False_WhenAbsent()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.IsFalse(sym.HasAttribute("global::System.SerializableAttribute"));
    }

    [TestMethod]
    public void TypeSymbol_GetAttribute_ReturnsAttribute_WhenPresent()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var attr = sym.GetAttribute("global::System.ObsoleteAttribute");
        Assert.IsTrue(attr.Exists);
    }

    [TestMethod]
    public void TypeSymbol_GetAttribute_NotExists_WhenAbsent()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var attr = sym.GetAttribute("global::System.SerializableAttribute");
        Assert.IsFalse(attr.Exists);
    }

    // ─── ImplementsInterface ──────────────────────────────────────────────

    [TestMethod]
    public void TypeSymbol_ImplementsInterface_True_WhenImplemented()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.IsTrue(sym.ImplementsInterface("global::TestNs.IMyInterface"));
    }

    [TestMethod]
    public void TypeSymbol_ImplementsInterface_False_WhenNotImplemented()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        Assert.IsFalse(sym.ImplementsInterface("global::System.IDisposable"));
    }

    // ─── FieldSymbol properties ───────────────────────────────────────────

    [TestMethod]
    public void FieldSymbol_IsReadOnly_TrueForReadOnlyField()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        FieldSymbol found = default;
        foreach (var f in sym.Fields)
            if (f.Name == "ReadOnlyField") { found = f; break; }
        Assert.IsTrue(found.IsReadOnly);
        Assert.IsFalse(found.IsStatic);
    }

    [TestMethod]
    public void FieldSymbol_IsStatic_TrueForStaticField()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        FieldSymbol found = default;
        foreach (var f in sym.Fields)
            if (f.Name == "StaticField") { found = f; break; }
        Assert.IsTrue(found.IsStatic);
        Assert.IsTrue(found.IsReadOnly);
    }

    [TestMethod]
    public void FieldSymbol_IsConst_TrueForConst()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyStruct").ToTypeSymbol();
        FieldSymbol found = default;
        foreach (var f in sym.Fields)
            if (f.Name == "MaxValue") { found = f; break; }
        Assert.IsTrue(found.IsConst);
    }

    [TestMethod]
    public void FieldSymbol_TypeFullName_ContainsInt()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        FieldSymbol found = default;
        foreach (var f in sym.Fields)
            if (f.Name == "StaticField") { found = f; break; }
        StringAssert.Contains(found.TypeFullName, "int");
    }

    // ─── PropertySymbol accessor presence ────────────────────────────────

    [TestMethod]
    public void PropertySymbol_GetOnly_NoSetter()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        PropertySymbol found = default;
        foreach (var p in sym.Properties)
            if (p.Name == "Name") { found = p; break; }
        Assert.IsTrue(found.Getter.Exists);
        Assert.IsFalse(found.Setter.Exists);
    }

    [TestMethod]
    public void PropertySymbol_InitOnly_SetterIsInitOnly()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        PropertySymbol found = default;
        foreach (var p in sym.Properties)
            if (p.Name == "Value") { found = p; break; }
        Assert.IsTrue(found.Getter.Exists);
        Assert.IsTrue(found.Setter.Exists);
        Assert.IsTrue(found.Setter.IsInitOnly);
    }

    [TestMethod]
    public void PropertySymbol_ReadWrite_BothAccessors()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        PropertySymbol found = default;
        foreach (var p in sym.Properties)
            if (p.Name == "ReadWrite") { found = p; break; }
        Assert.IsTrue(found.Getter.Exists);
        Assert.IsTrue(found.Setter.Exists);
        Assert.IsFalse(found.Setter.IsInitOnly);
    }

    // ─── MethodSymbol Parameters ──────────────────────────────────────────

    [TestMethod]
    public void MethodSymbol_Parameters_Count()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        MethodSymbol found = default;
        foreach (var m in sym.Methods)
            if (m.Name == "DoWork") { found = m; break; }
        Assert.AreEqual(2, found.ParameterCount);
    }

    [TestMethod]
    public void MethodSymbol_Parameters_TypeNames()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        MethodSymbol found = default;
        foreach (var m in sym.Methods)
            if (m.Name == "DoWork") { found = m; break; }
        var parameters = found.Parameters;
        Assert.AreEqual(2, parameters.Length);
        StringAssert.Contains(parameters[0].TypeFullName, "int");
        StringAssert.Contains(parameters[1].TypeFullName, "string");
    }

    // ─── SymbolFilters ────────────────────────────────────────────────────

    [TestMethod]
    public void SymbolFilters_Public_OnlyPublicFields()
    {
        var sym = TypeModelingTestHelper.GetTypeSymbol(SOURCE, "TestNs.MyClass").ToTypeSymbol();
        var publicFields = new List<FieldSymbol>();
        foreach (var f in sym.Fields.Public()) publicFields.Add(f);
        // StaticField, ReadOnlyField are public; _private is not
        Assert.AreEqual(2, publicFields.Count);
        Assert.IsTrue(publicFields.All(f => f.Accessibility == Accessibility.Public));
    }

    [TestMethod]
    public void SymbolFilters_WithAttribute_ReturnsMatchingMethods()
    {
        const string SRC = """
            using System;
            namespace TestNs2
            {
                public class Annotated
                {
                    [Obsolete] public void Old() { }
                    public void Current() { }
                    [Obsolete] public void AlsoOld() { }
                }
            }
            """;
        var sym = TypeModelingTestHelper.GetTypeSymbol(SRC, "TestNs2.Annotated").ToTypeSymbol();
        var withObsolete = new List<MethodSymbol>();
        foreach (var m in sym.Methods.WithAttribute("global::System.ObsoleteAttribute"))
            withObsolete.Add(m);
        Assert.AreEqual(2, withObsolete.Count);
        CollectionAssert.Contains(withObsolete.Select(m => m.Name).ToList(), "Old");
        CollectionAssert.Contains(withObsolete.Select(m => m.Name).ToList(), "AlsoOld");
    }
}
