// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

#if !NETSTANDARD1_6 && !NETSTANDARD2_0
namespace TestCentric.Engine.Internal
{
    /// <summary>
    /// Represents the manner in which test assemblies use
    /// application domains to provide isolation
    /// </summary>
    public enum DomainUsage
    {
        /// <summary>
        /// Use the default setting, depending on the runner
        /// and the nature of the tests to be loaded.
        /// </summary>
        Default,
        /// <summary>
        /// Don't create a test domain - run in the primary application domain.
        /// Note that this requires the tests to be available in the
        /// NUnit appbase or probing path.
        /// </summary>
        None,
        /// <summary>
        /// Run tests in a single separate test domain
        /// </summary>
        Single,
        /// <summary>
        /// Run tests in a separate domain per assembly
        /// </summary>
        Multiple
    }
}
#endif
