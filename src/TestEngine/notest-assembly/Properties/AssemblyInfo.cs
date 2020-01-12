// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using Test.Attributes;

// Attributes refrenced from other assemblies

// Even though this assembly has a reference to the NUnit
// framework, it declares itself as not containing tests.
[assembly: NUnit.Framework.NonTestAssembly]
[assembly: NUnit.Framework.LevelOfParallelism(3)]

// Attributes defined in this assembly
[assembly: NoArgs]

